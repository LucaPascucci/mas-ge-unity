using System;
using SynapsisLibrary.SynapsisEditor;
using UnityEngine;
namespace SynapsisLibrary
{
   public class SpawnerSynapsisGameObjects : MonoBehaviour
   {
      public enum SpawnModes { Circle, Random };

      [Rename("Prefab da instanziare")]
      /// <summary>
      /// Game object da istanziare
      /// </summary>
      public GameObject Prefab;

      [Rename("Nome base")]
      /// <summary>
      /// Nome base per tutti i prefab da istanziare
      /// </summary>
      public string BaseName;

      [Rename("Posizione di partenza")]
      public Vector3 SpawnPosition;

      [Rename("Numero di Prefab da istanziare")]
      [Range(1, float.MaxValue)]
      public int NumberOfPrefab;

      [Rename("Modalità")]
      public SpawnModes SpawnMode;

      [Rename("Raggio")]
      public float Radius;

      [Rename("Spaziatura")]
      public float Spacing;

      [Rename("Offset X")]
      public float SpacingXOffset;

      [Rename("Offset Y")]
      public float SpacingYOffset;

      [Rename("Offset Z")]
      public float SpacingZOffset;

      void Awake()
      {
         switch (SpawnMode)
         {
            case SpawnModes.Circle:
               for (int i = 1; i <= NumberOfPrefab; i++)
               {
                  Prefab.name = BaseName + i;
                  float angle = i * Mathf.PI * 2 / NumberOfPrefab;
                  Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Radius;
                  pos += SpawnPosition;
                  Instantiate(Prefab, pos, transform.rotation, transform.parent);
               }
               break;
            case SpawnModes.Random:
               for (int i = 1; i <= NumberOfPrefab; i++)
               {
                  Prefab.name = BaseName + i;
                  Vector3 pos = new Vector3(UnityEngine.Random.Range(0, SpacingXOffset), UnityEngine.Random.Range(0, SpacingYOffset), UnityEngine.Random.Range(0, SpacingZOffset));
                  pos += SpawnPosition;
                  Instantiate(Prefab, pos, transform.rotation, transform.parent);
               }
               break;
         }
      }

      private void Reset()
      {
         Prefab = null;
         BaseName = "";
         SpawnPosition = transform.position;
         NumberOfPrefab = 1;
         Radius = 5f;
         Spacing = 2f;
         SpacingXOffset = 5f;
         SpacingYOffset = 0f;
         SpacingZOffset = 5f;
      }

      /// <summary>
      /// Funzione per stampe a console già strutturate
      /// </summary>
      /// <param name="message">messaggio da stampare</param>
      public void SynapsisLog(string message)
      {
         Debug.Log(DateTime.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss\\Z") + " - [SynapsisSpawner - " + name + "]: " + message);
      }
   }
}
