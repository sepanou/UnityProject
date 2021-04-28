using UnityEditor;

public class Build {
    static void ServerBuild() {
        string[] defaultScene = {"Assets/Scenes/Hub.unity", "Assets/Scenes/Level1Creation.unity"};
        BuildPipeline.BuildPlayer(defaultScene, "./build/Linux.x86_64",
            BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
    }
}