using UnityEditor;
using UnityEngine;

namespace StarSalvager.Editor
{
    //Based on: https://gamedev.stackexchange.com/a/166800
    public class SpritesToolsWindow : EditorWindow
    {
        //============================================================================================================//

        [MenuItem("Window/My Utilities/Sprites Tools")]
        public static void SpriteTools()
        {
            var spriteTools = GetWindow<SpritesToolsWindow>("Sprites Tools", true);
            spriteTools.Show();
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Texture2D))
                spriteTools.spritesheet = (Texture2D) Selection.activeObject;
        }
        
        //============================================================================================================//

        public Texture2D spritesheet;

        private Vector2Int _mCommonPivot;
        private string _mLog;

        //============================================================================================================//
        
        private void OnGUI()
        {

            spritesheet =
                EditorGUILayout.ObjectField("Texture SpriteSheet", spritesheet, typeof(Texture2D), false) as
                    Texture2D;
            _mCommonPivot = EditorGUILayout.Vector2IntField("Pivot (Pixels)", _mCommonPivot);
            using (new EditorGUI.DisabledGroupScope(spritesheet == null))
                if (GUILayout.Button("Edit sprites"))
                    EditSprites();

            _mLog = EditorGUILayout.TextArea(_mLog);
        }

        //============================================================================================================//
        
        private void EditSprites()
        {
            if (spritesheet != null)
            {
                var spritesheetPath = AssetDatabase.GetAssetPath(spritesheet);

                if (!string.IsNullOrEmpty(spritesheetPath))
                {

                    var importer = AssetImporter.GetAtPath(spritesheetPath) as TextureImporter;

                    if (importer != null && importer.spritesheet != null &&
                        importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        var spritesMetaData = importer.spritesheet;
                        for (var i = 0; i < spritesMetaData.Length; i++)
                        {
                            var metaData = spritesMetaData[i];
                            metaData.pivot = new Vector2(_mCommonPivot.x / metaData.rect.width,
                                _mCommonPivot.y / metaData.rect.height);
                            metaData.alignment = (int) SpriteAlignment.Custom;
                            spritesMetaData[i] = metaData;
                        }

                        importer.spritesheet =
                            spritesMetaData; // seems like this setter internally change stuff (needed)
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();

                        _mLog += $"Edited {spritesMetaData.Length} sprites in {spritesheetPath}\n";
                        return;
                    }

                    _mLog += "Texture is not a spritesheet.\n";
                }
            }

            _mLog += "Could not complete action.\n";
        }
        
        //============================================================================================================//
    }
}