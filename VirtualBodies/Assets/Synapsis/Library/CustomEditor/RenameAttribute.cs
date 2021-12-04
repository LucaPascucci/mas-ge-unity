using UnityEngine;

namespace SynapsisLibrary.SynapsisEditor
{
   public class RenameAttribute : PropertyAttribute
   {
      public string NewName { get; private set; }
      public RenameAttribute(string name)
      {
         NewName = name;
      }
   }
}
