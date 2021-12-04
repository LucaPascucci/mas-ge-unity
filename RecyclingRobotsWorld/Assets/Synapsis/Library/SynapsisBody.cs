using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using SynapsisLibrary.CustomModels;
using SynapsisLibrary.SynapsisEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using WebSocketSharp;

namespace SynapsisLibrary
{
   [RequireComponent(typeof(Collider))]
   [RequireComponent(typeof(Rigidbody))]
   public abstract class SynapsisBody : MonoBehaviour, ISynapsisEventTarget
   {
      // ESEMPIO FINALE DI URL --> ws://localhost:9000/synapsiservice/ + body/ + NOME_ENTITA

      private SynapsisBodyInfo MindInfo;

      [Header("Synapsis Settings")]
      [Rename("Synapsis URL")]
      /// <summary>
      /// Indirizzo del middleware
      /// </summary>
      public string SynapsisUrl;

      [Rename("Tentativi di riconnessione")]
      [Range(0, 10)]
      /// <summary>
      /// Tentativi di riconnessione massimi in caso di problemi di connessione al WebSocket Server
      /// </summary>
      public int ReconnectionAttempts;

      [Rename("Istanziare Mock Entity?")]
      public bool MockEntityRequest;

      [Rename("Classe Mock da istanziare")]
      public string MockClass;

      [Header("General Settings")]
      [Rename("Velocità di movimento")]
      [Range(0.1f, 10)]
      public float MovementSpeed;

      /// <summary>
      /// Coda contenente i messaggi da inviare al MIDDLEWARE quando il collegamento non è stato ancora effettuato oppure è venuto meno
      /// </summary>
      /// <typeparam name="Message">Messaggio da inviare</typeparam>

      private ConcurrentQueue<Message> MessagesToSendToMiddleware = new ConcurrentQueue<Message>(); // Utilizzo una coda FIFO concorrente perchè il thread che inserisce i messaggi è diverso dal main thread (unity)
      // private Queue<Message> MessagesToSendToMiddleware = new Queue<Message>();

      /// <summary>
      /// Coda contenente i messaggi da inviare alla MENTE quando il collegamento non è stato ancora effettuato oppure è venuto meno
      /// </summary>
      /// <typeparam name="Message">Messaggio da inviare</typeparam>

      private ConcurrentQueue<Message> MessagesToSendToMind = new ConcurrentQueue<Message>(); // Utilizzo una coda FIFO concorrente perchè il thread che inserisce i messaggi è diverso dal main thread (unity)
      // private Queue<Message> MessagesToSendToMind = new Queue<Message>();

      /// <summary>
      /// WebSoket utilizzata per collegarsi al WebSocket Server
      /// </summary>
      private WebSocket WBSKT;

      /// <summary>
      /// Contesto di sincronizzazione (conterrà il contesto di sincronizzazione del main thread di unity)
      /// </summary>
      private SynchronizationContext SyncContext = null;

      private Collider ObjectCollider;

      private Coroutine CurrentCoroutine = null;
      private string DestinationEntityName = null;

      private Tree<string> HierarchyTree = null;
      public virtual void Awake()
      {
         name = Regex.Replace(name, @"\((\w+\))", ""); //Serve a pulire il nome quando vengono istanziati dei prefab ossia rimuove il (clone) collegato ad ogni oggetto creato

         MindInfo = new SynapsisBodyInfo(name);
         HierarchyTree = CreateSubTree(gameObject);

         if (gameObject.GetComponent<Collider>().GetType() == typeof(MeshCollider))
         {
            gameObject.GetComponent<MeshCollider>().convex = true;
         }
         gameObject.GetComponent<Collider>().isTrigger = true;

         SyncContext = SynchronizationContext.Current; // Ottengo il contesto di sincronizzazione del thread corrente cioè il main thread di unity

         //Creazione della WebSocket definendo indirizzo del server + informazioni iniziali (tipologia entità (body) + nome)
         WBSKT = new WebSocket(SynapsisUrl + Shared.SYNAPSIS_ENDPOINT_PATH + Shared.ENTITY_BODY_KEY + "/" + MindInfo.EntityName);
         //WS = new WebSocket("ws://synapsis-middleware.herokuapp.com/" + EndpointPath + ENTITY_PATH + EntityName);
         WBSKT.OnOpen += OnOpenHandler;
         WBSKT.OnMessage += OnMessageHandler;
         WBSKT.OnClose += OnCloseHandler;
         WBSKT.OnError += OnErrorHandler;

         // Avvio connessione
         WBSKT.ConnectAsync();

         CreateMyMockEntity();
      }

