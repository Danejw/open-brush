// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Events;
using Doozy.Runtime.Mody;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components.Internal;
using Doozy.Runtime.UIManager.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.UIManager.Components
{
    /// <summary>
    /// Toggle component based on UISelectable with category/name id identifier.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Doozy/UI/Components/UI Toggle")]
    [SelectionBase]
    public partial class UIToggle : UISelectableComponent<UIToggle>, IPointerClickHandler, ISubmitHandler
    {
        private static SignalStream s_stream;
        /// <summary> Signal stream for this component type </summary>
        public static SignalStream stream => s_stream ?? (s_stream = SignalsService.GetStream(k_StreamCategory, nameof(UIToggle)));

        /// <summary> All buttons that are active and enabled </summary>
        public static IEnumerable<UIToggle> availableToggles => database.Where(item => item.isActiveAndEnabled);

        public override SelectableType selectableType => SelectableType.Toggle;

        /// <summary> Category Name Id </summary>
        public UIToggleId Id;

        public override bool isOn
        {
            get => IsOn;
            set
            {
                bool previousValue = IsOn;
                IsOn = value;

                if (inToggleGroup)
                {
                    toggleGroup.ToggleChangedValue(toggle: this, animateChange: true);
                    return;
                }

                ValueChanged(previousValue: previousValue, newValue: value, animateChange: true);
            }
        }

        /// <summary> Returns TRUE if this toggle has a toggle group reference </summary>
        public bool inToggleGroup => ToggleGroup != null && ToggleGroup.toggles.Contains(this);

        [SerializeField] private UIToggleGroup ToggleGroup;
        /// <summary> Reference to the toggle group that this toggle belongs to </summary>
        public UIToggleGroup toggleGroup
        {
            get => ToggleGroup;
            internal set => ToggleGroup = value;
        }

        /// <summary> Toggle was clicked - executed when a toggle interaction happened </summary>
        public ModyEvent OnClickCallback;

        /// <summary> Toggle became ON - executed when isOn becomes TRUE </summary>
        public ModyEvent OnToggleOnCallback;

        /// <summary> Toggle became OFF - executed when isOn becomes FALSE </summary>
        public ModyEvent OnToggleOffCallback;

        /// <summary> Toggle changed its value - executed when isOn changes its value </summary>
        public BoolEvent OnValueChangedCallback;

        /// <summary> Toggle value changed callback. This special callback also sends when the event happened, the previousValue and the newValue </summary>
        public UnityAction<ToggleValueChangedEvent> onToggleValueChangedCallback { get; set; }

        protected UIToggle()
        {
            Id = new UIToggleId();
            OnClickCallback = new ModyEvent(nameof(OnClickCallback));
            OnToggleOnCallback = new ModyEvent(nameof(OnToggleOnCallback));
            OnToggleOffCallback = new ModyEvent(nameof(OnToggleOffCallback));
            OnValueChangedCallback = new BoolEvent();
        }

        protected override void Awake()
        {
            base.Awake();

            behaviours.SetSignalSource(gameObject);
        }

        protected override void Start()
        {
            base.Start();
            AddToToggleGroup(toggleGroup);
            if (inToggleGroup) return;
            this.SetIsOn(isOn, false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            behaviours?.Connect();
            ValueChanged(isOn, isOn, false);
            StartCoroutine(VisualUpdate());
        }

        private IEnumerator VisualUpdate()
        {
            yield return null;
            yield return null;
            RefreshState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            behaviours?.Disconnect();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ToggleValue();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            ToggleValue();
        }

        protected virtual void ToggleValue()
        {
            if (!IsActive() || !IsInteractable())
                return;

            isOn = !isOn;
            stream.SendSignal(new UIToggleSignalData(Id.Category, Id.Name, isOn ? CommandToggle.On : CommandToggle.Off, playerIndex, this));
        }

        public void AddToToggleGroup(UIToggleGroup targetToggleGroup)
        {
            if (targetToggleGroup == null) return;

            if (inToggleGroup)
            {
                // if (toggleGroup == targetToggleGroup)
                //     return;
                RemoveFromToggleGroup();
            }

            targetToggleGroup.AddToggle(this);
        }

        public void RemoveFromToggleGroup()
        {
            if (toggleGroup == null) return;
            toggleGroup.RemoveToggle(this);
        }

        protected internal virtual void UpdateValueFromGroup(bool newValue, bool animateChange, bool silent = false)
        {
            bool previousValue = IsOn;
            IsOn = newValue;

            if (silent)
                return;

            ValueChanged(previousValue, newValue, animateChange);
        }

        internal void ValueChanged(bool previousValue, bool newValue, bool animateChange)
        {
            switch (newValue)
            {
                case true:
                    OnToggleOnCallback?.Execute();
                    break;

                case false:
                    OnToggleOffCallback?.Execute();
                    break;
            }

            OnValueChangedCallback?.Invoke(newValue);
            onToggleValueChangedCallback?.Invoke(new ToggleValueChangedEvent(previousValue, newValue, animateChange));
            RefreshState();
        }

        #region Static Methods

        /// <summary> Get all the registered toggles with the given category and name </summary>
        /// <param name="category"> UIToggle category </param>
        /// <param name="name"> UIToggle name (from the given category) </param>
        public static IEnumerable<UIToggle> GetToggles(string category, string name) =>
            database.Where(toggle => toggle.Id.Category.Equals(category)).Where(toggle => toggle.Id.Name.Equals(name));

        /// <summary> Get all the registered toggles with the given category </summary>
        /// <param name="category"> UIToggle category </param>
        public static IEnumerable<UIToggle> GetAllTogglesInCategory(string category) =>
            database.Where(toggle => toggle.Id.Category.Equals(category));

        /// <summary> Get all the toggles that are active and enabled (all the visible/available toggles) </summary>
        public static IEnumerable<UIToggle> GetAvailableToggles() =>
            database.Where(toggle => toggle.isActiveAndEnabled);

        /// <summary> Get the selected toggle (if a toggle is not selected, this method returns null) </summary>
        public static UIToggle GetSelectedToggle() =>
            database.FirstOrDefault(toggle => toggle.isSelected);

        /// <summary> Select the toggle with the given category and name (if it is active and enabled) </summary>
        /// <param name="category"> UIToggle category </param>
        /// <param name="name"> UIToggle name (from the given category) </param>
        public static bool SelectToggle(string category, string name)
        {
            UIToggle toggle = availableToggles.FirstOrDefault(b => b.Id.Category.Equals(category) & b.Id.Name.Equals(name));
            if (toggle == null) return false;
            toggle.Select();
            return true;
        }

        #endregion

    }

    public static class UIToggleExtensions
    {
        public static T SetIsOn<T>(this T target, bool newValue, bool animateChange = true) where T : UIToggle
        {
            bool previousValue = target.isOn;
            target.IsOn = newValue;
            if (target.inToggleGroup)
            {
                target.toggleGroup.ToggleChangedValue(target, animateChange);
                return target;
            }
            target.ValueChanged(previousValue, newValue, animateChange);
            return target;
        }
    }
}
