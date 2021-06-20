using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public interface ISpineOverride
{
    MeshRenderer renderer { get; }
    SkeletonAnimation StateAnimator { get; }

    void SetSprite(Sprite sprite);
    void SetColor(Color color);
    void SetSortingLayer(string sortingLayerName, int sortingOrder = 0);
}
