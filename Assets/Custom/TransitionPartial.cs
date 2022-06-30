using System;
using System.Collections.Generic;

namespace FairyGUI {
    public partial class Transition {
        public float TotalDuration => _totalDuration;

        public List<string> GetAllLabel() {
            List<string> labelList = new List<string>();
            foreach (TransitionItem item in _items) {
                if (!string.IsNullOrEmpty(item.label)) {
                    labelList.Add(item.label);
                }
            }

            return labelList;
        }

        public bool TrySetHook(string label, TransitionHook callback) {
            try {
                SetHook(label, callback);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}