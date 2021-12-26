// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.UIElements;
using Doozy.Runtime.UIElements.Extensions;
using Doozy.Runtime.UIManager.Input;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.UIManager.Editors.Input
{
    [CustomEditor(typeof(InputToSignal), true)]
    [CanEditMultipleObjects]
    public class InputToSignalEditor : UnityEditor.Editor
    {
        public InputToSignal castedTarget => (InputToSignal)target;
        public IEnumerable<InputToSignal> castedTargets => targets.Cast<InputToSignal>();

        private static Color accentColor => EditorColors.UIManager.InputComponent;
        private static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.UIManager.InputComponent;

        private static IEnumerable<Texture2D> inputToSignalIconTextures => EditorMicroAnimations.UIManager.Icons.InputToSignal;
        
        //ToDo: maybe -> add a connected state indicator
        private static IEnumerable<Texture2D> disconnectedConnectedTextures => EditorMicroAnimations.EditorUI.Icons.DisconnectedConnected;
        

        private VisualElement root { get; set; }
        private FluidComponentHeader componentHeader { get; set; }

        private FluidToggleSwitch autoConnectSwitch { get; set; }
        private PropertyField uiInputModulePropertyField { get; set; }
        private PropertyField playerInputPropertyField { get; set; }
        private EnumField inputActionNameEnumField { get; set; }
        private TextField customInputActionNameTextField { get; set; }

        private FluidField uiInputModuleField { get; set; }
        private FluidField playerInputField { get; set; }
        private FluidField inputActionNameField { get; set; }
        private FluidField customInputActionNameField { get; set; }

        private SerializedProperty propertyAutoConnect { get; set; }
        private SerializedProperty propertyUIInputModule { get; set; }
        private SerializedProperty propertyPlayerInput { get; set; }
        private SerializedProperty propertyInputActionName { get; set; }
        private SerializedProperty propertyCustomInputActionName { get; set; }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor();
            Compose();
            return root;
        }

        private void OnDestroy()
        {
            componentHeader?.Recycle();
            uiInputModuleField?.Recycle();
            playerInputField?.Recycle();
            inputActionNameField?.Recycle();
            customInputActionNameField?.Recycle();
            autoConnectSwitch?.Recycle();
        }

        private void FindProperties()
        {
            propertyAutoConnect = serializedObject.FindProperty("AutoConnect");
            propertyUIInputModule = serializedObject.FindProperty("UIInputModule");
            propertyPlayerInput = serializedObject.FindProperty("PlayerInput");
            propertyInputActionName = serializedObject.FindProperty("InputActionName");
            propertyCustomInputActionName = serializedObject.FindProperty("CustomInputActionName");
        }

        private void InitializeEditor()
        {
            FindProperties();

            root = new VisualElement();

            componentHeader = FluidComponentHeader.Get()
                .SetAccentColor(accentColor)
                .SetComponentNameText(ObjectNames.NicifyVariableName(nameof(InputToSignal)))
                .SetIcon(inputToSignalIconTextures.ToList())
                .SetElementSize(ElementSize.Large)
                .AddManualButton("https://doozyentertainment.atlassian.net/wiki/spaces/DUI4/pages/1046675564/Input+To+Signal?atlOrigin=eyJpIjoiZjU4MWZjODY2OWNlNGJjNDljN2ZiNzE1OWZhZmI1ZTciLCJwIjoiYyJ9")
                .AddYouTubeButton("www.youtube.com/c/DoozyEntertainment");

            autoConnectSwitch =
                FluidToggleSwitch.Get()
                    .BindToProperty(propertyAutoConnect)
                    .SetLabelText("Auto Connect")
                    .SetToggleAccentColor(selectableAccentColor);

            uiInputModulePropertyField =
                DesignUtils.NewPropertyField(propertyUIInputModule)
                    .TryToHideLabel()
                    .SetStyleFlexGrow(1);

            playerInputPropertyField =
                DesignUtils.NewPropertyField(propertyPlayerInput)
                    .TryToHideLabel()
                    .SetStyleFlexGrow(1);

            inputActionNameEnumField =
                DesignUtils.NewEnumField(propertyInputActionName)
                    .SetStyleWidth(190);

            customInputActionNameTextField =
                DesignUtils.NewTextField(propertyCustomInputActionName)
                    .SetStyleFlexGrow(1);

            //FIELDS
            uiInputModuleField =
                FluidField.Get()
                    .SetLabelText("UI Input Module")
                    .AddFieldContent(uiInputModulePropertyField);

            playerInputField =
                FluidField.Get()
                    .SetLabelText("Player Input")
                    .AddFieldContent(playerInputPropertyField);

            inputActionNameField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetLabelText("Input Action Name")
                    .AddFieldContent(inputActionNameEnumField);

            customInputActionNameField =
                FluidField.Get()
                    .SetLabelText("Custom Input Action Name")
                    .AddFieldContent(customInputActionNameTextField);

            customInputActionNameField
                .SetEnabled((UIInputActionName)propertyInputActionName.enumValueIndex == UIInputActionName.CustomActionName);

            inputActionNameEnumField.RegisterValueChangedCallback(evt =>
            {
                if (evt?.newValue == null) return;
                customInputActionNameField.SetEnabled((UIInputActionName)evt.newValue == UIInputActionName.CustomActionName);
            });
        }

        private void Compose()
        {
            root
                .AddChild(componentHeader)
                .AddChild(DesignUtils.spaceBlock)
                .AddChild(autoConnectSwitch)
                .AddChild(DesignUtils.spaceBlock)
                .AddChild
                (
                    DesignUtils.row
                        .AddChild(uiInputModuleField)
                        .AddChild(DesignUtils.spaceBlock)
                        .AddChild(playerInputField)
                )
                .AddChild(DesignUtils.spaceBlock)
                .AddChild
                (
                    DesignUtils.row
                        .AddChild(inputActionNameField)
                        .AddChild(DesignUtils.spaceBlock)
                        .AddChild(customInputActionNameField)
                )
                ;
        }
    }
}
