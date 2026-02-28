using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace Unity6.LavosTrial.Build
{
    /// <summary>
    /// Unity6 Build Script - LavosTrial
    /// Handles multi-platform builds from command line
    /// </summary>
    public class BuildScript
    {
        private static string _outputName = "LavosTrial";

        public static void PerformBuild(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Debug.LogError("[BuildScript] No arguments provided. Usage: BuildScript.PerformBuild <Platform> [BuildPath] [OutputName]");
                return;
            }

            string platform = args[0];
            string buildPath = args.Length > 1 ? args[1] : "Build";
            _outputName = args.Length > 2 ? args[2] : "LavosTrial";

            Debug.Log($"[BuildScript] Starting build: Platform={platform}, Path={buildPath}, Name={_outputName}");

            switch (platform.ToLower())
            {
                case "windows":
                case "win":
                    BuildWindows(buildPath);
                    break;
                case "mac":
                case "macos":
                    BuildMac(buildPath);
                    break;
                case "linux":
                    BuildLinux(buildPath);
                    break;
                default:
                    Debug.LogError($"[BuildScript] Unknown platform: {platform}");
                    break;
            }
        }

        public static void PerformBuild()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            var platformArgs = new List<string>();
            string currentArg = "";

            foreach (string arg in args)
            {
                if (arg == "-executeMethod" || currentArg == "-executeMethod")
                {
                    currentArg = "";
                    continue;
                }
                if (arg.StartsWith("BuildScript.PerformBuild"))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(arg) && !arg.StartsWith("-"))
                {
                    platformArgs.Add(arg);
                }
            }

            if (platformArgs.Count > 0)
            {
                PerformBuild(platformArgs.ToArray());
            }
            else
            {
                Debug.LogWarning("[BuildScript] No platform specified, defaulting to Windows");
                BuildWindows("Build/Windows");
            }
        }

        private static void BuildWindows(string path)
        {
            string buildPath = Path.Combine(path, _outputName + ".exe");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Building Windows to: {buildPath}");
            UnityEditor.Build.Reporting.BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] Windows build SUCCEEDED: {report.summary.totalSize} bytes");
            }
            else
            {
                Debug.LogError($"[BuildScript] Windows build FAILED");
            }
        }

        private static void BuildMac(string path)
        {
            string buildPath = Path.Combine(path, _outputName + ".app");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Building macOS to: {buildPath}");
            UnityEditor.Build.Reporting.BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] macOS build SUCCEEDED: {report.summary.totalSize} bytes");
            }
            else
            {
                Debug.LogError($"[BuildScript] macOS build FAILED");
            }
        }

        private static void BuildLinux(string path)
        {
            string buildPath = Path.Combine(path, _outputName);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Building Linux to: {buildPath}");
            UnityEditor.Build.Reporting.BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] Linux build SUCCEEDED: {report.summary.totalSize} bytes");
            }
            else
            {
                Debug.LogError($"[BuildScript] Linux build FAILED");
            }
        }

        private static string[] GetScenePaths()
        {
            List<string> scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            if (scenes.Count == 0)
            {
                Debug.LogWarning("[BuildScript] No scenes enabled in Build Settings. Adding all scenes in 'Scenes' folder.");
                string[] allScenes = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
                foreach (string scene in allScenes)
                {
                    scenes.Add(scene.Replace(Application.dataPath, "Assets"));
                }
            }

            return scenes.ToArray();
        }
    }
}
