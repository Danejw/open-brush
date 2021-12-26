// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.UIManager.Input
{
    /// <summary>
    /// Bridge that connects the Input System with the Input Stream.
    /// It does that by listening for when a specific action has been performed and sends a meta-signal on the Input Stream.
    /// </summary>
    [AddComponentMenu("Doozy/UI/Input/Input To Signal")]
    public class InputToSignal : MonoBehaviour
    {
        /// <summary> Reference to the UIManager Input Settings </summary>
        public static UIManagerInputSettings inputSettings => UIManagerInputSettings.instance;
        
        public static HashSet<InputToSignal> database { get; } = new HashSet<InputToSignal>();
        public static void CleanDatabase() => database.Remove(null);

        [SerializeField] private bool AutoConnect;
        [SerializeField] private InputSystemUIInputModule UIInputModule;
        [SerializeField] private PlayerInput PlayerInput;
        [SerializeField] private int PlayerIndex;
        [SerializeField] private UIInputActionName InputActionName;
        [SerializeField] private string CustomInputActionName;

        private InputAction m_Action;
        private bool m_IsConnected;

        public PlayerInput playerInput => PlayerInput;
        public InputSystemUIInputModule uiInputModule => UIInputModule;
        public InputAction action => m_Action;
        public int playerIndex => hasPlayerInput ? PlayerInput.playerIndex : PlayerIndex;
        public string inputActionName => hasCustomActionName ? CustomInputActionName : InputActionName.ToString();
        public bool autoConnect => AutoConnect;
        public bool isConnected => m_IsConnected;
        public bool hasPlayerInput => playerInput != null;
        public bool hasUIInputModule => uiInputModule != null;
        public bool hasCustomActionName => InputActionName == UIInputActionName.CustomActionName;

        private void GetReferences()
        {
            PlayerInput ??= GetComponent<PlayerInput>();
            if (PlayerInput != null)
            {
                UIInputModule = PlayerInput.uiInputModule;
            }
            else
            {
                UIInputModule ??= GetComponent<InputSystemUIInputModule>();
            }
        }

        private void Reset()
        {
            AutoConnect = true;
            PlayerIndex = inputSettings.defaultPlayerIndex;
            InputActionName = UIInputActionName.Cancel;
            CustomInputActionName = string.Empty;
            GetReferences();
        }

        private void Awake()
        {
            database.Add(this);
            GetReferences();
            if (UIInputModule != null) return;
            enabled = false;
        }

        private void OnEnable()
        {
            CleanDatabase();
            if (autoConnect) Connect();
        }

        private void OnDisable()
        {
            CleanDatabase();
            Disconnect();
        }

        private void OnDestroy()
        {
            database.Remove(this);
        }

        public InputToSignal Connect()
        {
            if (isConnected) return this;
            (bool isValid, string message) = IsValid();
            if (!isValid)
            {
                Debug.Log(message);
                return this;
            }
            action.performed += OnActionPerformed;
            m_IsConnected = true;
            return this;
        }

        public InputToSignal Disconnect()
        {
            if (!isConnected) return this;
            if (action == null) return this;
            action.performed -= OnActionPerformed;
            m_IsConnected = false;
            return this;
        }

        public InputToSignal ConnectToAction(UIInputActionName actionName)
        {
            Disconnect();
            m_Action = null;
            InputActionName = actionName;
            CustomInputActionName = string.Empty;
            Connect();
            return this;
        }
        
        public InputToSignal ConnectToCustomAction(string actionName)
        {
            Disconnect();
            m_Action = null;
            InputActionName = UIInputActionName.CustomActionName;
            CustomInputActionName = actionName;
            Connect();
            return this;
        }

        public InputToSignal ConnectToCustomAction(InputAction inputAction)
        {
            if (inputAction == null) return this;
            Disconnect();
            InputActionName = UIInputActionName.CustomActionName;
            CustomInputActionName = inputAction.name;
            m_Action = inputAction;
            Connect();
            return this;
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            InputStream.stream.SendSignal(new InputSignalData(InputActionName, context, playerIndex, playerInput));
        }

        private (bool, string) IsValid()
        {
            if (m_Action != null) return (true, "Valid");
            if (UIInputModule == null) return (false, $"Not Valid: {nameof(UIInputModule)} is null");
            m_Action = null;

            m_Action = InputActionName switch
                       {
                           UIInputActionName.Point                    => UIInputModule.point.action,
                           UIInputActionName.Click                    => UIInputModule.leftClick.action,
                           UIInputActionName.MiddleClick              => UIInputModule.middleClick.action,
                           UIInputActionName.RightClick               => UIInputModule.rightClick.action,
                           UIInputActionName.ScrollWheel              => UIInputModule.scrollWheel.action,
                           UIInputActionName.Navigate                 => UIInputModule.move.action,
                           UIInputActionName.Submit                   => UIInputModule.submit.action,
                           UIInputActionName.Cancel                   => UIInputModule.cancel.action,
                           UIInputActionName.TrackedDevicePosition    => UIInputModule.trackedDevicePosition.action,
                           UIInputActionName.TrackedDeviceOrientation => UIInputModule.trackedDeviceOrientation.action,
                           UIInputActionName.CustomActionName         => UIInputModule.actionsAsset.FindAction(CustomInputActionName),
                           _                                          => throw new ArgumentOutOfRangeException()
                       };

            return
                m_Action == null
                    ? (false, $"Not Valid: {nameof(m_Action)} is null")
                    : (true, "Valid");
        }
    }
}
