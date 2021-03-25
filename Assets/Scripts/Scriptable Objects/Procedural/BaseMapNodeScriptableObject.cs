using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    public abstract class BaseMapNodeScriptableObject : ScriptableObject
    {
        
        
        [LabelWidth(35), VerticalGroup("row1/col2")]
        public string name;
        [LabelWidth(35), VerticalGroup("row1/col2"), OnValueChanged("UpdateSpritePreview")]
        public Sprite Sprite;
        
        
        #if UNITY_EDITOR
        
        private const float PREVIEW_SIZE = 100;

        
        [HorizontalGroup("row1", PREVIEW_SIZE)]
        [SerializeField, PreviewField(Height = PREVIEW_SIZE, Alignment = ObjectFieldAlignment.Right), VerticalGroup("row1/col1", Order = -100), HideLabel, ReadOnly]
        private Sprite spritePreview;
        
        private void UpdateSpritePreview()
        {
            spritePreview = Sprite;
        }
        
        #endif
    }
}
