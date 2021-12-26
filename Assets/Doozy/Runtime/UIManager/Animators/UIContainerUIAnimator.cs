// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.Reactor.Animations;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.UIManager.Animators
{
    /// <summary>
    /// Specialized animator component used to animate a RectTransform’s position, rotation, scale and alpha by listening to a target UIContainer (controller) show/hide commands.
    /// </summary>
    [AddComponentMenu("Doozy/UI/Animators/Container/UI Container UI Animator")]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(RectTransform))]
    public class UIContainerUIAnimator : BaseUIContainerAnimator
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

        [SerializeField] private UIAnimation ShowAnimation;
        /// <summary> Container Show Animation </summary>
        public UIAnimation showAnimation => ShowAnimation;

        [SerializeField] private UIAnimation HideAnimation;
        /// <summary> Container Hide Animation </summary>
        public UIAnimation hideAnimation => HideAnimation;

        public bool inLayoutGroup { get; private set; }

        #if UNITY_EDITOR
        protected override void Reset()
        {
            ShowAnimation ??= new UIAnimation(rectTransform);
            HideAnimation ??= new UIAnimation(rectTransform);

            ResetAnimation(ShowAnimation);
            ResetAnimation(HideAnimation);

            ShowAnimation.animationType = UIAnimationType.Show;
            ShowAnimation.Move.enabled = true;
            ShowAnimation.Move.fromDirection = MoveDirection.Left;

            HideAnimation.animationType = UIAnimationType.Hide;
            HideAnimation.Move.enabled = true;
            HideAnimation.Move.toDirection = MoveDirection.Right;

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
            LayoutGroupCheck();
        }

        protected override void OnEnable()
        {
            if (!inLayoutGroup)
                Connect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ShowAnimation?.Recycle();
            HideAnimation?.Recycle();
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
            if (!inLayoutGroup)
                return;

            if (ShowAnimation?.Move != null)
            {
                ShowAnimation.startPosition = rectTransform.anchoredPosition3D;
                ShowAnimation.UpdateValues();
            }

            if (HideAnimation?.Move != null)
            {
                HideAnimation.startPosition = rectTransform.anchoredPosition3D;
                HideAnimation.UpdateValues();
            }
        }

        protected override void ConnectToController()
        {
            base.ConnectToController();
            if (!controller) return;

            controller.showReactions.Add(ShowAnimation.Move);
            controller.showReactions.Add(ShowAnimation.Rotate);
            controller.showReactions.Add(ShowAnimation.Scale);
            controller.showReactions.Add(ShowAnimation.Fade);

            controller.hideReactions.Add(HideAnimation.Move);
            controller.hideReactions.Add(HideAnimation.Rotate);
            controller.hideReactions.Add(HideAnimation.Scale);
            controller.hideReactions.Add(HideAnimation.Fade);
        }

        protected override void DisconnectFromController()
        {
            base.DisconnectFromController();
            if (!controller) return;

            controller.showReactions.Remove(ShowAnimation.Move);
            controller.showReactions.Remove(ShowAnimation.Rotate);
            controller.showReactions.Remove(ShowAnimation.Scale);
            controller.showReactions.Remove(ShowAnimation.Fade);

            controller.hideReactions.Remove(HideAnimation.Move);
            controller.hideReactions.Remove(HideAnimation.Rotate);
            controller.hideReactions.Remove(HideAnimation.Scale);
            controller.hideReactions.Remove(HideAnimation.Fade);
        }

        public override void Show() =>
            ShowAnimation?.Play(PlayDirection.Forward);

        public override void Hide() =>
            HideAnimation?.Play(PlayDirection.Forward);

        public override void InstantShow() =>
            ShowAnimation?.SetProgressAtOne();

        public override void InstantHide() =>
            HideAnimation?.SetProgressAtOne();

        public override void ReverseShow() =>
            ShowAnimation?.Reverse();

        public override void ReverseHide() =>
            HideAnimation?.Reverse();

        public override void UpdateSettings()
        {
            ShowAnimation?.SetTarget(rectTransform);
            HideAnimation?.SetTarget(rectTransform);
        }

        public override void StopAllReactions()
        {
            ShowAnimation?.Stop();
            HideAnimation?.Stop();
        }

        private static void ResetAnimation(UIAnimation target)
        {
            target.Move.Reset();
            target.Rotate.Reset();
            target.Scale.Reset();
            target.Fade.Reset();

            target.Move.fromReferenceValue = ReferenceValue.StartValue;
            target.Rotate.fromReferenceValue = ReferenceValue.StartValue;
            target.Scale.fromReferenceValue = ReferenceValue.StartValue;
            target.Fade.fromReferenceValue = ReferenceValue.StartValue;

            target.Move.settings.duration = UIContainer.k_DefaultAnimationDuration;
            target.Rotate.settings.duration = UIContainer.k_DefaultAnimationDuration;
            target.Scale.settings.duration = UIContainer.k_DefaultAnimationDuration;
            target.Fade.settings.duration = UIContainer.k_DefaultAnimationDuration;
        }
    }
}
