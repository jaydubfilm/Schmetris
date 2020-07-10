using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Editor_Bot_Shape_Generator", menuName = "Star Salvager/Scriptable Objects/Editor Bot Shape Generator")]
    public class EditorBotShapeGeneratorScriptableObject : ScriptableObject
    {
        public List<EditorBotGeneratorData> m_editorBotGeneratorData = new List<EditorBotGeneratorData>();
        public List<EditorShapeGeneratorData> m_editorShapeGeneratorData = new List<EditorShapeGeneratorData>();

        public EditorBotGeneratorData GetEditorBotData(string name)
        {
            return m_editorBotGeneratorData
                .FirstOrDefault(p => p.Name == name);
        }

        public EditorShapeGeneratorData? GetEditorShapeData(string name)
        {
            return m_editorShapeGeneratorData
                .FirstOrDefault(p => p.Name == name);
        }

        public void AddEditorBotData(EditorBotGeneratorData data)
        {
            m_editorBotGeneratorData.Add(data);
        }

        public void AddEditorShapeData(EditorShapeGeneratorData data)
        {
            m_editorShapeGeneratorData.Add(data);
        }
    }
}