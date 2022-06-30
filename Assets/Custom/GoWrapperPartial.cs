using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI {
    public partial class GoWrapper {
        /// <summary>
        /// 渲染层级队列超过3000的不进行修改
        /// </summary>
        public void CacheRenderers() {
            if (_canvas != null)
                return;

            RecoverMaterials();
            _renderers.Clear();

            Renderer[] items = _wrapTarget.GetComponentsInChildren<Renderer>(true);

            int cnt = items.Length;
            _renderers.Capacity = cnt;
            for (int i = 0; i < cnt; i++) {
                Renderer r = items[i];
                Material[] mats = r.sharedMaterials;
                RendererInfo ri = new RendererInfo() {
                    renderer = r,
                    materials = mats,
                    sortingOrder = r.sortingOrder
                };
                _renderers.Add(ri);

                if (!_cloneMaterial && mats != null
                    && (r is SkinnedMeshRenderer || r is MeshRenderer))
                {
                    int mcnt = mats.Length;
                    for (int j = 0; j < mcnt; j++)
                    {
                        Material mat = mats[j];
                        if (mat != null && mat.renderQueue < 3000) //Set the object rendering in Transparent Queue as UI objects
                            mat.renderQueue = 3000;
                    }
                }
            }

            _renderers.Sort((RendererInfo c1, RendererInfo c2) => { return c1.sortingOrder - c2.sortingOrder; });

            _shouldCloneMaterial = _cloneMaterial;
        }

        /// <summary>
        /// 渲染层级队列超过3000的不进行修改
        /// </summary>
        void CloneMaterials() {
            _shouldCloneMaterial = false;

            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++) {
                RendererInfo ri = _renderers[i];
                Material[] mats = ri.materials;
                if (mats == null)
                    continue;

                bool shouldSetRQ = (ri.renderer is SkinnedMeshRenderer) || (ri.renderer is MeshRenderer);

                int mcnt = mats.Length;
                for (int j = 0; j < mcnt; j++) {
                    Material mat = mats[j];
                    if (mat == null)
                        continue;

                    //确保相同的材质不会复制两次
                    Material newMat;
                    if (!_materialsBackup.TryGetValue(mat, out newMat)) {
                        newMat = new Material(mat);
                        _materialsBackup[mat] = newMat;
                    }
                    mats[j] = newMat;

                    if (shouldSetRQ && mat.renderQueue < 3000) //Set the object rendering in Transparent Queue as UI objects
                        newMat.renderQueue = 3000;
                }

                if (customCloneMaterials != null)
                    customCloneMaterials.Invoke(_materialsBackup);
                else if (ri.renderer != null)
                    ri.renderer.sharedMaterials = mats;
            }
        }

        /// <summary>
        /// 设置包装对象，但是不会重置正在播放的动画
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cloneMaterial"></param>
        public void SetWrapTargetNotResetAnimation(GameObject target, bool cloneMaterial) {
            if (target == null) _flags &= ~Flags.SkipBatching;
            else _flags |= Flags.SkipBatching;
            InvalidateBatchingState();

            RecoverMaterials();

            _cloneMaterial = cloneMaterial;
            if (_wrapTarget != null)
                _wrapTarget.transform.SetParent(null, false);

            _canvas = null;
            _wrapTarget = target;
            _shouldCloneMaterial = false;
            _renderers.Clear();

            if (_wrapTarget != null) {
                this.gameObject.SetActive(true);//需要提前将GoWrapper设置成活跃的，不然设置父节点的时候会重置物体的Animator
                _wrapTarget.transform.SetParent(this.cachedTransform, false);
                _canvas = _wrapTarget.GetComponent<Canvas>();
                if (_canvas != null) {
                    _canvas.renderMode = RenderMode.WorldSpace;
                    _canvas.worldCamera = StageCamera.main;
                    _canvas.overrideSorting = true;

                    RectTransform rt = _canvas.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.position = new Vector3(0, 0, 0);
                    this.SetSize(rt.rect.width, rt.rect.height);
                }
                else {
                    CacheRenderers();
                    this.SetSize(0, 0);
                }

                SetGoLayers(this.layer);
            }
        }
    }
}