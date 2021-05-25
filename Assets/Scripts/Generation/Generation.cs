using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Generation {
	public static class Generation {
		private static Dictionary<RoomType, List<Room>> _availableRooms;
		private static readonly Random Random = new Random();
		private static List<Room> _recentlyAddedRooms;
		private static List<Room> _roomsToTreat;

		[Command(requiresAuthority = false)]
		// ReSharper disable once UnusedMember.Local
		public static void GenerateLevel(Level level) {
			if (level.alreadyGenerated) return;
			Room[,] rMap = level.RoomsMap;
			List<Room> lMap = level.RoomsList;
			_recentlyAddedRooms = new List<Room>();
			_roomsToTreat = new List<Room>();
			_availableRooms = GetLevels();
			int rMaxY = rMap.GetLength(0);
			int rMaxX = rMap.GetLength(1);
			int x = 50;
			int y = 50;
			Debug.Log(_availableRooms.Count);
			Room roomToPlace = _availableRooms[RoomType.Start][Random.Next(_availableRooms[RoomType.Start].Count)];
			(_roomsToTreat, _recentlyAddedRooms) = (_recentlyAddedRooms, new List<Room>());
			while (true) {
				if (TryAddRoom(lMap, rMap, roomToPlace, x, y))
					break;
				x = Random.Next(rMaxX);
				y = Random.Next(rMaxY);
			}

			int maxRooms = 20;
			bool placedPreBossRoom = false;
			while (!placedPreBossRoom) {
				Debug.Log("Saluuuuuuuuuut");
				if (lMap.Count >= 20) break; // Debug
				foreach (Room room in _roomsToTreat) {
					int j = 0;
					foreach ((char dir, int nbDir) in room._exits) {
						if (IsExitOccupied(rMap, room, dir, nbDir)) continue;
						(int rtcx, int rtcy) = room.uCoords;
						int i = 0;
						while (i < 100) {
							Room roomToAdd = GenerateRoom(level.Shop, level.Chests, Random, lMap);
							if ((dir == 'T' && TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1) ||
							     dir == 'B' && TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1) ||
							     dir == 'L' && TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy - nbDir + 1) ||
							     dir == 'R' && TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy - nbDir + 1))
							) {
								j++;
								if (roomToAdd.Type == RoomType.Chest)
									level.Chests += 1;
								if (roomToAdd.Type == RoomType.Shop)
									level.Shop = true;
								break;
							}

							i++;
						}
						if (i >= 100) {
							_recentlyAddedRooms.Add(room);
						}
					}
				}

				if (_recentlyAddedRooms.Count == 0) break; 
				(_roomsToTreat, _recentlyAddedRooms) = (_recentlyAddedRooms, new List<Room>());
			}
			/*while (!placedLastRoom) {
				foreach (Room roomToCheckOn in lMap) {
					if (roomToCheckOn.Type == RoomType.DeadEnd) continue;
					foreach ((char dir, int nbDir) in roomToCheckOn.Exits) {
						(int rtcy, int rtcx) = roomToCheckOn.Coordinates;
						Room roomToAdd = GenerateRoom(level.Shop, level.Chests, Random, lMap);
						if (!(dir == 'T' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1)
							: dir == 'B' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1)
							: dir == 'L' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1)
							: dir == 'R' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1)
							: throw new Exception("invalid letter")
						)) continue;
						if (roomToAdd.Type == RoomType.Chest)
							level.Chests += 1;
						if (roomToAdd.Type == RoomType.Shop)
							level.Shop = true;
					}
				}
				int prob = Random.Next(100);
				if (level.RoomsList.Count > 15 && prob < level.RoomsList.Count) return;
				foreach (Room roomToCheckOn in lMap) {
					foreach ((char dir, int nbDir) in roomToCheckOn.Exits) {
						if (roomToCheckOn.Type == RoomType.Start) continue;
						(int rtcy, int rtcx) = roomToCheckOn.Coordinates;
						Room roomToAdd = _availableRooms[RoomType.PreBoss][Random.Next(_availableRooms[RoomType.PreBoss].Count)];
						if (dir == 'T' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1)
							: dir == 'B' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1)
							: dir == 'L' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1)
							: dir == 'R' ? TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1)
							: throw new Exception("invalid letter")
						) placedLastRoom = true;
					} 
				}
			}*/
			// TODO: Need to add Boss Room & Exit room.
			level.alreadyGenerated = true;
		}

		private static bool IsExitOccupied(Room[,] rMap, Room room, char dir, int nbDir) {
			(int x, int y) = room.uCoords;
			(int uW, int uH) = room.UDim;
			switch (dir) {
				case 'T':
					return rMap[y - uH, x + nbDir - 1] != null;
				case 'B':
					return rMap[y + 1, x + nbDir - 1] != null;
				case 'R':
					return rMap[y - nbDir + 1, x + uW] != null;
				case 'L':
					return rMap[y - nbDir + 1, x - 1] != null;
				default:
					throw new ArgumentException("IsExitOccupied: wrong letter");
			}
		}

		private static Room GenerateRoom(bool isThereAShop, int chests, Random seed, ICollection lMap) {
			RoomType roomType = !isThereAShop && seed.Next(100) <= 10 + lMap.Count / 2
				? RoomType.Shop
				: seed.Next(100) <= 5 / (chests + 1)
				? RoomType.Chest
				: RoomType.Standard
			;
			if (roomType == RoomType.Shop) roomType = RoomType.Standard; // TODO : SHOP ROOM, REMOVE THIS LINE AFTER
			return _availableRooms[roomType][seed.Next(_availableRooms[roomType].Count)];
		}
		
		private static bool TryAddRoom(ICollection<Room> lMap, Room[,] rMap, Room room, int x, int y) {
			(int uroomWidth, int uroomHeight) = room.UDim;
			if (x < 0 || y < 0 || x >= rMap.GetLength(1) || y >= rMap.GetLength(0))
				return false;
			for (int i = y; i > y - uroomHeight; --i)
				for (int j = x; j < uroomWidth + x; ++j)
					if (rMap[i, j] != null)
						return false;
			if (!CheckForExits(rMap, room, x, y))
				return false;
			room.Coordinates = (x*20, y*-20);
			room.uCoords = (x, y);
			// Iterating twice because we checked if there was enough room (haha) first before placing anything
			for (int i = y; i > y - uroomHeight; --i)
				for (int j = x; j < uroomWidth + x; ++j)
					rMap[i, j] = room;
			lMap.Add(room);
			_recentlyAddedRooms.Add(room);
			AddPrefab(x*20,y*-20, room.Name);
			return true;
		}

		/*private static bool SubFunction(int nbDir, int a, int b, Room room, int desiredDir, bool isX, char opposite) {
			if (a != nbDir) return true;
			if (b < 0) return false;
			if (room == null) return true;
			Room neighbour = room;
			(int nX, int nY) = neighbour.Dimensions;
			desiredDir -= isX ? nX : nY;
			foreach ((char nDir, int nNbDir) in neighbour.Exits) {
				if (nDir != opposite || nNbDir != desiredDir) continue;
				return true;
			}
			return false;
		}*/

		private static bool CheckForExits(Room[,] rMap, Room room, int x, int y) {
			/*foreach ((char direction, int nbDir) in room.Exits) {
				if (!(direction == 'B' ? SubFunction(nbDir, j - x + 1, i - 1, rMap[i - 1, j], j + 1, true, 'T')
					: direction == 'T' ? SubFunction(nbDir, j - x + 1, i + 1, rMap[i + 1, j], j + 1, true, 'B')
					: direction == 'L' ? SubFunction(nbDir, i - y + 1, j - 1, rMap[i, j - 1], i + 1, false, 'R')
					: direction == 'R' ? SubFunction(nbDir, i - y + 1, j + 1, rMap[i, j - 1], i + 1, false, 'L')
					: throw new ArgumentException("invalid letter")
				)) return false;
			}
			return true;*/
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
					if (!(rMap[i + 1, j] == null ||
					      (SubFunction(rMap, j, i + 1, 'T') && SubFunction2(rMap, room, j, i,'B', uX, uY))))
						return false;
					if (!(rMap[i - 1, j] == null ||
					      (SubFunction(rMap, j, i - 1, 'B') && SubFunction2(rMap, room, j, i, 'T', uX, uY))))
						return false;
					if (!(rMap[i, j + 1] == null ||
					      (SubFunction(rMap, j + 1, i, 'L') && SubFunction2(rMap, room, j, i, 'R', uX, uY))))
						return false;
					if (!(rMap[i, j - 1] == null ||
					      (SubFunction(rMap, j - 1, i, 'R') && SubFunction2(rMap, room, j, i, 'L', uX, uY))))
						return false;
				}
			}
			return true;
		}

		private static bool SubFunction2(Room[,] rMap, Room room, int j, int i, char dirLkF, int uX, int uY) {
			(int x, int y) = room.uCoords;
			foreach ((char dir, int nbDir) in room._exits) {
				if (dir == dirLkF && SubSubFunction(room, j, i, dir, nbDir, uX, uY))
					return true;
			}
			return false;
		}
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
		
		private static bool SubSubFunction(Room room, int toX, int toY, char dir, int nbDir, int x = -1, int y = -1) {
			
			if (x == y && y == -1)
				(x, y) = room.uCoords;
			(int uW, int uH) = room.UDim;
			switch (dir) {
				case 'T':
					return x + nbDir - 1 == toX && y - uH + 1 == toY;
				case 'B':
					return x + nbDir - 1 == toX && y == toY;
				case 'L':
					return x == toX && y - nbDir + 1 == toY;
				case 'R':
					return x == toX + uW - 1 && y - nbDir + 1 == toY;
				default:
					throw new ArgumentException("SubSubFunction Generation, invalid direction/letter");
			}
		}
		
		public static Dictionary<RoomType, List<Room>> GetLevels() {;
			Dictionary<RoomType, List<Room>> ans = new Dictionary<RoomType, List<Room>>();
			for (int i = 0; i < 9; i++)
				ans.Add((RoomType) i, new List<Room>());
			foreach (Object o in Resources.LoadAll("Level1", typeof(GameObject))) {
				string roomName = o.name;
				Room room = new Room(Room.Generate(roomName), roomName);
				ans[room.Type].Add(room);
			}
			return ans;
		}

		[Command(requiresAuthority = false)]
		public static void AddPrefab(int x, int y, string roomName) {
			Object objectToAdd = null;
			foreach (Object o in Resources.LoadAll("Level1", typeof(GameObject))) {
				if (o.name != roomName) continue;
				objectToAdd = o; break;
			}
			if (objectToAdd == null) throw new Exception("Room cannot be found");
			Object.Instantiate(objectToAdd, new Vector3(x, y, 0), Quaternion.identity);
		}
	}
}
