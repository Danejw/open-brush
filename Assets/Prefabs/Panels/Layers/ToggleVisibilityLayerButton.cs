using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class ToggleVisibilityLayerButton : OptionButton
    {
        public delegate void OnVisiblityToggle(GameObject layerUi);
        public static event OnVisiblityToggle onVisiblityToggle;

        public float delay = 5f;
        private bool isDown = false;

        private void FixedUpdate()
        {
            if (m_CurrentButtonState == ButtonState.Pressed && !isDown)
                StartCoroutine(DelayAfterClick());
        }

        private IEnumerator DelayAfterClick()
        {
            isDown = true;
            onVisiblityToggle?.Invoke(transform.parent.gameObject);
            yield return new WaitForSeconds(delay * Time.deltaTime * 10);
            isDown = false;
        }
    }
}
