using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class ProjectileProfileData
    {
        [SerializeField, FoldoutGroup("$ProjectileType"), HorizontalGroup("$ProjectileType/row1"), DisplayAsString]
        private string m_projectileTypeID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        [Button("Copy"),HorizontalGroup("$ProjectileType/row1", 45)]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_projectileTypeID;
        }
        
#endif
        
        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("$ProjectileType/row2", 65), VerticalGroup("$ProjectileType/row2/left"), HideLabel]
        private Sprite m_sprite;
        
        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private string m_projectileType;
        
        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private float m_projectileSpeed;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_requiredRotate;
        

        public string ProjectileType => m_projectileType;

        public string ProjectileTypeID => m_projectileTypeID;

        public Sprite Sprite => m_sprite;

        public float ProjectileSpeed => m_projectileSpeed;

        public bool RequiresRotation => m_requiredRotate;
    }
}