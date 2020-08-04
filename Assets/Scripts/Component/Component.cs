using System.Collections;
using System.Collections.Generic;
using Recycling;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IAttachable
    {

 
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }
        public bool CountAsConnected { get; }
        public bool CanDisconnect { get; }
        public bool CanShift { get; }
        
        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            throw new System.NotImplementedException();
        }
        
        public void SetAttached(bool isAttached)
        {
            throw new System.NotImplementedException();
        }
    }
}

