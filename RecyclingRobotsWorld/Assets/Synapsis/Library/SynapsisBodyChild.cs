using SynapsisLibrary;
using UnityEngine;

public class SynapsisBodyChild : MonoBehaviour
{
   protected SynapsisBody RootBody;

   private void Awake()
   {
      // Metodo per avere l'istanza dello script che sta alla testa (root) del GameObject complesso
      RootBody = GetComponentInParent<SynapsisBody>();
   }

   public virtual string GetBodyName()
   {
      return RootBody.name;
   }
}
