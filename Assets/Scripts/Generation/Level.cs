﻿using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Generation {
	public class Level: NetworkBehaviour {
		public int Chests { get; set; }
		public bool Shop { get; set; } = false;
		[FormerlySerializedAs("LevelId")] [SerializeField] public int levelId;
		[SerializeField] public string lvlName;
		public Room[,] RoomsMap { get; } = new Room[101, 101];
		public List<Room> RoomsList { get; } = new List<Room>();
		public bool alreadyGenerated;

		public override void OnStartServer() {
			Generation.GenerateLevel(this);
		}
	}
}
