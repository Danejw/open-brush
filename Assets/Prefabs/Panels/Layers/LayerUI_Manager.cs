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

        [SerializeField] private bool createLayersOnStart = true;
        [SerializeField] private int howManyLayers = 4;

        private void Awake()
        {
            AddLayerButton.onAddLayer += CreateLayer;
            ClearLayerButton.onClearLayer += ClearLayer;
            DeleteLayerButton.onDeleteLayer += RemoveLayer;
            FocusLayerButton.onFocusedLayer += SetActiveLayer;
            ToggleVisibilityLayerButton.onVisiblityToggle += ToggleVisibility;
        }

        private void Destroy()
        {
            AddLayerButton.onAddLayer -= CreateLayer;
            ClearLayerButton.onClearLayer -= ClearLayer;
            DeleteLayerButton.onDeleteLayer -= RemoveLayer;
            FocusLayerButton.onFocusedLayer -= SetActiveLayer;
            ToggleVisibilityLayerButton.onVisiblityToggle -= ToggleVisibility;
        }

        private void Start()
        {
            sceneScript = FindObjectOfType<SceneScript>();

            // create main canvas layer ui
            if (sceneScript)
            {
                // pair maincanvas to layer prefab
                CanvasScript mainCanvas = sceneScript.MainCanvas;
                GameObject mainLayer = Instantiate(layerPrefab, this.transform);

                Debug.Log("Creating Layer " + mainLayer.name);

                // put it into the dict
                layerMap.Add(mainLayer, mainCanvas);
                layerObjects.Add(mainLayer);

                // set the name in the ui
                mainLayer.GetComponentInChildren<TMPro.TMP_Text>().text = GetLayerCanvas(mainLayer).name;
            }

            // create layer on start
            if (createLayersOnStart)
                for (int i = 0; i < howManyLayers; i++)
                    CreateLayer();            
        }

        private void OnEnable()
        {
            sceneScript = FindObjectOfType<SceneScript>();          
        }


        [EasyButtons.Button]
        public void CreateLayer()
        {
            CanvasScript canvas = sceneScript.AddLayer();
            GameObject layer = Instantiate(layerPrefab, this.transform);

            Debug.Log("Creating Layer " + layer.name);

            // put it into the dict
            layerMap.Add(layer, canvas);
            layerObjects.Add(layer);

            // set the layer name on the ui
            layer.GetComponentInChildren<TMPro.TMP_Text>().text = GetLayerCanvas(layer).name;
        }

        [EasyButtons.Button]
        public void RemoveLayer(GameObject layer)
        {
            Debug.Log("Removed Layer " + layer.name);

            sceneScript.DeleteLayer(GetLayerCanvas(layer));

            // remove from the dict
            layerMap.Remove(layer);
            layerObjects.Remove(layer);

            Destroy(layer);
        }

        [EasyButtons.Button]
        public void ClearLayer(GameObject layer)
        {
            CanvasScript canvas = GetLayerCanvas(layer);
            canvas.BatchManager.ResetPools();
        }

        [EasyButtons.Button]
        public void ToggleVisibility(GameObject layer)
        {
            Debug.Log("Toggled Layer Visibility of " + layer.name);

            CanvasScript canvasScript = GetLayerCanvas(layer);
            if (canvasScript.gameObject.activeSelf) canvasScript.gameObject.SetActive(false);
            else canvasScript.gameObject.SetActive(true);
        }

        [EasyButtons.Button]
        public void SetActiveLayer(GameObject layer)
        {
            Debug.Log("Set Active Layer to " + layer.name);

            sceneScript.ActiveCanvas = GetLayerCanvas(layer);
        }

        // Utils
        private CanvasScript GetLayerCanvas(GameObject layer)
        {
            return layerMap[layer];
        }

    }
}
