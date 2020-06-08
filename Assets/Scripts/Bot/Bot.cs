using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager;
using UnityEngine;

namespace StarSalvager
{
    public class Bot : MonoBehaviour, IHealth, IInput
    {
        //============================================================================================================//
        
        private List<AttachableBase> attachedBlocks;
        
        //============================================================================================================//

        public float StartingHealth => _startingHealth;
        private float _startingHealth;
        public float CurrentHealth => _currentHealth;
        private float _currentHealth;
        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }
        
        //============================================================================================================//
        public void AttachNewBit(Vector2Int coordinate, AttachableBase newAttachable, DIRECTION direction)
        {
            var target = attachedBlocks
                .FirstOrDefault(b => b.Coordinate == coordinate);
            
            if(target == null)
                throw new NullReferenceException($"Trying to add {newAttachable.name} to {coordinate}, but {coordinate} block does not exist");
            
            AttachNewBit(target, newAttachable, direction);
        }
        public void AttachNewBit(AttachableBase target, AttachableBase newAttachable, DIRECTION direction)
        {
            
        }
        

        //============================================================================================================//
        public void ChangeHealth(float amount)
        {
            _currentHealth += amount;
        }
        
        //============================================================================================================//
        
        public void InitInput()
        {
            throw new System.NotImplementedException();
        }

        public void DeInitInput()
        {
            throw new System.NotImplementedException();
        }
        
        //============================================================================================================//
    }
}