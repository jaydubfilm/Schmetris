using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.UI
{
    [System.Serializable]
    public class PartUIElementScrollView: UIElementContentScrollView<RemoteDataBase>
    {}

    [System.Serializable]
    public class CategoryElementScrollView : UIElementContentScrollView<string>
    { }

    [System.Serializable]
    public class ResourceUIElementScrollView: UIElementContentScrollView<CraftCost>
    {}

    [System.Serializable]
    public class BotShapeDataElementScrollView : UIElementContentScrollView<EditorGeneratorDataBase>
    { }

    [System.Serializable]
    public class LayoutElementScrollView : UIElementContentScrollView<ScrapyardLayout>
    { }
}