      /// <summary>
      /// Update is called every frame, if the MonoBehaviour is enabled.
      /// </summary>
      public virtual void Update()
      {
         if (ConnectionStatus.Connected.Equals(MindInfo.SynapsisStatus))
         {
            // Attivo quando sono presenti messaggi ancora non inviati
            for (var i = 0; i < MessagesToSendToMiddleware.Count; i++)
            {
               //Message message = MessagesToSendToMind.Dequeue();
               MessagesToSendToMiddleware.TryDequeue(out var message);
               SendMessage(message);
            }
         }

         if (ConnectionStatus.Connected.Equals(MindInfo.MindStatus))
         {
            // Attivo quando sono presenti messaggi ancora non inviati
            for (var i = 0; i < MessagesToSendToMind.Count; i++)
            {
               //Message message = MessagesToSendToMind.Dequeue();
               MessagesToSendToMind.TryDequeue(out var message);
               SendMessage(message);
            }
         }
      }

      public virtual void OnApplicationQuit()
      {
         if (WBSKT != null)
         {
            //NOTE Rimuovo la funzione OnMessageHandler per evitare che vengano ricevuti messagi durante la distruzione dell'oggetto che causano Exception --> MissingReferenceException
            WBSKT.OnMessage -= OnMessageHandler;
         }
         DeleteMyMockEntity();
      }

      /// <summary>
      /// Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
      /// </summary>
      public virtual void OnDestroy()
      {
         MindInfo.MindStatus = ConnectionStatus.Disconnected;
         MindInfo.SynapsisStatus = ConnectionStatus.Disconnected;
         if (WBSKT != null)
         {
            WBSKT.OnMessage -= OnMessageHandler;
            WBSKT.OnOpen -= OnOpenHandler;
            WBSKT.OnClose -= OnCloseHandler;
            WBSKT.OnError -= OnErrorHandler;
            WBSKT.Close();
         }
      }

      /// <summary>
      /// Metodo per valorizzare velocemente le variabili su Unity3D inspector grafico
      /// </summary>
      public virtual void Reset()
      {
         SynapsisUrl = "ws://localhost:9000/";
         ReconnectionAttempts = 5;
         MockEntityRequest = false;
         MockClass = "";
         MovementSpeed = 2.5f;
      }

      /// <summary>
      /// Rileva le collisioni in ingresso
      /// </summary>
      /// <param name="collision"> Collisione</param>
      public virtual void OnTriggerEnter(Collider collision)
      {
         string colliderName = collision.gameObject.name;

         if (collision.gameObject.GetComponent<SynapsisBodyChild>() != null)
         {
            colliderName = collision.gameObject.GetComponent<SynapsisBodyChild>().GetBodyName();
         }

         if (DestinationEntityName != null && DestinationEntityName.Equals(colliderName))
         {
            StopCoroutine(CurrentCoroutine);
            ArrivedToPerception(MindInfo.EntityName, DestinationEntityName);
            CurrentCoroutine = null;
            DestinationEntityName = null;
         }
         else
         {
            TouchedPerception(MindInfo.EntityName, colliderName);
         }
      }

