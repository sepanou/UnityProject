using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Generation {
	public class Level: NetworkBehaviour {
		public int Chests { get; set; }
		public bool Shop { get; set; }
		[FormerlySerializedAs("LevelId")] [SerializeField] public int levelId;
		[SerializeField] public string lvlName;
		public Room[,] RoomsMap { get; } = new Room[101, 101];
		public List<Room> RoomsList { get; } = new List<Room>();
		
		[SyncVar(hook = nameof(SyncAlreadyGeneratedChanged))] public bool alreadyGenerated;
		private void SyncAlreadyGeneratedChanged(bool o, bool n) {
			if (n) CustomNetworkManager.Instance.PlaySceneTransitionAnimation("EndTransition");
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteBoolean(alreadyGenerated);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			if (reader.ReadBoolean() == alreadyGenerated) return;
			SyncAlreadyGeneratedChanged(alreadyGenerated, !alreadyGenerated);
			alreadyGenerated = !alreadyGenerated;
		}

		public void Start() {
			if (!isServer) return;
			Generation.GenerateLevel(this);
		}
	}
}
