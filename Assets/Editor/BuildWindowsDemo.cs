using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildWindowsDemo
{
    private const string BuildDirectory = "Builds/Windows";
    private const string BuildPath = BuildDirectory + "/Metroidvania.exe";

    [MenuItem("Tools/Build/Windows Demo")]
    public static void BuildWindows()
    {
        Directory.CreateDirectory(BuildDirectory);

        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
            scenes = new[] { "Assets/Scenes/SampleScene.unity" };

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReportSummary(BuildPipeline.BuildPlayer(options));
    }

    private static string[] GetEnabledScenes()
    {
        return System.Array.ConvertAll(
            System.Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
            scene => scene.path);
    }

    private static void BuildReportSummary(UnityEditor.Build.Reporting.BuildReport report)
    {
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log("Windows demo build succeeded: " + BuildPath);
        else
            Debug.LogError("Windows demo build failed: " + report.summary.result);
    }
}
