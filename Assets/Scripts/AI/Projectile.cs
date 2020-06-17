using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Factories.Data;

namespace StarSalvager
{
    public class Projectile : MonoBehaviour
    {
        public new Transform transform;
        private SpriteRenderer m_spriteRenderer;

        public Vector3 m_travelDirectionNormalized = Vector3.zero;
        public ProjectileProfileData m_projectileData;

        private void Awake()
        {
            transform = gameObject.transform;
            m_spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            m_spriteRenderer.sprite = m_projectileData.Sprite;
        }

        // Update is called once per frame
        private void Update()
        {
            transform.position += m_travelDirectionNormalized * m_projectileData.ProjectileSpeed * Time.deltaTime;
        }
    }
}