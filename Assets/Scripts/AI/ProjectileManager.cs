using StarSalvager.AI;
using System.Collections.Generic;
using Recycling;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using Input = UnityEngine.Input;

public class ProjectileManager : IReset
{
    private List<Projectile> m_projectiles;

    //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
    //private float m_currentInput;

   // public bool Moving => _moving;
   // private bool _moving;

    //private float m_distanceHorizontal;
    
    //================================================================================================================//

    public ProjectileManager()
    {
        m_projectiles = new List<Projectile>();
       // RegisterMoveOnInput();
    }
    
    //IReset Functions
    //================================================================================================================//

    public void Activate()
    {

    }

    public void Reset()
    {
        for (int i = m_projectiles.Count - 1; i >= 0; i--)
        {
            Recycler.Recycle<Projectile>(m_projectiles[i].gameObject);
            m_projectiles.RemoveAt(i);
        }
    }
    
    //================================================================================================================//

    public void CleanProjectiles()
    {
        /*Vector3 gridMovement = Vector3.zero;
        if (m_distanceHorizontal != 0)
        {
            if (m_distanceHorizontal > 0)
            {
                float toMove = Mathf.Min(m_distanceHorizontal, Globals.BotHorizontalSpeed * Time.deltaTime);
                gridMovement = Vector3.right * toMove;
                m_distanceHorizontal -= toMove;
            }
            else if (m_distanceHorizontal < 0)
            {
                float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Globals.BotHorizontalSpeed * Time.deltaTime);
                gridMovement = Vector3.left * toMove;
                m_distanceHorizontal += toMove;
            }
        }*/

        CleanProjectiles(Globals.GridSizeY * 1.5f * Constants.gridCellSize);
            
        /*foreach (var projectile in m_projectiles)
        {
            projectile.transform.position -= gridMovement;
        }
        
        

        if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
        {
            Move(m_currentInput);
        }*/
    }

    public void AddProjectile(Projectile newProjectile)
    {
        m_projectiles.Add(newProjectile);
    }

    //IMoveOnInput functions
    //================================================================================================================//
    
   /* public void RegisterMoveOnInput()
    {
        InputManager.RegisterMoveOnInput(this);
    }

    public void Move(float direction)
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            m_currentInput = 0f;
            return;
        }

        m_currentInput = direction;

        m_distanceHorizontal += direction * Constants.gridCellSize;

        _moving = true;
    }*/
    
    //================================================================================================================//

    private void CleanProjectiles(float maxY)
    {
        for (int i = m_projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = m_projectiles[i];
            if (projectile == null)
            {
                m_projectiles.RemoveAt(i);
                continue;
            }

            if (!projectile.gameObject.activeSelf)
            {
                m_projectiles.RemoveAt(i);
                continue;
            }

            if (Mathf.Abs(projectile.transform.position.y) >= maxY)
            {
                Recycler.Recycle<Projectile>(projectile);
                
                m_projectiles.RemoveAt(i);
                continue;
            }
            
            
        }
    }
    
    //================================================================================================================//
}
