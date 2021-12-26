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

        // the input from the controller invokes onAddLayer message twice!!! >:(
        protected override void OnButtonPressed()
        {
            onAddLayer?.Invoke();
            Debug.Log("AddLayerButtonPressed");
        }
    } 
}
