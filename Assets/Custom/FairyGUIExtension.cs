using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FairyGUI
{
    public static class FairyGUIExtension {
        
        // ------------------------------------ GTextField ------------------------------------------
        public static void Set(this GTextField com, object val) {
            ((GTextFieldExtension)com)._Set(val);
        }

        public static void Refresh(this GTextField com, params object[] values) {
            ((GTextFieldExtension)com)._Refresh(values);
        }
        
        public static void RefreshValue(this GTextField com, string key, object value) {
            if (key == "" || value == null) {
                return;
            }
            ((GTextFieldExtension)com)._RefreshValue(key, value);
        }
        
        public static void RefreshValues(this GTextField com, Dictionary<string, object> values) {
            if (values == null) {
                return;
            }
            ((GTextFieldExtension)com)._RefreshValues(values);
        }

        // ------------------------------------ GRichTextField ------------------------------------------
        public static void Set(this GRichTextField com, object val) {
            ((GRichTextFieldExtension)com)._Set(val);
        }
        
        public static void Refresh(this GRichTextField com, params object[] values) {
            ((GRichTextFieldExtension)com)._Refresh(values);
        }
        
        public static void RefreshValue(this GRichTextField com, string key, object value) {
            if (key == "" || value == null) {
                return;
            }
            ((GRichTextFieldExtension)com)._RefreshValue(key, value);
        }
        
        public static void RefreshValues(this GRichTextField com, Dictionary<string, object> values) {
            if (values == null) {
                return;
            }
            ((GRichTextFieldExtension)com)._RefreshValues(values);
        }
    }

    // ------------------------------------ GComponentExtension ------------------------------------------
    public static class GComponentExtension {
        public static GObject GetChildRecursion(this GComponent com, string widgetName, string package = "") {
            GObject target = com.GetChild(widgetName);
            if (target != null && target.GameObjectVisible()) {
                return target;
            }

            var objs = com.GetChildren();
            for (var i = objs.Length - 1; i >= 0; i--) {
                GObject childObj = objs[i];
                if (childObj is GComponent childCom) {
                    target = childCom.GetChildRecursion(widgetName, package);
                    if (target != null && target.GameObjectVisible()) {
                        return target;
                    }
                }
                else if (childObj is GLoader childLoader) {
                    if (childLoader.component == null) {
                        continue;
                    }

                    // Logger.Inst?.Error($"loader component type: {childLoader.component.GetType()}, name: {childLoader.component.name}");

                    string targetURL = UIPackage.GetItemURL(package, widgetName);

                    if (childLoader.url == targetURL && childLoader.component.GameObjectVisible()) {
                        return childLoader.component;
                    }

                    if (childLoader.component.GetType().ToString().Contains(widgetName)) {
                        return childLoader.component;
                    }

                    target = childLoader.component.GetChildRecursion(widgetName, package);
                    if (target != null && target.GameObjectVisible()) {
                        return target;
                    }
                }
            }

            return null;
        }

        public static GObject GetChildRecursionByURL(this GComponent com, string url) {
            GObject target = null;
            var objs = com.GetChildren();
            for (var i = objs.Length - 1; i >= 0; i--) {
                GObject childObj = objs[i];
                if (childObj.resourceURL == url) {
                    target = childObj;
                }
            }

            if (target != null && target.GameObjectVisible()) {
                return target;
            }

            var objects = com.GetChildren();
            for (var i = objects.Length - 1; i >= 0; i--) {
                GObject childObj = objects[i];
                if (childObj is GComponent childCom) {
                    target = childCom.GetChildRecursionByURL(url);
                    if (target != null && target.GameObjectVisible()) {
                        return target;
                    }
                }
                else if (childObj is GLoader childLoader) {
                    if (childLoader.component == null) {
                        continue;
                    }

                    if (childLoader.url == url && childLoader.component.GameObjectVisible()) {
                        return childLoader.component;
                    }

                    target = childLoader.component.GetChildRecursionByURL(url);
                    if (target != null && target.GameObjectVisible()) {
                        return target;
                    }
                }
            }

            return null;
        }

        public static void GetChildrenRecursionByURL(this GComponent com, string url, ref List<GComponent> allComponents) {
            if (com.GetChildren().Length == 0) {
                return;
            }

            GObject[] children = com.GetChildren();
            foreach (GObject child in children) {
                if (child is GComponent childComponent) {
                    if (child.resourceURL == url) {
                        allComponents.Add(childComponent);
                    }
                    childComponent.GetChildrenRecursionByURL(url, ref allComponents);
                }
            }
        }

        public static List<T> GetChildren<T>(this GComponent component, Regex pattern) where T : GObject {
            List<T> children = new List<T>();
            foreach (GObject childObj in component._children) {
                if (childObj is T child && pattern.IsMatch(childObj.name)) {
                    children.Add(child);
                }
            }

            return children;
        }

        // 禁用GComponent下所有Transition的合批
        public static void InvalidateBatchingForAllTransitions(this GComponent component) {
            if (component == null) {
                return;
            }

            if (component.Transitions == null) {
                return;
            }

            foreach (Transition transition in component.Transitions) {
                transition.invalidateBatchingEveryFrame = true;
            }
        }
    }

    // ------------------------------------ GObjectExtension ------------------------------------------
    public static class GObjectExtension {
        public static bool GameObjectVisible(this GObject obj) {
            if (obj?.displayObject == null || obj.displayObject.gameObject == null) {
                return false;
            }
            return obj.displayObject.gameObject.activeInHierarchy;
        }
    }

    // ------------------------------------ CSharpExtension ------------------------------------------
    public static class CSharpExtension {
        public static string IconUrl(this string iconPath) {
            string[] iconSplit = iconPath.Split('/');
            return iconSplit.Length == 2 ? UIPackage.GetItemURL(iconSplit[0], iconSplit[1]) : UIPackage.GetItemURL("General", iconPath);
        }
    }
}