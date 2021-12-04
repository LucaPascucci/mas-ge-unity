using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SynapsisLibrary
{
   /// <summary>
   /// Interfaccia per rendere i metodi invocabili con tramite EventSystem di Unity
   /// </summary>
   public interface ISynapsisEventTarget : IEventSystemHandler
   {
      /// <summary>
      /// Metodo da invocare quando l'entità controparte (mind) è collegata
      /// </summary>
      void CounterpartEntityReady();

      /// <summary>
      /// Metodo da invocare quando l'entità controparte (mind) è scollegata
      /// </summary>
      void CounterpartEntityUnready();

      /// <summary>
      /// Metodo da invocare quando arriva un'azione generica
      /// </summary>
      /// <param name="action">Azione da eseguire</param>
      /// <param name="parameters">Parametri</param>
      void IncomingAction(string action, List<object> parameters);

      /// <summary>
      /// Medoto da invocare in caso di una azione di ricerca
      /// </summary>
      /// <param name="entityName">nome dell'entità da cercare</param>
      void SearchAction(string entityName);

      /// <summary>
      /// Medoto da invocare per andare verso una certa entità
      /// </summary>
      /// <param name="destinationEntityName">nome dell'entità da raggiungere</param>
      void GoToAction(string destinationEntityName);

      /// <summary>
      /// Metodo da invocare per fermare il movimento del corpo
      /// </summary>
      void StopAction();

      /// <summary>
      /// Metodo da invocare per raccogliere un corpo estraneo
      /// </summary>
      /// <param name="entityName">nome dell'entità da raccogliere</param>
      void PickUpAction(string entityName);

      /// <summary>
      /// Metodo da invocare per rilasciare un corpo estraneo
      /// </summary>
      /// <param name="entityName">nome dell'entità da rilasciare</param>
      void ReleaseAction(string entityName);
   }
}
