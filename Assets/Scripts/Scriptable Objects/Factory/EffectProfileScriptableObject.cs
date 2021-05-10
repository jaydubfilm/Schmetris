using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Effect_Profile", menuName = "Star Salvager/Scriptable Objects/Effect Profile")]
    public class EffectProfileScriptableObject : ScriptableObject
    {
        //Simple Sprite Prefab
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Basic")]
        public GameObject simpleSpritePrefab;
        
        //Bot Effect Prefabs
        //====================================================================================================================//

        [SerializeField, Required, BoxGroup("Bot")]
        public GameObject alertIconPrefab;

        //Damage Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Damage")]
        public GameObject damageEffectPrefab;

        //Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject weldEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject impactEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject thrustEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject explosionEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject mergeEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject lineRendererPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject trailRendererPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject bonusShapeEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Effects")]
        public GameObject bonusShapeParticlesPrefab;

        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitBlueParticlePrefab;
        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitGreenParticlePrefab;
        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitGreyParticlePrefab;
        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitRedParticlePrefab;
        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitWhiteParticlePrefab;
        [SerializeField, Required, BoxGroup("Effects/Bits")]
        public GameObject bitYellowParticlePrefab;

        //Part Effects
        //====================================================================================================================//
        [SerializeField, Required, BoxGroup("Part Effects")]
        public Sprite tripleTurretSprite;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject repairEffectPrefab;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject refinerEffectPrefab;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject boostRateEffectPrefab;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject gunEffectPrefab;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject freezeShockwaveEffectPrefab;
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject bombShockwaveEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject shieldEffectPrefab;
        
        [SerializeField, Required, BoxGroup("Part Effects")]
        public GameObject blasterLineEffectPrefab;

        //Particle Effect Prefabs
        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject labelPrefab;

        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject floatingTextPrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject magnetIconSpritePrefab;
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject fadeSpritePrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        public GameObject lineShrinkPrefab;

        //====================================================================================================================//
        
    }
}
