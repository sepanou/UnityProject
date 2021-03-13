using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generation;
using Mirror;

namespace Generation
{
    public class Level : NetworkBehaviour
    {
        private int _chests;
        private int _levelId;
        private string _levelName;
        private Room[,] _roomsMap;
        private List<Room> _roomsList;
        private char[,] _charMap;
        
        
        // Start is called before the first frame update
        void Start()
        {
            _roomsMap = new Room[20,20];
            _roomsList = new List<Room>();
            _charMap = new char[20,20];
        }

        // Update is called once per frame
        void Update()
        {

        }

        public int GetChest()
        {
            return _chests;
        }

        public int GetId()
        {
            return _levelId;
        }

        public string GetName()
        {
            return _levelName;
        }

        public Room[,] RoomsMap
        {
            get => _roomsMap;
            set => _roomsMap = value;
        }

        public List<Room> RoomsList
        {
            get => _roomsList;
            set => _roomsList = value;
        }

        public char[,] CharMap
        {
            get => _charMap;
            set => _charMap = value;
        }
    }
}
