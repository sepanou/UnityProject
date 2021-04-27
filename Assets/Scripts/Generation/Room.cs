using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation {
	public enum RoomType { Standard, Chest, Shop, Start, Exit, Boss, PreBoss, DeadEnd, Other }

	public class Room {
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

		private void Start() {
			IsDiscovered = false;
		}

		public Room(((int, int) dim, List<(char, int)> exits, RoomType type, int lvl, int id) p, string name) {
			Dimensions = p.dim;
			_exits = p.exits;
			Type = p.type;
			_level = p.lvl;
			_id = p.id;
			Name = name;
		}

		public static ((int, int), List<(char, int)>, RoomType, int, int) Generate(string name) {
			List<(char, int)> exits = new List<(char, int)>();
			Debug.Log(name);
			string dimensions = name.TakeWhile(c => c != 'e').Aggregate("", (current, c) => current + c);
			string[] tmp = dimensions.Split('x');
			(int, int) dim = (int.Parse(tmp[0]), int.Parse(tmp[1]));
			string exitsStr = "";
			char[] tmp2 = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'B', 'R', 'L',','};
			for (int i = dimensions.Length + 1; i < name.Length; i++) {
				if (!tmp2.Contains(name[i])) break;
				exitsStr += name[i];
			}

			char dir = '\0';
			foreach (char c in exitsStr) {
				if (c == 'T' || c == 'B' || c == 'L' || c == 'R') {
					dir = c;
					continue;
				}
				if (c == ',') continue;
				//Is a number here
				exits.Add((dir, c));
			}

			string typeStr = "";
			for (int i = 1; i < 4; ++i)
				typeStr += name[dimensions.Length + exitsStr.Length + i];

			RoomType type = typeStr == "Std" ? RoomType.Standard
				: typeStr == "Cst" ? RoomType.Chest
				: typeStr == "Shp" ? RoomType.Shop
				: typeStr == "Stt" ? RoomType.Start
				: typeStr == "Nxt" ? RoomType.Exit
				: typeStr == "Fbo" ? RoomType.Boss
				: typeStr == "Pfb" ? RoomType.PreBoss
				: typeStr == "Den" ? RoomType.DeadEnd
				: RoomType.Other
			;

			string levelAndId = "";
			for (int i = exitsStr.Length + dimensions.Length + 4; i < name.Length; i++) {
				if (name[i] == ' ' || name[i] == '(') break;//When you place 2 times the same room you get "[Name] (x)"
				levelAndId += name[i];
			}

			int level = 0;
			string nb = "";
			for (int i = 1; i < levelAndId.Length; i++) {
				if (i == levelAndId.Length || !('0' <= levelAndId[i] && levelAndId[i] <= '9')) {
					level = int.Parse(nb);
					nb = "";
				} else nb += levelAndId[i];
			}

			int id = int.Parse(nb);
			return (dim, exits, type, level, id);
			//Debug.Log(Name + '\n' + $"{Dimensions}" + '\n' + $"{_exits}" + "\n" + $"{Type}" + '\n' + $"{_level}" + '\n' + $"{_id}");
		}
	}
}
