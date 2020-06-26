using StarSalvager.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Constants;

public class ProjectileManager
{
    private List<Projectile> m_projectiles;

    //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
    private float m_currentInput;

    public bool Moving => _moving;
    private bool _moving;

    private float m_distanceHorizontal = 0.0f;

    public ProjectileManager()
    {
        m_projectiles = new List<Projectile>();
    }

    public void UpdateForces()
    {
        Vector3 gridMovement = Vector3.zero;
        if (m_distanceHorizontal != 0)
        {
            if (m_distanceHorizontal > 0)
            {
                float toMove = Mathf.Min(m_distanceHorizontal, Values.botHorizontalSpeed * Time.deltaTime);
                gridMovement = Vector3.right * toMove;
                m_distanceHorizontal -= toMove;
            }
            else if (m_distanceHorizontal < 0)
            {
                float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Values.botHorizontalSpeed * Time.deltaTime);
                gridMovement = Vector3.left * toMove;
                m_distanceHorizontal += toMove;
            }
        }

        for (int i = m_projectiles.Count - 1; i >= 0; i--)
        {
            if (m_projectiles[i] == null)
            {
                m_projectiles.RemoveAt(i);
                continue;
            }

            if (!m_projectiles[i].gameObject.activeSelf)
            {
                m_projectiles.RemoveAt(i);
                continue;
            }
            
            m_projectiles[i].transform.position -= gridMovement;
        }

        if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
        {
            Move(m_currentInput);
        }
    }

    public void AddProjectile(Projectile newProjectile)
    {
        m_projectiles.Add(newProjectile);
    }

    public void Move(float direction)
    {
        if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
        {
            m_currentInput = 0f;
            return;
        }

        m_currentInput = direction;

        m_distanceHorizontal += direction * Values.gridCellSize;

        _moving = true;
    }
}
