using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.UI.Hints;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Hints
{
    [CreateAssetMenu(fileName = "Hint Remote Data", menuName = "Star Salvager/Scriptable Objects/Hint Remote Data")]
    public class HintRemoteDataScriptableObject : ScriptableObject
    {
        [Serializable]
        public struct HintData
        {
            [FoldoutGroup("$type")]
            public HINT type;
            /*[FoldoutGroup("$type")]
            public string shortText;
            [FoldoutGroup("$type"), TextArea]
            public string longDescription;*/
            
            [FoldoutGroup("$type")]
            public List<HintText> hintTexts;
        }

        [Serializable]
        public struct HintText
        {
            [FoldoutGroup("$shortText")]
            public string shortText;
            [FoldoutGroup("$shortText"), TextArea]
            public string longDescription;
            [FoldoutGroup("$shortText")]
            public string continueText;
            [FoldoutGroup("$shortText")]
            public bool useMechanic;

        }

        [SerializeField, ListDrawerSettings(ShowPaging = false)]
        private HintData[] HintDatas;

        public HintData GetHintData(HINT hint)
        {
            return hint == HINT.NONE ? default : HintDatas.FirstOrDefault(x => x.type == hint);
        }

    }
}
