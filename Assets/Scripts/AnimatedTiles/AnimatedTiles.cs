using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimatedTiles : Tile
{
    public float animationSpeed, startTime;
    public Sprite[] animations;

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        tileAnimationData.animationStartTime = startTime;
        tileAnimationData.animationSpeed = animationSpeed;
        tileAnimationData.animatedSprites = animations;
        return true;
    }
    
#if UNITY_EDITOR
    [MenuItem("Assets/Create/AnimatedTile")]
    public static void CreateAnimatedTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Animated Tile", "animated", "Asset", "Save Animated Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AnimatedTiles>(), path);
    }
#endif
}
