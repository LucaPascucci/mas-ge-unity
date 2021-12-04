using UnityEditor;
using UnityEngine;

namespace SynapsisLibrary.SynapsisEditor
{
   [CustomPropertyDrawer(typeof(RenameAttribute))]
   public class RenameEditor : PropertyDrawer
   {
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).NewName));
      }
   }
}
