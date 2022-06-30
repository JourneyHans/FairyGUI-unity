using UnityEngine;

namespace FairyGUI {
    public partial class ScrollPane {
        private static float[] _bounceBackTweenTime = {0.4f, 0.1f}; // 回弹效果时间，目前不开放这个效果的设置

        // 下面两个变量存粹是为了覆盖官方变量
        public float TWEEN_TIME_GO => TweenTimeSet;
        public float TWEEN_TIME_DEFAULT => TweenTimeSet;

        public float TweenTimeSet {
            get {
                if (pageMode) {
                    return UIFeelConfig.PagingDuration; // 针对翻页惯性滚动时间，SetPos(ani)也使用同样的时间
                }

                return 0.3f;
            }
        }

        public int DampingRate {
            set => _dampingRate = value;
            get {
                if (_dampingRate == null) {
                    return UIFeelConfig.ScrollDampingRate;
                }

                return _dampingRate.Value;
            }
        }

        private int? _dampingRate;

        #region 针对嵌套滑动组件表现不太灵敏而做的修改

        private int? _forceSensitivityHorizontal;

        private int? _forceSensitivityVertical;

        public void SetForceSensitivity() {
            _forceSensitivityHorizontal = UIFeelConfig.HCustomScrollSensitive;
            _forceSensitivityVertical = UIFeelConfig.VCustomScrollSensitive;
        }

        private int SensitivityHorizontal {
            get {
                if (_forceSensitivityHorizontal != null) {
                    _forceSensitivityHorizontal = UIFeelConfig.HCustomScrollSensitive;
                    return _forceSensitivityHorizontal.Value;
                }

                return Stage.touchScreen ? UIConfig.touchScrollSensitivity : 8;
            }
        }

        private int SensitivityVertical {
            get {
                if (_forceSensitivityVertical != null) {
                    _forceSensitivityVertical = UIFeelConfig.VCustomScrollSensitive;
                    return _forceSensitivityVertical.Value;
                }

                return Stage.touchScreen ? UIConfig.touchScrollSensitivity : 8;
            }
        }

        #endregion

        public bool EnableScrollingClick { get; set; }

        private float _touchBeginTime;
        private bool _isQuickDrag;

        private void __touchBegin(EventContext context) {
            if (!_touchEffect)
                return;

            InputEvent evt = context.inputEvent;
            if (evt.button != 0)
                return;

            context.CaptureTouch();

            Vector2 pt = _owner.GlobalToLocal(evt.position);

            if (_tweening != 0) {
                KillTween();
                if (!pageMode && !EnableScrollingClick) {
                    // 不是PageView模式，且没有开启滚动点击，滚动过程中打断玩家的点击操作
                    Stage.inst.CancelClick(evt.touchId);
                }

                //立刻停止惯性滚动，可能位置不对齐，设定这个标志，使touchEnd时归位
                _dragged = true;
            }
            else
                _dragged = false;

            _containerPos = _container.xy;
            _beginTouchPos = _lastTouchPos = pt;
            _lastTouchGlobalPos = evt.position;
            _isHoldAreaDone = false;
            _velocity = Vector2.zero;
            _velocityScale = 1;
            _lastMoveTime = Time.unscaledTime;

            if (pageMode) {
                _touchBeginTime = Time.time;
            }
        }

