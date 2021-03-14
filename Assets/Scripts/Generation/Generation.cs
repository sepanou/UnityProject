using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Generation;
using Mirror;
using Random = System.Random;

namespace Generation
{
    public class Generation : NetworkBehaviour
    {
        private Dictionary<RoomType, List<Room>> _availableRooms;
        private Random _seed;
        
        void GenerateLevel(Level level)
        {
            _seed = new Random();
            Room[,] rMap = level.RoomsMap;
            List<Room> lMap = level.RoomsList;
            char[,] cMap = level.CharMap;
            _availableRooms = GetLevels();
            int rMaxY = rMap.GetLength(0);
            int rMaxX = rMap.GetLength(1);
            int x = _seed.Next(rMaxX);
            int y = _seed.Next(rMaxY);
            Room roomToPlace;
            roomToPlace = _availableRooms[RoomType.Start][_seed.Next(_availableRooms[RoomType.Start].Count)];
            while (true)
            {
                if (TryAddRoom(lMap, rMap, roomToPlace, x, y))
                {
                    break;
                }
            }
            bool PlacedLastRoom = false;
            while (!PlacedLastRoom)
            {
                foreach (Room roomToCheckOn in lMap)
                {
                    foreach ((char Dir, int nbDir) in roomToCheckOn.GetExits())
                    {
                        (int rtcy, int rtcx) = roomToCheckOn.Coordinates;
                        Room roomToAdd = GenerateRoom(level.Shop, level.Chests, _seed, lMap);
                        switch (Dir)
                        {
                            case 'T':
                                if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1))
                                {
                                    if (roomToAdd.GetRoomType() == RoomType.Chest)
                                    {
                                        level.Chests += 1;
                                    }

                                    if (roomToAdd.GetRoomType() == RoomType.Shop)
                                    {
                                        level._shop = true;
                                    }
                                }

                                break;
                            
                            case 'B':
                                if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1))
                                {
                                    if (roomToAdd.GetRoomType() == RoomType.Chest)
                                    {
                                        level.Chests += 1;
                                    }

                                    if (roomToAdd.GetRoomType() == RoomType.Shop)
                                    {
                                        level._shop = true;
                                    }
                                }

                                break;
                            
                            case 'L':
                                if (TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1))
                                {
                                    if (roomToAdd.GetRoomType() == RoomType.Chest)
                                    {
                                        level.Chests += 1;
                                    }

                                    if (roomToAdd.GetRoomType() == RoomType.Shop)
                                    {
                                        level._shop = true;
                                    }
                                }

