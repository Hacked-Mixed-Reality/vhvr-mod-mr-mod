using BepInEx;
using UnityEngine;
using ValheimVRMod.VRCore;
using ValheimVRMod.VRCore.UI;
using ValheimVRMod.Utilities;
using ValheimVRMod.Scripts;
using ValheimVRMod.Patches;

using static ValheimVRMod.Utilities.LogUtils;
using UnityEngine.SceneManagement;

namespace ValheimVRMod
{
    [BepInPlugin("org.bepinex.plugins.valheimvrmod", "ValheimVR Mod", "0.9.7")]
    public class ValheimVRMod : BaseUnityPlugin
    {

        public static System.Version PLUGIN_VERSION { get { return _version; } }
        private static System.Version _version = null;

        private GameObject vrPlayer;
        private GameObject vrGui;
        private GameObject BhapticsTactsuit;
        private LIVMod.StandardLIVMod livMod = null;

        void Awake() {
            _version = Info.Metadata.Version;
            VHVRConfig.InitializeConfiguration(Config);
            if (!VHVRConfig.ModEnabled())
            {
                LogInfo("ValheimVRMod is disabled via configuration.");
                enabled = false;
                return;
            }
            Game.isModded = true;
            LogInfo("ValheimVR Mod Awakens!");
#if NONVRMODE
            LogInfo("Running non-VR mode companion mod!");
#endif
        }

        void Start()
        {
            StartValheimVR();
            StartLiv();
        }
        public void StartLiv()
        {
            if (livMod == null)
            {
                Debug.Log("[Liv Mod] Attempting to start LIV mod.");
                GameObject livModParent = new GameObject();
                livMod = livModParent.AddComponent<LIVMod.StandardLIVMod>();
            }
            else
            {
                Debug.Log("[Liv Mod] Attempting to restart LIV mod.");
                livMod.TrySetupLiv();
            }

        }

        

        void Update()
        {
            if (VHVRConfig.NonVrPlayer()) {
                return;
            }

            if (Input.GetKeyDown(VHVRConfig.GetRecenterKey()))
            {
                VRManager.tryRecenter();
            }

            if (Time.frameCount % 200 == 0 && (livMod == null || !livMod.IsLivActive()))
                StartLiv();
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
              //  dumpall();
                
                LIVMod.LivDebug.LogAllCameraHierarchies();
                Debug.Log("#################################");
                Camera[] cameras = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID);
                foreach (Camera cam in cameras)
                {
                    if(cam.name== "LIV Camera")
                    {
                        cam.useOcclusionCulling = false;
                        
                        cam.cullingMatrix = new Matrix4x4(
                            new Vector4(1, 1, 1, 1),
                            new Vector4(1, 1, 1, 1),
                            new Vector4(1, 1, 1, 1),
                            new Vector4(1, 1, 1, 1)
                        );
                        //*/
                        CameraUtils.PrintCamera(cam);
                        break;
                    }
                    Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                }
            }
#endif
        }

        void StartValheimVR()
        {
            HarmonyPatcher.DoPatching();

            if (VHVRConfig.NonVrPlayer()) {
                LogDebug("Non VR Mode Patching Complete.");
                return;
            }

            if (VRManager.InitializeVR())
            {
                VRManager.StartVR();
                vrPlayer = new GameObject("VRPlayer");
                DontDestroyOnLoad(vrPlayer);
                vrPlayer.AddComponent<VRPlayer>();
                vrGui = new GameObject("VRGui");
                DontDestroyOnLoad(vrGui);
                vrGui.AddComponent<VRGUI>();
                if (VHVRConfig.RecenterOnStart())
                {
                    VRManager.tryRecenter();
                }
                if (VHVRConfig.BhapticsEnabled())
                {
                    BhapticsTactsuit = new GameObject("BhapticsTactsuit");
                    DontDestroyOnLoad(BhapticsTactsuit);
                    BhapticsTactsuit.AddComponent<BhapticsTactsuit>();
                }
            }
            else
            {
                LogError("Could not initialize VR.");
                enabled = false;
            }
        }

#if DEBUG
        void dumpall()
        {
            foreach (var o in GameObject.FindObjectsOfType<GameObject>())
            {
                LogDebug("Name + " + o.name + "   Layer = " + o.layer);
            }
        }
#endif
    }

}
