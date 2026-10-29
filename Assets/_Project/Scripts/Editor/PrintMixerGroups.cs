
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonDrift.Editor
{
    public static class PrintMixerGroups
    {
        public static void Print()
        {
            string mixerPath = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer == null)
            {
                Debug.LogError("[PRINT_GROUPS] Mixer not found!");
                return;
            }
            var all = mixer.FindMatchingGroups(string.Empty);
            Debug.Log($"[PRINT_GROUPS] Total matching groups for empty string: {all.Length}");
            foreach (var g in all)
            {
                Debug.Log($"[PRINT_GROUPS] Group: '{g.name}'");
            }
        }
    }
}