      /// <summary>
      /// Metodo per inviare le percezioni fisiche/visive del VirtualBody alla VirtualMind
      /// </summary>
      /// <param name="sender">Mittente</param>
      /// <param name="perception">Breve descrizione del messaggio. ES: pronto, ostacolo, fermo, ...</param>
      /// <param name="parameters">Lista di parametri collegati al contenuto</param>
      public void TransmitPerception(string sender, string perception, List<object> parameters)
      {
         Message message = new Message(CreateHierarchyPath(sender, HierarchyTree), MindInfo.EntityName, perception, parameters);
         SendMessage(message);
      }

      public void FoundPerception(string sender, string entityName)
      {
         TransmitPerception(sender, Shared.FOUND_PERCEPTION, new List<object>() { entityName });
      }

      public void ArrivedToPerception(string sender, string entityName)
      {
         TransmitPerception(sender, Shared.ARRIVED_TO_PERCEPTION, new List<object>() { entityName });
      }

      public void StoppedPerception(string sender)
      {
         TransmitPerception(sender, Shared.STOPPED_PERCEPTION, new List<object>());
      }

      public void TouchedPerception(string sender, string entityName)
      {
         TransmitPerception(sender, Shared.TOUCHED_PERCEPTION, new List<object>() { entityName });
      }

      public void PickedPerception(string sender, string entityName)
      {
         TransmitPerception(sender, Shared.PICKED_PERCEPTION, new List<object>() { entityName });
      }

      public void ReleasedPerception(string sender, string entityName)
      {
         TransmitPerception(sender, Shared.RELEASED_PERCEPTION, new List<object>() { entityName });
      }

      public void SelfDestruction(string sender)
      {
         TransmitPerception(sender, Shared.SELF_DESTRUCTION, new List<object>());
         Destroy(gameObject);
      }

      private void CreateMyMockEntity()
      {
         if (MockEntityRequest)
         {
            Message message = new Message(MindInfo.EntityName, Shared.SYNAPSIS_MIDDLEWARE, Shared.SYNAPSIS_MIDDLEWARE_CREATE_MOCK);
            message.addParam(MockClass);
            SendMessage(message);
         }
      }

      private void DeleteMyMockEntity()
      {
         if (MockEntityRequest)
         {
            Message message = new Message(MindInfo.EntityName, Shared.SYNAPSIS_MIDDLEWARE, Shared.SYNAPSIS_MIDDLEWARE_DELETE_MOCK);
            SendMessage(message);
         }
      }

      public virtual void SearchAction(string entityName)
      {
         //List<GameObject> gosFiltered = new List<GameObject>();
         GameObject[] gos = (GameObject[])FindObjectsOfType(typeof(GameObject));

         GameObject[] gosFiltered = Array.FindAll(gos, go => go.name.Contains(entityName));

         if (gosFiltered.Length > 0)
         {
            int index = UnityEngine.Random.Range(0, gosFiltered.Length);
            FoundPerception(MindInfo.EntityName, gosFiltered[index].name);
         }

      }

      public virtual void GoToAction(string destinationEntityName)
      {
         //Fermo la coroutine corrente
         if (CurrentCoroutine != null)
         {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
            DestinationEntityName = null;
         }
         GameObject target = GameObject.Find(destinationEntityName);
         DestinationEntityName = destinationEntityName;
         CurrentCoroutine = StartCoroutine(MoveFromTo(transform.position, target.transform.position));
      }

      public virtual void StopAction()
      {
         if (CurrentCoroutine != null)
         {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
            DestinationEntityName = null;
         }
         StoppedPerception(MindInfo.EntityName);
      }

      public virtual void PickUpAction(string entityName)
      {
         GameObject.Find(entityName).transform.SetParent(gameObject.transform);
         PickedPerception(MindInfo.EntityName, entityName);
      }

      public virtual void ReleaseAction(string entityName)
      {
         GameObject.Find(entityName).transform.parent = null;
         ReleasedPerception(MindInfo.EntityName, entityName);
      }

