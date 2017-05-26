using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace AmbientOcclusion2
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Rendering/Screen Space Ambient Occlusion")]
    public class SSAOConfig
    {
        public float m_Radius = 0.7f;
        public float m_OcclusionIntensity = 1.4f;
        public bool m_Enabled = true;
        public int m_SampleCount = 2;
        public int m_Blur = 2;
        public float m_OcclusionAttenuation = 14f;

        public static void Serialize(string filename, object instance)
        {
            try
            {
                TextWriter textWriter = (TextWriter) new StreamWriter(filename);
                new XmlSerializer(typeof(SSAOConfig)).Serialize(textWriter, instance);
                textWriter.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static SSAOConfig Deserialize(string filename)
        {
            try
            {
                TextReader textReader = (TextReader) new StreamReader(filename);
                object obj = new XmlSerializer(typeof(SSAOConfig)).Deserialize(textReader);
                textReader.Close();
                return (SSAOConfig) obj;
            }
            catch (Exception ex)
            {
                SSAOConfig.Serialize(filename, (object) new SSAOConfig());
                return new SSAOConfig();
            }
        }
    }
}