using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;

namespace NeonDrift.Editor
{
    public static class AudioMixerCreator
    {
        public static void CreateOrConfigureMixer()
        {
            string path = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer == null)
            {
                Debug.Log("[AudioMixerCreator] Loading or creating AudioMixer asset...");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioMixerCreator] Finished AudioMixer check.");
        }
    }
}
