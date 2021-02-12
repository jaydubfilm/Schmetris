using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarSalvager
{
    public class DecoyDrone : MonoBehaviour
    {
        [NonSerialized]
        public Bot bot;

        private float m_timer = 0.0f;

        private float m_timeAlive = 5.0f;
        private Vector2 m_positionMoveUpwards;

        // Start is called before the first frame update
        void Start()
        {
            m_positionMoveUpwards = (Vector2)transform.position + (Vector2.up * 10.0f * Constants.gridCellSize);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_timer >= m_timeAlive)
            {
                bot.DecoyDrone = null;
                Destroy(gameObject);
            }

            transform.position = Vector2.Lerp(transform.position, m_positionMoveUpwards, Time.deltaTime);
            m_timer += Time.deltaTime;
        }
    }
}