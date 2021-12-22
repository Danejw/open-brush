using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyButtons;

namespace TiltBrush.Layers
{
    public class LayerUI_Manager : MonoBehaviour
    {
        [SerializeField] private SceneScript sceneScript;
        [SerializeField] private GameObject layerPrefab;

        private List<GameObject> layerObjects = new List<GameObject>();
        private Dictionary<GameObject, CanvasScript> layerMap = new Dictionary<GameObject, CanvasScript>();

        [SerializeField] private Vector3 startingPositionRelativeCenterOfParent;
        [SerializeField] private float ySpacing;

        private void Awake()
        {
            AddLayerButton.onAddLayer += CreateLayer;
            DeleteLayerButton.onDeleteLayer += RemoveLayer;
            FocusLayerButton.onFocusedLayer += SetActiveLayer;
            ToggleVisibilityLayerButton.onVisiblityToggle += ToggleVisibility;
        }

        private void Destroy()
        {
            AddLayerButton.onAddLayer -= CreateLayer;
            DeleteLayerButton.onDeleteLayer -= RemoveLayer;
            FocusLayerButton.onFocusedLayer -= SetActiveLayer;
            ToggleVisibilityLayerButton.onVisiblityToggle -= ToggleVisibility;
        }



        private void OnEnable()
        {
            sceneScript = FindObjectOfType<SceneScript>();
        }

        [EasyButtons.Button]
        public void ReOrderList()
        {
            foreach (GameObject layer in layerObjects)
            {
                layer.transform.position -= new Vector3(0, ySpacing, 0);
            }
        }

        [EasyButtons.Button]
        public void CreateLayer()
        {
            CanvasScript canvas = sceneScript.AddLayer();
            GameObject layer = Instantiate(layerPrefab, this.transform);

            layer.transform.localScale = new Vector3(1, 1, 1);
            layer.transform.position = this.transform.position + startingPositionRelativeCenterOfParent;
            layer.transform.eulerAngles = transform.eulerAngles;

            // put it into the dict
            layerMap.Add(layer, canvas);
            layerObjects.Add(layer);

            ReOrderList();  
        }

        [EasyButtons.Button]
        public void RemoveLayer(GameObject layer)
        {
            sceneScript.DeleteLayer(GetLayerCanvas(layer));

            // remove from the dict
            layerMap.Remove(layer);
            layerObjects.Remove(layer);

            Destroy(layer);
        }

        [EasyButtons.Button]
        public void ToggleVisibility(GameObject layer)
        {
            CanvasScript canvasScript = GetLayerCanvas(layer);
            if (canvasScript.gameObject.activeSelf) canvasScript.gameObject.SetActive(false);
            else canvasScript.gameObject.SetActive(true);
        }

        [EasyButtons.Button]
        public void SetActiveLayer(GameObject layer)
        {
            sceneScript.ActiveCanvas = GetLayerCanvas(layer);
        }



        private CanvasScript GetLayerCanvas(GameObject layer)
        {
            return layerMap[layer];
        }

    }
}
