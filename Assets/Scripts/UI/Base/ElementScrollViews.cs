using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.UI
{
    [System.Serializable]
    public class PartUIElementScrollView: UIElementContentScrollView<BrickImageUIElement, RemoteDataBase>
    {}

    [System.Serializable]
    public class CategoryElementScrollView : UIElementContentScrollView<CategoryToggleUIElement, string>
    { }

    [System.Serializable]
    public class ResourceUIElementScrollView: UIElementContentScrollView<ResourceUIElement, ResourceAmount>
    {}

    [System.Serializable]
    public class BotShapeDataElementScrollView : UIElementContentScrollView<BotLoadListUIElement, EditorGeneratorDataBase>
    { }

    [System.Serializable]
    public class LayoutElementScrollView : UIElementContentScrollView<LayoutUIElement, ScrapyardLayout>
    { }
}

