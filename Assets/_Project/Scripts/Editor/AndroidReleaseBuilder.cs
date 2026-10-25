using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonDrift.Editor
{
    public static class AndroidReleaseBuilder
    {
        public static void ConfigureAndroidSettings()
        {
            Debug.Log("[AndroidReleaseBuilder] Configuring Player Settings for Android Release...");

            // Product name with exact trailing dot
            PlayerSettings.productName = "NEON DRIFT.";
            PlayerSettings.companyName = "Logmat";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.logmat.neondrift");

            // Strictly Portrait Only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Target architectures (ARM64)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Configure Gradle template flags
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = false; // Dev APK first

            // Link internal Unity Android SDK/NDK/JDK if not set
            string unityBasePath = EditorApplication.applicationContentsPath; // /Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents
            string hubEditorPath = Path.GetFullPath(Path.Combine(unityBasePath, "../../PlaybackEngines/AndroidPlayer"));
            
            string sdkPath = Path.Combine(hubEditorPath, "SDK");
            string ndkPath = Path.Combine(hubEditorPath, "NDK");
            string jdkPath = Path.Combine(hubEditorPath, "OpenJDK");

            if (Directory.Exists(sdkPath))
            {
                EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
                Debug.Log($"[AndroidReleaseBuilder] Set AndroidSdkRoot: {sdkPath}");
            }
            if (Directory.Exists(ndkPath))
            {
                EditorPrefs.SetString("AndroidNdkRootR23b", ndkPath);
                EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
                Debug.Log($"[AndroidReleaseBuilder] Set AndroidNdkRoot: {ndkPath}");
            }
            if (Directory.Exists(jdkPath))
            {
                EditorPrefs.SetString("JdkPath", jdkPath);
                Debug.Log($"[AndroidReleaseBuilder] Set JdkPath: {jdkPath}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[AndroidReleaseBuilder] Android Settings Configured.");
        }

        public static void BuildDevApk()
        {
            ConfigureAndroidSettings();

            string buildDir = Path.Combine(Application.dataPath, "../Builds/Android");
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string apkPath = Path.Combine(buildDir, "NeonDrift_Dev.apk");
            Debug.Log($"[AndroidReleaseBuilder] Starting Development APK Build to: {apkPath}");

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/NeonDistrictLoop.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidReleaseBuilder] SUCCESS! APK generated at: {apkPath} (Size: {summary.totalSize / (1024 * 1024)} MB)");
            }
            else
            {
                Debug.LogError($"[AndroidReleaseBuilder] Build FAILED with {summary.totalErrors} errors. Result: {summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[BuildError] {msg.content}");
                        }
                    }
                }
            }
        }
    }
}
