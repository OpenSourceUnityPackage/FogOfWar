using System;
using FogOfWarPackage;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Unit : MonoBehaviour, IFogOfWarEntity
{
    public ETeam team = ETeam.Team1;
    private bool m_isSelected = false;
    private TerrainFogOfWar m_terrainFogOfWar;
    private static readonly int FogOfWar = Shader.PropertyToID("_FogOfWar");

    #region MonoBehaviour

    private void Awake()
    {
        m_terrainFogOfWar = FindObjectOfType<TerrainFogOfWar>();
        GetComponent<MeshRenderer>().material.SetTexture(FogOfWar, m_terrainFogOfWar.RenderTexture);
    }

    private void OnEnable()
    {
        GameManager.Instance.RegisterUnit(team, this);
    }

    private void OnDisable()
    {
        if(gameObject.scene.isLoaded)
            GameManager.Instance.UnregisterUnit(team, this);
    }
    
    #endregion

    public Vector2 GetVisibilityPosition()
    {
        Vector3 position = transform.position;
        return new Vector2(position.x, position.z);
    }

    public float GetVisibilityRadius()
    {
        return 8f;
    }

    public float GetPermanentVisibilityRadius()
    {
        return 15f;
    }
}
