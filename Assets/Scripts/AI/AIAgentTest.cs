﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentTest : MonoBehaviour
{
    public new Transform transform;
    public Vector2 m_agentDestination = Vector2.zero;

    void Awake()
    {
        transform = gameObject.transform;
    }
}
