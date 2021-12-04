
using System.Collections.Generic;
using SynapsisLibrary;
using UnityEngine;

public class Bin : SynapsisBody
{

   Material BinMaterial;
   void Start()
   {
      BinMaterial = GetComponent<Renderer>().material;
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

      if (action.Equals("bin_type"))
      {
         switch (parametersArray[0])
         {
            case "plastic":
               BinMaterial.color = Color.yellow;
               break;
            case "paper":
               BinMaterial.color = Color.red;
               break;
            case "glass":
               BinMaterial.color = Color.blue;
               break;
         }
      }
   }
}
