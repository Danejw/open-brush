// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections;
using Doozy.Runtime.Reactor.Animations;
using Doozy.Runtime.Reactor.Animators.Internal;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.Reactor.Animators
{
    /// <summary>
    /// Specialized animator component used to animate a RectTransform’s position, rotation, scale and alpha.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Doozy/Reactor/Animators/UI Animator")]
    public class UIAnimator : ReactorAnimator
    {
        private Canvas m_Canvas;
        /// <summary> Reference to the Canvas component </summary>
        public Canvas canvas => m_Canvas ? m_Canvas : m_Canvas = GetComponent<Canvas>();

        private CanvasGroup m_CanvasGroup;
        /// <summary> Reference to the CanvasGroup component </summary>
        public CanvasGroup canvasGroup => m_CanvasGroup ? m_CanvasGroup : m_CanvasGroup = GetComponent<CanvasGroup>();

        private GraphicRaycaster m_GraphicRaycaster;
        /// <summary> Reference to the GraphicRaycaster component </summary>
        public GraphicRaycaster graphicRaycaster => m_GraphicRaycaster ? m_GraphicRaycaster : m_GraphicRaycaster = GetComponent<GraphicRaycaster>();

        private RectTransform m_RectTransform;
        /// <summary> Reference to the RectTransform component </summary>
        public RectTransform rectTransform => m_RectTransform ? m_RectTransform : m_RectTransform = GetComponent<RectTransform>();

        [SerializeField] private UIAnimation Animation;
        public new UIAnimation animation => Animation ??= new UIAnimation(rectTransform, canvasGroup);

        public bool inLayoutGroup { get; private set; }

        protected override void Awake()
        {
            m_Canvas = GetComponent<Canvas>();
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
            m_RectTransform = GetComponent<RectTransform>();
            base.Awake();
        }


        protected override void OnEnable()
        {
            ValidateAnimation();
            LayoutGroupCheck();

            if (inLayoutGroup)
                return;
            
            UpdateValues();
            RunBehaviour(OnEnableBehaviour);
        }

        public override void Play(PlayDirection playDirection)
        {
            base.Play(playDirection);
            animation.Play(playDirection);
        }

        public override void Play(bool inReverse = false)
        {
            base.Play(inReverse);
            animation.Play(inReverse);
        }


        public override void SetTarget(object target) =>
            SetTarget(target as RectTransform);

        public void SetTarget(RectTransform targetRectTransform, CanvasGroup targetCanvasGroup = null) =>
            animation.SetTarget(targetRectTransform, targetCanvasGroup);

        public override void ResetToStartValues(bool forced = false) =>
            animation.ResetToStartValues(forced);

        public override void ValidateAnimation()
        {
            if (animation.rectTransform != null)
                return;
            SetTarget(rectTransform);
            UpdateValues();
        }

        public override void UpdateValues() =>
            animation.UpdateValues();

        public override void PlayToProgress(float toProgress) =>
            animation.PlayToProgress(toProgress);

        public override void PlayFromProgress(float fromProgress) =>
            animation.PlayFromProgress(fromProgress);

        public override void PlayFromToProgress(float fromProgress, float toProgress) =>
            animation.PlayFromToProgress(fromProgress, toProgress);

        public override void Stop() =>
            animation.Stop();

        public override void Finish() =>
            animation.Finish();

        public override void Reverse() =>
            animation.Reverse();

        public override void Rewind() =>
            animation.Rewind();

        public override void Pause() =>
            animation.Pause();

        public override void Resume() =>
            animation.Resume();

        public override void SetProgressAtOne() =>
            animation.SetProgressAtOne();

        public override void SetProgressAtZero() =>
            animation.SetProgressAtZero();

        public override void SetProgressAt(float targetProgress) =>
            animation.SetProgressAt(targetProgress);

        protected override void Recycle() =>
            animation?.Recycle();

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
            yield return null;
            yield return null;
            UpdateStartPosition();
            
            UpdateValues();
            RunBehaviour(OnEnableBehaviour);
        }

        private void UpdateStartPosition()
        {
            if (!inLayoutGroup) return;
            if (animation.Move == null) return;
            if (animation.Move.isActive) return;
            animation.startPosition = rectTransform.anchoredPosition3D;
            animation.UpdateValues();
        }
    }
}