        private void __touchMove(EventContext context) {
            if (!_touchEffect || draggingPane != null && draggingPane != this ||
                GObject.draggingObject != null) //已经有其他拖动
                return;

            InputEvent evt = context.inputEvent;
            Vector2 pt = _owner.GlobalToLocal(evt.position);
            if (float.IsNaN(pt.x))
                return;

            #region 修改了源码的部分

            /***************** 我们修改的部分 *****************/
            int sensitivityHorizontal = SensitivityHorizontal;
            int sensitivityVertical = SensitivityVertical;
            /***************** 我们修改的部分 *****************/

            #endregion

            float diff;
            bool sv = false, sh = false;

            if (_scrollType == ScrollType.Vertical) {
                if (!_isHoldAreaDone) {
                    //表示正在监测垂直方向的手势
                    _gestureFlag |= 1;

                    diff = Mathf.Abs(_beginTouchPos.y - pt.y);
                    if (diff < sensitivityVertical)
                        return;

                    if ((_gestureFlag & 2) != 0) //已经有水平方向的手势在监测，那么我们用严格的方式检查是不是按垂直方向移动，避免冲突
                    {
                        float diff2 = Mathf.Abs(_beginTouchPos.x - pt.x);
                        if (diff < diff2) //不通过则不允许滚动了
                            return;
                    }
                }

                sv = true;
            }
            else if (_scrollType == ScrollType.Horizontal) {
                if (!_isHoldAreaDone) {
                    _gestureFlag |= 2;

                    diff = Mathf.Abs(_beginTouchPos.x - pt.x);
                    if (diff < sensitivityHorizontal)
                        return;

                    if ((_gestureFlag & 1) != 0) {
                        float diff2 = Mathf.Abs(_beginTouchPos.y - pt.y);
                        if (diff < diff2)
                            return;
                    }
                }

                sh = true;
            }
            else {
                _gestureFlag = 3;

                if (!_isHoldAreaDone) {
                    diff = Mathf.Abs(_beginTouchPos.y - pt.y);
                    if (diff < sensitivityVertical) {
                        diff = Mathf.Abs(_beginTouchPos.x - pt.x);
                        if (diff < sensitivityHorizontal)
                            return;
                    }
                }

                sv = sh = true;
            }

            Vector2 newPos = _containerPos + pt - _beginTouchPos;
            newPos.x = (int) newPos.x;
            newPos.y = (int) newPos.y;

            if (sv) {
                if (newPos.y > 0) {
                    if (!_bouncebackEffect)
                        _container.y = 0;
                    else if (_header != null && _header.maxHeight != 0)
                        _container.y = (int) Mathf.Min(newPos.y * 0.5f, _header.maxHeight);
                    else
                        _container.y = (int) Mathf.Min(newPos.y * 0.5f, _viewSize.y * PULL_RATIO);
                }
                else if (newPos.y < -_overlapSize.y) {
                    if (!_bouncebackEffect)
                        _container.y = -_overlapSize.y;
                    else if (_footer != null && _footer.maxHeight > 0)
                        _container.y = (int) Mathf.Max((newPos.y + _overlapSize.y) * 0.5f, -_footer.maxHeight) -
                                       _overlapSize.y;
                    else
                        _container.y = (int) Mathf.Max((newPos.y + _overlapSize.y) * 0.5f, -_viewSize.y * PULL_RATIO) -
                                       _overlapSize.y;
                }
                else
                    _container.y = newPos.y;
            }

            if (sh) {
                if (newPos.x > 0) {
                    if (!_bouncebackEffect)
                        _container.x = 0;
                    else if (_header != null && _header.maxWidth != 0)
                        _container.x = (int) Mathf.Min(newPos.x * 0.5f, _header.maxWidth);
                    else
                        _container.x = (int) Mathf.Min(newPos.x * 0.5f, _viewSize.x * PULL_RATIO);
                }
                else if (newPos.x < 0 - _overlapSize.x) {
                    if (!_bouncebackEffect)
                        _container.x = -_overlapSize.x;
                    else if (_footer != null && _footer.maxWidth > 0)
                        _container.x = (int) Mathf.Max((newPos.x + _overlapSize.x) * 0.5f, -_footer.maxWidth) -
                                       _overlapSize.x;
                    else
                        _container.x = (int) Mathf.Max((newPos.x + _overlapSize.x) * 0.5f, -_viewSize.x * PULL_RATIO) -
                                       _overlapSize.x;
                }
                else
                    _container.x = newPos.x;
            }

            //更新速度
            float deltaTime = Time.unscaledDeltaTime;
            float elapsed = (Time.unscaledTime - _lastMoveTime) * 60 - 1;
            if (elapsed > 1) //速度衰减
                _velocity = _velocity * Mathf.Pow(0.833f, elapsed);
            Vector2 deltaPosition = pt - _lastTouchPos;
            if (!sh)
                deltaPosition.x = 0;
            if (!sv)
                deltaPosition.y = 0;
            _velocity = Vector2.Lerp(_velocity, deltaPosition / deltaTime, deltaTime * 10);

            /*速度计算使用的是本地位移，但在后续的惯性滚动判断中需要用到屏幕位移，所以这里要记录一个位移的比例。
             *后续的处理要使用这个比例但不使用坐标转换的方法的原因是，在曲面UI等异形UI中，还无法简单地进行屏幕坐标和本地坐标的转换。
             */
            Vector2 deltaGlobalPosition = _lastTouchGlobalPos - evt.position;
            if (deltaPosition.x != 0)
                _velocityScale = Mathf.Abs(deltaGlobalPosition.x / deltaPosition.x);
            else if (deltaPosition.y != 0)
                _velocityScale = Mathf.Abs(deltaGlobalPosition.y / deltaPosition.y);

            _lastTouchPos = pt;
            _lastTouchGlobalPos = evt.position;
            _lastMoveTime = Time.unscaledTime;

            //同步更新pos值
            if (_overlapSize.x > 0)
                _xPos = Mathf.Clamp(-_container.x, 0, _overlapSize.x);
            if (_overlapSize.y > 0)
                _yPos = Mathf.Clamp(-_container.y, 0, _overlapSize.y);

            //循环滚动特别检查
            if (_loop != 0) {
                newPos = _container.xy;
                if (LoopCheckingCurrent())
                    _containerPos += _container.xy - newPos;
            }

            draggingPane = this;
            _isHoldAreaDone = true;
            _dragged = true;

            UpdateScrollBarPos();
            UpdateScrollBarVisible();
            if (_pageMode)
                UpdatePageController();
            _onScroll.Call();
        }

