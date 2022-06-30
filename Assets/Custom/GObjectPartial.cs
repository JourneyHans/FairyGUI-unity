using UnityEngine;

namespace FairyGUI {
    public partial class GObject {
        /// <summary>
        /// 
        /// </summary>
        public virtual string text {
            get => null;
            set => Debug.LogError($"{GetType().Name}对象无法直接设置text，继承的子类重写该方法才可以这样使用");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string icon {
            get => null;
            set => Debug.LogError($"{GetType().Name}对象无法直接设置icon，继承的子类重写该方法才可以这样使用");
        }
    }
}