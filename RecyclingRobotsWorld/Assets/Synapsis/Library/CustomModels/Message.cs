using System.Collections.Generic;
using Newtonsoft.Json;

namespace SynapsisLibrary.CustomModels
{
   /// <summary>
   /// Classe che rappresenta il messaggio scambiato con il middleware
   /// </summary>
   public class Message
   {
      // <summary>
      /// Costruttore
      /// </summary>
      /// <param name="sender">Mittente</param>
      /// <param name="receiver">Destinatario</param>
      /// <param name="content">Breve descrizione del messaggio. ES: pronto, ostacolo, fermo, ...</param>
      public Message(string sender, string receiver, string content)
      {
         Sender = sender;
         Receiver = receiver;
         Content = content;
         Parameters = new List<object>();
         TimeStats = new LinkedList<long>();
      }

      /// <summary>
      /// Costruttore
      /// </summary>
      /// <param name="sender">Mittente</param>
      /// <param name="receiver">Destinatario</param>
      /// <param name="content">Breve descrizione del messaggio. ES: pronto, ostacolo, fermo, ...</param>
      /// <param name="parameters">Lista di parametri collegati al contenuto</param>
      public Message(string sender, string receiver, string content, List<object> parameters)
      {
         Sender = sender;
         Receiver = receiver;
         Content = content;
         Parameters = parameters;
         TimeStats = new LinkedList<long>();
      }

      /// <summary>
      /// Costruttore
      /// </summary>
      /// <param name="sender">Mittente</param>
      /// <param name="receiver">Destinatario</param>
      /// <param name="content">Breve descrizione del messaggio. ES: pronto, ostacolo, fermo, ...</param>
      /// <param name="parameters">Lista di parametri collegati al contenuto</param>
      /// <param name="timeStats">Lista che memorizza i tempi di invio e ricezione del messaggio</param>
      [JsonConstructor] //Serve a definire il costruttore utilizzabile da JSON.net per la conversione di un JSON in Classe
      public Message(string sender, string receiver, string content, List<object> parameters, LinkedList<long> timeStats)
      {
         Sender = sender;
         Receiver = receiver;
         Content = content;
         Parameters = parameters;
         TimeStats = timeStats;
      }

      /// <summary>
      /// Mittente
      /// </summary>
      /// <value>Nome del mittente</value>
      public string Sender { get; set; }

      /// <summary>
      /// Destinatario
      /// </summary>
      /// <value>Nome del destinatario</value>
      public string Receiver { get; set; }

      /// <summary>
      /// Breve descrizione del messaggio
      /// </summary>
      /// <value>pronto, ostacolo, fermo</value>
      public string Content { get; set; }

      /// <summary>
      /// Lista di parametri collegati al contenuto
      /// </summary>
      public List<object> Parameters { get; set; }

      /// <summary>
      /// Lista di timestamp per controllare il tempo di invio/ricezione dei messaggi
      /// </summary>
      public LinkedList<long> TimeStats { get; set; }

      /// <summary>
      /// Metodo per aggiungere nuovi parametri al messaggio
      /// </summary>
      /// <param name="param"> Parametro</param>
      public void addParam(object param)
      {
         Parameters.Add(param);
      }

      /// <summary>
      /// Metodo per aggiungere nuovi timestamp per il controllo delle tempistiche
      /// </summary>
      /// <param name="time"> Timestamp</param>
      public void addTimeStat(long time)
      {
         TimeStats.AddLast(time);
      }

      /// <summary>
      /// Rimuove tutte le statistiche temporali del messaggio
      /// </summary>
      public void clearTimeStats()
      {
         TimeStats.Clear();
      }

      /// <summary>
      /// Converte il messaggio in stringa (formato JSON)
      /// </summary>
      /// <returns>stringa (formato JSON)</returns>
      public override string ToString()
      {
         return JsonConvert.SerializeObject(this);
      }

      /// <summary>
      /// Metodo per costruire l'oggetto Messaggio a partire dalla stringa che lo rappresenta
      /// </summary>
      /// <param name="message">stringa che rappresenta il messaggio</param>
      /// <returns>Oggetto Messaggio costruito</returns>
      public static Message BuildMessage(string message)
      {
         return JsonConvert.DeserializeObject<Message>(message);
      }

      /// Conenuto array --> [timestamp invio, timestamp ricezione su Synapsis, timestamp invio da Synapsis, timestamp ricezione]

      public long GetTimeFromSenderToSynapsis()
      {
         long[] temp = new long[TimeStats.Count];
         TimeStats.CopyTo(temp, 0);
         return (temp[1] - temp[0]);
      }

      public long GetSynapsisComputation()
      {
         long[] temp = new long[TimeStats.Count];
         TimeStats.CopyTo(temp, 0);
         return (temp[2] - temp[1]);
      }

      public long GetTimeFromSynapsisToReceiver()
      {
         long[] temp = new long[TimeStats.Count];
         TimeStats.CopyTo(temp, 0);
         return (temp[3] - temp[2]);
      }

      public long GetTotalTime()
      {
         return (TimeStats.Last.Value - TimeStats.First.Value);
      }

      /// S2S = Sender to Synapsis
      /// SC = Synapsis Computation
      /// S2R = Synapsis to Receiver

      public string GetCalculatedTimeStats()
      {
         return "Message TimeStats -> Total: " + GetTotalTime() + " mills - S2S: " + GetTimeFromSenderToSynapsis() + " mills - SC: " + GetSynapsisComputation() + " mills - S2R: " + GetTimeFromSynapsisToReceiver() + " mills";
      }
   }
}
