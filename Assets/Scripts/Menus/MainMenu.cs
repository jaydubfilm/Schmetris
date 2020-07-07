﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.SceneLoader;
using StarSalvager.Cameras;
using UnityEngine.SceneManagement;
using StarSalvager.Factories;
using StarSalvager.Values;
using StarSalvager.Cameras.Data;

namespace StarSalvager
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button m_toGameplayButton;
        [SerializeField]
        private Button m_toggleBitButton;
        [SerializeField]
        private Button m_toggleOrientationButton;
        [SerializeField]
        private Slider m_cameraZoomScaler;

        [SerializeField]
        private Button quitButton;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        // Start is called before the first frame update
        void Start()
        {
            m_toGameplayButton.onClick.AddListener(ToGameplayButtonPressed);
            m_toggleBitButton.onClick.AddListener(ToggleBitButtonPressed);
            m_toggleOrientationButton.onClick.AddListener(RotateOrientation);
            m_cameraZoomScaler.onValueChanged.AddListener(ScaleCamera);

            if (gameObject.scene == SceneManager.GetActiveScene())
                ScaleCamera(m_cameraZoomScaler.value);
            
            quitButton.onClick.AddListener(Application.Quit);
        }

        void OnDestroy()
        {
            m_toGameplayButton.onClick.RemoveListener(ToGameplayButtonPressed);
            m_toggleBitButton.onClick.RemoveListener(ToggleBitButtonPressed);
            m_toggleOrientationButton.onClick.RemoveListener(RotateOrientation);
        }

        private void ScaleCamera(float cameraZoomScalerValue)
        {
            Values.Globals.ColumnsOnScreen = (int)cameraZoomScalerValue;
            if (Values.Globals.ColumnsOnScreen % 2 == 0)
                Values.Globals.ColumnsOnScreen += 1;
            CameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen, Vector3.zero);

            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Values.Constants.GridWidthRelativeToScreen);
                Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Values.Constants.GridHeightRelativeToScreen * 2) / Values.Constants.gridCellSize);
            }
            else
            {
                Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Values.Constants.GridWidthRelativeToScreen * (Screen.height / (float)Screen.width));
                Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Values.Constants.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Values.Constants.gridCellSize);
            }
        }

        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "MainMenuScene");
        }

        private void ToggleBitButtonPressed()
        {
            FactoryManager.Instance.ToggleBitProfile();
        }

        private void RotateOrientation()
        {
            if (Globals.Orientation == ORIENTATION.HORIZONTAL)
                Globals.Orientation = ORIENTATION.VERTICAL;
            else
                Globals.Orientation = ORIENTATION.HORIZONTAL;
        }
    }
}