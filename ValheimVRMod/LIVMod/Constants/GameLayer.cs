using System;
using UnityEngine;

namespace ValheimVRMod.LIVMod.Constants

{
    public class LivGameLayer
    {
        private enum HideGameLayers
        {
            PlayerBody = 9,
        }

        private enum ReferenceGameLayers
        {
            PlayerBody = 9,
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