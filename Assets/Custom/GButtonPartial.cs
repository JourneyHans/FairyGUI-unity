using UnityEngine;

namespace FairyGUI {
    public partial class GButton {
        protected void SetState(string val) {
            if (_buttonController != null)
                _buttonController.selectedPage = val;

            if (_downEffect == 1) {
                int cnt = this.numChildren;
                if (val == DOWN || val == SELECTED_OVER || val == SELECTED_DISABLED) {
                    Color color = new Color(_downEffectValue, _downEffectValue, _downEffectValue);
                    for (int i = 0; i < cnt; i++) {
                        GObject obj = this.GetChildAt(i);
                        if ((obj is IColorGear) && !(obj is GTextField))
                            ((IColorGear)obj).color = color;
                    }
                }
                else {
                    for (int i = 0; i < cnt; i++) {
                        GObject obj = this.GetChildAt(i);
                        if ((obj is IColorGear) && !(obj is GTextField))
                            ((IColorGear)obj).color = Color.white;
                    }
                }
            }
            else if (_downEffect == 2) {
                if (val == DOWN || val == SELECTED_OVER || val == SELECTED_DISABLED) {
                    if (!_downScaled) {
                        _downScaled = true;
                        GTween.Kill(this, true);
                        TweenScale(new Vector2(scaleX * _downEffectValue, scaleY * _downEffectValue), 0.2f).SetIgnoreEngineTimeScale(true);
                    }
                }
                else {
                    if (_downScaled) {
                        _downScaled = false;
                        GTween.Kill(this, true);
                        TweenScale(new Vector2(scaleX / _downEffectValue, scaleY / _downEffectValue), 0.2f).SetEase(EaseType.BackOut).SetEaseOvershootOrAmplitude(3f).SetIgnoreEngineTimeScale(true);
                    }
                }
            }
        }
    }
}