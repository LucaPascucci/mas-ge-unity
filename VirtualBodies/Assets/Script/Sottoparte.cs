using System.Collections.Generic;
using UnityEngine;

public class Sottoparte : SynapsisBodyChild
{
   // Start is called before the first frame update
   void Start()
   {
      RootBody.TransmitPerception(name, "Over", new List<object>());
   }

   // Update is called once per frame
   void Update()
   {

   }
}
