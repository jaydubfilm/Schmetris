using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager
{
    public class UniverseMap : MonoBehaviour
    {
        [SerializeField, Required]
        private UniverseMapButton m_universeSectorButtonPrefab;

        [SerializeField, Required]
        private Canvas m_universeCanvas;

        [SerializeField]
        private List<SectorRemoteDataScriptableObject> m_sectorRemoteData;

        [SerializeField, Required]
        private RectTransform m_scrollRectArea;

        void Start()
        {
            HaltonSequence positionsequence = new HaltonSequence();
            Vector2 size = new Vector2(m_scrollRectArea.rect.xMax - m_scrollRectArea.rect.xMin, m_scrollRectArea.rect.yMax - m_scrollRectArea.rect.yMin);
            positionsequence.Reset();
            Vector3 position = Vector3.zero;
            for (int i = 0; i < m_sectorRemoteData.Count; i++)
            {
                positionsequence.Increment();
                //position.set(positionsequence.m_CurrentPos);
                //Debug.Log(positionsequence.m_CurrentPos);
                position = positionsequence.m_CurrentPos;
                position.x -=0.5f;
                position.y -= 0.5f;
                position.z =0.0f;
                position.x *= size.x;
                position.y *= size.y;
                UniverseMapButton button = GameObject.Instantiate(m_universeSectorButtonPrefab);
                button.SetupWaveButtons(m_sectorRemoteData[i].GetNumberOfWaves());
                button.transform.SetParent(m_scrollRectArea.transform);
                button.transform.localPosition = position;
                button.Text.text = "Sector " + (i + 1).ToString();
                button.SectorNumber = i;
                button.Button.onClick.AddListener(() =>
                {
                    button.SetActiveWaveButtons(!button.ButtonsActive);
                    /*Alert.ShowAlert("Sector " + (button.SectorNumber + 1), "Sector information", "Play", "Cancel", b =>
                    {
                        if (b)
                        {
                            Values.Globals.CurrentSector = button.SectorNumber;
                            SceneLoader.ActivateScene("AlexShulmanTestScene", "UniverseMapScene");
                        }
                    });*/
                });
            }

            /*foreach (var sectorData in m_sectorRemoteData)
            {
                Vector2 position = GetRandomPositionInScrollRect();
                print(position);
                Button button = GameObject.Instantiate(m_universeSectorButtonPrefab);
                button.transform.SetParent(m_scrollRectArea.transform);
                button.transform.localPosition = position;
                button.onClick.AddListener(() =>
                {
                    print(m_sectorRemoteData.IndexOf(sectorData));
                });
            }*/
        }

        private Vector2 GetRandomPositionInScrollRect()
        {
            return new Vector2(Random.Range(m_scrollRectArea.rect.xMin, m_scrollRectArea.rect.xMax), Random.Range(m_scrollRectArea.rect.yMin, m_scrollRectArea.rect.yMax));
        }
    }
}