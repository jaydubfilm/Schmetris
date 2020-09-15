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

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private ENEMY_ATTACKTYPE m_attackType;

        private bool showSpreadAngle => m_attackType == ENEMY_ATTACKTYPE.AtPlayerCone || m_attackType == ENEMY_ATTACKTYPE.Spray;
        [SerializeField, VerticalGroup("$ProjectileType/row2/right"), ShowIf("showSpreadAngle")]
        private float m_spreadAngle;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right"), ShowIf("m_attackType", ENEMY_ATTACKTYPE.Spray)]
        private int m_sprayCount;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_canHitAsteroids;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_addVelocityToProjectiles;


        public string ProjectileType => m_projectileType;

        public string ProjectileTypeID => m_projectileTypeID;

        public Sprite Sprite => m_sprite;

        public float ProjectileSpeed => m_projectileSpeed;

        public bool RequiresRotation => m_requiredRotate;

        public ENEMY_ATTACKTYPE AttackType => m_attackType;

        public float SpreadAngle => m_spreadAngle;

        public int SprayCount => m_sprayCount;

        public bool CanHitAsteroids => m_canHitAsteroids;
        public bool AddVelocityToProjectiles => m_addVelocityToProjectiles;
    }
}