      public abstract void IncomingAction(string action, List<object> parameters);

      public abstract void CounterpartEntityReady();

      public abstract void CounterpartEntityUnready();

      /// <summary>
      /// Metodo per inviare messaggi
      /// </summary>
      /// <param name="message">Messaggio da inviare</param>
      private void SendMessage(Message message)
      {
         switch (message.Receiver)
         {
            case Shared.SYNAPSIS_MIDDLEWARE:
               if (ConnectionStatus.Connected.Equals(MindInfo.SynapsisStatus))
               {
                  message.addTimeStat(CurrentTimeMillis());
                  WBSKT.SendAsync(message.ToString(), OnSendComplete);
               }
               else
               {
                  MessagesToSendToMiddleware.Enqueue(message);
               }
               break;
            default:
               if (ConnectionStatus.Connected.Equals(MindInfo.MindStatus))
               {
                  message.addTimeStat(CurrentTimeMillis());
                  WBSKT.SendAsync(message.ToString(), OnSendComplete);
               }
               else
               {
                  MessagesToSendToMind.Enqueue(message);
               }
               break;
         }
      }

      private IEnumerator MoveFromTo(Vector3 a, Vector3 b)
      {
         Vector3 dir = (b - a).normalized;
         Quaternion rotTo = Quaternion.LookRotation(dir);

         while (Vector3.Angle(transform.forward, dir) > 1)
         {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTo, Time.deltaTime * MovementSpeed * 10);
            yield return new WaitForFixedUpdate(); // Leave the routine and return here in the next frame
         }

         float step = (MovementSpeed / (a - b).magnitude) * Time.fixedDeltaTime;
         float t = 0;
         while (t <= 1.0f)
         {
            t += step; // Goes from 0 to 1, incrementing by step each time
            transform.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate(); // Leave the routine and return here in the next frame
         }
         transform.position = b;
      }

      /// <summary>
      /// Metodo per gestire l'apertura della connessione con la WebSocket (Viene invocato in un thread secondario)
      /// </summary>
      /// <param name="sender">Nome del sender (sempre WebSocketSharp.WebSocket)</param>
      /// <param name="e">System.EventArgs.Empty is passed as e, so you do not need to use it.</param>
      private void OnOpenHandler(object sender, EventArgs e)
      {
         MindInfo.CurrentReconnectionAttempt = 0; //azzero il contatore di riconnessioni
         MindInfo.SynapsisStatus = ConnectionStatus.Connected;

         //SynapsisLog("OnOpenHandler - WebSocket State:" + WS.ReadyState + " - Websocket Alive: " + WS.IsAlive);
      }

