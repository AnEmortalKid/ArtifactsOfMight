using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Branding.Inspection
{
    public class ShowBorderOnSelected :
        MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerDownHandler
    {
        public Image border;
        public Image background;
        public Color selectedBg = new(0.85f, 0.85f, 0.85f, 1f);
        public Color normalBg = Color.white;
        public Image[] extrasOnSel;

        void Awake()
        {
            if (border) border.enabled = false;
            if (background) background.color = normalBg;
        }

        public void OnSelect(BaseEventData e)
        {
            if (border) border.enabled = true;
            if (background) background.color = selectedBg;
            foreach(Image item in extrasOnSel)
            {
                item.enabled = true;
            }
        }

        public void OnDeselect(BaseEventData e)
        {
            if (border) border.enabled = false;
            if (background) background.color = normalBg;
            foreach (Image item in extrasOnSel)
            {
                item.enabled = false;
            }
        }

        // ensures mouse clicks actually trigger selection events
        public void OnPointerDown(PointerEventData e)
        {
            EventSystem.current?.SetSelectedGameObject(gameObject);
        }
    }
}
