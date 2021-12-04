using UnityEngine;
using Wrld;
using Wrld.Space;

public class CustomMapSettings : MonoBehaviour
{
   [Tooltip("In gradi")]
   [Range(-90.0f, 90.0f)]
   [SerializeField]
   public double Latitude;

   [Tooltip("In gradi")]
   [Range(-90.0f, 90.0f)]
   [SerializeField]
   public double Longitude;

   [Tooltip("Distanza di caching in metri")]
   [SerializeField]
   [Range(0, 10000)]
   public double Radius;

   public Camera renderingCamera;

   private void Awake()
   {
      // cache a "radius" meter  around this point

      Api.Instance.PrecacheApi.Precache(LatLong.FromDegrees(Latitude, Longitude), Radius, (result) =>
      {
         Debug.LogFormat("Precaching {0}", result.Succeeded ? "complete" : "failed");
      });

      Api.Instance.CameraApi.SetControlledCamera(renderingCamera);
   }

   private void Reset()
   {
      Latitude = 45.438786;
      Longitude = 10.992197;
      Radius = 5000;
   }
}
