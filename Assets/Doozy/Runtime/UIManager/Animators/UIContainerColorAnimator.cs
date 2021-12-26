// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Reactor;
using Doozy.Runtime.Reactor.Animations;
using Doozy.Runtime.Reactor.Targets;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;

namespace Doozy.Runtime.UIManager.Animators
{
    /// <summary>
    /// Specialized animator component used to animate the Color for a Reactor Color Target by listening to a UIContainer (controller) show/hide commands.
    /// </summary>
    [AddComponentMenu("Doozy/UI/Animators/Container/UI Container Color Animator")]
    public class UIContainerColorAnimator: BaseUIContainerAnimator
    {
        [SerializeField] private ReactorColorTarget ColorTarget;
        /// <summary> Reference to a color target component </summary>
        public ReactorColorTarget colorTarget => ColorTarget;
        
        /// <summary> Check if a color target is referenced or not </summary>
        public bool hasColorTarget => ColorTarget != null;
        
        [SerializeField] private ColorAnimation ShowAnimation;
        /// <summary> Container Show Animation </summary>
        public ColorAnimation showAnimation => ShowAnimation;

        [SerializeField] private ColorAnimation HideAnimation;
        /// <summary> Container Hide Animation </summary>
        public ColorAnimation hideAnimation => HideAnimation;
        
        #if UNITY_EDITOR
        protected override void Reset()
        {
            FindTarget();

            ShowAnimation ??= new ColorAnimation(colorTarget);
            HideAnimation ??= new ColorAnimation(colorTarget);

            ResetAnimation(ShowAnimation);
            ResetAnimation(HideAnimation);

            base.Reset();
        }
        #endif
        
        public void FindTarget()
        {
            if (ColorTarget != null)
                return;
            
            ColorTarget = ReactorColorTarget.FindTarget(gameObject);
            UpdateSettings();
        }
        
        protected override void Awake()
        {
            FindTarget();
            UpdateSettings();
            base.Awake();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ShowAnimation?.Recycle();
            HideAnimation?.Recycle();
        }
        
        protected override void ConnectToController()
        {
            base.ConnectToController();
            if (!controller) return;
            
            controller.showReactions.Add(ShowAnimation.animation);
            controller.hideReactions.Add(HideAnimation.animation);
        }

        protected override void DisconnectFromController()
        {
            base.DisconnectFromController();
            if (!controller) return;
            
            controller.showReactions.Remove(ShowAnimation.animation);
            controller.hideReactions.Remove(HideAnimation.animation);
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
            if(colorTarget == null)
                return;
            
            ShowAnimation?.SetTarget(colorTarget);
            HideAnimation?.SetTarget(colorTarget);
        }
        
        public override void StopAllReactions()
        {
            ShowAnimation?.Stop();
            HideAnimation?.Stop();
        }

        private static void ResetAnimation(ColorAnimation target)
        {
            target.animation.Reset();
            target.animation.enabled = true;
            target.animation.fromReferenceValue = ReferenceValue.CurrentValue;
            target.animation.settings.duration = UIContainer.k_DefaultAnimationDuration;
        }
    }
}
