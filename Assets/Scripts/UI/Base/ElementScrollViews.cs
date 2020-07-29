using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.UI
{
    [System.Serializable]
    public class PartUIElementScrollView: UIElementContentScrollView<PartRemoteData>
    {}

    [System.Serializable]
    public class CategoryElementScrollView : UIElementContentScrollView<string>
    { }

    [System.Serializable]
    public class ResourceUIElementScrollView: UIElementContentScrollView<ResourceAmount>
    {}

    [System.Serializable]
    public class BotShapeDataElementScrollView : UIElementContentScrollView<EditorGeneratorDataBase>
    { }
}

