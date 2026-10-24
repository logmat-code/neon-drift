using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeonDrift.Editor
{
    [InitializeOnLoad]
    public static class EditorAutoRunner
    {
        private const string FLAG_PATH = "Temp/RunBuildFlag.txt";

        static EditorAutoRunner()
        {
            EditorApplication.delayCall += CheckAndRun;
        }

        private static void CheckAndRun()
        {
            if (File.Exists(FLAG_PATH))
            {
                try
                {
                    File.Delete(FLAG_PATH);
                }
                catch {}

                Debug.Log("[NeonDrift AutoRunner] === Executing BuildAll inside Unity Editor ===");
                ProjectBuilder.BuildAll();
                
                Debug.Log("[NeonDrift AutoRunner] === Executing GameVerifier inside Unity Editor ===");
                GameVerifier.RunVerification();
            }
        }
    }
}
