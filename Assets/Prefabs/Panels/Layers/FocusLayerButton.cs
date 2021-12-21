using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class FocusLayerButton : OptionButton
    {
        public delegate void OnFocusedLayer(GameObject layerUi);
        public static event OnFocusedLayer onFocusedLayer;

        public float delay = 5f;
        private bool isDown = false;

        private void Update()
        {
                if (m_CurrentButtonState == ButtonState.Pressed)
                    if (!isDown)
                        StartCoroutine(DelayAfterClick());
        }

        private IEnumerator DelayAfterClick()
        {
            isDown = true;
            onFocusedLayer?.Invoke(transform.parent.gameObject);
            yield return new WaitForSeconds(delay * Time.deltaTime * 10);
            isDown = false;
        }

    }
}
