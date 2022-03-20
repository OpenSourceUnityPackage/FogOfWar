using UnityEngine;

/// <summary>
/// <see cref="https://www.youtube.com/watch?v=MUV9Nr-cIGU"/>
/// </summary>
public class FogOfWarEntity : MonoBehaviour
{
    [SerializeField]private Sprite MainFogZoneSprite;
    [SerializeField]private Sprite SubFogZoneSprite;
    
    [SerializeField]private int MainFogZoneScale = 10;
    [SerializeField]private int SubFogZoneScale = 25;
    
    // Start is called before the first frame update
    void Start()
    {
        int layerOpacFog = FogOfWar.Instance.LayerOpacFog;
        int layerSecondaryFog =  FogOfWar.Instance.LayerTransparentFog;
        
        // Opac fog
        {
            GameObject OpacFogZone = new GameObject("OpacFogZone");
            OpacFogZone.layer = layerOpacFog;
            OpacFogZone.transform.parent = transform;
            OpacFogZone.transform.localPosition = Vector3.zero;
            OpacFogZone.transform.rotation = Quaternion.Euler(90, 0, 0);
            OpacFogZone.transform.localScale = new Vector3(MainFogZoneScale, MainFogZoneScale, 1f);
            SpriteRenderer OpacFogZoneSpriteRenderer = OpacFogZone.AddComponent<SpriteRenderer>();
            OpacFogZoneSpriteRenderer.sprite = MainFogZoneSprite;
            OpacFogZoneSpriteRenderer.color = Color.red;
        }

        // Transparent fog
        {
            GameObject TransparentFogZone = new GameObject("TransparentFogZone");
            TransparentFogZone.layer = layerSecondaryFog;
            TransparentFogZone.transform.parent = transform;
            TransparentFogZone.transform.localPosition = Vector3.zero;
            TransparentFogZone.transform.rotation = Quaternion.Euler(90, 0, 0);
            TransparentFogZone.transform.localScale = new Vector3(SubFogZoneScale, SubFogZoneScale, 1f);
            SpriteRenderer TransparentFogZoneSpriteRenderer = TransparentFogZone.AddComponent<SpriteRenderer>();
            TransparentFogZoneSpriteRenderer.sprite = SubFogZoneSprite;
            TransparentFogZoneSpriteRenderer.color = Color.red;
        }

    }
}
