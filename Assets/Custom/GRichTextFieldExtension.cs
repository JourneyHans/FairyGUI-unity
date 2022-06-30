using System.Collections.Generic;
using FairyGUI.Utils;
using System;
using System.Text.RegularExpressions;

namespace FairyGUI
{
    /// <summary>
    /// GRichTextField class.
    /// </summary>
    public class GRichTextFieldExtension : GRichTextField
    {
        private string _loc;
        public override void Setup_AfterAdd(ByteBuffer buffer, int beginPos) {
            base.Setup_AfterAdd(buffer, beginPos);
            Localize();
        }

        private string MatchLocalize(Match match) {
            var idStr = match.Groups["id"].Value;
            int locID = Convert.ToInt32(idStr);
            if (locID > 0) {
                return UIConfig.LocalizationLoader?.Invoke(locID);
            }
            return "";
        }

        private void Localize() {
            if (data == null || string.IsNullOrEmpty((string)data)) {
                return;
            }

            var evaluator = new MatchEvaluator(MatchLocalize);
            _loc = Regex.Replace((string)data,  @"\[(?<id>\d+)\]", evaluator);
            text = _loc;
        }

        public void _Set(object val) {
            text = $"{val}";
        }
        
        public void _Refresh(params object[] values) {
            if (_loc != null) {
                text = string.Format(_loc, values);
                return;
            }
            text = string.Format(text, values);
        }
        
        public void _RefreshValue(string key, object value) {
            SetVar(key, value.ToString());
            FlushVars();
        }
        
        public void _RefreshValues(Dictionary<string, object> values) {
            foreach (var kv in values) {
                SetVar(kv.Key, $"{kv.Value}");
            }
            FlushVars();
        }
    }
}
