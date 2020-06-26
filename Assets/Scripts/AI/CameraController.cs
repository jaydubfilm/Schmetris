﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 startPos;
    Vector3 edgePos;
    Vector3 targetPos;
    public float smoothing = 4.0f;
    private float horzExtent;

    //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
    private float m_currentInput;

    protected new Transform transform
    {
        get
        {
            if (m_transform == null)
                m_transform = gameObject.GetComponent<Transform>();

            return m_transform;
        }
    }
    private Transform m_transform;

    //Init
    void Start()
    {
        DontDestroyOnLoad(this);
        startPos = transform.position;
        targetPos = startPos;
        horzExtent = (Camera.main.orthographicSize * Screen.width / Screen.height) / 2;
    }

    //Smooth camera to center over bot
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing * Time.deltaTime);
    }

    public void Move(float direction)
    {
        m_currentInput = direction;

        if (m_currentInput == 0)
        {
            targetPos = startPos;
            return;
        }

        Vector3 cameraOff = startPos;
        cameraOff.x += -direction * horzExtent;

        edgePos = cameraOff;
        targetPos = edgePos;
    }
}