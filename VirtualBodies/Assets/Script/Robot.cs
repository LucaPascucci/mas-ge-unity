using System.Collections.Generic;
using SynapsisLibrary;
using UnityEngine;

public class Robot : SynapsisBody
{
   private Material RobotMaterial;
   void Start()
   {
      RobotMaterial = GetComponent<Renderer>().material;
   }
   public override void CounterpartEntityReady() { }

   public override void CounterpartEntityUnready() { }

   public override void IncomingAction(string action, List<object> parameters)
   {
      switch (action)
      {
         case "robot_type":
            switch (parameters[0])
            {
               case "plastic":
                  RobotMaterial.color = Color.yellow;
                  break;
               case "paper":
                  RobotMaterial.color = Color.red;
                  break;
               case "glass":
                  RobotMaterial.color = Color.blue;
                  break;
            }
            break;
      }
   }
}
