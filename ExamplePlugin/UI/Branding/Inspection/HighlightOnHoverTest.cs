using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Branding.Inspection
{
    internal class HighlightOnHoverTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image target;
        public void OnPointerEnter(PointerEventData e) { if (target) target.enabled = true; }
        public void OnPointerExit(PointerEventData e) { if (target) target.enabled = false; }

    }
}
