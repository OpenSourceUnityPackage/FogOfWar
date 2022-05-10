using UnityEngine;

namespace FogOfWarPackage
{
    public interface IFogOfWarEntity
    {
        Vector2 GetVisibilityPosition();
        float GetVisibilityRadius();
        float GetPermanentVisibilityRadius();
    }
}