using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived ( int _fromClient, Packet _packet )
    {
        int _clientIdCheck = _packet.ReadInt ();
        string _username = _packet.ReadString ();

        Debug.Log ( $"{Server.clients [ _fromClient ].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient} [ {_username} ]." );
        if ( _fromClient != _clientIdCheck )
        {
            Debug.Log ( $"[{_username}][{_fromClient}] has assumed the wrong client ID ({_clientIdCheck})!" );
        }

        Server.clients [ _fromClient ].username = _username;
        Server.clients [ _fromClient ].ConnectPlayer ();
    }

    public static void Ping ( int _fromClient, Packet _packet )
    {
        string pingStartTime = _packet.ReadString ();

        DateTime parsedPingStartTime = DateTime.ParseExact ( pingStartTime, "o", CultureInfo.CurrentCulture );
        int travelTimeSpan = DateTime.Now.Millisecond - parsedPingStartTime.Millisecond;
        DateTime travelTime = parsedPingStartTime.AddMilliseconds ( travelTimeSpan );

        ServerSend.Ping ( _fromClient, travelTimeSpan, travelTime.ToString ( "o" ) );
    }

    public static void SpawnPlayer ( int _fromClient, Packet _packet )
    {
        int _clientIdCheck = _packet.ReadInt ();
        string _username = _packet.ReadString ();

        if ( _fromClient != _clientIdCheck )
        {
            Debug.Log ( $"[{_username}][{_fromClient}] has assumed the wrong client ID ({_clientIdCheck})!" );
        }
        else
        {
            Debug.Log ( $"[{_username}][{_fromClient}] has spawned." );
            Server.clients [ _fromClient ].SendIntoGame ( _username );
        }
    }

    public static void PlayerReady ( int _fromClient, Packet _packet )
    {
        int _clientIdCheck = _packet.ReadInt ();

        if ( _fromClient != _clientIdCheck )
        {
            Debug.Log ( $"Player [{_fromClient}] has assumed the wrong client ID ({_clientIdCheck})!" );
        }
        else
        {
            Server.clients [ _fromClient ].player.SendInitializedInventory ();
        }
    }

    public static void PlayerMovement ( int _fromClient, Packet _packet )
    {
        bool [] _inputs = new bool [ _packet.ReadInt () ];
        for ( int i = 0; i < _inputs.Length; i++ )
        {
            _inputs [ i ] = _packet.ReadBool ();
        }
        Quaternion _rotation = _packet.ReadQuaternion ();

        Server.clients [ _fromClient ].player.SetInput ( _inputs, _rotation );
    }

    public static void PlayerShoot ( int _fromClient, Packet _packet )
    {
        Vector3 _shootDirection = _packet.ReadVector3 ();

        Server.clients [ _fromClient ].player.Shoot ( _shootDirection );
    }
}
