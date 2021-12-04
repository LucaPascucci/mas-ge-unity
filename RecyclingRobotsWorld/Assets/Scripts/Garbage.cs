
using System.Collections.Generic;
using SynapsisLibrary;
using UnityEngine;

public class Garbage : SynapsisBody
{
   private Material GarbageMaterial;
   private Transform InitialParent;
   void Start()
   {
      //NOTE memorizzo il mio padre iniziale
      InitialParent = transform.parent;
      GarbageMaterial = GetComponent<Renderer>().material;
   }
   public override void CounterpartEntityReady()
   {
   }

   public override void CounterpartEntityUnready()
   {
   }

   public override void IncomingAction(string action, List<object> parameters)
   {
      object[] parametersArray = parameters.ToArray();

      switch (action)
      {
         case "garbage_type":
            switch (parametersArray[0])
            {
               case "plastic":
                  GarbageMaterial.color = Color.yellow;
                  break;
               case "paper":
                  GarbageMaterial.color = Color.red;
                  break;
               case "glass":
                  GarbageMaterial.color = Color.blue;
                  break;
            }
            break;
         case "recycle_me":
            gameObject.SetActive(false); //TODO da sostituire con selfDestruction
            SynapsisLog("Mi riciclo");
            break;
      }
   }

   private void OnTransformParentChanged()
   {
      if (transform.parent != null && "Position".Equals(transform.parent.name)) //NOTE inserito questo controllo per utilizzare WRLD3D
      {
         InitialParent = transform.parent;
      }
      else
      {
         if (transform.parent != null && !transform.parent.Equals(InitialParent))
         {
            TransmitPerception(name, "picked_up_by", new List<object>() { true, transform.parent.name }); //TODO mettere questa percezione nelle API?
         }
         else if (transform.parent == null && InitialParent != null)
         { //NOTE serve per riposizionare la spazzatura alla parentela iniziale
            transform.parent = InitialParent;
         }
      }
   }
}
