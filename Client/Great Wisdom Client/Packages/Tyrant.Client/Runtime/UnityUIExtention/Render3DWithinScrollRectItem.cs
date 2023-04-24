using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    public class Render3DWithinScrollRectItem : UIBehaviour, ICanvasElement
    {
        [SerializeField]
        private ScrollRect scrollRect = null;
        [SerializeField]
        private float viewLength = 1000;
        [SerializeField]
        private float viewDepth = 10;
        [SerializeField]
        private LayerMask layerMask;

        protected override void Awake()
        {
            base.Awake();

            if (scrollRect == null)
                scrollRect = GetComponentInParent<ScrollRect>();
            CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(this);
        }

        void ICanvasElement.GraphicUpdateComplete() { }

        void ICanvasElement.LayoutComplete()
        {
            //var worldCamera = GetComponentInParent<Canvas>().worldCamera;
            //if (worldCamera == null)
            //    worldCamera = Camera.main;
            //if (worldCamera == null)
            //    return;

            var corners = new Vector3[4];
            scrollRect.viewport.GetWorldCorners(corners);
            //var screenP0 = worldCamera.WorldToScreenPoint(corners[0]);
            //var screenP2 = worldCamera.WorldToScreenPoint(corners[2]);

            var cameraGameObject = new GameObject("Camera created by Render3DWithinScrollRectItem");
            cameraGameObject.transform.SetParent(scrollRect.viewport, false);
            var camera = cameraGameObject.AddComponent<Camera>();
            var pivotWidth = (scrollRect.viewport.pivot.x) * scrollRect.viewport.rect.width;
            var pivotHeight = (scrollRect.viewport.pivot.y) * scrollRect.viewport.rect.height;
            camera.transform.localPosition = new Vector3(scrollRect.viewport.rect.width * 0.5f - pivotWidth, scrollRect.viewport.rect.height * 0.5f - pivotHeight, -viewLength);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.depth = viewDepth;
            camera.orthographic = true;
            camera.farClipPlane = viewLength * 2;
            camera.cullingMask = layerMask.value;
            camera.orthographicSize = (corners[2].y - corners[0].y) / 2;

            RenderTexture rt = new RenderTexture((int)scrollRect.viewport.rect.width, (int)scrollRect.viewport.rect.height, 24);
            rt.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            camera.targetTexture = rt;

            var rawImageGameObject = new GameObject("RawImage created by Render3DWithinScrollRectItem");
            rawImageGameObject.transform.SetParent(scrollRect.viewport, false);
            var rawImage = rawImageGameObject.AddComponent<RawImage>();
            rawImage.texture = rt;
            rawImage.raycastTarget = false;
            var rawImageRectTransform = rawImage.GetComponent<RectTransform>();
            rawImageRectTransform.anchorMin = Vector2.zero;
            rawImageRectTransform.anchorMax = Vector2.one;
            rawImageRectTransform.offsetMin = Vector2.zero;
            rawImageRectTransform.offsetMax = Vector2.zero;
        }

        void ICanvasElement.Rebuild(CanvasUpdate executing) { }
    }
}