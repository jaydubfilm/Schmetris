using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace StarSalvager.Editor.CustomEditors
{
    //Taken From: https://odininspector.com/tutorials/how-to-create-custom-drawers-using-odin/how-to-make-a-custom-group
    public class ColoredFoldoutGroupAttribute : PropertyGroupAttribute
    {
        public float R, G, B, A;

        public ColoredFoldoutGroupAttribute(string path)
            : base(path)
        {
        }

        public ColoredFoldoutGroupAttribute(string path, float r, float g, float b, float a = 1f)
            : base(path)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var otherAttr = (ColoredFoldoutGroupAttribute) other;

            R = Math.Max(otherAttr.R, R);
            G = Math.Max(otherAttr.G, G);
            B = Math.Max(otherAttr.B, B);
            A = Math.Max(otherAttr.A, A);
        }
    }

    public class ColoredFoldoutGroupAttributeDrawer : OdinGroupDrawer<ColoredFoldoutGroupAttribute>
    {
        private LocalPersistentContext<bool> isExpanded;

        protected override void Initialize()
        {
            isExpanded = this.GetPersistentValue<bool>(
                "ColoredFoldoutGroupAttributeDrawer.isExpanded",
                GeneralDrawerConfig.Instance.ExpandFoldoutByDefault);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUIHelper.PushColor(new Color(Attribute.R, Attribute.G, Attribute.B, Attribute.A));
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            GUIHelper.PopColor(); 
            isExpanded.Value = SirenixEditorGUI.Foldout(isExpanded.Value, label);
            SirenixEditorGUI.EndBoxHeader();

            if (SirenixEditorGUI.BeginFadeGroup(this, isExpanded.Value))
            {
                for (int i = 0; i < Property.Children.Count; i++)
                {
                    Property.Children[i].Draw();
                }
            }

            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}
