using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    [RequireComponent(typeof(ScrollPages))]
    public class PageIndicatorWithScrollPages : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("子GameObjects会在初始化过程中被销毁。")]
        private Transform TogglesRoot = null;
        [SerializeField]
        private GameObject TogglePrefab = null;

        private ScrollPages scrollPages = null;
        private ToggleGroup toggleGroup = null;
        private List<Toggle> toggleList = new List<Toggle>();

        void Awake()
        {
            scrollPages = GetComponent< ScrollPages>();
            if (scrollPages != null)
            {
                scrollPages.PageCountChanged += ScrollPages_PageCountChanged;
                scrollPages.CurrentPageIndexChanged += ScrollPages_CurrentPageIndexChanged;
            }
            toggleGroup = gameObject.AddComponent<ToggleGroup>();
        }

        void OnDestroy()
        {
            toggleList.Clear();
            if (scrollPages != null)
            {
                scrollPages.PageCountChanged -= ScrollPages_PageCountChanged;
                scrollPages.CurrentPageIndexChanged -= ScrollPages_CurrentPageIndexChanged;
            }
        }

        private void ScrollPages_PageCountChanged()
        {
            if (TogglesRoot != null)
            {
                foreach (Transform child in TogglesRoot)
                    Destroy(child.gameObject);
                if (scrollPages != null && TogglePrefab != null)
                {
                    toggleList.Clear();
                    for (int i = 0; i < scrollPages.PageCount; ++i)
                    {
                        var toggleObj = Instantiate(TogglePrefab);
                        toggleObj.transform.SetParent(TogglesRoot, false);
                        var toggle = toggleObj.GetComponentInChildren<Toggle>();
                        if (toggle != null)
                        {
                            toggleList.Add(toggle);
                            toggle.group = toggleGroup;
                        }
                    }
                    ScrollPages_CurrentPageIndexChanged(scrollPages.CurrentPageIndex);
                }
                
            }
        }

        private void ScrollPages_CurrentPageIndexChanged(int obj)
        {
            if (obj < toggleList.Count)
                toggleList[obj].isOn = true;
        }
    }
}