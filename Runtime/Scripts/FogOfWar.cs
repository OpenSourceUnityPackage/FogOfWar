using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// <see cref="https://www.youtube.com/watch?v=MUV9Nr-cIGU"/>
/// </summary>
public class FogOfWar : MonoBehaviour
{
    [SerializeField, Min(0)] private int m_RenderTextureResolution = 256;
    [SerializeField, Min(0)] private int m_SquareSize = 50;

    private RenderTexture m_OpacFogRenderTexture;
    private RenderTexture m_TrasnparentFogRenderTexture;

    private static FogOfWar _Instance = null;
    private int m_layerOpacFog = 0;
    private int m_layerTransparentFog = 0;

    private readonly static string OpacLayerName = "FogOfWarOpac";
    private readonly static string TransparencyLayerName = "FogOfWarTrasnparent";
    
    static public FogOfWar Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<FogOfWar>();

                if (_Instance == null)
                {
                    GameObject newGO = new GameObject("FogOfWarManager");
                    _Instance = newGO.AddComponent<FogOfWar>();
                }
            }

            return _Instance;
        }
    }

    private void Awake()
    {
        FindLayers();
    }

    void Start()
    {
        CreateRenderTexture(out m_OpacFogRenderTexture);
        CreateRenderTexture(out m_TrasnparentFogRenderTexture);
        
        SetupCameras();
        SetupCanvas();
    }

    public int LayerOpacFog
    {
        get => m_layerOpacFog;
    }

    public int LayerTransparentFog
    {
        get => m_layerTransparentFog;
    }

    private void SetupCanvas()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        ((RectTransform) canvas.transform).sizeDelta = new Vector2(m_SquareSize * 2f, m_SquareSize * 2f);
        canvas.worldCamera = Camera.main;
        RawImage image = canvas.gameObject.GetComponentInChildren<RawImage>();
        image.material.SetTexture("_OpacFogOfWarRT", m_OpacFogRenderTexture);
        image.material.SetTexture("_TransparentFogOfWarRT", m_TrasnparentFogRenderTexture);
    }

    private void FindLayers()
    {
        m_layerOpacFog = LayerMask.NameToLayer(OpacLayerName);
        m_layerTransparentFog = LayerMask.NameToLayer(TransparencyLayerName);
    }

    private void SetupCameras()
    {
        foreach (var _cam in Camera.allCameras)
        {
            _cam.cullingMask = _cam.cullingMask & ~(1 << m_layerOpacFog);
            _cam.cullingMask = _cam.cullingMask & ~(1 << m_layerTransparentFog);
        }


        Camera[] cam = GetComponentsInChildren<Camera>();

        foreach (var _cam in cam)
        {
            _cam.orthographicSize = m_SquareSize;
            if (_cam.clearFlags == CameraClearFlags.Nothing)
            {
                _cam.targetTexture = m_TrasnparentFogRenderTexture;
                _cam.cullingMask = (1 << m_layerTransparentFog);
            }
            else
            {
                _cam.targetTexture = m_OpacFogRenderTexture;
                _cam.cullingMask = (1 << m_layerOpacFog);
            }
        }
    }

    private void CreateRenderTexture(out RenderTexture texture)
    {
        texture = new RenderTexture(m_RenderTextureResolution, m_RenderTextureResolution, 0, GraphicsFormat.R8_UNorm);
        texture.filterMode = FilterMode.Point;
        texture.anisoLevel = 0;
    }
    
#if UNITY_EDITOR
    [ContextMenu("Install package settings")]
     protected void Install()
     {
         AddLayer(OpacLayerName);
         AddLayer(TransparencyLayerName);
         AssetDatabase.SaveAssets();
         Debug.Log("Installation complete");
     }
     
     [ContextMenu("Uninstall package Settings")]
     protected void Uninstall()
     {
         RemoveLayer(OpacLayerName);
         RemoveLayer(TransparencyLayerName);
         AssetDatabase.SaveAssets();
         Debug.Log("Uninstall complete");
     }

     void AddLayer(string layerName)
     {
         // Open tag manager
         SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
         SerializedProperty layersProp = tagManager.FindProperty("layers");

         // First check if it is not already present
         bool hasLayer = false;
         int indexFree = -1;
         for (int i = 0; i < layersProp.arraySize; i++)
         {
             SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
             if (t.stringValue.Equals(layerName))
             {
                 hasLayer = true;
                 break;
             }
             else if (indexFree == -1 && t.stringValue.Equals(""))
             {
                 indexFree = i;
             }
         }

         if (!hasLayer)
         {
             layersProp.GetArrayElementAtIndex(indexFree).stringValue = layerName;
             tagManager.ApplyModifiedProperties();
             
             Debug.Log("Layer " + layerName + " add to layers");
         }
     }

     void RemoveLayer(string layerName)
     {
         // Open tag manager
         SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
         SerializedProperty layersProp = tagManager.FindProperty("layers");

         // First check if it is not already present
         for (int i = 0; i < layersProp.arraySize; i++)
         {
             SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
             if (t.stringValue.Equals(layerName))
             {
                 layersProp.GetArrayElementAtIndex(i).stringValue = "";
                 tagManager.ApplyModifiedProperties();
                 Debug.Log("Layer " + layerName + " remove to layers");
                 break;
             }
         }
     }
#endif
}
