using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.SceneLoader;
using StarSalvager.Cameras;
using UnityEngine.SceneManagement;

namespace StarSalvager
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button m_toGameplayButton;
        [SerializeField]
        private Slider m_cameraZoomScaler;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        // Start is called before the first frame update
        void Start()
        {
            m_toGameplayButton.onClick.AddListener(ToGameplayButtonPressed);
            m_cameraZoomScaler.onValueChanged.AddListener(ScaleCamera);

            if (gameObject.scene == SceneManager.GetActiveScene())
                ScaleCamera(m_cameraZoomScaler.value);
        }

        void OnDestroy()
        {
            m_toGameplayButton.onClick.RemoveListener(ToGameplayButtonPressed);
        }

        private void ScaleCamera(float cameraZoomScalerValue)
        {
            Values.Globals.ColumnsOnScreen = (int)cameraZoomScalerValue;
            if (Values.Globals.ColumnsOnScreen % 2 == 0)
                Values.Globals.ColumnsOnScreen += 1;
            CameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen, Vector3.zero);
            Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Values.Constants.GridWidthRelativeToScreen);
            Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Values.Constants.GridHeightRelativeToScreen * 2) / Values.Constants.gridCellSize);
        }

        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "MainMenuScene");
        }
    }
}