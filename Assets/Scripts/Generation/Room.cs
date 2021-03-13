using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Generation
{
    public enum RoomType
    {
        Standard,
        Chest,
        Shop,
        Start,
        Exit,
        Boss,
        PreBoss,
        Other
    }

    public class Room : NetworkBehaviour
    {
        private bool _isDiscovered = false;
        private (int, int) _dimensions;
        private string _name;
        private RoomType _type;
        private List<(char, int)> _exits;
        private int _level;
        private int _id;
        private (int, int) _coordinate; //Bottom Left of the Room

        public void Start()
        {
            _exits = new List<(char, int)>();
            _name = "16x16eB1T2L4R2StdL1R1";
            string dimensions = "";
            for (int i = 0; i < _name.Length; i++)
            {
                if (_name[i] == 'e')
                {
                    break;
                }

                dimensions += _name[i];
            }

            string[] tmp = dimensions.Split('x');
            _dimensions = (int.Parse(tmp[0]), int.Parse(tmp[1]));
            string exits = "";
            char[] tmp2 = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'B', 'R', 'L'};
            for (int i = dimensions.Length + 1; i < _name.Length; i++)
            {
                if (!tmp2.Contains(_name[i]))
                {
                    break;
                }

                exits += _name[i];
            }

            char way = exits[0];
            string nbExit = "";
            for (int i = 1; i <= exits.Length; i++)
            {
                if (i == exits.Length)
                {
                    _exits.Add((way, int.Parse(nbExit)));
                    break;
                }

                if (!('0' <= exits[i] && exits[i] <= '9'))
                {
                    _exits.Add((way, int.Parse(nbExit)));
                    way = exits[i];
                    nbExit = "";
                }
                else
                {
                    nbExit += exits[i];
                }
            }

            string type = "";
            for (int i = dimensions.Length + exits.Length + 1; i < dimensions.Length + exits.Length + 4; i++)
            {
                type += _name[i];
            }

            switch (type)
            {
                case ("Std"):
                    _type = RoomType.Standard;
                    break;
                case ("Cst"):
                    _type = RoomType.Chest;
                    break;
                case "Shp":
                    _type = RoomType.Shop;
                    break;
                case "Stt":
                    _type = RoomType.Start;
                    break;
                case "Nxt":
                    _type = RoomType.Exit;
                    break;
                case "Fbo":
                    _type = RoomType.Boss;
                    break;
                case "Pfb":
                    _type = RoomType.PreBoss;
                    break;
                default:
                    _type = RoomType.Other;
                    break;
            }

            string levelAndId = "";
            for (int i = exits.Length + dimensions.Length + 4; i < _name.Length; i++)
            {
                levelAndId += _name[i];
            }

            string nb = "";
            for (int i = 1; i < levelAndId.Length; i++)
            {
                if (i == levelAndId.Length || !('0' <= levelAndId[i] && levelAndId[i] <= '9'))
                {
                    _level = int.Parse(nb);
                    nb = "";
                }
                else
                {
                    nb += levelAndId[i];
                }
            }

            _id = int.Parse(nb);
            _isDiscovered = false;
            Debug.Log(_name + '\n' + $"{_dimensions}" + '\n' + $"{_exits}" + "\n" + $"{_type}" + '\n' +
                              $"{_level}" + '\n' + $"{_id}");
        }

        public bool IsDiscovered()
        {
            return _isDiscovered;
        }

        public (int, int) GetDimensions()
        {
            return _dimensions;
        }

        public string GetName()
        {
            return _name;
        }

        public RoomType GetRoomType()
        {
            return _type;
        }
        
        public List<(char, int)> GetExits()
        {
            return _exits;
        }

        public int GetLevel()
        {
            return _level;
        }

        public int GetId()
        {
            return _id;
        }

        public (int, int) Dimensions
        {
            get => _dimensions;
            set => _dimensions = value;
        }
    }
}
