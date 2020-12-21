using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;
using StarSalvager.Projectiles;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class ProjectileProfileData
    {
        public enum TowType : int
        {
            JunkBit
        }
        
        public string ProjectileType => m_projectileType;

        public string ProjectileTypeID => m_projectileTypeID;

        public Sprite Sprite => m_sprite;

        public bool UseTrail => useTrail;
        public Color Color => color;

        public float ProjectileSpeed => m_projectileSpeed;

        public float ProjectileRange => m_projectileRange;

        public bool RequiresRotation => m_requiredRotate;

        public FIRE_TYPE FireType => fireType;
        public bool FireAtTarget => fireTowardsTarget;

        public float SpreadAngle => m_spreadAngle;

        public int SprayCount => m_sprayCount;

        public bool IsTow => m_isTow;
        public TowType TowObjectType => m_towType;
        public bool CanHitAsteroids => m_canHitAsteroids;
        public bool AddVelocityToProjectiles => m_addVelocityToProjectiles;

        //====================================================================================================================//

        [SerializeField, FoldoutGroup("$ProjectileType"), HorizontalGroup("$ProjectileType/row1"), DisplayAsString]
        private string m_projectileTypeID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        [Button("Copy"), HorizontalGroup("$ProjectileType/row1", 45)]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_projectileTypeID;
        }

#endif

        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$ProjectileType/row2", 65), VerticalGroup("$ProjectileType/row2/left"), HideLabel]
        private Sprite m_sprite;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private string m_projectileType;

        [SerializeField, HorizontalGroup("$ProjectileType/row2/right/trail")]
        private bool useTrail;

        [SerializeField, HorizontalGroup("$ProjectileType/row2/right/trail"), HideLabel, EnableIf("useTrail")]
        private Color color = Color.white;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private float m_projectileSpeed;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right"),
         InfoBox("A value of 0 means that the projectile will continue until offscreen", VisibleIf = "noRange"),
         SuffixLabel("units", true)]
        private float m_projectileRange;

        private bool noRange => m_projectileRange == 0f;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right"), LabelText("Sprite Must Rotate")]
        private bool m_requiredRotate;

        //====================================================================================================================//

        /*[SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private ENEMY_ATTACKTYPE m_attackType;*/
        
        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private FIRE_TYPE fireType;
        
        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool fireTowardsTarget;

        private bool showSpreadAngle => fireType == FIRE_TYPE.FIXED_SPRAY || showSprayCount;

        private bool showSprayCount => fireType == FIRE_TYPE.RANDOM_SPRAY ||
                                       fireType == FIRE_TYPE.FIXED_SPRAY;




        [SerializeField, VerticalGroup("$ProjectileType/row2/right"), ShowIf(nameof(showSpreadAngle))]
        private float m_spreadAngle;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right"),
         ShowIf(nameof(showSprayCount))]
        private int m_sprayCount;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_isTow;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right"), ShowIf("m_isTow")]
        private TowType m_towType;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_canHitAsteroids;

        [SerializeField, VerticalGroup("$ProjectileType/row2/right")]
        private bool m_addVelocityToProjectiles;

        //====================================================================================================================//

    }
}