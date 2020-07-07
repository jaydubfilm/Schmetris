﻿using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.UI
{
    [System.Serializable]
    public class PartUIElementScrollView: UIElementContentScrollView<PartRemoteData>
    {}
    
    [System.Serializable]
    public class ResourceUIElementScrollView: UIElementContentScrollView<ResourceAmount>
    {}
}
