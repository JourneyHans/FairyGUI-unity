using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI {
    public partial class NGraphics {
        #region 扩展的字体描边

        public Vector4[] Tangents;
        public Vector2[] UV2;
        public int MaxCount = 1024;
        public int CurCount = 0;
        public float OutlineWidth = 0;
        public Color OutlineColor;
        protected List<Vector4> TempList;

        #endregion

        public NGraphics(GameObject gameObject) {
            this.gameObject = gameObject;

            _alpha = 1f;
            _shader = ShaderConfig.imageShader;
            _color = Color.white;
            _meshFactory = this;

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            if (UIConfig.useCustomOutline) {
                _propertyBlock = new MaterialPropertyBlock();
            }

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.receiveShadows = false;

            mesh = new Mesh();
            mesh.name = gameObject.name;
            mesh.MarkDynamic();

            meshFilter.mesh = mesh;

            meshFilter.hideFlags = DisplayObject.hideFlags;
            meshRenderer.hideFlags = DisplayObject.hideFlags;
            mesh.hideFlags = DisplayObject.hideFlags;

            Stats.LatestGraphicsCreation++;
        }

        public void Update(UpdateContext context, float alpha, bool grayed) {
            Stats.GraphicsCount++;

            if (_meshDirty) {
                _alpha = alpha;
                UpdateMeshNow();
            }
            else if (_alpha != alpha)
                ChangeAlpha(alpha);

            if (_propertyBlock != null && _blockUpdated) {
                meshRenderer.SetPropertyBlock(_propertyBlock);
                _blockUpdated = false;
            }

            if (_customMatarial != 0) {
                if ((_customMatarial & 2) != 0 && _material != null)
                    context.ApplyClippingProperties(_material, false);
            }
            else {
                if (_manager != null) {
                    if (_maskFlag == 1) {
                        _material = _manager.GetMaterial((int)MaterialFlags.AlphaMask | _materialFlags, BlendMode.Normal, context.clipInfo.clipId);
                        context.ApplyAlphaMaskProperties(_material, false);
                    }
                    else {
                        int matFlags = _materialFlags;
                        if (grayed)
                            matFlags |= (int)MaterialFlags.Grayed;

                        if (context.clipped) {
                            if (context.stencilReferenceValue > 0)
                                matFlags |= (int)MaterialFlags.StencilTest;
                            if (context.rectMaskDepth > 0) {
                                if (context.clipInfo.soft)
                                    matFlags |= (int)MaterialFlags.SoftClipped;
                                else
                                    matFlags |= (int)MaterialFlags.Clipped;
                            }

                            _material = _manager.GetMaterial(matFlags, blendMode, context.clipInfo.clipId);
                            if (_manager.firstMaterialInFrame)
                                context.ApplyClippingProperties(_material, true);
                        }
                        else
                            _material = _manager.GetMaterial(matFlags, blendMode, 0);
                    }
                }
                else
                    _material = null;

                if (!Material.ReferenceEquals(_material, meshRenderer.sharedMaterial))
                    meshRenderer.sharedMaterial = _material;
            }

            if (UIConfig.useCustomOutline && OutlineWidth != 0 && meshRenderer) {
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }

            if (_maskFlag != 0) {
                if (_maskFlag == 1)
                    _maskFlag = 2;
                else {
                    if (_stencilEraser != null)
                        _stencilEraser.enabled = false;

                    _maskFlag = 0;
                }
            }
        }
        
        void UpdateMeshNow() {
            _meshDirty = false;

            if (_texture == null || _meshFactory == null) {
                if (mesh.vertexCount > 0) {
                    mesh.Clear();

                    if (meshModifier != null)
                        meshModifier();
                }
                return;
            }

            VertexBuffer vb = VertexBuffer.Begin();
            vb.contentRect = _contentRect;
            vb.uvRect = _texture.uvRect;
            if (_texture != null)
                vb.textureSize = new Vector2(_texture.width, _texture.height);
            else
                vb.textureSize = new Vector2(0, 0);
            if (_flip != FlipType.None) {
                if (_flip == FlipType.Horizontal || _flip == FlipType.Both) {
                    float tmp = vb.uvRect.xMin;
                    vb.uvRect.xMin = vb.uvRect.xMax;
                    vb.uvRect.xMax = tmp;
                }
                if (_flip == FlipType.Vertical || _flip == FlipType.Both) {
                    float tmp = vb.uvRect.yMin;
                    vb.uvRect.yMin = vb.uvRect.yMax;
                    vb.uvRect.yMax = tmp;
                }
            }
            vb.vertexColor = _color;
            _meshFactory.OnPopulateMesh(vb);

            int vertCount = vb.currentVertCount;
            if (vertCount == 0) {
                if (mesh.vertexCount > 0) {
                    mesh.Clear();

                    if (meshModifier != null)
                        meshModifier();
                }
                vb.End();
                return;
            }

            if (_texture.rotated) {
                float xMin = _texture.uvRect.xMin;
                float yMin = _texture.uvRect.yMin;
                float yMax = _texture.uvRect.yMax;
                float tmp;
                for (int i = 0; i < vertCount; i++) {
                    Vector2 vec = vb.uvs[i];
                    tmp = vec.y;
                    vec.y = yMin + vec.x - xMin;
                    vec.x = xMin + yMax - tmp;
                    vb.uvs[i] = vec;
                }
            }

            hasAlphaBackup = vb._alphaInVertexColor;
            if (hasAlphaBackup) {
                if (_alphaBackup == null)
                    _alphaBackup = new List<byte>();
                else
                    _alphaBackup.Clear();
                for (int i = 0; i < vertCount; i++) {
                    Color32 col = vb.colors[i];
                    _alphaBackup.Add(col.a);

                    col.a = (byte)(col.a * _alpha);
                    vb.colors[i] = col;
                }
            }
            else if (_alpha != 1) {
                for (int i = 0; i < vertCount; i++) {
                    Color32 col = vb.colors[i];
                    col.a = (byte)(col.a * _alpha);
                    vb.colors[i] = col;
                }
            }

            if (_vertexMatrix != null) {
                Vector3 camPos = _vertexMatrix.cameraPos;
                Vector3 center = new Vector3(camPos.x, camPos.y, 0);
                center -= _vertexMatrix.matrix.MultiplyPoint(center);
                for (int i = 0; i < vertCount; i++) {
                    Vector3 pt = vb.vertices[i];
                    pt = _vertexMatrix.matrix.MultiplyPoint(pt);
                    pt += center;
                    Vector3 vec = pt - camPos;
                    float lambda = -camPos.z / vec.z;
                    pt.x = camPos.x + lambda * vec.x;
                    pt.y = camPos.y + lambda * vec.y;
                    pt.z = 0;

                    vb.vertices[i] = pt;
                }
            }

            mesh.Clear();

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
            mesh.SetVertices(vb.vertices);
            if (vb._isArbitraryQuad)
                mesh.SetUVs(0, vb.FixUVForArbitraryQuad());
            else
                mesh.SetUVs(0, vb.uvs);
            mesh.SetColors(vb.colors);
            mesh.SetTriangles(vb.triangles, 0);
            if (vb.uvs2.Count == vb.uvs.Count)
                mesh.SetUVs(1, vb.uvs2);

#if !UNITY_5_6_OR_NEWER
            _colors = null;
#endif
#else
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uv = new Vector2[vertCount];
            _colors = new Color32[vertCount];
            int[] triangles = new int[vb.triangles.Count];

            vb.vertices.CopyTo(vertices);
            vb.uvs.CopyTo(uv);
            vb.colors.CopyTo(_colors);
            vb.triangles.CopyTo(triangles);

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.colors32 = _colors;

            if(vb.uvs2.Count==uv.Length)
            {
                uv = new Vector2[vertCount];
                vb.uvs2.CopyTo(uv);
                mesh.uv2 = uv;
            }
#endif
            vb.End();

            if (UIConfig.useCustomOutline) {
                if (Tangents != null && CurCount > 0) {
                    if (null == TempList) {
                        TempList = new List<Vector4>();
                    }
                    TempList.Clear();
                    for (int i = 0; i < CurCount; i++) {
                        TempList.Add(Tangents[i]);
                    }
                    mesh.SetUVs(2, TempList);

                    TempList.Clear();
                    for (int i = 0; i < CurCount; i++) {
                        TempList.Add(new Vector4(OutlineColor.r, OutlineColor.g, OutlineColor.b, OutlineWidth));
                    }
                    mesh.SetUVs(3, TempList);
                }
                if (UV2 != null) {
                    mesh.SetUVs(1, UV2, 0, CurCount);
                }

            }

            if (meshModifier != null)
                meshModifier();
        }
    }
}