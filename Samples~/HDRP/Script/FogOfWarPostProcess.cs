using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Debug = UnityEngine.Debug;

namespace FogOfWarPackage
{
    [Serializable, DebuggerDisplay(k_DebuggerDisplay)]
    public class TerrainFogOfWarBufferParameter : VolumeParameter<TerrainFogOfWar[]>
    {
        /// <summary>
        /// Creates a new <seealso cref="UnityEngine.Rendering.FloatParameter"/> instance.
        /// </summary>
        /// <param name="value">The initial value to store in the parameter</param>
        /// <param name="overrideState">The initial override state for the parameter</param>
        public TerrainFogOfWarBufferParameter(TerrainFogOfWar[] value, bool overrideState = false)
            : base(value, overrideState) {}

        /// <summary>
        /// Interpolates between two <c>float</c> values.
        /// </summary>
        /// <param name="from">The start value</param>
        /// <param name="to">The end value</param>
        /// <param name="t">The interpolation factor in range [0,1]</param>
        public sealed override void Interp(TerrainFogOfWar[] from, TerrainFogOfWar[] to, float t)
        {
            m_Value = t < 0.5 ? from : to;
        }
    }
    
    [Serializable, VolumeComponentMenu("Post-processing/Custom/FogOfWarPostProcess")]
    public sealed class FogOfWarPostProcess : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        Material m_Material;

        public bool IsActive() => Application.isPlaying &&  m_Material != null && intensity.value > 0f;

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        
        public TerrainFogOfWarBufferParameter terrainsFogOfWar = new TerrainFogOfWarBufferParameter(new TerrainFogOfWar[0], true);
        
        private static readonly int FogOfWarProp = Shader.PropertyToID("_FogOfWar");
        private static readonly int TerrainSizePosProp = Shader.PropertyToID("_TerrainSizePos");
        private static readonly int IntensityProp = Shader.PropertyToID("_Intensity");
        private static readonly int InputTextureProp = Shader.PropertyToID("_InputTexture");

        const string kShaderName = "Hidden/Shader/FogOfWarPostProcess";

        public override void Setup()
        {
            if (Shader.Find(kShaderName) != null)
                m_Material = CoreUtils.CreateEngineMaterial(Shader.Find(kShaderName));
            else
                Debug.LogError(
                    $"Unable to find shader '{kShaderName}'. Post Process Volume FogOfWarPostProcess is unable to load.");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null || terrainsFogOfWar == null || terrainsFogOfWar.value.Length == 0)
                return;
            
            using (new ProfilingScope(cmd, new ProfilingSampler(name)))
            {
                // Do post process pass for each terrain (TODO: find a way to optimize)
                foreach (TerrainFogOfWar terrainFogOfWar in terrainsFogOfWar.value)
                {
                    Terrain terrain = terrainFogOfWar.Terrain;

                    if (terrain == null)
                        return;
                    TerrainData terrainData = terrain.terrainData;

                    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                    propertyBlock.SetFloat(IntensityProp, intensity.value);
                    propertyBlock.SetTexture(InputTextureProp, source);
                    propertyBlock.SetTexture(FogOfWarProp, terrainFogOfWar.RenderTexture);
                    propertyBlock.SetVector(TerrainSizePosProp,
                        new Vector4(terrain.GetPosition().x, terrain.GetPosition().z, 1 / terrainData.size.x,
                            1 / terrainData.size.z));

                    HDUtils.DrawFullScreen(cmd, m_Material, destination, propertyBlock);
                }
            }
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_Material);
        }
    }
}
