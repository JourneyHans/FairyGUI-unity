using UnityEngine;

namespace FairyGUI {
    public partial class LongPressGesture {
        public EventListener WhenUpdateEndCallback;
        void __timer(object param)
        {
            Vector2 pt = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touchId));
            if (Mathf.Pow(pt.x - _startPoint.x, 2) + Mathf.Pow(pt.y - _startPoint.y, 2) > Mathf.Pow(holdRangeRadius, 2))
            {
                Timers.inst.Remove(__timer);
                WhenUpdateEndCallback.Call();
                return;
            }
            if (!_started)
            {
                _started = true;
                onBegin.Call();

                if (!once)
                    Timers.inst.Add(interval, 0, __timer);
            }

            onAction.Call();
        }
    }
}