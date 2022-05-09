using System.Collections;
using System.Collections.Generic;
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
