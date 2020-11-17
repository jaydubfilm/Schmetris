using UnityEngine;

namespace StarSalvager.Prototype
{
    public class MoveWithObstacles : MonoBehaviour, IObstacle
    {
        //IObstacles Properties
        //====================================================================================================================//
        
        public bool CanMove => true;
        public bool IsRegistered { get; set; }
        public bool IsMarkedOnGrid { get; set; }

        //Unity Functions
        //====================================================================================================================//
        
        private void OnEnable()
        {
            LevelManager.Instance.ObstacleManager.AddObstacleToList(this);
        }

        private void OnDestroy()
        {
            if (!LevelManager.Instance)
                return;
            
            LevelManager.Instance.ObstacleManager.ForceRemoveObstacleFromList(this);
        }

        //IObstacle Functions
        //====================================================================================================================//
        
        public void SetColliderActive(bool active)
        { }
    }
}
