using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class ToggleVisibilityLayerButton : OptionButton
    {
        public delegate void OnVisiblityToggle(GameObject layerUi);
        public static event OnVisiblityToggle onVisiblityToggle;

        protected override void OnButtonPressed() => onVisiblityToggle?.Invoke(transform.parent.gameObject);
    }
}
