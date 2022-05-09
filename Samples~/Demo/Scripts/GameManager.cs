using System;
using System.Collections.Generic;
using FogOfWarPackage;
using UnityEngine;

public enum ETeam : int
{
    Team1 = 0,
    Team2 = 1,
    [InspectorName(null)] TeamCount = 2
}

public class GameManager : MonoBehaviour
{
    public Camera m_camera;
    private List<Unit>[] m_teamsUnits = new List<Unit>[(int) ETeam.TeamCount];
    
    private Terrain[] terrains;
    private TerrainFogOfWar[] FogTeam1;
    private TerrainFogOfWar[] FogTeam2;

    private float m_GUIUpdateTimerMax = 3f;
    private float m_GUIUpdateTimerCurrent = 3f;
    private Vector2[] m_stats;
    
#if UNITY_EDITOR
    private static readonly int s_shaderPropertyMap1 = Shader.PropertyToID("_Map1");
    private static readonly int s_shaderPropertyMap2 = Shader.PropertyToID("_Map2");

    [SerializeField] private bool m_drawDebug = false;

    private bool m_prevDrawDebug = false;

    private Material[] m_prevTerrainMaterial;
    private Material m_debugMaterial;
#endif

    #region Singleton

    private static GameManager m_Instance = null;

    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<GameManager>();
                if (m_Instance == null)
                {
                    GameObject newObj = new GameObject("GameManager");
                    m_Instance = Instantiate(newObj).AddComponent<GameManager>();
                }
            }

            return m_Instance;
        }
    }

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        for (var index = 0; index < m_teamsUnits.Length; index++)
        {
            m_teamsUnits[index] = new List<Unit>();
        }

        terrains = FindObjectsOfType<Terrain>();
        m_prevTerrainMaterial = new Material[terrains.Length];
        FogTeam1 = new TerrainFogOfWar[terrains.Length];
        FogTeam2 = new TerrainFogOfWar[terrains.Length];

        for (int i = 0; i < terrains.Length; i++)
        {
            TerrainFogOfWar[] terrainFogOfWars = terrains[i].GetComponents<TerrainFogOfWar>();
            FogTeam1[i] = terrainFogOfWars[0];
            FogTeam2[i] = terrainFogOfWars[1];
        }
    }


    private void Update()
    {
        // Fog of war
#if UNITY_EDITOR
        if (m_prevDrawDebug != m_drawDebug)
        {
            m_prevDrawDebug = m_drawDebug;

            if (m_drawDebug)
            {
                for (var index = 0; index < terrains.Length; index++)
                {
                    var terrain = terrains[index];
                    m_prevTerrainMaterial[index] = terrain.materialTemplate;
                    terrain.materialTemplate = new Material(Shader.Find("MapMerger"));
                    ;
                }
            }
            else
            {
                for (var index = 0; index < terrains.Length; index++)
                {
                    terrains[index].materialTemplate = m_prevTerrainMaterial[index];
                }
            }
        }

        if (m_drawDebug)
        {
            for (var index = 0; index < terrains.Length; index++)
            {
                terrains[index].materialTemplate.SetTexture(s_shaderPropertyMap1,
                    FogTeam1[index].RenderTexture);
                terrains[index].materialTemplate.SetTexture(s_shaderPropertyMap2,
                    FogTeam2[index].RenderTexture);
            }
        }
        
        m_GUIUpdateTimerCurrent += Time.deltaTime;
#endif
    }

   void OnGUI()
   {
       while (m_GUIUpdateTimerCurrent >= m_GUIUpdateTimerMax)
       {
           m_GUIUpdateTimerCurrent -= m_GUIUpdateTimerMax;
           
           m_stats = GetStats();
       }
       GUILayout.BeginVertical("box");
       for (var index = 0; index < m_stats.Length; index++)
       {
           var stat = m_stats[index];
           GUILayout.BeginHorizontal("box");

           GUILayout.Label(terrains[index].name);
           GUILayout.Label($"Red : {stat.x * 100f}%");
           GUILayout.Label($"Green : {stat.y * 100f}%");

           GUILayout.EndHorizontal();
       }

       GUILayout.EndVertical();
   }
   #endregion

    Vector2[] GetStats()
    {
        Vector2[] rst = new Vector2[terrains.Length];
        
        for (var index = 0; index < terrains.Length; index++)
        {
            // Influence map 1 and 2 can have different size
            int width1 = FogTeam1[index].RenderTexture.width;
            int height1 = FogTeam1[index].RenderTexture.height;
            int width2 = FogTeam2[index].RenderTexture.width;
            int height2 = FogTeam2[index].RenderTexture.height;
            int width = Math.Min(width1, width2);
            int height = Math.Min(height1, height2);

            float widthStep1 = width1 / (float)width;
            float heightStep1 = height1 / (float)height;
            float widthStep2 = width2 / (float)width;
            float heightStep2 = height2 / (float)height;
            
            Color[] colors1 = FogTeam1[index].GetDatas();
            Color[] colors2 = FogTeam2[index].GetDatas();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float i1 = colors1[(int)(x * widthStep1 + y * heightStep1 * width1)].r;
                    float i2 = colors2[(int)(x * widthStep2 + y * heightStep2 * width2)].r;
                    
                    rst[index].x += (i1 >= i2) ? i1 : 0;
                    rst[index].y += (i2 > i1) ? i2 : 0;
                }                
            }

            rst[index].y /= width * height;
            rst[index].x /= width * height;
        }

        return rst;
    }

    /// <summary>
    /// Need to be called in OnEnable
    /// </summary>
    /// <example>
    ///private void OnEnable()
    ///{
    ///    GameManager.Instance.RegisterUnit(team, this);
    ///}
    /// </example>
    /// <param name="team"></param>
    public void RegisterUnit(ETeam team, Unit unit)
    {
        m_teamsUnits[(int) team].Add(unit);

        switch (team)
        {
            case ETeam.Team1:
                foreach (TerrainFogOfWar terrainFogOfWar in FogTeam1)
                    terrainFogOfWar.RegisterEntity(unit);
                break;
            case ETeam.Team2:
                foreach (TerrainFogOfWar terrainFogOfWar in FogTeam2)
                    terrainFogOfWar.RegisterEntity(unit);
                break;
            case ETeam.TeamCount:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
    }

    /// <summary>
    /// Need to be called in OnDisable
    /// </summary>
    /// <example>
    ///private void OnDisable()
    ///{
    ///    if(gameObject.scene.isLoaded)
    ///        GameManager.Instance.UnregisterUnit(team, this);
    ///}
    /// </example>
    /// <param name="team"></param>
    public void UnregisterUnit(ETeam team, Unit unit)
    {
        m_teamsUnits[(int) team].Remove(unit);
        switch (team)
        {
            case ETeam.Team1:
                foreach (TerrainFogOfWar terrainFogOfWar in FogTeam1)
                    terrainFogOfWar.UnregisterEntity(unit);
                break;
            case ETeam.Team2:
                foreach (TerrainFogOfWar terrainFogOfWar in FogTeam2)
                    terrainFogOfWar.UnregisterEntity(unit);
                break;
            case ETeam.TeamCount:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
    }
}