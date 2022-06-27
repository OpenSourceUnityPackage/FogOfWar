using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FogOfWarPackage
{
    [RequireComponent(typeof(Terrain))]
    public class TerrainFogOfWar : MonoBehaviour
    {
        public ComputeShader m_computeShader;
        
        [SerializeField, OnRangeChangedCall(3, 14, "GenerateRenderTexture")]
        [Tooltip("2^resolution is the size of renderTexture used. 3 is size of 8, 4 is 16, 5 is 32...")]
        private int resolution = 8;
        
        public bool lowPrecision = true;

        private readonly List<IFogOfWarEntity> m_fogOfWarEntities = new List<IFogOfWarEntity>();

        private ComputeBuffer m_Datas;
        private int m_kernelIndex;

        static readonly ProfilerMarker s_PreparePerfMarker =
            new ProfilerMarker(ProfilerCategory.Render, "TerrainFogOfWar.Update");

        private static readonly int s_shaderPropertyDataCount = Shader.PropertyToID("_DataCount");
        private static readonly int s_shaderPropertyTextureSize = Shader.PropertyToID("_TextureSize");
        private static readonly int s_shaderPropertyDatas = Shader.PropertyToID("_Datas");
        private static readonly int s_shaderPropertyTextureOut = Shader.PropertyToID("_TextureOut");

        private const string s_cmdName = "Process fog of war";

        public int Resolution
        {
            get => TwoPowX(resolution);
            set
            {
                resolution = value;
                GenerateRenderTexture();
            }
        }
#if UNITY_EDITOR
        private static readonly int s_shaderPropertyTexture = Shader.PropertyToID("_Texture");

        [SerializeField] private bool m_drawDebug = false;

        private bool m_prevDrawDebug = false;

        private Material m_prevTerrainMaterial;
        private Material m_debugMaterial;
#endif

        public RenderTexture RenderTexture { get; private set; }
        public Terrain Terrain { get; private set; }

        #region MonoBehaviour
        private void Awake()
        {
            m_kernelIndex = m_computeShader.FindKernel("mainFogOfWar");
            Terrain = GetComponent<Terrain>();
            Assert.IsFalse(Terrain.terrainData.size.x != Terrain.terrainData.size.z, "Terrain need to be squared to process disc as fast as possible");
        }

        private void OnEnable()
        {
            GenerateRenderTexture();
        }

        private void OnDisable()
        {
            if (m_Datas == null) return;
            m_Datas.Dispose();
            m_Datas = null;
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_fogOfWarEntities.Count == 0)
                return;
            
            UpdateBuffer();
            if (m_Datas == null)
                return;
            
            int resol = Resolution;
            
            using (s_PreparePerfMarker.Auto())
            {
                CommandBuffer commandBuffer = new CommandBuffer {name = s_cmdName};
                commandBuffer.SetComputeIntParam(m_computeShader, s_shaderPropertyDataCount, m_Datas.count);
                commandBuffer.SetComputeIntParam(m_computeShader, s_shaderPropertyTextureSize, resol);
                commandBuffer.SetComputeBufferParam(m_computeShader, m_kernelIndex, s_shaderPropertyDatas, m_Datas);
                commandBuffer.SetComputeTextureParam(m_computeShader, m_kernelIndex, s_shaderPropertyTextureOut, RenderTexture);
                commandBuffer.DispatchCompute(m_computeShader, m_kernelIndex, resol / 8, resol / 8, 1);
                Graphics.ExecuteCommandBuffer(commandBuffer);
            }

#if UNITY_EDITOR
            if (m_prevDrawDebug != m_drawDebug)
            {
                m_prevDrawDebug = m_drawDebug;

                if (m_drawDebug)
                {
                    if (m_debugMaterial == null)
                        m_debugMaterial = new Material(Shader.Find("Debug/DebugFogOfWar"));

                    m_prevTerrainMaterial = Terrain.materialTemplate;
                    Terrain.materialTemplate = m_debugMaterial;
                    Terrain.materialTemplate.SetTexture(s_shaderPropertyTexture, RenderTexture);
                }
                else
                {
                    Terrain.materialTemplate = m_prevTerrainMaterial;
                }
            }
#endif
        }
#endregion

        public void RegisterEntity(IFogOfWarEntity fogOfWarEntity)
        {
            m_fogOfWarEntities.Add(fogOfWarEntity);
        }

        public void UnregisterEntity(IFogOfWarEntity fogOfWarEntity)
        {
            m_fogOfWarEntities.Remove(fogOfWarEntity);
        }

        public Color[] GetDatas()
        {
            return GetDatas(0, 0, RenderTexture.width, RenderTexture.height);
        }

        public Color[] GetDatas(int x, int y, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, lowPrecision ? TextureFormat.RG16 : TextureFormat.RG32, false);
            Rect rectReadPicture = new Rect(x, y, width, height);
         
            RenderTexture.active = RenderTexture;
         
            // Read pixels
            texture.ReadPixels(rectReadPicture, 0, 0);
            texture.Apply();
         
            RenderTexture.active = null; // added to avoid errors
            Color[] rst = texture.GetPixels();
            Texture2D.Destroy(texture);

            return rst;
        }

        private void GenerateRenderTexture()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif
            int resol = Resolution;
            RenderTexture = new RenderTexture(resol, resol, 0,
                lowPrecision ? RenderTextureFormat.RG16 : RenderTextureFormat.RG32)
            {
                enableRandomWrite = true, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            RenderTexture.Create();
            
#if UNITY_EDITOR
                if (m_drawDebug)
                    Terrain.materialTemplate.SetTexture(s_shaderPropertyTexture, RenderTexture);
#endif
        }

        private static Vector2 Remap01(Vector2 value, Vector2 pos, Vector2 size)
        {
            return (value - pos) / size;
        }

        private void UpdateBuffer()
        {
            List<Vector4> buffer = new List<Vector4>(m_fogOfWarEntities.Count);

            TerrainData terrainData = Terrain.terrainData;
            Vector2 terrainSize = new Vector2(terrainData.size.x, terrainData.size.z);
            Vector2 terrainMinPos = new Vector2(Terrain.GetPosition().x, Terrain.GetPosition().z);
            Vector2 terrainMaxPos = terrainMinPos + terrainSize;
            Vector2 positionOffset = terrainSize / (2f * Resolution);

            foreach (IFogOfWarEntity entity in m_fogOfWarEntities)
            {
                Vector2 entityPos = entity.GetVisibilityPosition() - positionOffset;
                float mainRadius = entity.GetVisibilityRadius();
                float subRadius = entity.GetPermanentVisibilityRadius();

                if (IsDiscInsideAABB(entityPos, Mathf.Max(mainRadius, subRadius), terrainMinPos, terrainMaxPos))
                {
                    Vector2 posInTerrainSpace = Remap01(entityPos, terrainMinPos, terrainSize);
                    // assuming terrain is squared
                    mainRadius /= terrainSize.x; 
                    subRadius /= terrainSize.x;
                    
                    buffer.Add(new Vector4(posInTerrainSpace.x, posInTerrainSpace.y,
                        mainRadius * mainRadius, subRadius * subRadius));
                }
            }
            
            UpdateComputeBuffer(buffer.Count);
            m_Datas?.SetData(buffer);
        }

        private void UpdateComputeBuffer(int size)
        {
            if (m_Datas != null)
            {
                if (m_Datas.count == size)
                    return;
                
                m_Datas.Dispose();
                m_Datas = null;
            }

            if (size > 0)
                m_Datas = new ComputeBuffer(size, 4 * 4);
        }

        // From https://stackoverflow.com/questions/11196700/math-pow-taking-an-integer-value
        private static int TwoPowX(int power)
        {
            return 1 << power;
        }
        
        // From https://codereview.stackexchange.com/questions/145809/high-performance-branchless-intersection-testing-sphere-aabb-aabb-aabb
        private static bool IsDiscInsideAABB(Vector2 discCenter, float discRadius, Vector2 rectMin, Vector2 rectMax)
        {
            float ex = Mathf.Max(rectMin.x - discCenter.x, 0f) + Mathf.Max(discCenter.x - rectMax.x, 0f);
            float ey = Mathf.Max(rectMin.y - discCenter.y, 0f) + Mathf.Max(discCenter.y - rectMax.y, 0f);
            
            return (ex < discRadius) && (ey < discRadius) && (ex * ex + ey * ey < discRadius * discRadius);
        }
    }
}
