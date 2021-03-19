using System.Collections.Generic;
using Mirror;

namespace Generation {
	public class Level: NetworkBehaviour {
		public int Chests { get; set; }
		public bool Shop { get; set; } = false;
		public int LevelId { get; private set; }
		public string LevelName { get; private set; }
		public Room[,] RoomsMap { get; private set; } = new Room[20,20];
		public List<Room> RoomsList { get; private set; } = new List<Room>();
	}
}
