using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class Cell : MonoBehaviour
    {
        public int xOffset;
        public int yOffset;
        public int matchType;

        // Start is called before the first frame update
        void Start()
        {
            xOffset = Mathf.RoundToInt((transform.position.x - transform.parent.position.x) / ScreenStuff.colSize);
            yOffset = Mathf.RoundToInt((transform.position.y - transform.parent.position.y) / ScreenStuff.rowSize);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ExplodeCell()
        {
            Animator anim;

            anim = gameObject.GetComponentInChildren<Bit>().GetComponent<Animator>();
            anim.enabled = true;
        }
    }
}