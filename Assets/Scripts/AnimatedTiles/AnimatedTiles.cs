using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AnimatedTiles {
	public class AnimatedTiles: Tile {
		public float animationSpeed, startTime;
		public Sprite[] animations;

		public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData) {
			tileAnimationData.animationStartTime = startTime;
			tileAnimationData.animationSpeed = animationSpeed;
			tileAnimationData.animatedSprites = animations;
			return true;
		}
	
#if UNITY_EDITOR
		[MenuItem("Assets/Create/AnimatedTile")]
		public static void CreateAnimatedTile() {
			string path = EditorUtility.SaveFilePanelInProject("Save Animated Tile", "animated", "Asset", "Save Animated Tile", "Assets");
			if (path.Length == 0) return;
			AssetDatabase.CreateAsset(CreateInstance<AnimatedTiles>(), path);
		}
#endif
	}
}
