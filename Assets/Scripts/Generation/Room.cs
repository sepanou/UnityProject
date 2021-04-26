using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Generation {
	public enum RoomType { Standard, Chest, Shop, Start, Exit, Boss, PreBoss, DeadEnd, Other }

	public class Room: NetworkBehaviour {
		public bool IsDiscovered { get; private set; } = false;
		public (int, int) Dimensions;
		public string Name { get; private set; }
		public RoomType Type { get; private set; }
		private List<(char, int)> _exits;
		public IReadOnlyList<(char, int)> Exits => _exits.AsReadOnly();
		private int _level;
		private int _id;
		private (int, int) _coordinate; // Bottom
		
		public int GetLevel() => _level;
		public int GetId() => _id;
		public (int, int) Coordinates; // Left of the Room

		public void Start() { // Should be changed to generate on server side so when we get them form the game files we generate their specs
			_exits = new List<(char, int)>();
			Name = this.name;
			Debug.Log(Name);
			string dimensions = Name.TakeWhile(c => c != 'e').Aggregate("", (current, c) => current + c);
			string[] tmp = dimensions.Split('x');
			Dimensions = (int.Parse(tmp[0]), int.Parse(tmp[1]));
			string exits = "";
			char[] tmp2 = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'B', 'R', 'L',','};
			for (int i = dimensions.Length + 1; i < Name.Length; i++) {
				if (!tmp2.Contains(Name[i])) break;
				exits += Name[i];
			}

			char dir = '\0';
			foreach (char c in exits) {
				if (c == 'T' || c == 'B' || c == 'L' || c == 'R') {
					dir = c;
					continue;
				}
				if (c == ',') continue;
				//Is a number here
				_exits.Add((dir, c));
			}

			string type = "";
			for (int i = 1; i < 4; ++i)
				type += Name[dimensions.Length + exits.Length + i];

			Type = type == "Std" ? RoomType.Standard
				: type == "Cst" ? RoomType.Chest
				: type == "Shp" ? RoomType.Shop
				: type == "Stt" ? RoomType.Start
				: type == "Nxt" ? RoomType.Exit
				: type == "Fbo" ? RoomType.Boss
				: type == "Pfb" ? RoomType.PreBoss
				: type == "Den" ? RoomType.DeadEnd
				: RoomType.Other
			;

			string levelAndId = "";
			for (int i = exits.Length + dimensions.Length + 4; i < Name.Length; i++) {
				if (Name[i] == ' ' || Name[i] == '(') break;//When you place 2 times the same room you get "[Name] (x)"
				levelAndId += Name[i];
			}

			string nb = "";
			for (int i = 1; i < levelAndId.Length; i++) {
				if (i == levelAndId.Length || !('0' <= levelAndId[i] && levelAndId[i] <= '9')) {
					_level = int.Parse(nb);
					nb = "";
				} else nb += levelAndId[i];
			}

			_id = int.Parse(nb);
			IsDiscovered = false;
			Debug.Log(Name + '\n' + $"{Dimensions}" + '\n' + $"{_exits}" + "\n" + $"{Type}" + '\n' + $"{_level}" + '\n' + $"{_id}");
		}
	}
}
