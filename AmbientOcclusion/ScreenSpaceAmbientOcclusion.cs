using System;
using UnityEngine;

namespace AmbientOcclusion2
{
    public class ScreenSpaceAmbientOcclusion : MonoBehaviour
    {
        public int m_Downsampling = 1;
        public float m_MinZ = 0.1f;
        public int passes = 4;
        public bool showSSAO = true;
        public SSAOConfig config;
        public Shader m_SSAOShader;
        private Material m_SSAOMaterial;
        public Texture2D m_RandomTexture;
        private bool m_Supported;

        public void Init()
        {
            try
            {
                this.config = SSAOConfig.Deserialize("AmbientOcclusionConfig.xml");
                if (this.config == null)
                    this.config = new SSAOConfig();
            }
            catch (Exception ex)
            {
                this.config = new SSAOConfig();
            }
            this.enabled = this.config.m_Enabled;
            if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                this.m_Supported = false;
                this.enabled = false;
            }
            else
            {
                this.CreateMaterials();
                if (!(bool) ((UnityEngine.Object) this.m_SSAOMaterial) || this.m_SSAOMaterial.passCount != 5)
                {
                    this.m_Supported = false;
                    this.enabled = false;
                }
                else
                {
                    this.m_Supported = true;
                    this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
                }
            }
        }

        public void Unload()
        {
            this.config.m_Enabled = this.enabled;
            SSAOConfig.Serialize("AmbientOcclusionConfig.xml", (object) this.config);
        }

        private static Material CreateMaterial(Shader shader)
        {
            if (!(bool) ((UnityEngine.Object) shader))
                return (Material) null;
            return new Material(shader);
        }

        private static void DestroyMaterial(Material mat)
        {
            if (!(bool) ((UnityEngine.Object) mat))
                return;
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object) mat);
            mat = (Material) null;
        }

        public void Enable()
        {
            this.enabled = true;
        }

        public void Disable()
        {
            this.enabled = false;
        }

        private void OnDisable()
        {
            ScreenSpaceAmbientOcclusion.DestroyMaterial(this.m_SSAOMaterial);
        }

        private void CreateMaterials()
        {
            if ((bool) ((UnityEngine.Object) this.m_SSAOMaterial) || !this.m_SSAOShader.isSupported)
                return;
            this.m_SSAOMaterial = ScreenSpaceAmbientOcclusion.CreateMaterial(this.m_SSAOShader);
            this.m_SSAOMaterial.SetTexture("_RandomTexture", (Texture) this.m_RandomTexture);
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!this.m_Supported || !this.m_SSAOShader.isSupported)
            {
                this.enabled = false;
            }
            else
            {
                if (!this.showSSAO)
                    return;
                this.CreateMaterials();
                RenderTexture renderTexture = RenderTexture.GetTemporary(source.width / this.m_Downsampling,
                    source.height / this.m_Downsampling, 0);
                float fieldOfView = this.GetComponent<Camera>().fieldOfView;
                float farClipPlane = this.GetComponent<Camera>().farClipPlane;
                float y = Mathf.Tan((float) ((double) fieldOfView * (Math.PI / 180.0) * 0.5)) * farClipPlane;
                this.m_SSAOMaterial.SetVector("_FarCorner",
                    (Vector4) new Vector3(y * this.GetComponent<Camera>().aspect, y, farClipPlane));
                int num1;
                int num2;
                if ((bool) ((UnityEngine.Object) this.m_RandomTexture))
                {
                    num1 = this.m_RandomTexture.width;
                    num2 = this.m_RandomTexture.height;
                }
                else
                {
                    num1 = 1;
                    num2 = 1;
                }
                this.m_SSAOMaterial.SetVector("_NoiseScale",
                    (Vector4) new Vector3((float) renderTexture.width / (float) num1,
                        (float) renderTexture.height / (float) num2, 0.0f));
                this.m_SSAOMaterial.SetVector("_Params",
                    new Vector4(this.config.m_Radius, this.m_MinZ, 1f / this.config.m_OcclusionAttenuation,
                        this.config.m_OcclusionIntensity));
                bool flag = this.config.m_Blur > 0;
                Graphics.Blit(flag ? (Texture) null : (Texture) source, renderTexture, this.m_SSAOMaterial,
                    this.config.m_SampleCount);
                if (flag)
                {
                    RenderTexture temporary1 = RenderTexture.GetTemporary(source.width, source.height, 0);
                    this.m_SSAOMaterial.SetVector("_TexelOffsetScale",
                        new Vector4((float) this.config.m_Blur / (float) source.width, 0.0f, 0.0f, 0.0f));
                    this.m_SSAOMaterial.SetTexture("_SSAO", (Texture) renderTexture);
                    Graphics.Blit((Texture) null, temporary1, this.m_SSAOMaterial, 3);
                    RenderTexture.ReleaseTemporary(renderTexture);
                    RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0);
                    this.m_SSAOMaterial.SetVector("_TexelOffsetScale",
                        new Vector4(0.0f, (float) this.config.m_Blur / (float) source.height, 0.0f, 0.0f));
                    this.m_SSAOMaterial.SetTexture("_SSAO", (Texture) temporary1);
                    Graphics.Blit((Texture) source, temporary2, this.m_SSAOMaterial, 3);
                    RenderTexture.ReleaseTemporary(temporary1);
                    renderTexture = temporary2;
                }
                this.m_SSAOMaterial.SetTexture("_SSAO", (Texture) renderTexture);
                Graphics.Blit((Texture) source, destination, this.m_SSAOMaterial, this.passes);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }
    }
}