using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TiltBrush.Layers
{
    public class DeleteLayerButton : BaseButton
    {
        public delegate void OnDeleteLayer(GameObject layerUi);
        public static event OnDeleteLayer onDeleteLayer;

        public float delay = .05f;
        private bool isDown = false;

        private void Update()
        {
            if (m_CurrentButtonState == ButtonState.Pressed && !isDown)
                StartCoroutine(DelayAfterClick());
        }

        private IEnumerator DelayAfterClick()
        {
            isDown = true;
            onDeleteLayer?.Invoke(transform.parent.gameObject);           
            yield return new WaitForSeconds(delay * Time.deltaTime * 10);
            isDown = false;
        }

    }
}
