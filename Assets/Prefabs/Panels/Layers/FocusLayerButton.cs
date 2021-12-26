using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class FocusLayerButton : OptionButton
    {
        public delegate void OnFocusedLayer(GameObject layerUi);
        public static event OnFocusedLayer onFocusedLayer;

        bool canPress = true;

        protected override void OnButtonPressed()
        {
            if (canPress)
                StartCoroutine(debounce());
        }

        private IEnumerator debounce()
        {
            canPress = false;
            onFocusedLayer?.Invoke(transform.parent.gameObject);
            yield return new WaitForEndOfFrame();
            canPress = true;
        }
    }
}
