using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public LayerMask selectableLayers = ~0;
        public Material maskMaterial;
        public Material blurMaterialHorizontal;
        public Material blurMaterialVertical;
        public Material compositeMaterial;
        public Color outlineColor = Color.green;
        [Range(0.5f, 4f)]
        public float blurSize = 1f;
    }

    class OutlinePass : ScriptableRenderPass
    {
        private readonly OutlineSettings settings;
        private ProfilingSampler m_ProfilingSampler;

        public OutlinePass(OutlineSettings s)
        {
            settings = s;
            m_ProfilingSampler = new ProfilingSampler("Outline");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            var cameraData = frameContext.Get<UniversalCameraData>();
            var resources = frameContext.Get<UniversalResourceData>();

            // Create all textures once at the beginning
            var maskDesc = new TextureDesc(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight)
            {
                colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                name = "OutlineMask",
                clearBuffer = true,
                clearColor = Color.clear,
                enableRandomWrite = false
            };

            var blurDesc = new TextureDesc(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight)
            {
                colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                name = "BlurTexture",
                clearBuffer = true,
                clearColor = Color.clear,
                enableRandomWrite = false
            };

            TextureHandle maskTex = renderGraph.CreateTexture(maskDesc);
            TextureHandle blurTexH = renderGraph.CreateTexture(blurDesc);
            TextureHandle blurTexV = renderGraph.CreateTexture(blurDesc);

            // Pass 1: Render mask
            RenderMaskPass(renderGraph, maskTex, cameraData, resources);

            // Pass 2: Horizontal blur
            RenderBlurPass(renderGraph, maskTex, blurTexH, settings.blurMaterialHorizontal, "Outline Horizontal Blur", true);

            // Pass 3: Vertical blur
            RenderBlurPass(renderGraph, blurTexH, blurTexV, settings.blurMaterialVertical, "Outline Vertical Blur", false);

            // Pass 4: Composite (needs both original mask and blurred mask)
            RenderCompositePass(renderGraph, maskTex, blurTexV, resources.activeColorTexture, cameraData);
        }

        private void RenderMaskPass(RenderGraph renderGraph, TextureHandle maskTex, UniversalCameraData cameraData, UniversalResourceData resources)
        {
            using (var builder = renderGraph.AddRasterRenderPass<MaskPassData>("Outline Mask Pass", out var passData))
            {
                passData.settings = settings;

                builder.SetRenderAttachment(maskTex, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((MaskPassData data, RasterGraphContext ctx) =>
                {
                    var cmd = ctx.cmd;

                    // Set outline color to mask material
                    data.settings.maskMaterial.SetColor("_OutlineColor", data.settings.outlineColor);

                    var selected = Object.FindObjectsByType<Selectable>(FindObjectsSortMode.None);
                    int renderedCount = 0;
                    foreach (var obj in selected)
                    {
                        if (obj.IsSelected)
                        {
                            var renderer = obj.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                cmd.DrawRenderer(renderer, data.settings.maskMaterial);
                                renderedCount++;
                            }
                        }
                    }
                    // Debug: Log if no objects were rendered
                    if (renderedCount == 0)
                    {
                        Debug.LogWarning($"OutlineFeature: No selected objects with renderers found! Total SelectableObjects: {selected.Length}");
                    }
                });
            }
        }

        private void RenderBlurPass(RenderGraph renderGraph, TextureHandle inputTex, TextureHandle outputTex, Material blurMaterial, string passName, bool isHorizontal)
        {
            using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>(passName, out var passData))
            {
                passData.settings = settings;
                passData.inputTexture = inputTex;
                passData.blurMaterial = blurMaterial;

                builder.SetRenderAttachment(outputTex, 0);
                builder.UseTexture(inputTex, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((BlurPassData data, RasterGraphContext ctx) =>
                {
                    var cmd = ctx.cmd;
                    data.blurMaterial.SetFloat("_BlurSize", data.settings.blurSize);
                    data.blurMaterial.SetTexture("_MainTex", data.inputTexture);

                    // Draw fullscreen triangle
                    cmd.DrawProcedural(Matrix4x4.identity, data.blurMaterial, 0, MeshTopology.Triangles, 3, 1);
                });
            }
        }

        private void RenderCompositePass(RenderGraph renderGraph, TextureHandle maskTex, TextureHandle blurTex, TextureHandle colorTarget, UniversalCameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<CompositePassData>("Outline Composite", out var passData))
            {
                passData.settings = settings;
                passData.maskTexture = maskTex;
                passData.blurTexture = blurTex;

                builder.SetRenderAttachment(colorTarget, 0, AccessFlags.ReadWrite); // IMPORTANT: ReadWrite to preserve scene
                builder.UseTexture(maskTex, AccessFlags.Read);
                builder.UseTexture(blurTex, AccessFlags.Read);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((CompositePassData data, RasterGraphContext ctx) =>
                {
                    var cmd = ctx.cmd;

                    // Set shader properties for composite
                    data.settings.compositeMaterial.SetColor("_OutlineColor", data.settings.outlineColor);
                    data.settings.compositeMaterial.SetTexture("_MaskTex", data.maskTexture);
                    data.settings.compositeMaterial.SetTexture("_BlurTex", data.blurTexture);

                    // Draw fullscreen triangle to composite outline
                    cmd.DrawProcedural(Matrix4x4.identity, data.settings.compositeMaterial, 0, MeshTopology.Triangles, 3, 1);
                });
            }
        }

        private class MaskPassData
        {
            public OutlineSettings settings;
        }

        private class BlurPassData
        {
            public OutlineSettings settings;
            public TextureHandle inputTexture;
            public Material blurMaterial;
        }

        private class CompositePassData
        {
            public OutlineSettings settings;
            public TextureHandle maskTexture;
            public TextureHandle blurTexture;
        }
    }

    public OutlineSettings settings = new OutlineSettings();
    private OutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.maskMaterial != null && settings.blurMaterialHorizontal != null &&
            settings.blurMaterialVertical != null && settings.compositeMaterial != null)
        {
            renderer.EnqueuePass(outlinePass);
        }
    }
}
