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
            JunkBit,
            Mine,
            Bumper,
        }

        //Properties
        //====================================================================================================================//

        #region Public Properties

        public string ProjectileType => m_projectileType;

        public string ProjectileTypeID => m_projectileTypeID;

        public Sprite Sprite => m_sprite;

        public bool UseTrail => useTrail;
        public Color Color => color;

        public float ProjectileDamage => m_projectileDamage;
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
        
        public bool DestroyAtScreenBottom => destroyAtScreenBottom;
        public bool EffectOnDeath => effectOnDeath;

        #endregion //Public Properties

        [SerializeField, FoldoutGroup("$title"), HorizontalGroup("$title/row1"), DisplayAsString]
        private string m_projectileTypeID = System.Guid.NewGuid().ToString();

        [SerializeField, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$title/row2", 65), VerticalGroup("$title/row2/left"), HideLabel]
        private Sprite m_sprite;
        
        [VerticalGroup("$title/row2/right")]
        public bool isImplemented = true;

        [SerializeField, VerticalGroup("$title/row2/right")]
        private string m_projectileType;

        [SerializeField, HorizontalGroup("$title/row2/right/trail")]
        private bool useTrail;

        [SerializeField, HorizontalGroup("$title/row2/right/trail"), HideLabel, EnableIf("useTrail")]
        private Color color = Color.white;
        
        

        [SerializeField, FoldoutGroup("$title")]
        private float m_projectileDamage;

        [SerializeField, FoldoutGroup("$title")]
        private float m_projectileSpeed;

        [SerializeField,FoldoutGroup("$title"),
         InfoBox("A value of 0 means that the projectile will continue until offscreen", VisibleIf = "noRange"),
         SuffixLabel("units", true)]
        private float m_projectileRange;

        private bool noRange => m_projectileRange == 0f;

        [SerializeField, FoldoutGroup("$title"), LabelText("Sprite Must Rotate")]
        private bool m_requiredRotate;

        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("$title")]
        private FIRE_TYPE fireType;
        
        [SerializeField, FoldoutGroup("$title")]
        private bool fireTowardsTarget;

        private bool showSpreadAngle => fireType == FIRE_TYPE.FIXED_SPRAY || showSprayCount;

        private bool showSprayCount => fireType == FIRE_TYPE.RANDOM_SPRAY ||
                                       fireType == FIRE_TYPE.FIXED_SPRAY;




        [SerializeField, FoldoutGroup("$title"), ShowIf(nameof(showSpreadAngle))]
        private float m_spreadAngle;

        [SerializeField, FoldoutGroup("$title"),
         ShowIf(nameof(showSprayCount))]
        private int m_sprayCount;

        [SerializeField, FoldoutGroup("$title")]
        private bool m_isTow;

        [SerializeField, FoldoutGroup("$title"), ShowIf("m_isTow")]
        private TowType m_towType;

        [SerializeField, FoldoutGroup("$title")]
        private bool m_canHitAsteroids;

        [SerializeField, FoldoutGroup("$title")]
        private bool m_addVelocityToProjectiles;
        
        [SerializeField, FoldoutGroup("$title")]
        private bool destroyAtScreenBottom;
        
        [SerializeField, FoldoutGroup("$title")]
        private bool effectOnDeath;

        //Unity Editor
        //====================================================================================================================//
        
        public string title => $"{ProjectileType} {(isImplemented ? string.Empty : "[NOT IMPLEMENTED]")}";

#if UNITY_EDITOR
        [Button("Copy"), HorizontalGroup("$title/row1", 45)]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_projectileTypeID;
        }

#endif

        //====================================================================================================================//
        
    }
}