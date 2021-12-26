// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Doozy.Runtime.UIManager.Components.Internal
{
    public abstract class UISelectableComponent<T> : UISelectable where T : UISelectable
    {
        public static HashSet<T> database { get; } = new HashSet<T>();

        public T component { get; private set; }

        [SerializeField] private UIBehaviours Behaviours;
        public UIBehaviours behaviours => Behaviours;

        public bool isSelected => EventSystem.current.currentSelectedGameObject == gameObject;

        protected UISelectableComponent()
        {
            Behaviours = new UIBehaviours();
        }

        protected override void Awake()
        {
            database.Add(component = GetComponent<T>());
            base.Awake();
        }

        protected override void OnEnable()
        {
            CleanDatabase();
            base.OnEnable();
        }

        protected override void OnDestroy()
        {
            database.Remove(component);
            CleanDatabase();
            base.OnDestroy();
        }

        protected static void CleanDatabase() =>
            database.Remove(null);
    }
}
