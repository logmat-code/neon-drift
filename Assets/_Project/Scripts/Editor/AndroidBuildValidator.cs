using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonDrift.Editor
{
    public static class AndroidBuildValidator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/NeonDistrictLoop.unity";
        private const string BUILD_OUT_DIR = "Builds/Android/";

        public static void ValidateAndroidBuild()
        {
            if (!Directory.Exists(BUILD_OUT_DIR)) Directory.CreateDirectory(BUILD_OUT_DIR);

            Debug.Log("[AndroidBuildValidator] Starting Android Build Verification (Target: Android Portrait)...");

            EditorUserBuildSettings.buildAppBundle = true; // Build AAB
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { SCENE_PATH };
            buildPlayerOptions.locationPathName = BUILD_OUT_DIR + "NEON_DRIFT_Release.aab";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            Debug.Log($"[AndroidBuildValidator] Result: {summary.result}, Total Time: {summary.totalTime.TotalSeconds:F1}s, Output: {summary.totalSize / (1024 * 1024)} MB");

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[AndroidBuildValidator] ANDROID BUILD SUCCEEDED! AAB Package ready for Play Console.");
            }
            else
            {
                Debug.LogError($"[AndroidBuildValidator] ANDROID BUILD FAILED with {summary.totalErrors} errors.");
            }
        }
    }
}
