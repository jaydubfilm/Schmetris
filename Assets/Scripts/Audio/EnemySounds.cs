using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.Audio.Enemies
{
    [Serializable]
    public class EnemySoundClip : BaseSound
    {
        public override AudioClip clip
        {
            get => _clip;
            set => _clip = value;
        }

        [SerializeField, AssetSelector(Paths = "Assets/Audio/SFX")]
        private AudioClip _clip;
    }

    public interface IPlayEnemySounds
    {
        EnemySoundBase EnemySoundBase { get; }
    }
    public interface IPlayEnemySounds<T> : IPlayEnemySounds where T: EnemySoundBase
    {
        T EnemySound { get; }
    }

    public abstract class EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip attackSound;
        [BoxGroup] public EnemySoundClip deathSound;
    }
    
    //====================================================================================================================//

    //FIXME If the enemies change a lot this setup will not be sustainable.

    [Serializable]
    public class FlySounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip moveDownSound;
    }

    [Serializable]
    public class DataLeechSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip latchOnSound;
    }

    [Serializable]
    public class MoonMinerSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip attackEnd;
        [BoxGroup] public EnemySoundClip chargeLaser;
    }

    [Serializable]
    public class VoltSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip anticipationSound;
    }

    [Serializable]
    public class SquartSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip fleeSound;
    }

    [Serializable]
    public class ToughMotherSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip spawnLeechSound;
        [BoxGroup] public EnemySoundClip shieldSound;
    }

    [Serializable]
    public class ShardSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip lockPositionSound;

        [FormerlySerializedAs("beginAttachFallSound")] [BoxGroup]
        public EnemySoundClip beginAttackFallSound;
    }

    [Serializable]
    public class SleeperMineSounds : EnemySoundBase
    {
        
    }

    [Serializable]
    public class SensorMineSounds : EnemySoundBase
    {
        [BoxGroup("Idle Loop"), HideLabel]
        public LoopingSound idleLoop;
        [BoxGroup("Warning Loop"), HideLabel]
        public LoopingSound warningLoop;
        
    }

    [Serializable]
    public class BorrowerSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip latchOntoBotSound;
        [BoxGroup] public EnemySoundClip waitSound;
    }

    [Serializable]
    public class LaserTurretSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip chargeLaser;
    }

    [Serializable]
    public class PulseCannonSounds : EnemySoundBase
    { }

    [Serializable]
    public class IceWingSounds : EnemySoundBase
    {
        [BoxGroup] public EnemySoundClip swoopSound;
        [BoxGroup] public EnemySoundClip freezeSound;
    }
}
