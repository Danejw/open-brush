using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class FocusLayerButton : BaseButton
    {
        public bool debug = true;

        public delegate void OnFocusedLayer(GameObject layerUi);
        public static event OnFocusedLayer onFocusedLayer;

        protected override void OnButtonPressed() => onFocusedLayer?.Invoke(transform.parent.gameObject);
    }
}
