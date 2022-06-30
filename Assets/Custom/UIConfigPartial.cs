using System;

namespace FairyGUI {
    public partial class UIConfig {
        /// <summary>
        /// 是否启用自定义的描边效果。
        /// false则使用原来的复制出4个Mesh的方式
        /// true使用新的基于像素的描边，但是会采样8+1次.
        /// </summary>
        public static bool useCustomOutline = false;

        /// <summary>
        /// 不能出现在行首的字符，注释掉的不常用，等实在要用的时候再加上吧
        /// </summary>
        public static char[] LineBreakingFollowingCharacters = {
            ')',
            //']','｝','〉',
            '》', '’', '”', '‐', '－', '-', '+', '＋', '゠', '–', '〜', '?', '？', '!', '！',
            //'‼','⁇','⁈','⁉','・',
            '、', '%', ',', '.', ':', ';', '。', '！',
            //'］',
            '）', '：', '；', '＝',
            //'}',
            '°', '\"', '℃', '％', '，', '．', '…',
        };

        /// <summary>
        /// 需要和数字一起处理换行的字符
        /// </summary>
        public static char[] LineBreakingWithNumberCharacters = {
            '(',
            //'[','｛','〈',
            '《', '〝', '‘', '“', '$', '—', '［', '（', '{', '£', '¥', '°', '\"', '＄', '￥', '#',
        };

        // 多语言加载委托
        public static Func<int, string> LocalizationLoader = null;
    }
}
