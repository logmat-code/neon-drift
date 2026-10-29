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

            // For development APK, use standard debug keystore to prevent missing keystore errors
            PlayerSettings.Android.useCustomKeystore = false;

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
        public static void BuildSignedReleaseAab()
        {
            ConfigureAndroidSettings();

            // Configure Icon
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Icons/AppIcon_512.png");
            if (icon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { icon });
                Debug.Log("[AndroidReleaseBuilder] Assigned 512x512 Master AppIcon to Android TargetGroup.");
            }

            // Keystore Configuration
            string credPath = "/Users/logmat/.gemini/antigravity/brain/keystore_secure/keystore_credentials.txt";
            if (File.Exists(credPath))
            {
                string[] lines = File.ReadAllLines(credPath);
                string ksPath = "";
                string alias = "";
                string pass = "";
                foreach (var line in lines)
                {
                    if (line.StartsWith("KEYSTORE_PATH=")) ksPath = line.Substring("KEYSTORE_PATH=".Length);
                    if (line.StartsWith("KEY_ALIAS=")) alias = line.Substring("KEY_ALIAS=".Length);
                    if (line.StartsWith("KEYSTORE_PASSWORD=")) pass = line.Substring("KEYSTORE_PASSWORD=".Length);
                }

                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = ksPath;
                PlayerSettings.Android.keystorePass = pass;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = pass;
                Debug.Log($"[AndroidReleaseBuilder] Configured Custom Keystore: {ksPath} with Alias: {alias}");
            }
            else
            {
                Debug.LogWarning("[AndroidReleaseBuilder] Keystore credentials file not found at " + credPath);
            }

            // Production settings
            PlayerSettings.Android.bundleVersionCode = 2;
            PlayerSettings.bundleVersion = "1.0.1";
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;

            string releaseDir = Path.Combine(Application.dataPath, "../Builds/Android/Release");
            if (!Directory.Exists(releaseDir)) Directory.CreateDirectory(releaseDir);

            string aabPath = Path.Combine(releaseDir, "NeonDrift_Release_v1.aab");
            Debug.Log($"[AndroidReleaseBuilder] Starting Signed Release AAB Build to: {aabPath}");

            string[] scenes = new string[] { "Assets/_Project/Scenes/NeonDistrictLoop.unity" };
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = aabPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidReleaseBuilder] SUCCESS! Signed AAB generated at: {aabPath} (Size: {summary.totalSize / (1024 * 1024)} MB)");
            }
            else
            {
                Debug.LogError($"[AndroidReleaseBuilder] AAB Build FAILED with {summary.totalErrors} errors. Result: {summary.result}");
            }
        }
    }
}
