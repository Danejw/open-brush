using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

namespace TiltBrush.Layers
{
    public class AddLayerButton : BaseButton
    {
        public delegate void OnAddLayer();
        public static event OnAddLayer onAddLayer;

        public float delay = 5f;
        private bool isDown = false;

        public void FixedUpdate()
        {
            if (m_CurrentButtonState == ButtonState.Pressed && !isDown)
                StartCoroutine(DelayAfterClick());
        }

        private IEnumerator DelayAfterClick()
        {
            isDown = true;
            onAddLayer?.Invoke();
            yield return new WaitForSeconds(delay * Time.deltaTime * 10);
            isDown = false;
        }

    }
}