      /// <summary>
      /// Metodo per gestire la ricezione di messaggi della WebSocket (Viene invocato in un thread secondario)
      /// </summary>
      /// <param name="sender">Nome del sender (sempre WebSocketSharp.WebSocket)</param>
      /// <param name="e">
      /// Message data, you should access e.Data or e.RawData property.
      /// If e.IsText is true, you should access e.Data that returns a string (represents a text message).
      /// Or if e.IsBinary is true, you should access e.RawData that returns a byte[] (represents a binary message).
      /// </param>
      private void OnMessageHandler(object sender, MessageEventArgs e)
      {
         long currentMills = CurrentTimeMillis();

         // Costruisco l'oggetto messaggio che viene passato come stringa (formato JSON)
         Message message = Message.BuildMessage(e.Data);
         message.addTimeStat(currentMills);
         //SynapsisLog("OnMessageHandler --> " + message);

         if (Shared.SYNAPSIS_MIDDLEWARE.Equals(message.Receiver))
         {
            if (message.Content.Equals(Shared.COUNTERPART_READY)) // Messaggio ricevuto dal middleware quando vengono collegati mente e corpo
            {
               MindInfo.MindStatus = ConnectionStatus.Connected;

               SyncContext.Send(f =>
               {
                  ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.CounterpartEntityReady()); // messaggio inviato al gameobject collegato a questa websocket
               }, null);
               SynapsisLog("OnMessageHandler --> Controparte collegata");

            }
            else if (message.Content.Equals(Shared.COUNTERPART_UNREADY)) // Messaggio ricevuto dal middleware quando vengono scollegati mente e corpo
            {
               MindInfo.MindStatus = ConnectionStatus.Disconnected;

               SyncContext.Send(f =>
               {
                  ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.CounterpartEntityUnready()); // messaggio inviato al gameobject collegato a questa websocket
               }, null);
               SynapsisLog("OnMessageHandler --> Controparte scollegata");
            }
         }
         else
         {
            switch (message.Content)
            {
               case Shared.SEARCH_ACTION:
                  // invia un messaggio SINCRONO(tramite Send()) ad un contesto di sincronizzazione che in questo caso deve essere il main thread di Unity perchè utilizzo syncContext (Con Post() invio ASINCRONO)
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.SearchAction((string)message.Parameters[0]));
                  }, null);
                  break;
               case Shared.GO_TO_ACTION:
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.GoToAction((string)message.Parameters[0]));
                  }, null);
                  break;
               case Shared.STOP_ACTION:
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.StopAction());
                  }, null);
                  break;
               case Shared.PICK_UP_ACTION:
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.PickUpAction((string)message.Parameters[0]));
                  }, null);
                  break;
               case Shared.RELEASE_ACTION:
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.ReleaseAction((string)message.Parameters[0]));
                  }, null);
                  break;
               case Shared.SELF_DESTRUCTION:
                  SyncContext.Send(f =>
                  {
                     //NOTE Rimuovo la funzione OnMessageHandler per evitare che vengano ricevuti messagi durante la distruzione dell'oggetto che causano Exception --> MissingReferenceException
                     WBSKT.OnMessage -= OnMessageHandler;
                     Destroy(gameObject);
                  }, null);
                  break;
               default:
                  // invia un messaggio SINCRONO(tramite Send()) ad un contesto di sincronizzazione che in questo caso deve essere il main thread di Unity perchè utilizzo syncContext (Con Post() invio ASINCRONO)
                  SyncContext.Send(f =>
                  {
                     ExecuteEvents.Execute<ISynapsisEventTarget>(gameObject, null, (x, y) => x.IncomingAction(message.Content, message.Parameters)); // messaggio inviato al gameobject collegato a questa websocket
                  }, null);
                  break;
            }
         }
      }

      /// <summary>
      /// Metodo per gestire la chiusura di connessione della WebSocket
      /// </summary>
      /// <param name="sender">Nome del sender (sempre WebSocketSharp.WebSocket)</param>
      /// <param name="e">The reason for the close
      /// e.Code property returns a ushort that represents the status code for the close.
      /// e.Reason property returns a string that represents the reason for the close.
      /// </param>
      private void OnCloseHandler(object sender, CloseEventArgs closeEventArgs) // Viene eseguito in un thread secondario
      {
         //e.Code property returns a ushort that represents the status code for the close, and e.Reason property returns a string that represents the reason for the close.

         MindInfo.MindStatus = ConnectionStatus.Disconnected;
         MindInfo.SynapsisStatus = ConnectionStatus.Disconnected;

         //SynapsisLog("OnCloseHandler | reason: " + closeEventArgs.Reason + " -> WebSocket State:" + WS.ReadyState + " - Websocket Alive: " + WS.IsAlive);

         if (!closeEventArgs.WasClean)
         {
            if (!WBSKT.IsAlive)
            {
               MindInfo.CurrentReconnectionAttempt++;
               // Tentativo di riconnessione
               if (ReconnectionAttempts >= MindInfo.CurrentReconnectionAttempt)
               {
                  SynapsisLog("OnCloseHandler | Riconnessione in corso... Tentativo " + MindInfo.CurrentReconnectionAttempt);
                  Thread.Sleep(MindInfo.ReconnectionTimeout);
                  WBSKT.Connect();
               }
               else
               {
                  Debug.LogError("Tentativi di riconnessione finiti... RICONNESSIONE FALLITA!");
               }
            }
         }
      }

      /// <summary>
      /// Metodo per venire a conoscenza del corretto invio di un messaggio
      /// </summary>
      /// <param name="success">Indica il successo/fallimento dell'invio del messaggio</param>
      private void OnSendComplete(bool success)
      {
         //Log(": Messaggio inviato correttamente? --> " + success);
      }

      /// <summary>
      /// Metodo per gestire gli errori emessi dalla WebSocket
      /// </summary>
      /// <param name="sender">Nome del sender (sempre WebSocketSharp.WebSocket)</param>
      /// <param name="e">The error message
      /// e.Message property returns a string that represents the error message.
      /// e.Exception property returns a System.Exception instance that represents the cause of the error if it is due to an exception.
      /// </param>
      private void OnErrorHandler(object sender, ErrorEventArgs e) //Viene eseguito in un thread secondario
      {
         MindInfo.MindStatus = ConnectionStatus.Disconnected;
         MindInfo.SynapsisStatus = ConnectionStatus.Disconnected;

         Debug.LogError(MindInfo.EntityName + ": OnErrorHandler | message: " + e.Message + " - exception: " + e.Exception + " -> WebSocket State:" + WBSKT.ReadyState + " - Websocket Alive: " + WBSKT.IsAlive);
      }

      /// <summary>
      /// Funzione per stampe a console già strutturate
      /// </summary>
      /// <param name="message">messaggio da stampare</param>
      public void SynapsisLog(string message)
      {
         Debug.Log("[Synapsis - " + MindInfo.EntityName + "]: " + message);
      }

      /// <summary>
      /// Genera ricorsivamente la struttura gerarchica del gameObject complesso
      /// </summary>
      /// <param name="go">GameObject di cui generare la struttura gerarchica</param>
      /// <returns>L'albero della gerachia</returns>
      private Tree<string> CreateSubTree(GameObject go)
      {
         Tree<string> subTree = new Tree<string>(go.name);

         Transform[] tList = go.GetComponentsInChildren<Transform>(); //NOTE prende anche se stesso quindi ho messo anche il filtro sul nome
         if (tList != null)
         {
            foreach (Transform t in tList)
            {
               if (t != null && t.gameObject != null && !go.name.Equals(t.gameObject.name))
               {
                  subTree.AddChild(CreateSubTree(t.gameObject));
               }
            }
         }
         return subTree;
      }

      /// <summary>
      /// Genera ricorsivamente il path della struttura gerarchica in base al mittente della percezione
      /// </summary>
      /// <param name="sender">nome del mittente</param>
      /// <returns>Path nel formato "padre.figlio.sottofiglio..."</returns>
      private string CreateHierarchyPath(string sender, Tree<string> hierarchyTree)
      {
         if (hierarchyTree.Value.Equals(sender))
         {
            return hierarchyTree.Value;
         }
         else if (hierarchyTree.ChildrenCount == 0)
         {
            return "";
         }
         else
         {
            foreach (Tree<string> child in hierarchyTree.Children)
            {
               string subPath = CreateHierarchyPath(sender, child);
               if (subPath != "")
               {
                  return hierarchyTree.Value + "." + subPath;
               }
            }
            return "";
         }
      }

      /// <summary>
      /// Funzione che calcola lo stesso timestamp di System.currentTimeMillis() (Java)
      /// </summary>
      /// <returns>Millisecondi trascorsi dal 1 Gennaio 1970 ad oggi</returns>
      private static long CurrentTimeMillis()
      {
         return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
      }
   }
}
