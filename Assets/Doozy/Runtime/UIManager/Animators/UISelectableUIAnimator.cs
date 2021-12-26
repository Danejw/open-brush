// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.Reactor.Animations;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.UIManager.Animators
{
    /// <summary>
    /// Specialized animator component used to animate a RectTransform’s position, rotation, scale and alpha by listening to a target UISelectable (controller) selection state changes.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Doozy/UI/Animators/Selectable/UI Selectable UI Animator")]
    public class UISelectableUIAnimator : BaseUISelectableAnimator
    {
        private RectTransform m_RectTransform;
        /// <summary> Reference to the RectTransform component </summary>
        public RectTransform rectTransform => m_RectTransform ? m_RectTransform : m_RectTransform = GetComponent<RectTransform>();
        
        private Canvas m_Canvas;
        /// <summary> Reference to the Canvas component </summary>
        public Canvas canvas => m_Canvas ? m_Canvas : m_Canvas = GetComponent<Canvas>();

        private CanvasGroup m_CanvasGroup;
        /// <summary> Reference to the CanvasGroup component </summary>
        public CanvasGroup canvasGroup => m_CanvasGroup ? m_CanvasGroup : m_CanvasGroup = GetComponent<CanvasGroup>();

        private GraphicRaycaster m_GraphicRaycaster;
        /// <summary> Reference to the GraphicRaycaster component </summary>
        public GraphicRaycaster graphicRaycaster => m_GraphicRaycaster ? m_GraphicRaycaster : m_GraphicRaycaster = GetComponent<GraphicRaycaster>();

        [SerializeField] private UIAnimation NormalAnimation;
        /// <summary> Animation for the Normal selection state </summary>
        public UIAnimation normalAnimation => NormalAnimation;

        [SerializeField] private UIAnimation HighlightedAnimation;
        /// <summary> Animation for the Highlighted selection state </summary>
        public UIAnimation highlightedAnimation => HighlightedAnimation;

        [SerializeField] private UIAnimation PressedAnimation;
        /// <summary> Animation for the Pressed selection state </summary>
        public UIAnimation pressedAnimation => PressedAnimation;

        [SerializeField] private UIAnimation SelectedAnimation;
        /// <summary> Animation for the Selected selection state </summary>
        public UIAnimation selectedAnimation => SelectedAnimation;

        [SerializeField] private UIAnimation DisabledAnimation;
        /// <summary> Animation for the Disabled selection state </summary>
        public UIAnimation disabledAnimation => DisabledAnimation;

        public bool inLayoutGroup { get; private set; }

        /// <summary> Get the Animation triggered by the given selection state </summary>
        /// <param name="state"> Target selection state </param>
        public UIAnimation GetAnimation(UISelectionState state)
        {
            switch (state)
            {
                case UISelectionState.Normal: return NormalAnimation;
                case UISelectionState.Highlighted: return HighlightedAnimation;
                case UISelectionState.Pressed: return PressedAnimation;
                case UISelectionState.Selected: return SelectedAnimation;
                case UISelectionState.Disabled: return DisabledAnimation;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #if UNITY_EDITOR
        protected override void Reset()
        {
            NormalAnimation ??= new UIAnimation(rectTransform);
            HighlightedAnimation ??= new UIAnimation(rectTransform);
            PressedAnimation ??= new UIAnimation(rectTransform);
            SelectedAnimation ??= new UIAnimation(rectTransform);
            DisabledAnimation ??= new UIAnimation(rectTransform);

            ResetAnimation(NormalAnimation);
            ResetAnimation(HighlightedAnimation);
            ResetAnimation(PressedAnimation);
            ResetAnimation(SelectedAnimation);
            ResetAnimation(DisabledAnimation);

            NormalAnimation.animationType = UIAnimationType.Reset;
            NormalAnimation.Move.enabled = true;
            NormalAnimation.Rotate.enabled = true;
            NormalAnimation.Scale.enabled = true;
            NormalAnimation.Fade.enabled = true;

            base.Reset();
        }
        #endif

        protected override void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_Canvas = GetComponent<Canvas>();
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
            
            base.Awake();
            UpdateSettings();
        }

        protected override void OnEnable()
        {
            LayoutGroupCheck();
            if (!inLayoutGroup)
                Connect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (UISelectionState state in UISelectable.uiSelectionStates)
                GetAnimation(state)?.Recycle();
        }
        
        private void LayoutGroupCheck()
        {
            System.Diagnostics.Debug.Assert(rectTransform != null, nameof(rectTransform) + " != null");
            Transform parent = rectTransform.parent;
            LayoutGroup layoutGroup = parent != null ? parent.GetComponent<LayoutGroup>() : null;
            inLayoutGroup = layoutGroup != null;
            if (layoutGroup == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            StartCoroutine(InitializeInLayoutGroup());
        }

        private IEnumerator InitializeInLayoutGroup()
        {
            yield return null;     //wait 1 frame
            yield return null;     //wait 1 frame
            UpdateStartPosition(); //get new position set by the layout group
            Connect();             //connect to the target controller
        }

        private void UpdateStartPosition()
        {
            if (!inLayoutGroup) return;
            foreach (UISelectionState state in UISelectable.uiSelectionStates)
            {
                UIAnimation uiAnimation = GetAnimation(state);
                if (uiAnimation?.Move == null) continue;
                uiAnimation.startPosition = rectTransform.anchoredPosition3D;
                uiAnimation.UpdateValues();
            }
        }

        public override bool IsStateEnabled(UISelectionState state)
        {
            UIAnimation uiAnimation = GetAnimation(state);
            return uiAnimation != null && uiAnimation.isEnabled;
        }

        public override void UpdateSettings()
        {
            foreach (UISelectionState state in UISelectable.uiSelectionStates)
                GetAnimation(state).SetTarget(rectTransform);
        }

        public override void StopAllReactions()
        {
            foreach (UISelectionState state in UISelectable.uiSelectionStates)
                GetAnimation(state)?.Stop();
        }

        public override void Play(UISelectionState state) =>
            GetAnimation(state)?.Play();

        private static void ResetAnimation(UIAnimation target)
        {
            target.Move.Reset();
            target.Rotate.Reset();
            target.Scale.Reset();
            target.Fade.Reset();

            target.animationType = UIAnimationType.State;

            target.Move.fromReferenceValue = ReferenceValue.CurrentValue;
            target.Rotate.fromReferenceValue = ReferenceValue.CurrentValue;
            target.Scale.fromReferenceValue = ReferenceValue.CurrentValue;
            target.Fade.fromReferenceValue = ReferenceValue.CurrentValue;

            target.Move.settings.duration = UISelectable.k_DefaultAnimationDuration;
            target.Rotate.settings.duration = UISelectable.k_DefaultAnimationDuration;
            target.Scale.settings.duration = UISelectable.k_DefaultAnimationDuration;
            target.Fade.settings.duration = UISelectable.k_DefaultAnimationDuration;
        }
    }
}
