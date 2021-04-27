using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[RequireComponent ( typeof ( SimulationHelper ) )]
public class ServerSimulation : MonoBehaviour
{
    #region Delegates

    public delegate void ClientInputHandler ( int clientId, ClientInputState clientInputState );
    public static ClientInputHandler OnInputReceived;

    #endregion

    #region Constants

    // State caching
    public const uint STATE_CACHE_SIZE = 1024;

    #endregion

    #region Members

    // Server tick
    public static uint ServerTick { get; private set; } = 0;

    // Player-ClientInputState input processing register
    private static Dictionary<Player, Queue<ClientInputState>> m_clientInputs = new Dictionary<Player, Queue<ClientInputState>> ();

    #endregion

    private void FixedUpdate ()
    {
        // Run simulation loop on main thread
        ThreadManager.ExecuteOnMainThread ( SimulationLoop );
    }

    /// <summary>
    /// Main server simulation loop.
    /// </summary>
    private void SimulationLoop ()
    {
        // Process client inputs and apply to the player
        ProcessClientInput ();

        // Simulate physics in the scene
        Physics.Simulate ( Time.fixedDeltaTime );

        // Create new state and send to the clients
        ApplyServerState ();

        // Increment server tick
        ServerTick++;
    }

    #region Simulation Sub-processes

    private void ProcessClientInput ()
    {
        foreach ( KeyValuePair<Player, Queue<ClientInputState>> client in m_clientInputs.ToArray () )
        {
            Player player = client.Key;
            ClientInputState [] inputArray = client.Value.ToArray ();

            // Null check
            if ( player == null || inputArray == null || inputArray.Length == 0 )
            {
                continue;
            }

            // Resimlutes any 'past' actions - lag compensation
            LagCompensationHandler.Resimulate ( ServerTick, player.Id, inputArray );

            // Used for indexing input state array
            int index = 0;

            // Process each input state from input array
            while ( index < inputArray.Length && inputArray [ index ] != null )
            {
                // Process interaction input
                OnInputReceived?.Invoke ( player.Id, inputArray [ index ] );

                // Process movement input
                player.MovementController.ProcessInputs ( inputArray [ index ] );
                player.LookOriginController.ProcessInput ( inputArray [ index ] );
                index++;
            }
        }
    }

    private void ApplyServerState ()
    {
        foreach ( KeyValuePair<Player, Queue<ClientInputState>> client in m_clientInputs.ToArray () )
        {
            Player player = client.Key;
            Queue<ClientInputState> inputQueue = client.Value;

            // Null check
            if ( player == null || inputQueue == null )
            {
                continue;
            }

            // Stores a reference to the input state dequeued from the input array
            ClientInputState inputState;

            // Process each input from input array
            while ( inputQueue.Count > 0 && ( inputState = inputQueue.Dequeue () ) != null )
            {
                // Obtain the current SimulationState
                SimulationState state = new SimulationState
                {
                    Position = player.Rigidbody.position,
                    Rotation = player.Rigidbody.rotation,
                    Velocity = player.MovementController.Velocity,
                    SimulationFrame = inputState.SimulationFrame,
                    ServerTick = ServerTick,
                    DeltaTime = Time.deltaTime
                };

                // Send the state back to the client
                ServerSend.PlayerInputProcessed ( player.Id, StateToBytes ( state ) );

                // Send player orientation to all clients
                player.SendPlayerStateAll ( inputState );
            }
        }
        // Cache server state
        SimulationHelper.AddState ( ServerTick );
    }

    #endregion

    #region Client Events

    public static void OnClientInputStateReceived ( Client client, byte [] inputs )
    {
        if ( client == null || client.player == null )
        {
            return;
        }
        ClientInputState message = BytesToState ( inputs );

        // Client inputs
        // Ensure the key exists, if it doesn't, create it
        if ( !m_clientInputs.ContainsKey ( client.player ) )
        {
            m_clientInputs.Add ( client.player, new Queue<ClientInputState> () );
        }
        // Add the input to the appropriate queue
        m_clientInputs [ client.player ].Enqueue ( message );
    }

    public static void OnClientDisconnected ( Client client )
    {
        if ( client == null )
        {
            throw new NullReferenceException ( "Client is null." );
        }
        m_clientInputs.Remove ( client.player );
    }

    #endregion

    #region Util

    private static byte [] StateToBytes ( SimulationState inputState )
    {
        using ( var ms = new MemoryStream () )
        {
            BinaryFormatter formatter = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate ();
            QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate ();

            surrogateSelector.AddSurrogate ( typeof ( Vector3 ), new StreamingContext ( StreamingContextStates.All ), vector3SS );
            surrogateSelector.AddSurrogate ( typeof ( Quaternion ), new StreamingContext ( StreamingContextStates.All ), quaternionSS );
            formatter.SurrogateSelector = surrogateSelector;
            formatter.Serialize ( ms, inputState );
            return ms.ToArray ();
        }
    }

    private static ClientInputState BytesToState ( byte [] arr )
    {
        using ( var memStream = new MemoryStream () )
        {
            BinaryFormatter formatter = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate ();
            QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate ();

            surrogateSelector.AddSurrogate ( typeof ( Vector3 ), new StreamingContext ( StreamingContextStates.All ), vector3SS );
            surrogateSelector.AddSurrogate ( typeof ( Quaternion ), new StreamingContext ( StreamingContextStates.All ), quaternionSS );
            formatter.SurrogateSelector = surrogateSelector;
            memStream.Write ( arr, 0, arr.Length );
            memStream.Seek ( 0, SeekOrigin.Begin );
            ClientInputState obj = ( ClientInputState ) formatter.Deserialize ( memStream );
            return obj;
        }
    }

    #endregion
}
