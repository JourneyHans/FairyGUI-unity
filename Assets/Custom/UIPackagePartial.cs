using System;
using System.IO;
using UnityEngine;

namespace FairyGUI {
    public partial class UIPackage {
        // #region XAssets加载方式
        //
        // public static LoadResource _loadFromBundle = (string name, string extension, Type type, out DestroyMethod destroyMethod) => {
        //         destroyMethod = DestroyMethod.Unload;
        //         libx.AssetRequest request = libx.Assets.LoadAsset($"Assets/Res/FGUI/{name}{extension}", type);
        //         return request.asset;
        //     };
        //
        // #endregion

        public static UIPackage AddPackage(string descFilePath) {
            if (descFilePath.StartsWith("Assets/")) {
#if UNITY_EDITOR
                return AddPackage(descFilePath, _loadFromAssetsPath);
#else
                Debug.LogWarning("FairyGUI: failed to load package in '" + descFilePath + "'");
                return null;
#endif
            }

            // // XAssets方式
            // return AddPackage(descFilePath, _loadFromBundle);
            return AddPackage(descFilePath, _loadFromResourcesPath);
        }

        void LoadAtlas(PackageItem item) {
            string ext = Path.GetExtension(item.file);
            string fileName = item.file.Substring(0, item.file.Length - ext.Length);

            if (_loadAsyncFunc != null) {
                _loadAsyncFunc(fileName, ext, typeof(Texture), item);
                if (item.texture == null)
                    item.texture = new NTexture(null, new Rect(0, 0, item.width, item.height));
                item.texture.destroyMethod = DestroyMethod.None;
            }
            else {
                Texture tex = null;
                Texture alphaTex = null;
                DestroyMethod dm;

                if (_fromBundle) {
                    if (_resBundle != null)
                        tex = _resBundle.LoadAsset<Texture>(fileName);
                    else
                        Debug.LogWarning("FairyGUI: bundle already unloaded.");

                    dm = DestroyMethod.None;
                }
                else
                    tex = (Texture)_loadFunc(fileName, ext, typeof(Texture), out dm);

                if (tex == null)
                    Debug.LogWarning("FairyGUI: texture '" + item.file + "' not found in " + this.name);

                else if (!(tex is Texture2D)) {
                    Debug.LogWarning("FairyGUI: settings for '" + item.file + "' is wrong! Correct values are: (Texture Type=Default, Texture Shape=2D)");
                    tex = null;
                }
                else {
                    if (((Texture2D)tex).mipmapCount > 1)
                        Debug.LogWarning("FairyGUI: settings for '" + item.file + "' is wrong! Correct values are: (Generate Mip Maps=unchecked)");
                }

                // 不考虑alpha通道贴图的加载，目前没有打这种贴图。
                // if (tex != null) {
                //     fileName = fileName + "!a";
                //     if (_fromBundle) {
                //         if (_resBundle != null)
                //             alphaTex = _resBundle.LoadAsset<Texture2D>(fileName);
                //     }
                //     else
                //         alphaTex = (Texture2D)_loadFunc(fileName, ext, typeof(Texture2D), out dm);
                // }

                if (tex == null) {
                    tex = NTexture.CreateEmptyTexture();
                    dm = DestroyMethod.Destroy;
                }

                if (item.texture == null) {
                    item.texture = new NTexture(tex, alphaTex, (float)tex.width / item.width, (float)tex.height / item.height);
                    item.texture.onRelease += (NTexture t) => {
                        if (onReleaseResource != null)
                            onReleaseResource(item);
                    };
                }
                else
                    item.texture.Reload(tex, alphaTex);
                item.texture.destroyMethod = dm;
            }
        }
    }
}