using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LagCompensationHandler
{
    #region Models

    private struct ShootEventData
    {
        public uint Tick;
        public float ClientSubFrame;

        public ShootEventData ( uint tick, float clientSubFrame )
        {
            Tick = tick;
            ClientSubFrame = clientSubFrame;
        }
    }

    #endregion

    private static Dictionary<int, Queue<ShootEventData>> m_clientShootEvents = new Dictionary<int, Queue<ShootEventData>> ();

    public static void Resimulate ( uint serverTick, int clientId, ClientInputState [] inputArray )
    {
        while ( HasShootEvents ( clientId ) )
        {
            // Get input state with shoot input
            int shootIndex = 0;
            for ( int i = 0; i < inputArray.Length; i++ )
            {
                if ( inputArray [ i ].Shoot )
                {
                    shootIndex = i;
                    break;
                }
            }

            Player player = Server.clients [ clientId ].player;
            ShootEventData eventData = m_clientShootEvents [ clientId ].Dequeue ();
            uint eventTick = ( uint ) Mathf.Min ( eventData.Tick, inputArray [ shootIndex ].ServerTick );
            float clientSubFrame = eventData.ClientSubFrame;
            Vector3 lookDirection = inputArray [ shootIndex ].LookDirection;
            void action () => player.Shoot ( lookDirection );
            SimulationHelper.Simulate ( serverTick, eventTick, action, clientSubFrame );

            // Reinitialize value to 0, indicating this
            // event tick has been processed
            //m_clientShootEvents [ clientId ] = new ShootEventData ( 0, 0 );
        }
    }

    #region Client Event Handlers

    // Weapon shooting
    // TODO: Add clientSubFrame [0f-1f]
    public static void OnClientWeaponShoot ( Client client, uint tick, float clientSubFrame )
    {
        if ( client == null || client.player == null )
        {
            return;
        }

        // Ensure the key exists, if it doesn't, create it
        if ( !m_clientShootEvents.ContainsKey ( client.id ) )
        {
            m_clientShootEvents.Add ( client.id, new Queue<ShootEventData> () );
        }
        // Add the shoot event data
        m_clientShootEvents [ client.id ].Enqueue ( new ShootEventData ( tick, clientSubFrame ) );
    }

    #endregion

    #region Util

    private static bool HasShootEvents ( int clientId )
    {
        return m_clientShootEvents.ContainsKey ( clientId ) && m_clientShootEvents [ clientId ].Count > 0;
    }

    #endregion
}
