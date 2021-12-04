using System.Collections.Generic;
using SynapsisLibrary;

public class Test : SynapsisBody
{
   /// <summary>
   /// Metodo invocato per ogni messaggio inviato dalla controparte (mind)
   /// </summary>
   /// <param name="action">Azione da svolgere</param>
   /// <param name="parameters">Parametri aggiuntivi</param>
   public override void IncomingAction(string action, List<object> parameters)
   {
   }

   /// <summary>
   /// Metodo invocato quando la controparte (mind) è collegata
   /// </summary>
   public override void CounterpartEntityReady()
   {
   }

   /// <summary>
   /// Metodo invocato quando la controparte (mind) è scollegata
   /// </summary>
   public override void CounterpartEntityUnready()
   {
   }
}
