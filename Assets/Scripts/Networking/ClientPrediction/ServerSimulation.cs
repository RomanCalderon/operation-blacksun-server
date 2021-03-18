using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

public class ServerSimulation : MonoBehaviour
{
    private static Dictionary<Player, Queue<ClientInputState>> m_clientInputs = new Dictionary<Player, Queue<ClientInputState>> ();

    private void FixedUpdate ()
    {
        ThreadManager.ExecuteOnMainThread ( SimulationLoop );
    }

    private void SimulationLoop ()
    {
        // Process client input
        foreach ( KeyValuePair<Player, Queue<ClientInputState>> entry in m_clientInputs )
        {
            Player player = entry.Key;
            ClientInputState [] queue = entry.Value.ToArray ();

            // Declare the ClientInputState that we're going to be using
            ClientInputState inputState;
            int inputStateIndex = 0;

            // Obtain ClientInputStates from the queue
            while ( queue.Length > 0 && inputStateIndex < queue.Length && ( inputState = queue [ inputStateIndex ] ) != null )
            {
                // Process the input
                player.MovementController.ProcessInputs ( inputState );
                player.LookOriginController.ProcessInput ( inputState );
                inputStateIndex++;
            }
        }

        // Simulate physics
        Physics.Simulate ( Time.fixedDeltaTime );

        // Send new state back to the client
        foreach ( KeyValuePair<Player, Queue<ClientInputState>> entry in m_clientInputs )
        {
            Player player = entry.Key;
            Queue<ClientInputState> queue = entry.Value;

            // Declare the ClientInputState that we're going to be using
            ClientInputState inputState;

            // Obtain ClientInputStates from the queue
            while ( queue.Count > 0 && ( inputState = queue.Dequeue () ) != null )
            {
                // Obtain the current SimulationState
                SimulationState state = player.CurrentSimulationState ( inputState.SimulationFrame );

                // Send the state back to the client
                ServerSend.PlayerInputProcessed ( player.Id, StateToBytes ( state ) );

                // Send player orientation to all clients
                player.SendPlayerStateAll ( inputState );
            }
        }
    }

    public static void OnClientInputStateReceived ( Client client, byte [] inputs )
    {
        ClientInputState message = BytesToState ( inputs );

        // Ensure the key exists, if it doesn't, create it
        if ( m_clientInputs.ContainsKey ( client.player ) == false )
        {
            Debug.Log ( "new client added to input processing queue" );
            m_clientInputs.Add ( client.player, new Queue<ClientInputState> () );
        }

        // Add the input to the appropriate queue
        m_clientInputs [ client.player ].Enqueue ( message );
    }

    public static void OnClientDisconnected ( Client client )
    {
        if ( client == null )
        {
            throw new System.NullReferenceException ( "Client is null." );
        }

        if ( m_clientInputs.Remove ( client.player ) )
        {
            Debug.Log ( "Successfully removed client from input queue." );
        }
    }

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
