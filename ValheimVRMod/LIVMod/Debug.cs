using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimVRMod.LIVMod
{
    internal class LivDebug
    {
        public static void LogCameraHierarchy(Camera camera)
        {
            if(camera == null)
            {
                throw new ArgumentNullException();
            }
            Debug.Log("------------------------------------");
            Debug.Log("Camera at:" +  camera.transform.position.ToString());
            Debug.Log("------------------------------------");
            Debug.Log("Get Camera Hierarchy for " + camera.name + ":");
            Debug.Log(camera.name);
            GameObject obj = camera.transform.parent?.gameObject;
            while (obj != null)
            {
                Debug.Log(obj.name + "--" + obj.transform.position.ToString());
                obj = obj.transform.parent?.gameObject;
            }
            Debug.Log("End Camera Hierarchy:");
        }

        public static void LogAllCameraHierarchies()
        {
            Camera[] cameras = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID);
            foreach (Camera cam in cameras) 
            {
                LogCameraHierarchy(cam);
            }
        }
    }
}
