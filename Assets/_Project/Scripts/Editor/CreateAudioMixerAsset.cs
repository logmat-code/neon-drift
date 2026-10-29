
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.Reflection;

namespace NeonDrift.Editor
{
    public static class CreateAudioMixerAsset
    {
        public static void Create()
        {
            string path = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (existing != null)
            {
                Debug.Log("[CreateAudioMixerAsset] Mixer already exists at " + path);
                return;
            }

            // In Unity Editor, AudioMixer can be created via ProjectWindowUtil.CreateAssetWithContent or internal AudioMixerController
            var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            if (audioUtil != null)
            {
                var createMethod = audioUtil.GetMethod("CreateMixerControllerAtPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (createMethod != null)
                {
                    var mixer = createMethod.Invoke(null, new object[] { path });
                    Debug.Log("[CreateAudioMixerAsset] Created AudioMixerController via reflection: " + mixer);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
            Debug.LogWarning("[CreateAudioMixerAsset] Could not find AudioMixerController.CreateMixerControllerAtPath");
        }
    }
}
