using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyButtons;
using System;

namespace TiltBrush.Layers
{
    public class LayerUI_Manager : MonoBehaviour
    {
        public bool debug = false;

        public delegate void OnActiveSceneChanged(GameObject layer);
        public static event OnActiveSceneChanged onActiveSceneChanged;

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

            App.Scene.ActiveCanvasChanged += ActiveSceneChanged;
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

                if (debug) Debug.Log("Creating Layer " + mainLayer.name);

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

            if (debug) Debug.Log("Creating Layer " + layer.name);

            // put it into the dict
            layerMap.Add(layer, canvas);
            layerObjects.Add(layer);

            // set the layer name on the ui
            if(GetLayerCanvas(layer))
                layer.GetComponentInChildren<TMPro.TMP_Text>().text = GetLayerCanvas(layer).name;
        }

        [EasyButtons.Button]
        public void RemoveLayer(GameObject layer)
        {
            if (!GetLayerCanvas(layer)) return;

            if (debug) Debug.Log("Removed Layer " + layer.name);

            sceneScript.DeleteLayer(GetLayerCanvas(layer));

            // remove from the dict
            layerMap.Remove(layer);
            layerObjects.Remove(layer);

            Destroy(layer);
        }

        [EasyButtons.Button]
        public void ClearLayer(GameObject layer)
        {
            if (!GetLayerCanvas(layer)) return;

            CanvasScript canvas = GetLayerCanvas(layer);
            canvas.BatchManager.ResetPools();
        }

        [EasyButtons.Button]
        public void ToggleVisibility(GameObject layer)
        {
            if (!GetLayerCanvas(layer)) return;

            if (debug) Debug.Log("Toggled Layer Visibility of " + layer.name);

            CanvasScript canvasScript = GetLayerCanvas(layer);
            if (canvasScript.gameObject.activeSelf) canvasScript.gameObject.SetActive(false);
            else canvasScript.gameObject.SetActive(true);
        }

        [EasyButtons.Button]
        public void SetActiveLayer(GameObject layer)
        {
            if (!GetLayerCanvas(layer)) return;

            sceneScript.ActiveCanvas = GetLayerCanvas(layer);

            if (debug) Debug.Log("Set Active Layer to " + sceneScript.ActiveCanvas);
        }

        [EasyButtons.Button]
        public void PrintDictionary()
        {
            foreach (var layer in layerMap)
            {
                if (debug) Debug.Log("Key: " + layer.Key + "Value: " + layer.Value);
                if (debug) Debug.Log("Key's HashCode: " + layer.Key.GetHashCode());
            }
        }

        private void ActiveSceneChanged(CanvasScript prev, CanvasScript current)
        {
            // unOptimized code.... searched trhough the dictionary to find a value and return a key, invoke a message with that key as its parameter
            foreach (var layer in layerMap)
                if (layer.Value == current)
                    onActiveSceneChanged?.Invoke(layer.Key);
        }

        // Utils
        [EasyButtons.Button]
        [SerializeField] 
        private CanvasScript GetLayerCanvas(GameObject layer)
        {      
            try
            {
                if (debug) Debug.Log("Canvas Value: " + layerMap[layer]);

                return layerMap[layer];
            }
            catch (KeyNotFoundException e)
            {
                if (debug) Debug.LogException(e);
                return null;
            }
            
        }
    }
}
