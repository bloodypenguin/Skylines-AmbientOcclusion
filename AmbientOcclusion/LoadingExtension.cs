using ICities;
using System;
using System.Reflection;
using ColossalFramework.Plugins;
using UnityEngine;

namespace AmbientOcclusion2
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private Material SSAOMaterial = (Material) null;
        private ScreenSpaceAmbientOcclusion ssao;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
                return;
            GameObject gameObject1 = Camera.main.gameObject;
            this.ssao = gameObject1.GetComponent<ScreenSpaceAmbientOcclusion>();
            if ((UnityEngine.Object) this.ssao == (UnityEngine.Object) null)
                this.ssao = gameObject1.AddComponent<ScreenSpaceAmbientOcclusion>();
            GameObject gameObject2 = GameObject.Find("ModControl");
            if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
            {
                gameObject2 = new GameObject("ModControl");
                gameObject2.AddComponent<ModControl>();
            }
            gameObject2.GetComponent("ModControl").SendMessage("addMod", (object) "AmbientOcclusion");
            gameObject2.GetComponent("ModControl").SendMessage("setAction", (object) new Action(this.OnModControlGUI));
            gameObject2.GetComponent("ModControl").SendMessage("setHeight", (object) 70f);
            this.ssao.m_RandomTexture = RandomTexture.getRandomTexture();
            LoadShaders();
            this.ssao.m_SSAOShader = this.SSAOMaterial.shader;
            this.ssao.Init();
        }

        void LoadShaders()
        {
            string assetsUri = "file:///" + modPath.Replace("\\", "/") + "/ambientocclusionshaders";
            WWW www = new WWW(assetsUri);
            AssetBundle assetBundle = www.assetBundle;

            CheckAssetBundle(assetBundle, assetsUri);
            ThrowPendingCheckErrors();

            string ssaoAssetName = "Assets/AssetBundle/SSAOShader.shader";
            Shader ssaoShaderContent = assetBundle.LoadAsset(ssaoAssetName) as Shader;

            CheckShader(ssaoShaderContent, assetBundle, ssaoAssetName);
            ThrowPendingCheckErrors();

            SSAOMaterial = new Material(ssaoShaderContent);

            CheckMaterial(SSAOMaterial, ssaoAssetName);
            ThrowPendingCheckErrors();

            assetBundle.Unload(false);
        }

        private static string cachedModPath = null;
        private string checkErrorMessage = null;


        static string modPath
        {
            get
            {
                if (cachedModPath == null)
                {
                    cachedModPath =
                        PluginManager.instance.FindPluginInfo(Assembly.GetAssembly(typeof(Mod))).modPath;
                }

                return cachedModPath;
            }
        }

        void HandleCheckError(string message)
        {
#if (DEBUG)
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, message);
#endif
            if (checkErrorMessage == null)
            {
                checkErrorMessage = message;
            }
            else
            {
                checkErrorMessage += "; " + message;
            }
        }

        void ThrowPendingCheckErrors()
        {
            if (checkErrorMessage != null)
            {
                throw new Exception(checkErrorMessage);
            }
        }

        void CheckAssetBundle(AssetBundle assetBundle, string assetsUri)
        {
            if (assetBundle == null)
            {
                HandleCheckError("AssetBundle with URI '" + assetsUri + "' could not be loaded");
            }
#if (DEBUG)
            else
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Mod Assets URI: " + assetsUri);
                foreach (string asset in assetBundle.GetAllAssetNames())
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Asset: " + asset);
                }
            }
#endif
        }

        void CheckShader(Shader shader, string source)
        {
            if (shader == null)
            {
                HandleCheckError("Shader " + source + " is missing or invalid");
            }
            else
            {
                if (!shader.isSupported)
                {
                    HandleCheckError("Shader '" + shader.name + "' " + source + " is not supported");
                }
#if (DEBUG)
                else
                {
                    DebugOutputPanel.AddMessage(
                        PluginManager.MessageType.Message,
                        "Shader '" + shader.name + "' " + source + " loaded");
                }
#endif
            }
        }

        void CheckShader(Shader shader, AssetBundle assetBundle, string shaderAssetName)
        {
            CheckShader(shader, "from asset '" + shaderAssetName + "'");
        }

        void CheckMaterial(Material material, string materialAssetName)
        {
            if (material == null)
            {
                HandleCheckError("Material for shader '" + materialAssetName + "' could not be created");
            }
#if (DEBUG)
            else
            {
                DebugOutputPanel.AddMessage(
                    PluginManager.MessageType.Message,
                    "Material for shader '" + materialAssetName + "' created");
            }
#endif
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            try
            {
                this.ssao.Unload();
            }
            catch (Exception ex)
            {
            }
        }

        private void OnModControlGUI()
        {
            this.ssao.enabled = GUI.Toggle(new Rect(0.0f, 0.0f, 100f, 20f), this.ssao.enabled, new GUIContent("SSAO"));
            this.ssao.config.m_Radius =
                GUI.HorizontalSlider(new Rect(0.0f, 30f, 100f, 20f), this.ssao.config.m_Radius, 0.0f, 4f);
            GUI.Label(new Rect(105f, 30f, 100f, 20f), "Radius");
            this.ssao.config.m_OcclusionIntensity = GUI.HorizontalSlider(new Rect(0.0f, 50f, 100f, 20f),
                this.ssao.config.m_OcclusionIntensity, 0.0f, 6f);
            GUI.Label(new Rect(105f, 50f, 100f, 20f), "Intensity");
        }
    }
}