                                break;
                            
                            
                            case 'R':
                                if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1))
                                {
                                    if (roomToAdd.GetRoomType() == RoomType.Chest)
                                    {
                                        level.Chests += 1;
                                    }

                                    if (roomToAdd.GetRoomType() == RoomType.Shop)
                                    {
                                        level._shop = true;
                                    }
                                }

                                break;
                        }
                    }
                }

                int prob = _seed.Next(100);
                if (level.RoomsList.Count > 15 && prob < level.RoomsList.Count)
                {
                    foreach (Room roomToCheckOn in lMap)
                    {
                        foreach ((char Dir, int nbDir) in roomToCheckOn.GetExits())
                        {
                            (int rtcy, int rtcx) = roomToCheckOn.Coordinates;
                            Room roomToAdd =
                                _availableRooms[RoomType.PreBoss][_seed.Next(_availableRooms[RoomType.PreBoss].Count)];
                            switch (Dir)
                            {
                                case 'T':
                                    if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy + 1))
                                    {
                                        PlacedLastRoom = true;
                                    }

                                    break;
                            
                                case 'B':
                                    if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + nbDir - 1, rtcy - 1))
                                    {
                                        PlacedLastRoom = true;
                                    }

                                    break;
                            
                                case 'L':
                                    if (TryAddRoom(lMap, rMap, roomToAdd, rtcx - 1, rtcy + nbDir - 1))
                                    {
                                        PlacedLastRoom = true;
                                    }

                                    break;
                            
                            
                                case 'R':
                                    if (TryAddRoom(lMap, rMap, roomToAdd, rtcx + 1, rtcy + nbDir - 1))
                                    {
                                        PlacedLastRoom = true;
                                    }

                                    break;
                            }
                        } 
                    }
                }
            }
            // Need to add Boss Room & Exit room.

        }

        private Room GenerateRoom(bool isThereAShop, int chests, Random seed, List<Room> lMap)
        {
            RoomType toGenerate;
            int prob = seed.Next(100);
            if (!isThereAShop && prob <= 10 + lMap.Count / 2)
            {
                isThereAShop = true;
                return _availableRooms[RoomType.Shop][seed.Next(_availableRooms[RoomType.Shop].Count)];
            }

            if (prob <= 10 / (chests + 1))
            {
                return _availableRooms[RoomType.Chest][seed.Next(_availableRooms[RoomType.Chest].Count)];
            }
            
            return _availableRooms[RoomType.Standard][seed.Next(_availableRooms[RoomType.Standard].Count)];
        }
        private bool TryAddRoom(List<Room> lMap, Room[,] rMap, Room room, int x, int y)
        {
            (int roomWidth, int roomHeight) = room.GetDimensions();
            if (x < 0 || y < 0 || x >= rMap.GetLength(1) || y >= rMap.GetLength(0))
            {
                return false;
            }
            for (int i = y; i < roomHeight/16 + y; i++)
            {
                for (int j = x; j < roomWidth/16 + x; j++)
                {
                    if (rMap[y, x] != null)
                    {
                        return false;
                    }

                    if (!CheckForExits(rMap, room, x, y, i, j))
                    {
                        return false;
                    }
                }
            }
            // Iterating twice because we checked if there was enough room (haha) first before placing anything
            for (int i = y; i < roomHeight/16 + y; i++)
            {
                for (int j = x; j < roomWidth/16 + x; j++)
                {
                    rMap[y, x] = room;
                }
            }

            room.Coordinates = (y, x);
            lMap.Add(room);
            return true;
        }

        private bool CheckForExits(Room[,] rMap, Room room, int x, int y, int i, int j)
        {
            foreach ((char direction, int nbDir) in room.GetExits())
            {
                switch (direction)
                {
                    case 'B':
                        if (j - x + 1 == nbDir)
                        {
                            if (i - 1 < 0)
                            {
                                return false;
                            }

                            if (rMap[i - 1, j] != null)
                            {
                                Room neighbour = rMap[i - 1, j];
                                (int nX, _) = neighbour.Dimensions;
                                int desiredDir = j - nX + 1;
                                bool compatibleExit = false;
                                foreach ((char nDir, int nNbDir) in neighbour.GetExits())
                                {
                                    if (nDir == 'T' && nNbDir == desiredDir)
                                    {
                                        compatibleExit = true;
                                        break;
                                    }
                                }

                                if (!compatibleExit)
                                {
                                    return false;
                                }
                            }
                        }

                        break;
                    
                    case 'T':
                        if (j - x + 1 == nbDir)
                        {
                            if (i + 1 >= rMap.GetLength(0))
                            {
                                return false;
                            }

                            if (rMap[i + 1, j] != null)
                            {
                                Room neighbour = rMap[i + 1, j];
                                (int nX, _) = neighbour.Dimensions;
                                int desiredDir = j - nX + 1;
                                bool compatibleExit = false;
                                foreach ((char nDir, int nNbDir) in neighbour.GetExits())
                                {
                                    if (nDir == 'B' && nNbDir == desiredDir)
                                    {
                                        compatibleExit = true;
                                        break;
                                    }
                                }

                                if (!compatibleExit)
                                {
                                    return false;
                                }
                            }
                        }

                        break;
                    
                    case 'L':
                        if (i - y + 1 == nbDir)
                        {
                            if (j - 1 < 0)
                            {
                                return false;
                            }

                            if (rMap[i, j-1] != null)
                            {
                                Room neighbour = rMap[i, j-1];
                                (_, int nY) = neighbour.Dimensions;
                                int desiredDir = i - nY + 1;
                                bool compatibleExit = false;
                                foreach ((char nDir, int nNbDir) in neighbour.GetExits())
                                {
                                    if (nDir == 'R' && nNbDir == desiredDir)
                                    {
                                        compatibleExit = true;
                                        break;
                                    }
                                }

                                if (!compatibleExit)
                                {
                                    return false;
                                }
                            }
                        }

                        break;
                    
                    case 'R':
                        if (i - y + 1 == nbDir)
                        {
                            if (j + 1 >= rMap.GetLength(1))
                            {
                                return false;
                            }

                            if (rMap[i, j + 1] != null)
                            {
                                Room neighbour = rMap[i, j + 1];
                                (_, int nY) = neighbour.Dimensions;
                                int desiredDir = i - nY + 1;
                                bool compatibleExit = false;
                                foreach ((char nDir, int nNbDir) in neighbour.GetExits())
                                {
                                    if (nDir == 'L' && nNbDir == desiredDir)
                                    {
                                        compatibleExit = true;
                                        break;
                                    }
                                }

                                if (!compatibleExit)
                                {
                                    return false;
                                }
                            }
                        }

                        break;
                }
            }

            return true;
        }

        private Dictionary<RoomType, List<Room>> GetLevels()
        {
            throw new NotImplementedException();
        }
    }
}
