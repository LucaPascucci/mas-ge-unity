using UnityEditor;

namespace SynapsisLibrary.SynapsisEditor
{
   // Custom Editor using SerializedProperties.
   // Automatic handling of multi-object editing, undo, and Prefab overrides.
   [CustomEditor(typeof(SpawnerSynapsisGameObjects)), CanEditMultipleObjects]
   public class SpawnerSynapsisGameObjectsEditor : Editor
   {
      SerializedProperty prefab_Prop;
      SerializedProperty base_Name_Prop;
      SerializedProperty spawn_Position_Prop;
      SerializedProperty prefab_Number_Prop;
      SerializedProperty mode_Prop;
      SerializedProperty radius_Prop;
      SerializedProperty spacing_Prop;
      SerializedProperty spacing_X_Offset_Prop;
      SerializedProperty spacing_Y_Offset_Prop;
      SerializedProperty spacing_Z_Offset_Prop;

      void OnEnable()
      {
         // Setup the SerializedProperties
         prefab_Prop = serializedObject.FindProperty("Prefab");
         base_Name_Prop = serializedObject.FindProperty("BaseName");
         spawn_Position_Prop = serializedObject.FindProperty("SpawnPosition");
         prefab_Number_Prop = serializedObject.FindProperty("NumberOfPrefab");
         mode_Prop = serializedObject.FindProperty("SpawnMode");
         radius_Prop = serializedObject.FindProperty("Radius");
         spacing_X_Offset_Prop = serializedObject.FindProperty("SpacingXOffset");
         spacing_Y_Offset_Prop = serializedObject.FindProperty("SpacingYOffset");
         spacing_Z_Offset_Prop = serializedObject.FindProperty("SpacingZOffset");
      }

      public override void OnInspectorGUI()
      {
         serializedObject.Update();

         EditorGUILayout.PropertyField(prefab_Prop);
         EditorGUILayout.PropertyField(base_Name_Prop);
         EditorGUILayout.PropertyField(spawn_Position_Prop);
         EditorGUILayout.PropertyField(prefab_Number_Prop);
         EditorGUILayout.PropertyField(mode_Prop);

         SpawnerSynapsisGameObjects.SpawnModes spawnMode = (SpawnerSynapsisGameObjects.SpawnModes)mode_Prop.enumValueIndex;

         switch (spawnMode)
         {
            case SpawnerSynapsisGameObjects.SpawnModes.Circle:
               EditorGUILayout.PropertyField(radius_Prop);
               break;

            case SpawnerSynapsisGameObjects.SpawnModes.Random:
               EditorGUILayout.PropertyField(spacing_X_Offset_Prop);
               EditorGUILayout.PropertyField(spacing_Y_Offset_Prop);
               EditorGUILayout.PropertyField(spacing_Z_Offset_Prop);
               break;
         }
         serializedObject.ApplyModifiedProperties();
      }
   }
}
