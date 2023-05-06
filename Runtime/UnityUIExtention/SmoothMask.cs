using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    public class SmoothMask : MonoBehaviour, IMaterialModifier
    {
        [SerializeField]
        private List<Graphic> graphicContents = new List<Graphic>();

        private Material smoothMaskMaterial = null;
        private Material SmoothMaskMaterial
        {
            get
            {
                if (smoothMaskMaterial == null)
                    smoothMaskMaterial = new Material(Shader.Find("Hidden/SmoothMask"));
                return smoothMaskMaterial;
            }
        }

        void OnEnable()
        {
            OnValidate();
        }

        public void AddContent(Graphic graphicContent)
        {
            graphicContents.Add(graphicContent);
            UpdateGraphic(graphicContent);
        }

        public void RemoveContent(Graphic graphicContent)
        {
            if (graphicContent != null)
                graphicContent.material = null;
            graphicContents.Remove(graphicContent);
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            UpdateGraphicContents(false);
            return baseMaterial;
        }

        void OnValidate()
        {
            UpdateGraphicContents();
        }

        void UpdateGraphicContents(bool setMaterial = true)
        {
            foreach (var graphic in graphicContents)
                UpdateGraphic(graphic, setMaterial);
        }

        private void UpdateGraphic(Graphic graphicContent, bool setMaterial = true)
        {
            if (graphicContent == null)
                return;
            var maskImage = GetComponent<Image>();
            if (maskImage != null && maskImage.sprite != null && SmoothMaskMaterial.HasProperty("_Mask"))
                SmoothMaskMaterial.SetTexture("_Mask", maskImage.sprite.texture);
            if (setMaterial)
                graphicContent.material = SmoothMaskMaterial;
        }
    }
}