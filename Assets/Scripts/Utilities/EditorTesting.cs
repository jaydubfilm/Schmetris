using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.UI;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class EditorTesting : MonoBehaviour
    {
#if UNITY_EDITOR

        [Button("Clear Remote Data"), DisableInPlayMode, HorizontalGroup("Row1"), GUIColor(0.8f, 0.2f,0.2f)]
        private void ClearRemoteData()
        {
            Files.ClearRemoteData();
        }

        [Button("Show Current Account Stats"), DisableInEditorMode, HorizontalGroup("Row1"), PropertyOrder(-100)]
        private void ShowAccountStats()
        {
            if (!Application.isPlaying) 
                return;
            
            if (PlayerDataManager.HasPlayerAccountData())
            {
                var newString = PlayerDataManager.GetAccountSummaryString();

                Alert.ShowAlert("Account Tracking Statistics", newString, "Ok", null);
            }
            else
            {
                Alert.ShowAlert("No Account Loaded", "No account loaded. Load an account and then click this again.", "Ok", null);
            }
        }

#endif
    }
}
