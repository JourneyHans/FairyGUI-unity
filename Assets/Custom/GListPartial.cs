using System;
using UnityEngine;

namespace FairyGUI {
    public partial class GList {
        public int FirstIndex => _firstIndex;

        /// <summary>
        /// 自定义扩展，滚动到让目标显示在中心位`置
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ani"></param>
        public void ScrollToMiddleView(int index, bool ani) {
            if (_virtual) {
                if (_numItems == 0)
                    return;

                CheckVirtualList();

                if (index >= _virtualItems.Count)
                    throw new Exception("Invalid child index: " + index + ">" + _virtualItems.Count);

                if (_loop)
                    index = Mathf.FloorToInt((float)_firstIndex / _numItems) * _numItems + index;

                Rect rect;
                ItemInfo ii = _virtualItems[index];
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal) {
                    float pos = 0;
                    for (int i = _curLineItemCount - 1; i < index; i += _curLineItemCount)
                        pos += _virtualItems[i].size.y + _lineGap;
                    // TODO: 目前只有竖向的改为滑动到中间，其他方法还未修改
                    rect = new Rect(0, pos - (viewHeight - ii.size.y) * 0.5f, _itemSize.x, ii.size.y);
                }
                else if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowVertical) {
                    float pos = 0;
                    for (int i = _curLineItemCount - 1; i < index; i += _curLineItemCount)
                        pos += _virtualItems[i].size.x + _columnGap;
                    rect = new Rect(pos, 0, ii.size.x, _itemSize.y);
                }
                else {
                    int page = index / (_curLineItemCount * _curLineItemCount2);
                    rect = new Rect(page * viewWidth + (index % _curLineItemCount) * (ii.size.x + _columnGap),
                        (index / _curLineItemCount) % _curLineItemCount2 * (ii.size.y + _lineGap),
                        ii.size.x, ii.size.y);
                }

                if (this.scrollPane != null)
                    scrollPane.ScrollToView(rect, ani, true);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(this.TransformRect(rect, parent), ani, true);
            }
            else {
                GObject obj = GetChildAt(index);
                if (this.scrollPane != null)
                    scrollPane.ScrollToView(obj, ani, true);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(obj, ani, true);
            }
        }

        /// <summary>
        /// Set the list item count. 
        /// If the list is not virtual, specified number of items will be created. 
        /// If the list is virtual, only items in view will be created.
        /// </summary>
        public int numItems {
            get {
                if (_virtual)
                    return _numItems;
                else
                    return _children.Count;
            }
            set {
                if (_virtual) {
                    if (itemRenderer == null)
                        throw new Exception("FairyGUI: Set itemRenderer first!");

                    _numItems = value;
                    if (_loop)
                        _realNumItems = _numItems * 6;//设置6倍数量，用于循环滚动
                    else
                        _realNumItems = _numItems;

                    //_virtualItems的设计是只增不减的
                    int oldCount = _virtualItems.Count;
                    if (_realNumItems > oldCount) {
                        for (int i = oldCount; i < _realNumItems; i++) {
                            ItemInfo ii = new ItemInfo();
                            if (itemProvider != null) {
                                //如果有itemProvider的话，初始化VirtualItem使用ItemProvider返回的组件大小
                                var obj = GetFromPool(itemProvider.Invoke(i));
                                ii.size = obj.size;
                                RemoveChildToPool(obj);//因为GetFromPoll会创建实例，所以需要归还到对象池子里
                            }
                            else {
                                ii.size = _itemSize;
                            }
                            //ii.size = _itemSize;

                            _virtualItems.Add(ii);
                        }
                    }
                    else {
                        for (int i = _realNumItems; i < oldCount; i++)
                            _virtualItems[i].selected = false;
                    }

                    if (_virtualListChanged != 0)
                        Timers.inst.Remove(this.RefreshVirtualList);
                    //立即刷新
                    this.RefreshVirtualList(null);
                }
                else {
                    int cnt = _children.Count;
                    if (value > cnt) {
                        for (int i = cnt; i < value; i++) {
                            if (itemProvider == null)
                                AddItemFromPool();
                            else
                                AddItemFromPool(itemProvider(i));
                        }
                    }
                    else {
                        RemoveChildrenToPool(value, cnt);
                    }

                    if (itemRenderer != null) {
                        for (int i = 0; i < value; i++)
                            itemRenderer(i, GetChildAt(i));
                    }
                }
            }
        }

        public void ScrollRight(int scrollCount, bool anim) {
            var index = _firstIndex;
            scrollPane.ScrollRight(scrollCount, anim);

        }

        public void ScrollLeft(int scrollCount, bool anim) {
            scrollPane.ScrollLeft(scrollCount, anim);

        }
    }
}