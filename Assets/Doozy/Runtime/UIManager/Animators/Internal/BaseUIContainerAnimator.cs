// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.UIManager.Containers;
// ReSharper disable MemberCanBeProtected.Global

namespace Doozy.Runtime.UIManager.Animators
{
    public abstract class BaseUIContainerAnimator : BaseTargetComponentAnimator<UIContainer>
    {
        protected override void ConnectToController()
        {
            if (controller == null)
                return;

            controller.showHideExecute += Execute;
            if(controller.executedFirstCommand)
                Execute(controller.previouslyExecutedCommand);
        }

        protected override void DisconnectFromController()
        {
            if (controller == null)
                return;

            controller.showHideExecute -= Execute;
        }

        protected virtual void Execute(ShowHideExecute execute)
        {
            switch (execute)
            {
                case ShowHideExecute.Show:
                    Show();
                    return;

                case ShowHideExecute.Hide:
                    Hide();
                    return;

                case ShowHideExecute.InstantShow:
                    InstantShow();
                    return;

                case ShowHideExecute.InstantHide:
                    InstantHide();
                    return;

                case ShowHideExecute.ReverseShow:
                    ReverseShow();
                    return;

                case ShowHideExecute.ReverseHide:
                    ReverseHide();
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(execute), execute, null);
            }
        }

        public abstract void Show();
        public abstract void ReverseShow();

        public abstract void Hide();
        public abstract void ReverseHide();

        public abstract void InstantShow();
        public abstract void InstantHide();

    }
}
