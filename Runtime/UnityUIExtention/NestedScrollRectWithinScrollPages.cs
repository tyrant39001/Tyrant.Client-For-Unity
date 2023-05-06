using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    [RequireComponent(typeof(ScrollRect))]
    public class NestedScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private bool isVertical = false;

        void Awake()
        {
            selfScrollRect = GetComponent<ScrollRect>();
        }

        private ScrollRect selfScrollRect = null;
        private ScrollRect scroll_Pages = null;
        private ScrollRect Scroll_Pages
        {
            get
            {
                if (scroll_Pages == null)
                {
                    if (transform.parent != null)
                        scroll_Pages = transform.parent.GetComponentInParent<ScrollRect>();
                }
                return scroll_Pages;
            }
        }

        //[SerializeField]
        //private float horizontalClamp = 0.1f;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            isVertical = Mathf.Abs(eventData.delta.y) >= Mathf.Abs(eventData.delta.x);
            if (selfScrollRect != null)
                selfScrollRect.enabled = isVertical;
            if (Scroll_Pages != null)
            {
                foreach (var com in Scroll_Pages.GetComponents<Component>())
                {
                    var beginDragHandler = com as IBeginDragHandler;
                    if (beginDragHandler != null)
                        beginDragHandler.OnBeginDrag(eventData);
                }
            }
        }

        //private bool isDraging = false;
        //private bool moveHorizontal = false;
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //if (!isDraging)
            //{
            //    moveHorizontal = Mathf.Abs(eventData.delta.y) <= horizontalClamp;
            //    if (selfScrollRect != null)
            //        selfScrollRect.enabled = !moveHorizontal;
            //    isDraging = true;
            //}
            if (Scroll_Pages != null && !isVertical)
            {
                foreach (var com in Scroll_Pages.GetComponents<Component>())
                {
                    var dragHandler = com as IDragHandler;
                    if (dragHandler != null)
                        dragHandler.OnDrag(eventData);
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //isDraging = false;
            if (selfScrollRect != null)
                selfScrollRect.enabled = true;
            if (Scroll_Pages != null)
            {
                foreach (var com in Scroll_Pages.GetComponents<Component>())
                {
                    var endDragHandler = com as IEndDragHandler;
                    if (endDragHandler != null)
                        endDragHandler.OnEndDrag(eventData);
                }
            }
        }
    }
}