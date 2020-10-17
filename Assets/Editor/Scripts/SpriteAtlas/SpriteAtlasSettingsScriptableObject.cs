using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace StarSalvager.Editor.CustomEditors
{
    [CreateAssetMenu(fileName = "Sprite Atlas Settings", menuName = "Star Salvager/Sprite Atlas/Settings Asset")]
    public class SpriteAtlasSettingsScriptableObject : ScriptableObject
    {
        public SpriteAtlas partsAtlas;
        public SpriteAtlas bitsAtlas;


        [FolderPath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Sprites/")]
        public string partSpritePath;

        [FolderPath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Sprites/")]
        public string bitSpritePath;
    }
}
