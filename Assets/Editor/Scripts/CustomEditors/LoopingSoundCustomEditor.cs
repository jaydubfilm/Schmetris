using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using StarSalvager.Audio.Data;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Editor.CustomEditors
{
    public class LoopingSoundCustomEditor : OdinValueDrawer<LoopingSound>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            var loopingSound = ValueEntry.SmartValue;
            
            GUIHelper.PushLabelWidth(0);
            loopingSound.clip = EditorGUI.ObjectField(rect.AlignLeft(rect.width * 0.35f), string.Empty, loopingSound.clip, typeof(AudioClip), false) as AudioClip;
            GUIHelper.PopLabelWidth();
            
            GUIHelper.PushLabelWidth(100);
            loopingSound.maxChannels = EditorGUI.IntSlider(rect.AlignRight(rect.width * 0.65f), "Max Channels", loopingSound.maxChannels, 0, 32);
            GUIHelper.PopLabelWidth();


            ValueEntry.SmartValue = loopingSound;
        }
    }
}
