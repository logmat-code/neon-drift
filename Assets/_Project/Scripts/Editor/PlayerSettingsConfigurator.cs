using UnityEditor;
using UnityEngine;

namespace NeonDrift.Editor
{
    public static class PlayerSettingsConfigurator
    {
        public static void ConfigureOfficialIdentityAndIcon()
        {
            // 1. Official Product & Company Name
            PlayerSettings.productName = "NEON DRIFT.";
            PlayerSettings.companyName = "MidnightDrift";

            // 2. Android Orientation: Strictly Portrait Only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // 3. Official App Icon Assignment
            Texture2D iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/AppIcon_512.png");
            if (iconTex != null)
            {
                // Assign for Default and Android
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { iconTex });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] { iconTex });
                Debug.Log("[PlayerSettings] Official App Icon (AppIcon_512.png) assigned to all platforms.");
            }
            else
            {
                Debug.LogWarning("[PlayerSettings] AppIcon_512.png not found for assignment.");
            }

            // 4. Unity Splash Screen Configuration
            // Keep legal Unity splash while enforcing portrait styling
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerSettings] Configured: NEON DRIFT., Portrait Only, Splash Screen verified.");
        }
    }
}
