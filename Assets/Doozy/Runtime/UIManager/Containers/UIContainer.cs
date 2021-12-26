// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Mody;
using Doozy.Runtime.Reactor.Internal;
using Doozy.Runtime.Reactor.Reactions;
using Doozy.Runtime.UIManager.Events;
using Doozy.Runtime.UIManager.Input;
using Doozy.Runtime.UIManager.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local

namespace Doozy.Runtime.UIManager.Containers
{
    /// <summary>
    /// Basic container with show and hide capabilities.
    /// All other containers use this as their base.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Doozy/UI/Containers/UI Container")]
    [SelectionBase]
    public class UIContainer : MonoBehaviour, ICanvasElement, IUseMultiplayerInfo
    {
        public const string k_StreamCategory = nameof(UIContainer);
        public const float k_DefaultAnimationDuration = 0.3f;

        #region MultiplayerInfo

        [SerializeField] private MultiplayerInfo MultiplayerInfo;
        public MultiplayerInfo multiplayerInfo => MultiplayerInfo;
        public bool hasMultiplayerInfo => multiplayerInfo != null;
        public int playerIndex => multiplayerMode & hasMultiplayerInfo ? multiplayerInfo.playerIndex : inputSettings.defaultPlayerIndex;
        public void SetMultiplayerInfo(MultiplayerInfo info) => MultiplayerInfo = info;

        #endregion

        /// <summary> Reference to the UIManager Input Settings </summary>
        public static UIManagerInputSettings inputSettings => UIManagerInputSettings.instance;

        /// <summary> True Multiplayer Mode is enabled </summary>
        public static bool multiplayerMode => inputSettings.multiplayerMode;

        private Canvas m_Canvas;
        /// <summary> Reference to the Canvas component </summary>
        public Canvas canvas => m_Canvas ? m_Canvas : m_Canvas = GetComponent<Canvas>();

        private CanvasGroup m_CanvasGroup;
        /// <summary> Reference to the CanvasGroup component </summary>
        public CanvasGroup canvasGroup => m_CanvasGroup ? m_CanvasGroup : m_CanvasGroup = GetComponent<CanvasGroup>();

        /// <summary> TRUE if a CanvasGroup component is attached </summary>
        public bool hasCanvasGroup { get; private set; }
        
        private GraphicRaycaster m_GraphicRaycaster;
        /// <summary> Reference to the GraphicRaycaster component </summary>
        public GraphicRaycaster graphicRaycaster => m_GraphicRaycaster ? m_GraphicRaycaster : m_GraphicRaycaster = GetComponent<GraphicRaycaster>();

        private RectTransform m_RectTransform;
        /// <summary> Reference to the RectTransform component </summary>
        public RectTransform rectTransform => m_RectTransform ? m_RectTransform : m_RectTransform = GetComponent<RectTransform>();

        /// <summary> Container behaviour on Start </summary>
        public ContainerBehaviour OnStartBehaviour = ContainerBehaviour.Disabled;

        #region Visibility

        /// <summary> Current visibility state </summary>
        private VisibilityState m_VisibilityState = VisibilityState.Visible;

