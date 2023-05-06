using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    public class ScrollPages : UIBehaviour, IBeginDragHandler, IEndDragHandler, ICanvasElement
    {
        private enum DragState
        {
            HasNotDrag, //从来没拖动过
            BeginDrag,
            EndDrag,
        }

        [SerializeField][Tooltip("要附加滑页功能的ScrollRect组件，None为在当前GameObject及其父GameObject上查找。只能在编辑器中设置")]
        private ScrollRect scrollRect = null;
        /// <summary>
        /// 滑动速度，越大越快
        /// </summary>
        [Tooltip("滑动速度，越大越快")]
        public float smooting = 5;
        [SerializeField][Tooltip("分页的数量，只能在编辑器中设置。若要在运行时更改，请设置属性PageCount的值")]
        private uint pageCount;
        [Tooltip("初始页索引，从0开始，必须小于pageCount")]
        public uint initPageIndex = 0;
        /// <summary>
        /// 滑动速度是否受<seealso cref="Time.timeScale"/>的影响
        /// </summary>
        [Tooltip("滑动速度是否受Time.timeScale的影响")]
        public bool useScaleTime = false;
        [SerializeField][Tooltip("true表示将scrollRect的Movement Type属性设置为Unrestricted，false表示保留原有设置")]
        private bool unrestrictedMovementType = false;

        /// <summary>
        /// 当前页索引变化时的事件
        /// </summary>
        public event Action<int> CurrentPageIndexChanged;
        /// <summary>
        /// 当页滑动到当前页的位置时的事件
        /// </summary>
        public event Action<int> ScrollFinfished;
        /// <summary>
        /// 当页数量变化时的事件
        /// </summary>
        public event Action PageCountChanged;

        private DragState dragState = DragState.HasNotDrag;
        private List<float> listPageValue = new List<float>();
        private float targetPos = 0;
        private bool hasInvokeScrollFinfished = false;

        private bool controlNormalizedPosition = false;  // 控制NormalizedPosition还是控制Content的位置

        private bool pageCountChanged = true;
        /// <summary>
        /// 获取或设置分页的数量。注意，先填充好ScrollRect的滚动内容再设置此属性。
        /// </summary>
        public uint PageCount
        {
            get { return pageCount; }
            set
            {
                if (pageCount == value)
                    return;
                //if (value == 0)
                //    throw new ArgumentException("PageCount can not be zero", nameof(PageCount));
                pageCount = value;
                pageCountChanged = true;
                if (gameObject.activeInHierarchy)
                    CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            }
        }

        private int currentPageIndex;
        /// <summary>
        /// 获取或设置当前页的索引。此方式从当前页过度到设置的索引。如果需要立刻改变请调用
        /// </summary>
        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
            set
            {
                if (currentPageIndex == value)
                    return;
                currentPageIndex = value;
                if (value < 0 || value >= listPageValue.Count)
                    return;
                dragState = DragState.EndDrag;
                targetPos = listPageValue[value];
                hasInvokeScrollFinfished = false;
                try
                {
                    CurrentPageIndexChanged?.Invoke(value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void SetCurrentPageIndexImmediate(uint pageIndex)
        {
            if (currentPageIndex == pageIndex)
                return;
            currentPageIndex = (int)pageIndex;
            SetCurrentPageIndexImmediate();
        }

        private void SetCurrentPageIndexImmediate()
        {
            if (currentPageIndex < 0 || currentPageIndex >= listPageValue.Count)
                return;
            targetPos = listPageValue[currentPageIndex];
            if (scrollRect != null)
                scrollRect.horizontalNormalizedPosition = targetPos;
            try
            {
                CurrentPageIndexChanged?.Invoke(currentPageIndex);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void UpdateListPageValue()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (scrollRect == null || pageCount == 0 || !pageCountChanged)
                return;

            listPageValue.Clear();
            if (pageCount != 0)
            {
                var contentWidth = scrollRect.content.rect.width;
                var viewportWidth = scrollRect.viewport.rect.width;
                var pageWidth = contentWidth / pageCount;
                var baseWidth = contentWidth - viewportWidth;
                if (baseWidth > 0)
                {
                    controlNormalizedPosition = true;
                    for (float i = 0; i < pageCount; i++)
                        listPageValue.Add((i * pageWidth + pageWidth / 2 - viewportWidth / 2) / baseWidth);
                }
                else // 内容比视口小，normalizedPosition不是0就是1，需要控制content的位置
                {
                    controlNormalizedPosition = false;
                    scrollRect.movementType = ScrollRect.MovementType.Unrestricted; // 强制关闭限制以控制content
                    scrollRect.horizontalNormalizedPosition = 0.0f; // 这会使content移动到最左侧，方便计算相对位置
                    var pos0 = scrollRect.content.localPosition.x + (viewportWidth - pageWidth) / 2;
                    listPageValue.Add(pos0);
                    for (float i = 1; i < pageCount; i++)
                        listPageValue.Add(pos0 - i * pageWidth);
                }
            }
            else
                targetPos = 0.0f;

            if (listPageValue.Count > 0)
            {
                if (currentPageIndex == 0) //没有设置过当前页就以初始页作为当前页
                    currentPageIndex = (int)initPageIndex;
                if (currentPageIndex >= listPageValue.Count)
                    currentPageIndex = listPageValue.Count - 1;
                SetCurrentPageIndexImmediate();
            }

            try
            {
                PageCountChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 

            pageCountChanged = false;
        }

        void Update()
        {
            if (scrollRect != null)
            {
                switch (dragState)
                {
                    case DragState.HasNotDrag:
                        if (controlNormalizedPosition)
                            scrollRect.horizontalNormalizedPosition = targetPos;
                        else
                        {
                            var localPosition = scrollRect.content.localPosition;
                            localPosition.x = targetPos;
                            scrollRect.content.localPosition = localPosition;
                        }
                        break;
                    //case DragState.BeginDrag:
                    //    break;
                    case DragState.EndDrag:
                        if (controlNormalizedPosition)
                        {
                            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetPos, (useScaleTime ? Time.deltaTime : Time.unscaledDeltaTime) * smooting);
                        }
                        else
                        {
                            var localPositionX = Mathf.Lerp(scrollRect.content.localPosition.x, targetPos, (useScaleTime ? Time.deltaTime : Time.unscaledDeltaTime) * smooting);
                            var localPosition = scrollRect.content.localPosition;
                            localPosition.x = localPositionX;
                            scrollRect.content.localPosition = localPosition;
                        }
                        if (!hasInvokeScrollFinfished && Math.Abs(scrollRect.horizontalNormalizedPosition - targetPos) <= 1E-3)
                        {
                            hasInvokeScrollFinfished = true;
                            try
                            {
                                ScrollFinfished?.Invoke(CurrentPageIndex);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 拖动开始
        /// </summary>
        /// <param name="eventData"></param>
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragState = DragState.BeginDrag;
        }

        /// <summary>
        /// 拖拽结束
        /// </summary>
        /// <param name="eventData"></param>
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (scrollRect == null)
                return;

            dragState = DragState.EndDrag;
            var pos = controlNormalizedPosition ? scrollRect.horizontalNormalizedPosition : scrollRect.content.localPosition.x; //获取拖动的值
            var lastDis = float.MaxValue;
            var currentPageIndex = 0;
            for (int i = 0; i < listPageValue.Count; i++) // 找出拖动的位置和页的距离最小的页
            {              
                var dis = Mathf.Abs(listPageValue[i] - pos);
                if (dis < lastDis)
                    lastDis = dis;
                else
                    break;
                currentPageIndex = i;
            }

            CurrentPageIndex = currentPageIndex;
        }

        public void MovePrevious()
        {
            if (scrollRect == null)
                return;
            if (CurrentPageIndex <= 0)
                return;
            --CurrentPageIndex;
        }

        public void MoveNext()
        {
            if (scrollRect == null)
                return;
            if (CurrentPageIndex >= listPageValue.Count - 1)
                return;
            ++CurrentPageIndex;
        }

        protected override void Start()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponentInParent<ScrollRect>();
                if (scrollRect != null && unrestrictedMovementType)
                    scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            }
        }

        protected override void OnEnable()
        {
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        void ICanvasElement.Rebuild(CanvasUpdate executing){}

        void ICanvasElement.LayoutComplete()
        {
            UpdateListPageValue();
        }

        void ICanvasElement.GraphicUpdateComplete(){}
    }
}