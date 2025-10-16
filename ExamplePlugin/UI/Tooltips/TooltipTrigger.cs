using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ExamplePlugin.UI.Tooltips
{
    // Possible to use event trigger  and pointer move handler
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public TooltipData Data;
        public RectTransform Anchor;

        public void OnPointerEnter(PointerEventData e)
        {
            Log.Info("PointerEnter");
            TooltipSystem.Instance.QueueShow(Data, Anchor, snapToAnchor: false);
        }

        public void OnPointerExit(PointerEventData e)
        {
            Log.Info("PointerExit");
            TooltipSystem.Instance.CancelShow();
        }

        public void OnPointerMove(PointerEventData e)
        {
            TooltipSystem.Instance.Move(e.position);
        }
    }
}
