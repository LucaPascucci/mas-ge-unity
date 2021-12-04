using System;
using System.Collections.Generic;

namespace SynapsisLibrary.CustomModels
{
   ///<summary>Representa un nodo dell'albero (può essere anche la radice)</summary>
   ///<typeparam name="T">Il tipo di valore contenuto dal nodo</typeparam>
   public class Tree<T>
   {
      /// <summary>
      /// Contiene il valore del nodo
      /// </summary>
      private T value;

      /// <summary>
      /// Indica se il nodo corrente ha un padre
      /// </summary>
      private bool hasParent;

      // Contains the children of the node (zero or more)
      /// <summary>
      /// Lista che contiene i figli del nodo (zero o più) i figli sono sempre Tree<T>
      /// </summary>
      private List<Tree<T>> children;

      /// <summary>Costruttore del nodo</summary>
      /// <param name="value">Valore del nodo</param>
      public Tree(T value)
      {
         if (value == null)
         {
            throw new ArgumentNullException("Non è possibile inserire null");
         }
         this.value = value;
         this.children = new List<Tree<T>>();
      }

      /// <summary>Il valore del nodo</summary>
      public T Value
      {
         get
         {
            return this.value;
         }

         set
         {
            this.value = value;
         }
      }

      /// <summary>Numero di figli del nodo</summary>
      public int ChildrenCount
      {
         get
         {
            return this.children.Count;
         }
      }

      /// <summary>
      /// Figli del nodo
      /// </summary>
      /// <value>Lista di figli del nodo</value>
      public List<Tree<T>> Children
      {
         get
         {
            return this.children;
         }
      }

      /// <summary>
      /// Aggiunge un figlio al nodo
      /// </summary>
      /// <param name="child">Figlio da aggiungere</param>
      public void AddChild(Tree<T> child)
      {
         if (child == null)
         {
            throw new ArgumentNullException("Non è possibile inserire null");
         }

         if (child.hasParent)
         {
            throw new ArgumentException("Il nodo ha già un padre!");
         }

         child.hasParent = true;
         this.children.Add(child);
      }

      /// <summary>
      /// Ritorna il figlio del nodo ad una data posizone
      /// </summary>
      /// <param name="index">indice del figlio desiderato</param>
      /// <returns>Il figlio alla posizione data</returns>
      public Tree<T> GetChild(int index)
      {
         return this.children[index];
      }
   }
}
