using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    [RequireComponent(typeof(Image))]
    public class ClickToDeactiveSelf : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }

    [RequireComponent(typeof(Image))]
    public class ClickDoNothing : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
        }
    }

    public class ClickToDeactiveObj : MonoBehaviour, IPointerClickHandler
    {
        public GameObject ObjToClose = null;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (ObjToClose != null)
                ObjToClose.SetActive(false);
        }
    }
}
