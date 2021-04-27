using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Generation {
	public static class Generation{
		private static Dictionary<RoomType, List<Room>> _availableRooms;
		private static readonly Random Random = new Random();

		[Command(requiresAuthority = false)]
		// ReSharper disable once UnusedMember.Local
		static void GenerateLevel(Level level) {
			if (level.alreadyGenerated) return;
			Room[,] rMap = level.RoomsMap;
			List<Room> lMap = level.RoomsList;
			_availableRooms = GetLevels();
			int rMaxY = rMap.GetLength(0);
			int rMaxX = rMap.GetLength(1);
			int x = Random.Next(rMaxX);
			int y = Random.Next(rMaxY);
			Room roomToPlace = _availableRooms[RoomType.Start][Random.Next(_availableRooms[RoomType.Start].Count)];
			while (true)
				if (TryAddRoom(lMap, rMap, roomToPlace, x, y))
					break;
			bool placedLastRoom = false;
			while (!placedLastRoom) {
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
			}
			// TODO: Need to add Boss Room & Exit room.
			level.alreadyGenerated = true;
		}

		private static Room GenerateRoom(bool isThereAShop, int chests, Random seed, ICollection lMap) {
			RoomType roomType = !isThereAShop && seed.Next(100) <= 10 + lMap.Count / 2
				? RoomType.Shop
				: seed.Next(100) <= 10 / (chests + 1)
				? RoomType.Chest
				: RoomType.Standard
			;
			return _availableRooms[roomType][seed.Next(_availableRooms[roomType].Count)];
		}
		
		private static bool TryAddRoom(ICollection<Room> lMap, Room[,] rMap, Room room, int x, int y) {
			(int roomWidth, int roomHeight) = room.Dimensions;
			if (x < 0 || y < 0 || x >= rMap.GetLength(1) || y >= rMap.GetLength(0))
				return false;
			for (int i = y; i < roomHeight / 16 + y; ++i)
				for (int j = x; j < roomWidth / 16 + x; ++j)
					if (rMap[y, x] != null || !CheckForExits(rMap, room, x, y, i, j))
						return false;
			// Iterating twice because we checked if there was enough room (haha) first before placing anything
			for (int i = y; i < roomHeight / 16 + y; ++i)
				for (int j = x; j < roomWidth / 16 + x; ++j)
					rMap[y, x] = room;
			room.Coordinates = (y, x);
			lMap.Add(room);
			AddPrefab(x,y, room.Name);
			return true;
		}

		private static bool SubFunction(int nbDir, int a, int b, Room room, int desiredDir, bool isX, char opposite) {
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
		}

		private static bool CheckForExits(Room[,] rMap, Room room, int x, int y, int i, int j) {
			foreach ((char direction, int nbDir) in room.Exits) {
				if (!(direction == 'B' ? SubFunction(nbDir, j - x + 1, i - 1, rMap[i - 1, j], j + 1, true, 'T')
					: direction == 'T' ? SubFunction(nbDir, j - x + 1, i + 1, rMap[i + 1, j], j + 1, true, 'B')
					: direction == 'L' ? SubFunction(nbDir, i - y + 1, j - 1, rMap[i, j - 1], i + 1, false, 'R')
					: direction == 'R' ? SubFunction(nbDir, i - y + 1, j + 1, rMap[i, j - 1], i + 1, false, 'L')
					: throw new ArgumentException("invalid letter")
				)) return false;
			}
			return true;
		}
		
		public static Dictionary<RoomType, List<Room>> GetLevels() {
			Dictionary<RoomType, List<Room>> ans = new Dictionary<RoomType, List<Room>>();
			for (int i = 0; i < 9; i++)
				ans.Add((RoomType) i, new List<Room>());
			foreach (Object o in Resources.LoadAll("Level", typeof(GameObject))) {
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
