using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ScrapyardUI : MonoBehaviour
    {
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject partElementPrefab;

        [SerializeField, Required, BoxGroup("Part UI")]
        private RectTransform partListContentTransform;
        
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;
        
        //============================================================================================================//
        
        [SerializeField, BoxGroup("View")]
        private SliderText zoomSliderText;

        [SerializeField, Required, BoxGroup("View")]
        private Button leftTurnButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button rightTurnButton;
        
        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button SaveButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button LoadButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button ReadyButton;


        //============================================================================================================//

        [SerializeField]
        private Scrapyard m_scrapyard;

        
        private void Start()
        {
            zoomSliderText.Init();

            InitPartUI();

            InitButtons();
        }
        
        //============================================================================================================//

        private void InitButtons()
        {
            leftTurnButton.onClick.AddListener(() =>
            {
                m_scrapyard.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                m_scrapyard.RotateBots(1.0f);
            });

            ReadyButton.onClick.AddListener(() =>
            {
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
            });
        }

        private void InitPartUI()
        {
            //FIXME This needs to move to the Factory
            foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
            {
                var partTemp = Instantiate(partElementPrefab).GetComponent<PartUIElement>();
                partTemp.gameObject.name = $"{partRemoteData.partType}_UIElement";
                partTemp.transform.SetParent(partListContentTransform, false);
                partTemp.transform.localScale = Vector3.one;
                
                partTemp.Init(partRemoteData, PartPressed);
            }
        }
        
        //============================================================================================================//

        private void PartPressed(PART_TYPE partType)
        {
            Debug.Log($"Selected {partType}");
        }

    }
}

