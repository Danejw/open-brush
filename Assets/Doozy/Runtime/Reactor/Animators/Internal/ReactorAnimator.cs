// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using UnityEngine;

namespace Doozy.Runtime.Reactor.Animators.Internal
{
    [Serializable]
    public abstract class ReactorAnimator : MonoBehaviour
    {
        /// <summary> Animator name </summary>
        public string AnimatorName;

        /// <summary> animator behaviour on Start </summary>
        public AnimatorBehaviour OnStartBehaviour = AnimatorBehaviour.Disabled;
        
        /// <summary> animator behaviour on Enable </summary>
        public AnimatorBehaviour OnEnableBehaviour = AnimatorBehaviour.Disabled;
        
        protected virtual void Awake()
        {
            ValidateAnimation();
        }

        protected virtual void OnEnable()
        {
            ValidateAnimation();
            UpdateValues();
            RunBehaviour(OnEnableBehaviour);
        }

        protected virtual void Start()
        {
            UpdateValues();
            RunBehaviour(OnStartBehaviour);
        }

        protected virtual void OnDestroy()
        {
            Recycle();
        }

        protected void RunBehaviour(AnimatorBehaviour behaviour)
        {
            switch (behaviour)
            {
                case AnimatorBehaviour.Disabled:
                    //ignored
                    return;
                
                case AnimatorBehaviour.PlayForward:
                    Play(PlayDirection.Forward);
                    return;
                
                case AnimatorBehaviour.PlayReverse:
                    Play(PlayDirection.Reverse);
                    return;
                
                case AnimatorBehaviour.SetFromValue:
                    SetProgressAtZero();
                    return;
                
                case AnimatorBehaviour.SetToValue:
                    SetProgressAtOne();
                    return;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void Play(bool inReverse = false) =>
            ValidateAnimation();

        public virtual void Play(PlayDirection playDirection) =>
            ValidateAnimation();

        public abstract void SetTarget(object target);
        public abstract void ResetToStartValues(bool forced = false);
        public abstract void ValidateAnimation();
        public abstract void UpdateValues();

        public abstract void PlayToProgress(float toProgress);
        public abstract void PlayFromProgress(float fromProgress);
        public abstract void PlayFromToProgress(float fromProgress, float toProgress);

        public abstract void Stop();
        public abstract void Finish();
        public abstract void Reverse();
        public abstract void Rewind();
        public abstract void Pause();
        public abstract void Resume();

        public abstract void SetProgressAtOne();
        public abstract void SetProgressAtZero();
        public abstract void SetProgressAt(float targetProgress);
        protected abstract void Recycle();
    }
}
