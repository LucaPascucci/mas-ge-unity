using UnityEngine;

namespace SynapsisLibrary.CustomModels
{
   public class SynapsisBodyInfo
   {
      /// <summary>
      /// Nome dell'entità (intesa come GameObject). Viene memorizzata in questa variabile per evitare problemi con il main thread di unity
      /// </summary>
      public string EntityName;

      /// <summary>
      /// Riferimento alla testa
      /// </summary>
      public Transform MyHead;

      /// <summary>
      /// Rappresenza lo stato di collegamento a Synapsis
      /// </summary>
      public ConnectionStatus SynapsisStatus = ConnectionStatus.Disconnected;

      /// <summary>
      /// Rappresenza lo stato di collegamento alla mente
      /// </summary>
      public ConnectionStatus MindStatus = ConnectionStatus.Disconnected;

      /// <summary>
      /// Tentativo di riconnessione attuale
      /// </summary>
      internal int CurrentReconnectionAttempt = 0;

      /// <summary>
      /// Millisecondi di attesa tra un tentativo di riconessione e l'altro
      /// </summary>
      internal int ReconnectionTimeout = 5000;

      int NumberOfReceivedMessages = 0;

      private long TotalSendTime = 0;

      private long TotalReceiveTime = 0;

      private long TotalComputationTime = 0;

      private long TotalTime = 0;

      public SynapsisBodyInfo(string name)
      {
         EntityName = name;
      }

      void AddNewMessage(Message message)
      {
         NumberOfReceivedMessages++;
         TotalSendTime += message.GetTimeFromSenderToSynapsis();
         TotalComputationTime += message.GetSynapsisComputation();
         TotalReceiveTime += message.GetTimeFromSynapsisToReceiver();
         TotalTime += message.GetTotalTime();
      }
   }
}
