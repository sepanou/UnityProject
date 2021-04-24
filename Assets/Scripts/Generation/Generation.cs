using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;

namespace Generation {
	public class Generation: NetworkBehaviour {
		private Dictionary<RoomType, List<Room>> _availableRooms;
		private readonly Random _random = new Random();
		
		void GenerateLevel(Level level) {
			Room[,] rMap = level.RoomsMap;
			List<Room> lMap = level.RoomsList;
			_availableRooms = GetLevels();
			int rMaxY = rMap.GetLength(0);
			int rMaxX = rMap.GetLength(1);
			int x = _random.Next(rMaxX);
			int y = _random.Next(rMaxY);
			Room roomToPlace = _availableRooms[RoomType.Start][_random.Next(_availableRooms[RoomType.Start].Count)];
			while (true)
				if (TryAddRoom(lMap, rMap, roomToPlace, x, y))
					break;
			bool placedLastRoom = false;
			while (!placedLastRoom) {
				foreach (Room roomToCheckOn in lMap) {
					foreach ((char dir, int nbDir) in roomToCheckOn.GetExits()) {
						(int rtcy, int rtcx) = roomToCheckOn.Coordinates;
						Room roomToAdd = GenerateRoom(level.Shop, level.Chests, _random, lMap);
						if (!(dir switch {
							'T' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1),
							'B' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1),
							'L' => TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1),
							'R' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1),
							_ => throw new Exception("invalid letter")
						})) continue;
						if (roomToAdd.GetRoomType() == RoomType.Chest)
							level.Chests += 1;
						if (roomToAdd.GetRoomType() == RoomType.Shop)
							level.Shop = true;
					}
				}
				int prob = _random.Next(100);
				if (level.RoomsList.Count > 15 && prob < level.RoomsList.Count) return;
				foreach (Room roomToCheckOn in lMap) {
					foreach ((char dir, int nbDir) in roomToCheckOn.GetExits()) {
						(int rtcy, int rtcx) = roomToCheckOn.Coordinates;
						Room roomToAdd = _availableRooms[RoomType.PreBoss][_random.Next(_availableRooms[RoomType.PreBoss].Count)];
						if (dir switch {
								'T' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1),
								'B' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1),
								'L' => TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1),
								'R' => TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1),
								_ => throw new Exception("invalid letter")
						}) placedLastRoom = true;
					} 
				}
			}
			// TODO: Need to add Boss Room & Exit room.
		}

		private Room GenerateRoom(bool isThereAShop, int chests, Random seed, ICollection lMap) {
			RoomType roomType = !isThereAShop && seed.Next(100) <= 10 + lMap.Count / 2
				? RoomType.Shop
				: seed.Next(100) <= 10 / (chests + 1)
				? RoomType.Chest
				: RoomType.Standard
			;
			return _availableRooms[roomType][seed.Next(_availableRooms[roomType].Count)];
		}
		
		private bool TryAddRoom(ICollection<Room> lMap, Room[,] rMap, Room room, int x, int y) {
			(int roomWidth, int roomHeight) = room.GetDimensions();
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
			return true;
		}

		private bool SubFunction(int nbDir, int a, int b, Room room, int desiredDir, bool isX) {
			if (a != nbDir) return true;
			if (b < 0) return false;
			if (room == null) return true;
			Room neighbour = room;
			(int nX, int nY) = neighbour.Dimensions;
			desiredDir -= isX ? nX : nY;
			foreach ((char nDir, int nNbDir) in neighbour.GetExits()) {
				if (nDir != 'T' || nNbDir != desiredDir) continue;
				return true;
			}
			return false;
		}

		private bool CheckForExits(Room[,] rMap, Room room, int x, int y, int i, int j) {
			foreach ((char direction, int nbDir) in room.GetExits()) {
				if (!(direction switch {
					'B' => SubFunction(nbDir, j - x + 1, i - 1, rMap[i - 1, j], j + 1, true),
					'T' => SubFunction(nbDir, j - x + 1, i + 1, rMap[i + 1, j], j + 1, true),
					'L' => SubFunction(nbDir, i - y + 1, j - 1, rMap[i, j - 1], i + 1, false),
					'R' => SubFunction(nbDir, i - y + 1, j + 1, rMap[i, j - 1], i + 1, false),
					_ => throw new ArgumentException("invalid letter")
				})) return false;
			}
			return true;
		}

		private Dictionary<RoomType, List<Room>> GetLevels() {
			throw new NotImplementedException();
		}
	}
}
