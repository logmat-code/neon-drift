using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;
using NeonDrift.Presentation;
using NeonDrift.Progression;
using NeonDrift.Tutorial;

namespace NeonDrift.Editor
{
    public static class CaptureAuditScreenshots
    {
        public static void CaptureAllViews()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);
            if (pm.CurrentData == null) pm.LoadProfile();

            string dir = "/Users/logmat/Desktop/CLAUDE/NeonDrift/Assets/_Project/Art/LiveCaptures/";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            int w = 1080;
            int h = 1920;

            Camera cam = Camera.main;
            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(w, h, TextureFormat.RGB24, false);

            // 1. CAPTURE HOME
            gm.EnterState(GameState.Home);
            RenderAndSave(cam, rt, screenShot, dir + "Audit_01_Home.png");

            // 2. CAPTURE GARAGE (BUY BUTTON VISIBLE & INTERACTABLE)
            gm.EnterState(GameState.Garage);
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            gv.SelectNextCar(); // Show Akuma 86
            RenderAndSave(cam, rt, screenShot, dir + "Audit_02_Garage_Shop_BuyReady.png");

            // 3. CAPTURE FIRST DRIVE TUTORIAL
            gm.StartRun();
            var tutMgr = UnityEngine.Object.FindAnyObjectByType<TutorialManager>(FindObjectsInactive.Include);
            var tutView = UnityEngine.Object.FindAnyObjectByType<TutorialView>(FindObjectsInactive.Include);
            tutMgr.StartTutorial();
            tutView.DisplayStep(TutorialManager.TutorialStep.Steering);
            RenderAndSave(cam, rt, screenShot, dir + "Audit_03_FirstDrive_Tutorial.png");

            // 4. CAPTURE GAMEPLAY WITH VISIBLE TRAFFIC & OBSTACLES
            tutView.gameObject.SetActive(false);
            tutMgr.currentStep = TutorialManager.TutorialStep.Completed;
            // Let traffic move and spawn
            for (int i = 0; i < 50; i++)
            {
                player.transform.position += new Vector3(0, 0.8f, 0);
                track.ProcessUpdate(0.04f);
                tm.UpdateTraffic(0.04f);
            }
            cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 14f, -10f);
            RenderAndSave(cam, rt, screenShot, dir + "Audit_04_Gameplay_Traffic_Obstacles.png");

            // 5. CAPTURE GAME OVER / RESULTS
            gm.EndRun("VEHICLE DESTROYED");
            RenderAndSave(cam, rt, screenShot, dir + "Audit_05_Results_GameOver.png");

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(screenShot);

            Debug.Log("[SCREENSHOTS] All 5 Audit Screenshots captured successfully!");
        }

        private static void RenderAndSave(Camera cam, RenderTexture rt, Texture2D tex, string path)
        {
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved capture to: {path}");
        }
    }
}
