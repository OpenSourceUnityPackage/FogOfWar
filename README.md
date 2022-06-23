<h1 align="center" style="border-bottom: none;">Fog of war📦 </h1>
<h3 align="center">Fog of war post process effect for unity URP/HDRP</h3>
<p align="center">
  <a href="https://github.com/semantic-release/semantic-release/actions?query=workflow%3ATest+branch%3Amaster">
    <img alt="Build states" src="https://github.com/semantic-release/semantic-release/workflows/Test/badge.svg">
  </a>
  <a href="https://github.com/semantic-release/semantic-release/actions?query=workflow%3ATest+branch%3Amaster">
    <img alt="semantic-release: angular" src="https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release">
  </a>
  <a href="LICENSE">
    <img alt="License" src="https://img.shields.io/badge/License-MIT-blue.svg">
  </a>
</p>
<p align="center">
  <a href="package.json">
    <img alt="Version" src="https://img.shields.io/github/package-json/v/OpenSourceUnityPackage/FogOfWar">
  </a>
  <a href="#LastActivity">
    <img alt="LastActivity" src="https://img.shields.io/github/last-commit/OpenSourceUnityPackage/FogOfWar">
  </a>
</p>

## What is it ?
This package allows you to easily integrate the fog of war with your terrain component.  
![Capture d’écran 2022-06-23 225326](https://user-images.githubusercontent.com/55276408/175397798-bac89b2d-e1a9-4d00-9f78-2378468c39fc.png)

## How to use ?
Don't forget to import a URP or HDRP sample depending on your needs.   
These will contain resources for integrating Fog of War into your rendering pipeline.  
If you need a demo, you can import the demo through the package manager.  
![Capture d’écran 2022-06-23 213023](https://user-images.githubusercontent.com/55276408/175396850-8f7c0cbc-1322-443a-9113-9d7a9f517c4d.png)

### URP
For URP, you must first ensure that the URP sample has been imported into your project and of course that the URP package is installed in your project.  
Then create a forward renderer data and add the Post process fog of war function to it.  
![Capture d’écran 2022-06-23 214146](https://user-images.githubusercontent.com/55276408/175396911-ce7ad290-5b8f-4cec-acdc-efd59c168e7b.png)

Now create a Universal Render Pipeline asset and link your forward render data to it.

Now you need to go to Project Settings/Graphic and change the scriptable render pipeline asset to your own.  
You can of course integrate the fog of war function into your own URP asset.  
In Project Settings/Quality, change the render asset to your new asset.  
URP is ready, you now need to link your terrain with your asset like this:
```c#
    [SerializeField] TerrainFogOfWar[] m_fogTeam1;
    [SerializeField] ForwardRendererData m_rendererData;
    private PostProcessFogOfWarFeature m_fowFeature;
    
    private void OnEnable()
    {
        m_fowFeature = m_rendererData.rendererFeatures.OfType<PostProcessFogOfWarFeature>().FirstOrDefault();
     
        if (m_fowFeature == null)
            return;
         
        m_fowFeature.settings.terrainFogOfWars = m_fogTeam1;
        m_rendererData.SetDirty();
    }
```


### HDRP
To enable the fog of war feature in the HDRP, create a volume profile. Add the Post-processing/custom/fogOfWarPostProcess override.  
Now add a volume component to your scene and attach your new profile to it.  
The HDRP is ready, you now need to link your terrain to your asset in this way:
```c#
    [SerializeField] TerrainFogOfWar[] FogTeam1;
    [SerializeField] Volume postProcessVolume;

    void OnEnable() 
    {
        postProcessVolume.profile.TryGet(out FogOfWarPostProcess fow);
        fow.terrainsFogOfWar.value = FogTeam1;
    }
```

### General
For both URP and HDRP, add a fog of war script to your scene with the same gameObject as your terrain component.  
The fog of war script allows you to control the resolution of the textures.
![Capture d’écran 2022-06-23 214209](https://user-images.githubusercontent.com/55276408/175396932-5e4f57bc-5a16-4e8c-9f3d-005370595818.png)

In your game manager, you need to get your script and define your list of entities inherited from IFogOfWarEntity.
For example in the GameManager singleton, each spawn or destroy unit calls these functions:
```c#
    public void RegisterUnit(Unit unit)
    {
        terrainFogOfWar.RegisterEntity(unit);
    }
    
    public void UnregisterUnit(Unit unit)
    {
        terrainFogOfWar.UnregisterEntity(unit);
    }
```
You can also get fog of war data to hide object for example. See example in sample.

## Implementation
The fog of war uses a render texture to be drawn.  
This renderTexture is filled with a compute shader and is used by the screen space post-processing effect. 
