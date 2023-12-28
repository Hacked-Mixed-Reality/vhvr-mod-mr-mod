using System;
using UnityEngine;

namespace ValheimVRMod.LIVMod.Constants

{
    public class LivGameLayer
    {
        public enum HideGameLayers
        {
            PlayerBody = 7,
        }

        private enum ReferenceGameLayers
        {
            Character = 9,
        }

        public static LayerMask GetLayerMask()
        {
            LayerMask layerMask = ~0;
            
            foreach (var item in Enum.GetValues(typeof(HideGameLayers)))
            {
                layerMask &= ~(1 << (int)item);
            }
            Debug.Log("[Mixed Reality Telemetry] Layer Mask: " + layerMask.value.ToString());
            return layerMask;
        }
    }
}