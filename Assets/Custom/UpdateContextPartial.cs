using UnityEngine;

namespace FairyGUI {
    public partial class UpdateContext {
        public void ApplyClippingProperties(Material mat, bool isStdMaterial) {
            if (rectMaskDepth > 0) //在矩形剪裁下，且不是遮罩对象
            {
                mat.SetVector(ShaderConfig.ID_ClipBox, clipInfo.clipBox);
                if (clipInfo.soft)
                    mat.SetVector(ShaderConfig.ID_ClipSoftness, clipInfo.softness);
            }

            if (stencilReferenceValue > 0) {
                mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Equal);
                mat.SetInt(ShaderConfig.ID_Stencil, stencilCompareValue);
                mat.SetInt(ShaderConfig.ID_Stencil2, stencilCompareValue);
                mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                mat.SetInt(ShaderConfig.ID_StencilReadMask, stencilReferenceValue | (stencilReferenceValue - 1));
                mat.SetInt(ShaderConfig.ID_ColorMask, 15);
            }
            else {
                mat.SetInt(ShaderConfig.ID_StencilComp, (int)UnityEngine.Rendering.CompareFunction.Always);
                mat.SetInt(ShaderConfig.ID_Stencil, 0);
                mat.SetInt(ShaderConfig.ID_Stencil2, 0);
                mat.SetInt(ShaderConfig.ID_StencilOp, (int)UnityEngine.Rendering.StencilOp.Keep);
                mat.SetInt(ShaderConfig.ID_StencilReadMask, 255);
                mat.SetInt(ShaderConfig.ID_ColorMask, 15);
            }

            // if (!isStdMaterial) {
            //     if (rectMaskDepth > 0) {
            //         if (clipInfo.soft)
            //             mat.EnableKeyword("SOFT_CLIPPED");
            //         else
            //             mat.EnableKeyword("CLIPPED");
            //     }
            //     else {
            //         mat.DisableKeyword("CLIPPED");
            //         mat.DisableKeyword("SOFT_CLIPPED");
            //     }
            // }
        }
    }
}