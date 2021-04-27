using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Generation {
	public class Level: NetworkBehaviour {
		public int Chests { get; set; }
		public bool Shop { get; set; } = false;
		[SerializeField] public int LevelId;
		[SerializeField] public string lvlName;
		public Room[,] RoomsMap { get; } = new Room[20, 20];
		public List<Room> RoomsList { get; } = new List<Room>();
		public bool alreadyGenerated = false;

		private void Start() {
			Generation.AddPrefab(100, 0, "16x16eB1NxtL1R1");
		}
	}
}