        /// <summary> Current visibility state </summary>
        public VisibilityState visibilityState
        {
            get => m_VisibilityState;
            private set
            {
                m_VisibilityState = value;
                OnVisibilityChangedCallback?.Invoke(m_VisibilityState);
                switch (value)
                {
                    case VisibilityState.Visible:
                        ExecuteOnVisible();
                        break;
                    case VisibilityState.Hidden:
                        ExecuteOnHidden();
                        break;
                    case VisibilityState.IsShowing:
                        ExecuteOnShow();
                        break;
                    case VisibilityState.IsHiding:
                        ExecuteOnHide();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        /// <summary> Visibility state is Visible </summary>
        public bool isVisible => visibilityState == VisibilityState.Visible;

        /// <summary> Visibility state is Hidden </summary>
        public bool isHidden => visibilityState == VisibilityState.Hidden;

        /// <summary> Visibility state is IsShowing - Show animation is running </summary>
        public bool isShowing => visibilityState == VisibilityState.IsShowing;

        /// <summary> Visibility state is IsHiding - Hide animation is running </summary>
        public bool isHiding => visibilityState == VisibilityState.IsHiding;

        /// <summary> Visibility state is either IsShowing or IsHiding - either Show or Hide animation is running </summary>
        public bool inTransition => isShowing || isHiding;

        /// <summary> Show animation started - executed when visibility state changed to IsShowing </summary>
        public ModyEvent OnShowCallback;

        /// <summary> Visible - Show animation finished - executed when visibility state changed to Visible </summary>
        public ModyEvent OnVisibleCallback;

        /// <summary> Hide animation started - executed when visibility state changed to IsHiding </summary>
        public ModyEvent OnHideCallback;

        /// <summary> Hidden - Hide animation finished - callback invoked when visibility state changed to Hidden </summary>
        public ModyEvent OnHiddenCallback;

        /// <summary> Visibility changed - callback invoked when visibility state changed </summary>
        public VisibilityStateEvent OnVisibilityChangedCallback;

        /// <summary> Action invoked every time before the container needs to change its state </summary>
        public UnityAction<ShowHideExecute> showHideExecute { get; set; }

        public bool executedFirstCommand { get; protected set; }
        public ShowHideExecute previouslyExecutedCommand { get; protected set; }

        #endregion

        /// <summary> AnchoredPosition3D to snap to on Awake </summary>
        public Vector3 CustomStartPosition;

        /// <summary> If TRUE, the rectTransform.anchoredPosition3D will 'snap' to the CustomStartPosition on Awake </summary>
        public bool UseCustomStartPosition;

        /// <summary> If TRUE, after this container gets shown, it will get automatically hidden after the AutoHideAfterShowDelay time interval has passed </summary>
        public bool AutoHideAfterShow;

        /// <summary> If AutoHideAfterShow is TRUE, this is the time interval after which this container will get automatically hidden </summary>
        public float AutoHideAfterShowDelay;

        /// <summary> If TRUE, when this container gets hidden, the GameObject this container component is attached to, will be disabled </summary>
        public bool DisableGameObjectWhenHidden;

        /// <summary> If TRUE, when this container gets hidden, the Canvas component found on the same GameObject this container component is attached to, will be disabled </summary>
        public bool DisableCanvasWhenHidden = true;

        /// <summary> If TRUE, when this container gets hidden, the GraphicRaycaster component found on the same GameObject this container component is attached to, will be disabled </summary>
        public bool DisableGraphicRaycasterWhenHidden = true;

        private HashSet<Reaction> m_ShowReactions;
        internal HashSet<Reaction> showReactions => m_ShowReactions ??= new HashSet<Reaction>();

        private HashSet<Reaction> m_HideReactions;
        internal HashSet<Reaction> hideReactions => m_HideReactions ??= new HashSet<Reaction>();

        private Coroutine m_AutoHideCoroutine;
        private Coroutine m_CoroutineIsShowing;
        private Coroutine m_CoroutineIsHiding;

        public UIContainer()
        {
            UseCustomStartPosition = true;

            OnShowCallback = new ModyEvent(nameof(OnShowCallback));
            OnVisibleCallback = new ModyEvent(nameof(OnVisibleCallback));
            OnHideCallback = new ModyEvent(nameof(OnHideCallback));
            OnHiddenCallback = new ModyEvent(nameof(OnHiddenCallback));
            OnVisibilityChangedCallback = new VisibilityStateEvent();
        }

        #if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        #endif // if UNITY_EDITOR

        #region ICanvasElement

        public virtual void Rebuild(CanvasUpdate executing) {}
        public virtual void LayoutComplete() {}
        public virtual void GraphicUpdateComplete() {}
        public bool IsDestroyed() => this == null;

        #endregion

        protected virtual void Awake()
        {
            BackButton.Initialize();

            m_Canvas = GetComponent<Canvas>();
            m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
            m_RectTransform = GetComponent<RectTransform>();
            hasCanvasGroup = canvasGroup != null;

            showReactions.Remove(null);
            hideReactions.Remove(null);

            executedFirstCommand = false;

            if (UseCustomStartPosition)
            {
                SetCustomStartPosition(CustomStartPosition);
            }
        }

        protected virtual void OnEnable()
        {
            hasCanvasGroup = canvasGroup != null;
        }

        protected virtual void Start()
        {
            RunBehaviour(OnStartBehaviour);
        }

        protected virtual void OnDisable()
        {
            StopIsShowingCoroutine();
            StopIsHidingCoroutine();

            showReactions.Remove(null);
            foreach (Reaction reaction in showReactions)
                reaction.Stop();

            hideReactions.Remove(null);
            foreach (Reaction reaction in hideReactions)
                reaction.Stop();
        }

        protected virtual void OnDestroy() {}

        public void SetCustomStartPosition(Vector3 startPosition, bool jumpToPosition = true)
        {
            CustomStartPosition = startPosition;

            showReactions.Remove(null);
            foreach (Reaction reaction in showReactions)
                if (reaction is UIMoveReaction moveReaction)
                    moveReaction.startPosition = startPosition;

            hideReactions.Remove(null);
            foreach (Reaction reaction in hideReactions)
                if (reaction is UIMoveReaction moveReaction)
                    moveReaction.startPosition = startPosition;

            if (jumpToPosition)
                rectTransform.anchoredPosition3D = startPosition;
        }

        private void ExecutedCommand(ShowHideExecute command)
        {
            showHideExecute?.Invoke(command);
            executedFirstCommand = true;
            previouslyExecutedCommand = command;
        }

        #region Instant Show/Hide/Toggle

        public void InstantShow()
        {
            if (isVisible) return;

            StopIsShowingCoroutine();
            StopIsHidingCoroutine();

            canvas.enabled = true;           //enable the canvas
            graphicRaycaster.enabled = true; //enable the graphic raycaster
            if (hasCanvasGroup) canvasGroup.blocksRaycasts = graphicRaycaster.enabled;
            gameObject.SetActive(true); //set the active state to true (in case it has been disabled when hidden)

            ExecutedCommand(ShowHideExecute.InstantShow);

            visibilityState = VisibilityState.IsShowing;
            visibilityState = VisibilityState.Visible;
        }



        public void InstantHide()
        {
            if (isHidden) return;

            StopIsShowingCoroutine();
            StopIsHidingCoroutine();

            ExecutedCommand(ShowHideExecute.InstantHide);

            visibilityState = VisibilityState.IsHiding;
            visibilityState = VisibilityState.Hidden;
        }

        public void InstantToggle(bool show)
        {
            if (show)
            {
                InstantShow();
                return;
            }
            InstantHide();
        }

        #endregion

        #region Animated Show/Hide/Toggle

        public void Show()
        {
            // if (!isActiveAndEnabled) return;
            if (isShowing || isVisible) return;

            if (isHiding)
            {
                StopIsHidingCoroutine();

                ExecutedCommand(ShowHideExecute.ReverseHide);

                m_CoroutineIsShowing = StartCoroutine(IsShowing());
                return;
            }

            canvas.enabled = true; //enable the canvas
            if (DisableGraphicRaycasterWhenHidden)
            {
                graphicRaycaster.enabled = true; //enable the graphic raycaster
                if (hasCanvasGroup) canvasGroup.blocksRaycasts = graphicRaycaster.enabled;
            }
            gameObject.SetActive(true); //set the active state to true (in case it has been disabled when hidden)

            ExecutedCommand(ShowHideExecute.Show);

            m_CoroutineIsShowing = StartCoroutine(IsShowing());
        }

        private void StopIsShowingCoroutine()
        {
            if (m_CoroutineIsShowing == null)
                return;
            StopCoroutine(m_CoroutineIsShowing);
            m_CoroutineIsShowing = null;
        }

        private IEnumerator IsShowing()
        {
            StopIsHidingCoroutine();
            yield return null;
            visibilityState = VisibilityState.IsShowing;
            while (isShowing)
            {
                yield return null;
                bool showIsActive = showReactions.Any(show => show.isActive);
                bool hideIsActive = hideReactions.Any(hide => hide.isActive);

                if (showIsActive || hideIsActive)
                    continue;

                visibilityState = VisibilityState.Visible;
            }

            ExecutedCommand(ShowHideExecute.InstantShow);

            m_CoroutineIsShowing = null;
        }

        public void Hide()
        {
            if (!isActiveAndEnabled) return;
            if (isHiding || isHidden) return;

            if (isShowing)
            {
                StopIsShowingCoroutine();

                ExecutedCommand(ShowHideExecute.ReverseShow);

                m_CoroutineIsHiding = StartCoroutine(IsHiding());
                return;
            }

            ExecutedCommand(ShowHideExecute.Hide);

            m_CoroutineIsHiding = StartCoroutine(IsHiding());
        }

        private void StopIsHidingCoroutine()
        {
            if (m_CoroutineIsHiding == null)
                return;
            StopCoroutine(m_CoroutineIsHiding);
            m_CoroutineIsHiding = null;
        }

        private IEnumerator IsHiding()
        {
            StopIsShowingCoroutine();
            yield return null;
            visibilityState = VisibilityState.IsHiding;
            while (isHiding)
            {
                yield return null;
                bool showIsActive = showReactions.Any(show => show.isActive);
                bool hideIsActive = hideReactions.Any(hide => hide.isActive);

                if (showIsActive || hideIsActive)
                    continue;

                visibilityState = VisibilityState.Hidden;
            }

            ExecutedCommand(ShowHideExecute.InstantHide);

            m_CoroutineIsHiding = null;
        }

        public void Toggle(bool show)
        {
            if (show)
            {
                Show();
                return;
            }
            Hide();
        }

        #endregion

        private void ExecuteOnShow()
        {
            OnShowCallback.Execute();
            // showHideCommand?.Invoke(ShowHideCommand.Show);
        }

        private void ExecuteOnHide()
        {
            OnHideCallback.Execute();
            StopAutoHide();
            // showHideCommand?.Invoke(ShowHideCommand.Hide);
        }

        private void ExecuteOnVisible()
        {
            OnVisibleCallback.Execute();
            StartAutoHide();
        }

        private void ExecuteOnHidden()
        {
            OnHiddenCallback.Execute();
            canvas.enabled = !DisableCanvasWhenHidden;                     //disable the canvas, if the option is enabled
            graphicRaycaster.enabled = !DisableGraphicRaycasterWhenHidden; //disable the graphic raycaster, if the option is enabled
            if (hasCanvasGroup) canvasGroup.blocksRaycasts = graphicRaycaster.enabled;
            gameObject.SetActive(!DisableGameObjectWhenHidden); //set the active state to false, if the option is enabled
            // StartCoroutine(DisableGameObjectWithDelay());
        }

        private IEnumerator DisableGameObjectWithDelay()
        {
            //we need to wait for 3 frames to make sure all the connected animators have had enough time to initialize (it takes 2 frames for a position animator to get its start position from a layout group (THANKS UNITY!!!) FML)
            yield return null; //wait 1 frame (1 for the money)
            yield return null; //wait 1 frame (2 for the show)
            yield return null; //wait 1 frame (3 to get ready)
            // ...and 4 to f@#king go!
            gameObject.SetActive(!DisableGameObjectWhenHidden); //set the active state to false, if the option is enabled
        }


        private void StartAutoHide()
        {
            StopAutoHide();
            if (!AutoHideAfterShow) return;
            m_AutoHideCoroutine = StartCoroutine(AutoHideEnumerator());
        }

        private void StopAutoHide()
        {
            if (m_AutoHideCoroutine == null) return;
            StopCoroutine(m_AutoHideCoroutine);
            m_AutoHideCoroutine = null;
        }

        private IEnumerator AutoHideEnumerator()
        {
            yield return new WaitForSecondsRealtime(AutoHideAfterShowDelay);
            Hide();
            m_AutoHideCoroutine = null;
        }

        private void RunBehaviour(ContainerBehaviour behaviour)
        {
            switch (behaviour)
            {
                case ContainerBehaviour.Disabled:
                    //ignored
                    return;

                case ContainerBehaviour.InstantHide:
                    m_VisibilityState = VisibilityState.Visible;
                    InstantHide();
                    return;

                case ContainerBehaviour.InstantShow:
                    m_VisibilityState = VisibilityState.Hidden;
                    InstantShow();
                    return;

                case ContainerBehaviour.Hide:
                    m_VisibilityState = VisibilityState.Visible;
                    Hide();
                    return;

                case ContainerBehaviour.Show:
                    m_VisibilityState = VisibilityState.Hidden;
                    Show();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null);
            }
        }
    }
}
