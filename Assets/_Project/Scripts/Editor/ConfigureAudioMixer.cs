
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.Reflection;
using System;
using System.Collections.Generic;
using NeonDrift.Presentation;
using UnityEditor.SceneManagement;

namespace NeonDrift.Editor
{
    public static class ConfigureAudioMixer
    {
        public static void Configure()
        {
            string path = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer == null)
            {
                Debug.LogError("[ConfigureAudioMixer] Could not load mixer at " + path);
                return;
            }

            var controllerType = typeof(AudioImporter).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            var groupType = typeof(AudioImporter).Assembly.GetType("UnityEditor.Audio.AudioMixerGroupController");
            
            // Get masterGroup
            var masterProp = controllerType.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var masterGroup = masterProp.GetValue(mixer);

            string[] groupNames = new string[] { "Music", "Engine", "SFX", "Ambience", "UI" };

            // Method to create new group: CreateNewGroup(string name, bool subGroup)
            var createNewGroupMethod = controllerType.GetMethod("CreateNewGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var AddChildToGroupMethod = controllerType.GetMethod("AddChildToGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Or inspect groups
            var groupsProp = mixer.GetType().GetProperty("groups", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var groups = groupsProp != null ? (Array)groupsProp.GetValue(mixer) : null;
            
            Dictionary<string, AudioMixerGroup> createdGroups = new Dictionary<string, AudioMixerGroup>();
            var allGroups = mixer.FindMatchingGroups(string.Empty);
            foreach (var g in allGroups)
            {
                createdGroups[g.name] = g;
            }

            foreach (var gName in groupNames)
            {
                if (!createdGroups.ContainsKey(gName))
                {
                    try
                    {
                        if (createNewGroupMethod != null)
                        {
                            var newGroup = createNewGroupMethod.Invoke(mixer, new object[] { gName, false });
                            if (newGroup != null && AddChildToGroupMethod != null)
                            {
                                AddChildToGroupMethod.Invoke(mixer, new object[] { masterGroup, newGroup });
                            }
                            Debug.Log("[ConfigureAudioMixer] Created group: " + gName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[ConfigureAudioMixer] Error adding group " + gName + ": " + ex.Message);
                    }
                }
            }

            AssetDatabase.SaveAssets();

            // Wire to AudioManager in scene
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am != null)
            {
                var serializedAm = new SerializedObject(am);
                var masterMixerProp = serializedAm.FindProperty("masterMixer");
                if (masterMixerProp != null)
                {
                    masterMixerProp.objectReferenceValue = mixer;
                    serializedAm.ApplyModifiedProperties();
                    Debug.Log("[ConfigureAudioMixer] Assigned masterMixer to AudioManager.");
                }
            }
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ConfigureAudioMixer] Saved scene with mixer.");
        }
    }
}
