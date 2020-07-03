using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.SceneLoader;

namespace StarSalvager
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button m_toGameplayButton;

        // Start is called before the first frame update
        void Start()
        {
            m_toGameplayButton.onClick.AddListener(ToGameplayButtonPressed);
        }

        void OnDestroy()
        {
            m_toGameplayButton.onClick.RemoveListener(ToGameplayButtonPressed);
        }

        private void ScaleCamera()
        {
            CameraController.SetOrthographicSize(Values.gridCellSize * ColumnsOnScreen);
            m_gridSizeX = (int)(ColumnsOnScreen * Values.GridWidthRelativeToScreen);
            m_gridSizeY = (int)((Camera.main.orthographicSize * Values.GridHeightRelativeToScreen * 2) / Values.gridCellSize);
        }

        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "MainMenuScene");
        }
    }
}