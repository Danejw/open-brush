// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.UIManager.Input;
using Doozy.Runtime.UIManager.ScriptableObjects;
using UnityEngine.InputSystem;

namespace Doozy.Runtime.UIManager
{
    [Serializable]
    public struct InputSignalData
    {
        public static UIManagerInputSettings inputSettings => UIManagerInputSettings.instance;
        public static bool multiplayerMode => inputSettings.multiplayerMode;

        public InputAction.CallbackContext callbackContext { get; }
        public PlayerInput playerInput { get; }
        public bool hasPlayerInput => playerInput != null;
        public bool ignorePlayerIndex => playerIndex == inputSettings.defaultPlayerIndex;
        public int playerIndex { get; }
        public UIInputActionName inputActionName { get; }

        public InputSignalData(UIInputActionName inputActionName, int playerIndex) : this(inputActionName, new InputAction.CallbackContext(), playerIndex, null) {}

        public InputSignalData(UIInputActionName inputActionName, InputAction.CallbackContext callbackContext, int playerIndex, PlayerInput playerInput = null)
        {
            this.inputActionName = inputActionName;
            this.callbackContext = callbackContext;
            this.playerIndex = playerIndex;
            this.playerInput = playerInput;
        }

        public override string ToString()
        {
            string message = callbackContext.action != null ? $"'{callbackContext.action.name}'" : inputActionName.ToString();
            if (multiplayerMode && !ignorePlayerIndex) message += $" called by Player {playerIndex}";
            return message;
        }
    }
}
