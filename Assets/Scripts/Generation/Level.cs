using System.Collections.Generic;
using Mirror;

namespace Generation {
	public class Level: NetworkBehaviour {
		public Level(int levelId, string levelName) {
			LevelId = levelId;
			LevelName = levelName;
		}
		public int Chests { get; set; }
		public bool Shop { get; set; } = false;
		public int LevelId { get; }
		public string LevelName { get; }
		public Room[,] RoomsMap { get; } = new Room[20, 20];
		public List<Room> RoomsList { get; } = new List<Room>();
	}
}
