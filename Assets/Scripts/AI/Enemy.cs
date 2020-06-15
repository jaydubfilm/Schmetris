﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;

namespace StarSalvager
{
    public class Enemy : MonoBehaviour
    {
        public new Transform transform;
        public Vector2 m_agentDestination = Vector2.zero;

        [SerializeField] private EnemyTypeScriptableObject m_enemyType;

        private SpriteRenderer m_spriteRenderer;

        public EnemyData m_enemyData;

        private void Awake()
        {
            transform = gameObject.transform;
            m_spriteRenderer = GetComponent<SpriteRenderer>();

            m_spriteRenderer.sprite = m_enemyType.GetSprite();
        }

        public void ProcessMovement(Vector3 direction)
        {
            transform.position = transform.position + (direction * m_enemyData.GetMovementSpeed() * Time.deltaTime);
        }
    }
}