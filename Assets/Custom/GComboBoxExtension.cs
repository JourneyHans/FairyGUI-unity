using System.Text.RegularExpressions;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI {
    public class GComboBoxExtension : GComboBox {
        public override void Setup_AfterAdd(ByteBuffer buffer, int beginPos) {
            base.Setup_AfterAdd(buffer, beginPos);
            Localize();
        }

        private void Localize() {
            for (int i = 0; i < values.Length; i++) {
                Match idStr = Regex.Match(values[i], @"\d+");
                if (int.TryParse(idStr.ToString(), out int locID)) {
                    items[i] = UIConfig.LocalizationLoader?.Invoke(locID);
                }
                else {
                    Debug.LogError($"多语言解析失败：{idStr}");
                }
            }
        }
    }
}