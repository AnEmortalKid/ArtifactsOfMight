using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace ExamplePlugin.UI.Branding.Inspection
{
    /// <summary>
    /// Testing handler click thing for tabs
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        readonly List<GameObject> tabs = new();

        public void Register(GameObject tab) => tabs.Add(tab);

        public void Select(GameObject tab)
        {
            EventSystem.current?.SetSelectedGameObject(tab);
            foreach (var t in tabs)
                if (t != tab)
                    ExecuteEvents.Execute<IDeselectHandler>(t, null, (h, _) => h.OnDeselect(null));
        }
    }
}
