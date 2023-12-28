using LIV.SDK.Unity;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using ValheimVRMod.LIVMod.Constants;
using Object = UnityEngine.Object;

namespace ValheimVRMod.LIVMod
{

    public class StandardLIVMod : MonoBehaviour
    {
        public static Action OnPlayerReady;
        private GameObject character = null;
        private GameObject livObject;
        private Camera spawnedCamera;
        private static LIV.SDK.Unity.LIV livInstance;

        private void OnEnable()
        {
            SetUpLiv();
            OnPlayerReady += TrySetupLiv;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SystemLibrary.LoadLibrary($@"{AppDomain.CurrentDomain.BaseDirectory}\LIVAssets\LIV_Bridge.dll");

        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            TrySetupLiv();
            ShiftPlayerCharacter();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log(">>> F3: TrySetupLiv");
                TrySetupLiv();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log(">>> F4: ShiftPlayerCharacter");
                ShiftPlayerCharacter();
            }
            //I'm sorry. I hate this.
            if (character == null)
                ShiftPlayerCharacter();
            UpdateFollowSpawnedCamera();
        }

        public bool IsLivActive()
        {
            return GetLivCamera() != null;
        }

        public void TrySetupLiv()
        {

            Camera[] arrCam = Object.FindObjectsOfType<Camera>().ToArray();
            //Some telemetry to be used during first setup to find the total number of cameras
            //Debug.Log("[LIV Mod]>>> Camera count: " + arrCam.Length);
            foreach (Camera cam in arrCam)
            {
                //LivDebug.LogCameraHierarchy(cam);
                if (cam.name.Contains("LIV "))
                {
                    continue;
                }
                else if (cam.name.Contains(AssetNames.PlayerHeadCamera))
                {
                    SetUpLiv(cam);
                    break;
                } //else Debug.Log("[LIV Telemetry](Target Cameras): " + cam.name);
                //Above is Some telemetry to be used during first setup to find camera names
            }
        }

        private void UpdateFollowSpawnedCamera()
        {
            var livRender = GetLivRender();
            if (livRender == null || spawnedCamera == null) return;

            // When spawned objects get removed in Boneworks, they might not be destroyed and just be disabled.
            if (!spawnedCamera.gameObject.activeInHierarchy)
            {
                spawnedCamera = null;
                return;
            }

            var cameraTransform = spawnedCamera.transform;
            livRender.SetPose(cameraTransform.position, cameraTransform.rotation, spawnedCamera.fieldOfView);
        }

        private static void SetUpLiv()
        {
            AssetManager assetManager = new AssetManager($@"{AppDomain.CurrentDomain.BaseDirectory}\LIVAssets\");
            try
            {
                //I'm not loading LIV late enough, as the LIV game object is destroyed and recreated (especially on scene load)
                //So we're wrapping this in a try/catch as the only negative effect is some red text in the log, and I don't like that.
                var livAssetBundle = assetManager.LoadBundle("liv-shaders-563");
                SDKShaders.LoadFromAssetBundle(livAssetBundle);
            }
            catch (Exception)
            {
            }
        }

        public static Camera GetLivCamera()
        {
            try
            {
                return !livInstance ? null : livInstance.HMDCamera;
            }
            catch (Exception)
            {
                livInstance = null;
            }
            return null;
        }

        private static SDKRender GetLivRender()
        {
            try
            {
                return !livInstance ? null : livInstance.render;
            }
            catch (Exception)
            {
                livInstance = null;
            }
            return null;
        }

        private void SetUpLiv(Camera camera)
        {
            if (!camera)
            {
                Debug.Log("No camera provided, aborting LIV setup.");
                return;
            }

            var livCamera = GetLivCamera();
            if (livCamera == camera)
            {
                Debug.Log("LIV already set up with this camera, aborting LIV setup.");
                return;
            }

            Debug.Log($"Setting up LIV with camera: {camera.name}...");
            if (livObject)
            {
                Object.Destroy(livObject);
            }

            Transform cameraParent;
            if (AssetNames.UseSpecificCameraParent)
            {
                cameraParent = GameObject.Find(AssetNames.CameraParent).transform;
            }
            else
            {
                cameraParent = camera.transform.parent;
            }
            var cameraPrefab = new GameObject("LivCameraPrefab");
            cameraPrefab.SetActive(false);
            var cameraFromPrefab = cameraPrefab.AddComponent<Camera>();
            cameraFromPrefab.allowHDR = false;
            cameraPrefab.transform.SetParent(cameraParent, false);


            cameraFromPrefab.cullingMatrix = Matrix4x4.Ortho(-99999, 99999, -99999, 99999, 0.001f, 99999) *
                    Matrix4x4.Translate(Vector3.forward * -99999 / 2f) *
                    cameraFromPrefab.worldToCameraMatrix;


            livObject = new GameObject("LIV");
            livObject.SetActive(false);

            livInstance = livObject.AddComponent<LIV.SDK.Unity.LIV>();
            livInstance.HMDCamera = camera;
            livInstance.MRCameraPrefab = cameraFromPrefab;
            livInstance.stage = cameraParent;
            livInstance.disableStandardAssets = true;
            livInstance.fixPostEffectsAlpha = true;

            livInstance.spectatorLayerMask = LivGameLayer.GetLayerMask();
            ShiftPlayerCharacter();
            //livInstance.spectatorLayerMask = ~0;
            //livInstance.spectatorLayerMask &= ~(1 << (int)LivGameLayer.PlayerBody);
            //livInstance.spectatorLayerMask &= ~(1 << (int)LivGameLayer.BuddyBot);
            //livInstance.spectatorLayerMask &= ~(1 << (int)LivGameLayer.RenderProxy);
            //Keeping hand because of one object in right hand couldn't be bothered changing layer

            livObject.SetActive(true);
        }

        public void ShiftPlayerCharacter()
        {
            var playerClone = GameObject.Find("Player(Clone)");
            if (playerClone == null) return;
            var visual = playerClone.transform.Find("Visual");
            if (visual == null) return;
            character = visual.transform.Find("body").gameObject;
            if (character == null) return;

            character.layer = (int)LivGameLayer.HideGameLayers.PlayerBody;
            Debug.Log(">>> F4: Shifted Player Character to Layer 7");
        }
    }
}