        private void __touchEnd(EventContext context) {
            if (draggingPane == this)
                draggingPane = null;

            _gestureFlag = 0;

            if (!_dragged || !_touchEffect) {
                _dragged = false;
                return;
            }

            _dragged = false;
            _tweenStart = _container.xy;

            Vector2 endPos = _tweenStart;
            bool flag = false;
            if (_container.x > 0) {
                // 超过左边
                endPos.x = 0;
                flag = true;
            }
            else if (_container.x < -_overlapSize.x) {
                // 超过右边
                endPos.x = -_overlapSize.x;
                flag = true;
            }

            if (_container.y > 0) {
                endPos.y = 0;
                flag = true;
            }
            else if (_container.y < -_overlapSize.y) {
                endPos.y = -_overlapSize.y;
                flag = true;
            }

            if (flag) {
                // 超框回弹的一些处理
                _tweenChange = endPos - _tweenStart;
                if (_tweenChange.x < -UIConfig.touchDragSensitivity || _tweenChange.y < -UIConfig.touchDragSensitivity)
                    DispatchEvent("onPullDownRelease", null);
                else if (_tweenChange.x > UIConfig.touchDragSensitivity || _tweenChange.y > UIConfig.touchDragSensitivity)
                    DispatchEvent("onPullUpRelease", null);

                if (_headerLockedSize > 0 && endPos[_refreshBarAxis] == 0) {
                    endPos[_refreshBarAxis] = _headerLockedSize;
                    _tweenChange = endPos - _tweenStart;
                }
                else if (_footerLockedSize > 0 && endPos[_refreshBarAxis] == -_overlapSize[_refreshBarAxis]) {
                    float max = _overlapSize[_refreshBarAxis];
                    if (max == 0)
                        max = Mathf.Max(_contentSize[_refreshBarAxis] + _footerLockedSize - _viewSize[_refreshBarAxis], 0);
                    else
                        max += _footerLockedSize;
                    endPos[_refreshBarAxis] = -max;
                    _tweenChange = endPos - _tweenStart;
                }

                _tweenDuration.Set(_bounceBackTweenTime[0], _bounceBackTweenTime[1]);
            }
            else {
                if (pageMode) {
                    _isQuickDrag = Time.time - _touchBeginTime <= UIFeelConfig.PageQuickDragThreshold;
                }

                //更新速度
                if (!_inertiaDisabled) {
                    float elapsed = (Time.unscaledTime - _lastMoveTime) * 60 - 1;
                    if (elapsed > 1)
                        _velocity = _velocity * Mathf.Pow(0.833f, elapsed);

                    //根据速度计算目标位置和需要时间
                    endPos = UpdateTargetAndDuration(_tweenStart);
                }
                else
                    _tweenDuration.Set(TweenTimeSet, TweenTimeSet);

                Vector2 oldChange = endPos - _tweenStart;

                //调整目标位置
                LoopCheckingTarget(ref endPos);

                if (_pageMode || _snapToItem)
                    AlignPosition(ref endPos, true);

                _tweenChange = endPos - _tweenStart;
                if (_tweenChange.x == 0 && _tweenChange.y == 0) {
                    UpdateScrollBarVisible();
                    return;
                }

                //如果目标位置已调整，随之调整需要时间
                if (_pageMode || _snapToItem) {
                    FixDuration(0, oldChange.x);
                    FixDuration(1, oldChange.y);
                }
            }

            StartTween(2);
        }

        float AlignByPage(float pos, int axis, bool inertialScrolling) {
            if (!(pos <= 0) || !(pos >= -_overlapSize[axis])) {
                return pos;
            }

            int page = Mathf.FloorToInt(-pos / _pageSize[axis]);

            float change = inertialScrolling ? (pos - _containerPos[axis]) : (pos - _container.xy[axis]);
            float testPageSize = Mathf.Min(_pageSize[axis], _contentSize[axis] - (page + 1) * _pageSize[axis]);
            float delta = -pos - page * _pageSize[axis];

            //页面吸附策略
            if (Mathf.Abs(change) > _pageSize[axis]) //如果滚动距离超过1页,则需要超过页面的一半，才能到更下一页
            {
                if (delta > testPageSize * 0.5f) {
                    page++;
                }
            }
            else //否则只需要页面的1/3，当然，需要考虑到左移和右移的情况
            {
                float threshold = _isQuickDrag ? UIFeelConfig.PagingSensitive : UIFeelConfig.PageSlowMoveTolerance;
                float targetValue = testPageSize * (change < 0 ? threshold : 1 - threshold);

                if (delta > targetValue) {
                    page++;
                }
            }

            //重新计算终点
            pos = -page * _pageSize[axis];
            if (pos < -_overlapSize[axis]) //最后一页未必有pageSize那么大
                pos = -_overlapSize[axis];

            return pos;
        }


    }
}