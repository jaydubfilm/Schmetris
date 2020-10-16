using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using UnityEngine;

namespace StarSalvager.Tutorial.Data
{
    [CreateAssetMenu(fileName = "Tutorial Remote Data", menuName = "Star Salvager/Tutorial/Remote Data Asset")]
    public class TutorialDataScriptableObject : ScriptableObject
    {
        [Required]
        public SectorRemoteDataScriptableObject SectorRemoteData;
        //Sector wave[0] stages:
        //----------------------
        // [0]: Nothing
        // [1]: Singles. Green Grey, Blue Yellow
        // [2]: Singles. Grey, Yellow.
        // [3]: Singles. Bumper, Green, Grey, Blue, Yellow
        // [4]: Singles. Red
        
        [SerializeField, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        private List<TutorialStepData> tutorialSteps = new List<TutorialStepData>
        {
            /* [0] */new TutorialStepData {title = "Intro Step"},
            /* [1] */new TutorialStepData {title = "Movement"},
            /* [2] */new TutorialStepData {title = "Rotate"},
            /* [3] */new TutorialStepData {title = "Falling Bits"},
            /* [4] */new TutorialStepData {title = "Combo"},
            /* [5] */new TutorialStepData {title = "Magnet"},

            /* [6] */new TutorialStepData {title = "Combo-magnet-1"},
            /* [7] */new TutorialStepData {title = "Combo-magnet-2"},

            /* [8] */new TutorialStepData {title = "Magnet-combo-1"},
            /* [9] */new TutorialStepData {title = "Magnet-combo-2"},

            /* [10] */new TutorialStepData {title = "Pulsar"},
            /* [11] */new TutorialStepData {title = "Pulsar-1"},

            /* [12] */new TutorialStepData {title = "Fuel"},
            /* [13] */new TutorialStepData {title = "Fuel-1"},
            /* [14] */new TutorialStepData {title = "Fuel-2"},
        };

        public TutorialStepData GetTutorialStepData(int index)
        {
            return tutorialSteps[index];
        }
        
        public TutorialStepData this[int index] => tutorialSteps[index];
    }
}
