using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Generation {
	public static class Generation { 
		private static Dictionary<RoomType, List<Room>> _availableRooms;
		private static readonly Random Random = new Random();
		private static List<Room> _recentlyAddedRooms;
		private static List<Room> _roomsToTreat;
		private static int _uniqueIds;
		
		[Server]
		// ReSharper disable once UnusedMember.Local
		public static IEnumerator GenerateLevel(Level level) {
			if (level.alreadyGenerated) yield break;
			Room[,] rMap = level.RoomsMap;
			List<Room> lMap = level.RoomsList;
			_recentlyAddedRooms = new List<Room>();
			_roomsToTreat = new List<Room>();
			_availableRooms = GetLevels();
			int rMaxY = rMap.GetLength(0);
			int rMaxX = rMap.GetLength(1);
			int x = 50;
			int y = 50;
			Room roomToPlace = _availableRooms[RoomType.Start][Random.Next(_availableRooms[RoomType.Start].Count)];
			(_roomsToTreat, _recentlyAddedRooms) = (_recentlyAddedRooms, new List<Room>());
			while (true) {
				if (TryAddRoom(lMap, rMap, roomToPlace, x, y))
					break;
				x = Random.Next(rMaxX);
				y = Random.Next(rMaxY);
			}
			Debug.Log("Generation placed 1st room");
			bool placedPreBossRoom = false;
			while (!placedPreBossRoom) {
				foreach (Room room in _roomsToTreat) {
					foreach ((char dir, int nbDir) in room._exits) {
						if (IsExitOccupied(rMap, room, dir, nbDir)) continue;
						(int rtcX, int rtcY) = room.UCoords;
						(int uW, int uH) = room.UDim;
						while (true) {
							Room roomToAdd = GenerateRoom(level.Shop, level.Chests, Random, lMap, placedPreBossRoom);
							if ((dir == 'T' && TryAddRoom(lMap, rMap, roomToAdd, rtcX + nbDir - 1, rtcY - uH) ||
							     dir == 'B' && TryAddRoom(lMap, rMap, roomToAdd, rtcX + nbDir - 1, rtcY + 1) ||
							     dir == 'L' && TryAddRoom(lMap, rMap, roomToAdd, rtcX - 1, rtcY - nbDir + 1) ||
							     dir == 'R' && TryAddRoom(lMap, rMap, roomToAdd, rtcX + uW, rtcY - nbDir + 1))
							) {
								if (roomToAdd.Type == RoomType.Chest) ++level.Chests;
								if (roomToAdd.Type == RoomType.Shop) {
									level.Shop = true;
									Debug.Log("Placed shop");
								}

								if (roomToAdd.Type == RoomType.PreBoss) {
									placedPreBossRoom = true;
									Debug.Log("PLaced Pre Boss Room");
								}
								_recentlyAddedRooms.Add(roomToAdd);
								break;
							}

							yield return null;
						}
					}
					if (!AreExitsOccupied(rMap, room)) _recentlyAddedRooms.Add(room);
				}

				if (_recentlyAddedRooms.Count == 0) Debug.Log("Ptn mec ça marche pas enft");
				(_roomsToTreat, _recentlyAddedRooms) = (_recentlyAddedRooms, new List<Room>());
				_roomsToTreat.Shuffle();
			}

			Debug.Log("Phase 1 complétée");
			List<Room> oldRooms = new List<Room>(lMap);
			List<Room> standardAddedRooms = new List<Room>();
			while (true) {
				foreach (Room room in oldRooms) {
					if (AreExitsOccupied(rMap, room)) continue;
					foreach ((char dir, int nbDir) in room._exits) {
						if (IsExitOccupied(rMap, room, dir, nbDir)) continue;
						(x, y) = room.UCoords;
						(int uH, int uW) = room.UDim;
						Room placedRoom;
						bool didPlacedRoom = dir == 'T' ? TryAddRoom(lMap, rMap, FindDeadEnd('B', out placedRoom), x + nbDir - 1, y - uH)
							: dir == 'B' ? TryAddRoom(lMap, rMap, FindDeadEnd('T', out placedRoom), x + nbDir - 1, y + 1)
							: dir == 'L' ? TryAddRoom(lMap, rMap, FindDeadEnd('R', out placedRoom), x - 1, y - nbDir + 1)
							: dir == 'R' ? TryAddRoom(lMap, rMap, FindDeadEnd('L', out placedRoom), x + uW, y - nbDir + 1)
							: throw new ArgumentException("Wrong letter");
						if (didPlacedRoom) {
							if (placedRoom.Type == RoomType.DeadEndChest) ++level.Chests;
							continue;
						}
						// This is ugly but it does work :p
						List<Room> rooms = new List<Room>(_availableRooms[RoomType.Standard]);
						rooms.Shuffle();
						foreach (Room roomToReplace in rooms) {
							if (dir == 'T' && TryAddRoom(lMap, rMap, roomToReplace, x + nbDir - 1, y - uH) ||
								dir == 'B' && TryAddRoom(lMap, rMap, roomToReplace, x + nbDir - 1, y + 1) ||
								dir == 'L' && TryAddRoom(lMap, rMap, roomToReplace, x - 1, y - nbDir + 1) ||
								dir == 'R' && TryAddRoom(lMap, rMap, roomToReplace, x + uW, y - nbDir + 1)
							) {
								standardAddedRooms.Add(roomToReplace);
								break;
							}
							
							yield return null;
						}
					}
				}

				if (standardAddedRooms.Count == 0) break;
				(standardAddedRooms, oldRooms) = (new List<Room>(), standardAddedRooms);
			}
			Debug.Log("Phase 2 complétée");
			//Filling phase
			foreach (Room room in lMap) {
				(x, y) = room.UCoords;
				(int uW, int uH) = room.UDim;
				for (int i = y + 1; i >= y - uH; --i) {
					for (int j = x - 1; j <= x + uW; j++) {
						if (rMap[i,j] == null) AddPrefab(j*20, i*-20, "Filler");
						yield return null;
					}
				}
			}
			level.alreadyGenerated = true;
			Debug.Log("terminé xptdr");
		}

		[Server]
		private static Room FindDeadEnd(char dir, out Room room) {
			RoomType type = Random.Next(100) <= 25 ? RoomType.DeadEndChest : RoomType.DeadEnd;
			foreach (Room roomToPlace in _availableRooms[type]) {
				++_uniqueIds;
				if (roomToPlace._exits[0].Item1 == dir) {
					room = new Room(roomToPlace, _uniqueIds);
					return room;
				}
			}
			throw new ArgumentException("FindDeadEnd: Room not found");
		}
		[Server]
		private static bool AreExitsOccupied(Room[,] rMap, Room room) {
			(int x, int y) = room.UCoords;
			(int uW, int uH) = room.UDim;
			foreach ((char dir, int nbDir) in room._exits) {
				switch (dir) {
					case 'T':
						if (y - 1 < 0 || rMap[y - uH, x + nbDir - 1] == null) return false;
						break;
					case 'B':
						if (y + 1 > rMap.GetLength(0) || rMap[y + 1, x + nbDir - 1] == null) return false;
						break;
					case 'L':
						if (x - 1 < 0 || rMap[y - nbDir + 1, x - 1] == null) return false;
						break;
					case 'R':
						if (x + 1 > rMap.GetLength(1) || rMap[y - nbDir + 1, x + uW] == null) return false;
						break;
					default:
						throw new ArgumentException("AreExitsOccupied: Wrong letter for an exit");
				}
			}
			return true;
		}
		
		[Server]
		private static bool IsExitOccupied(Room[,] rMap, Room room, char dir, int nbDir) {
			(int x, int y) = room.UCoords;
			(int uW, int uH) = room.UDim;
			return dir == 'T' ? rMap[y - uH, x + nbDir - 1] != null
				: dir == 'B' ? rMap[y + 1, x + nbDir - 1] != null
				: dir == 'R' ? rMap[y - nbDir + 1, x + uW] != null
				: dir == 'L' ? rMap[y - nbDir + 1, x - 1] != null
				: throw new ArgumentException("IsExitOccupied: wrong letter");
		}

		[Server]
		private static Room GenerateRoom(bool isThereAShop, int chests, Random seed, ICollection lMap, bool placedPreBossRoom) {
			RoomType roomType =
					!isThereAShop && seed.Next(100) <= (10 + lMap.Count / 2 >= 100 ? 80 : 10 + lMap.Count / 2)
						? RoomType.Shop
						: isThereAShop && !placedPreBossRoom && lMap.Count >= 20 &&
						  seed.Next(100) <= (-10 + lMap.Count * 2 >= 100 ? 80 : -10 + lMap.Count * 2)
							? RoomType.PreBoss
							: seed.Next(100) <= 10 / (chests + 1)
								? RoomType.Chest
								: RoomType.Standard
				;
			return new Room(_availableRooms[roomType][seed.Next(_availableRooms[roomType].Count)], ++_uniqueIds);
		}
		
		[Server]
		private static bool TryAddRoom(ICollection<Room> lMap, Room[,] rMap, Room room, int x, int y) {
			(int uRoomWidth, int uRoomHeight) = room.UDim;
			if (x < 0 || y < 0 || x >= rMap.GetLength(1) || y >= rMap.GetLength(0))
				return false;
			for (int i = y; i > y - uRoomHeight; --i)
				for (int j = x; j < uRoomWidth + x; ++j)
					if (rMap[i, j] != null)
						return false;
			if (!CheckForExits(rMap, room, x, y))
				return false;
			room.Coordinates = (x*20, y*-20);
			room.UCoords = (x, y);
			// Iterating twice because we checked if there was enough room (haha) first before placing anything
			for (int i = y; i > y - uRoomHeight; --i)
				for (int j = x; j < uRoomWidth + x; ++j)
					rMap[i, j] = room;
			lMap.Add(room);
			_recentlyAddedRooms.Add(room);
			AddPrefab(x*20,y*-20, room.Name);
			return true;
		}

		[Server]
		private static bool CheckForExits(Room[,] rMap, Room room, int x, int y) {
			(int uX, int uY) = (x, y);
			(int uW, int uH) = room.UDim;
			foreach ((char dir, int nbDir) in room._exits) {
				if (!(dir == 'T' && SubFunction(rMap, uX + nbDir - 1, uY - uH, 'B') ||
				      (dir == 'B' && SubFunction(rMap,uX + nbDir - 1, uY + 1, 'T')) ||
				      (dir == 'L' && SubFunction(rMap, uX - 1, uY - nbDir + 1, 'R')) ||
				      (dir == 'R' && SubFunction(rMap, uX + uW, uY - nbDir + 1, 'L')))
				) return false;
			}
			for (int i = uY; i > uY - uH; i--) {
				for (int j = uX; j < uW + uX; j++) {
					if (!(rMap[i + 1, j] == null || rMap[i + 1, j].UniqueId == room.UniqueId ||
					      (SubFunction(rMap, j, i + 1, 'T') == SubFunction2(room, j, i, 'B', uX, uY))))
						return false;
					if (!(rMap[i - 1, j] == null || rMap[i - 1, j].UniqueId == room.UniqueId ||
					      (SubFunction(rMap, j, i - 1, 'B') == SubFunction2(room, j, i, 'T', uX, uY))))
						return false;
					if (!(rMap[i, j + 1] == null || rMap[i, j + 1].UniqueId == room.UniqueId ||
					      (SubFunction(rMap, j + 1, i, 'L') == SubFunction2(room, j, i, 'R', uX, uY))))
						return false;
					if (!(rMap[i, j - 1] == null || rMap[i, j - 1].UniqueId == room.UniqueId ||
					      (SubFunction(rMap, j - 1, i, 'R') == SubFunction2(room, j, i, 'L', uX, uY))))
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// SubFunction in case of rooms which have not been placed yet
		/// </summary>
		/// <param name="room"></param>
		/// <param name="j"></param>
		/// <param name="i"></param>
		/// <param name="dirLkF"></param>
		/// <param name="uX"></param>
		/// <param name="uY"></param>
		/// <returns></returns>
		[Server]
		private static bool SubFunction2(Room room, int j, int i, char dirLkF, int uX, int uY) {
			foreach ((char dir, int nbDir) in room._exits) {
				if (dir == dirLkF && SubSubFunction(room, j, i, dir, nbDir, uX, uY))
					return true;
			}
			return false;
		}
		/// <summary>
		/// This function will return false if we cannot place the room given in argument in this particular place.
		/// It will verify for every exit of the room (Exits given in a previous function), if it is exiting into
		/// nothing (aka not placed room yet) it will return true, if it's out of bounds, it will return false.
		/// If there is another room, it will verify if it is the direction we're looking for (aka if we're checking the
		/// top exit of a room and there is a room at the top we're gonna search for bottom exits of the room. Then the
		/// SubSubFunction will verify if the bottom exit is correctly placed in front of the other exit in case of
		/// 36x36 or more rooms.
		/// </summary>
		/// <param name="rMap">Map of the Level</param>
		/// <param name="toX">Coordinates we're looking at for the exit.</param>
		/// <param name="toY">Coordinates we're looking at for the exit</param>
		/// <param name="dirLkF">Direction we're looking for, for instance if we were previously checking on top exits
		/// of a room, we will be looking for bottom exits on other rooms</param>
		/// <returns>True if the room can be placed here without conflicting with any exits, false otherwise</returns>
		[Server]
		private static bool SubFunction(Room[,] rMap,int toX, int toY, char dirLkF) { //DirLookingFor
			if (toX < 0 || toY < 0 || toX >= rMap.GetLength(1) || toY >= rMap.GetLength(0))
				return false;
			if (rMap[toY, toX] == null) return true;
			Room room = rMap[toY, toX];
			foreach ((char dir, int nbDir) in room._exits) {
				if (dir == dirLkF && SubSubFunction(room, toX, toY, dir, nbDir))
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// This function will verify if the exit called in SubFunction is correctly placed in case of 36x36 or more rooms
		/// </summary>
		/// <param name="room"></param>
		/// <param name="toX"></param>
		/// <param name="toY"></param>
		/// <param name="dir"></param>
		/// <param name="nbDir"></param>
		/// <param name="x">Given if the room isn't placed yet, aka doesn't have coordinates yet</param>
		/// <param name="y">Same than x</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		[Server]
		private static bool SubSubFunction(Room room, int toX, int toY, char dir, int nbDir, int x = -1, int y = -1) {
			if (x == y && y == -1)
				(x, y) = room.UCoords;
			(int uW, int uH) = room.UDim;
			switch (dir) {
				case 'T':
					return x + nbDir - 1 == toX && y - uH + 1 == toY;
				case 'B':
					return x + nbDir - 1 == toX && y == toY;
				case 'L':
					return x == toX && y - nbDir + 1 == toY;
				case 'R':
					return x + uW - 1 == toX && y - nbDir + 1 == toY;
				default:
					throw new ArgumentException("SubSubFunction Generation, invalid direction/letter");
			}
		}
		
		[Server]
		private static Dictionary<RoomType, List<Room>> GetLevels() {
			Dictionary<RoomType, List<Room>> ans = new Dictionary<RoomType, List<Room>>();
			for (int i = 0; i < 10; i++)
				ans.Add((RoomType) i, new List<Room>());
			foreach (Object o in Resources.LoadAll("Level1", typeof(GameObject))) {
				string roomName = o.name;
				Room room = new Room(Room.Generate(roomName), roomName);
				ans[room.Type].Add(room);
			}
			return ans;
		}

		[Server]
		private static void AddPrefab(int x, int y, string roomName) {
			Object objectToAdd = null;
			foreach (Object o in Resources.LoadAll("Level1", typeof(GameObject))) {
				if (o.name != roomName) continue;
				objectToAdd = o; break;
			}
			if (objectToAdd == null) throw new Exception("Room cannot be found");
			NetworkServer.Spawn((GameObject)Object.Instantiate(objectToAdd, new Vector3(x, y, 0), Quaternion.identity));
		}
		
		/// <summary>
		/// Shuffles a list, thanks ~Stack Overflow~ !
		/// (It's better when you understand what you do instead of copy-pasting from Stack Overflow...)
		/// </summary>
		/// <param name="list"></param>
		/// <typeparam name="T"></typeparam>
		[Server]
		private static void Shuffle<T>(this IList<T> list) {
			for (int i = 0; i < list.Count; ++i) {
				int j = Random.Next(list.Count);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}
	}
}