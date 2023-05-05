using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace Tyrant.ComponentsForUnity
{
    /// <summary>
    /// 一个开关，点击激活或关闭目标。例如点击一个按钮显示或隐藏指定的UI面板
    /// </summary>
    public class PannelToggle : MonoBehaviour, IPointerClickHandler
    {
        public ToggleType ToggleType = ToggleType.AutoActiveOrDeactive;
        public GameObject Target;
        public bool UseTargetActiveState = true;
        private bool internalActiveState = false;

        void Start()
        {
            internalActiveState = Target.activeSelf;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (Target == null)
                return;
            switch (ToggleType)
            {
                case ToggleType.AutoActiveOrDeactive:
                    if (UseTargetActiveState)
                        Target.SetActive(!Target.activeSelf);
                    else
                    {
                        internalActiveState = !internalActiveState;
                        Target.SetActive(internalActiveState);
                    }
                    break;
                case ToggleType.AlwaysActive:
                    if (!Target.activeSelf)
                        Target.SetActive(true);
                    break;
                case ToggleType.AlwaysDeactive:
                    if (Target.activeSelf)
                        Target.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 开关类型，只用于<see cref="PannelToggle"/>
    /// </summary>
    public enum ToggleType
    {
        /// <summary>
        /// 激活或关闭
        /// </summary>
        AutoActiveOrDeactive = 0,
        /// <summary>
        /// 总是激活
        /// </summary>
        AlwaysActive = 1,
        /// <summary>
        /// 总是关闭
        /// </summary>
        AlwaysDeactive = 2,
    }
}