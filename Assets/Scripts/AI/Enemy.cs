using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;

namespace StarSalvager
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyTypeScriptableObject m_enemyType;

        private SpriteRenderer m_spriteRenderer;

        private void Awake()
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();

            m_spriteRenderer.sprite = m_enemyType.GetSprite();
        }

        private void Start()
        {
            Debug.Log("Enemy Name: " + m_enemyType.GetName() + " with health " + m_enemyType.GetHealth());
        }
    }
}