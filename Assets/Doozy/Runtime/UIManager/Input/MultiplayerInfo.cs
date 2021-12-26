// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.UIManager.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.UIManager.Input
{
    /// <summary>
    /// Special component that assigns a player index to a UI structure.
    /// It can either get it from a Player Input component, or use a custom player index.
    /// This component is available only if Multiplayer mode is enabled.
    /// </summary>
    [AddComponentMenu("Doozy/UI/Input/Multiplayer Info")]
    public class MultiplayerInfo : MonoBehaviour
    {
        /// <summary> Reference to the UIManager Input Settings </summary>
        public static UIManagerInputSettings inputSettings => UIManagerInputSettings.instance;

        /// <summary> True if Multiplayer Mode is enabled </summary>
        public static bool multiplayerMode => inputSettings.multiplayerMode;
        
        [SerializeField] private bool AutoUpdate = true;
        [SerializeField] private PlayerInput PlayerInput;
        [SerializeField] private int CustomPlayerIndex;
        [SerializeField] private bool UseCustomPlayerIndex;

        public bool hasPlayerInput => playerInput != null;
       
        public PlayerInput playerInput
        {
            get => PlayerInput;
            set => PlayerInput = value;
        }

        public int customPlayerIndex
        {
            get => CustomPlayerIndex;
            set
            {
                UseCustomPlayerIndex = true;
                CustomPlayerIndex = value;
            }
        }

        public bool useCustomPlayerIndex
        {
            get => UseCustomPlayerIndex;
            set => UseCustomPlayerIndex = value;
        }

        public int playerIndex =>
            useCustomPlayerIndex
                ? customPlayerIndex
                : hasPlayerInput
                    ? playerInput.playerIndex
                    : inputSettings.defaultPlayerIndex;

        public bool ignorePlayerIndex => 
            playerIndex == inputSettings.defaultPlayerIndex; 
        
        public bool autoUpdate
        {
            get => AutoUpdate;
            set => AutoUpdate = value;
        }

        private void GetReferences()
        {
            PlayerInput ??= GetComponent<PlayerInput>();
        }

        private void Reset()
        {
            GetReferences();
        }

        private void Awake()
        {
            if (AutoUpdate)
                UpdateReferences();
        }

        public void UpdateReferences()
        {
            // BroadcastMessage("SetPlayerIndex", this);

            IUseMultiplayerInfo[] users = GetComponentsInChildren<IUseMultiplayerInfo>();
            if (users == null || users.Length == 0)
                return;

            foreach (IUseMultiplayerInfo user in users)
                user.SetMultiplayerInfo(this);
        }

        public MultiplayerInfo SetAutoUpdate(bool value)
        {
            autoUpdate = value;
            return this;
        }
        
        public MultiplayerInfo SetPlayerInput(PlayerInput value)
        {
            PlayerInput = value;
            return this;
        }
        
        public MultiplayerInfo SetCustomPlayerIndex(int value)
        {
            customPlayerIndex = value;
            return this;
        }
        
        public MultiplayerInfo SetUseCustomPlayerIndex(bool value)
        {
            useCustomPlayerIndex = value;
            return this;
        }
    }
}
