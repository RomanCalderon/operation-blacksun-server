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
        ServerSend.Ping ( _fromClient );
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

    #region Player

    public static void PlayerReady ( int _fromClient, Packet _packet )
    {
        int _clientIdCheck = _packet.ReadInt ();

        if ( _fromClient != _clientIdCheck )
        {
            Debug.Log ( $"Player [{_fromClient}] has assumed the wrong client ID ({_clientIdCheck})!" );
        }
        else
        {
            if ( Server.clients [ _fromClient ].player != null )
            {
                Server.clients [ _fromClient ].player.InitializeInventory ();
            }
        }
    }

    public static void PlayerInput ( int _fromClient, Packet _packet )
    {
        int length = _packet.ReadInt ();
        byte [] inputs = _packet.ReadBytes ( length );

        ServerSimulation.OnClientInputStateReceived ( Server.clients [ _fromClient ], inputs );
    }

    public static void WeaponShoot ( int _fromClient, Packet _packet )
    {
        uint tick = _packet.ReadUInt ();
        float clientSubFrame = _packet.ReadFloat ();

        LagCompensationHandler.OnClientWeaponShoot ( Server.clients [ _fromClient ], tick, clientSubFrame );
    }

    public static void WeaponSwitch ( int _fromClient, Packet _packet )
    {
        int activeWeaponIndex = _packet.ReadInt ();

        Server.clients [ _fromClient ].player.WeaponSwitch ( activeWeaponIndex );
    }

    public static void WeaponReload ( int _fromClient, Packet _packet )
    {
        Server.clients [ _fromClient ].player.WeaponReload ();
    }

    public static void WeaponCancelReload ( int _fromClient, Packet _packet )
    {
        Server.clients [ _fromClient ].player.CancelWeaponReload ();
    }

    #region Inventory

    public static void PlayerTransferSlotContents ( int _fromClient, Packet _packet )
    {
        string fromSlotId = _packet.ReadString ();
        string toSlotId = _packet.ReadString ();
        int transferMode = _packet.ReadInt ();

        Server.clients [ _fromClient ].player.InventoryManager.TransferContents ( fromSlotId, toSlotId, transferMode );
    }

    public static void PlayerInventoryReduceItem ( int _fromClient, Packet _packet )
    {
        string playerItemId = _packet.ReadString ();
        int reductionAmount = _packet.ReadInt ();

        Server.clients [ _fromClient ].player.InventoryManager.ReduceItem ( playerItemId, reductionAmount );
    }

    public static void PlayerDropItem ( int _fromClient, Packet _packet )
    {
        string fromSlotId = _packet.ReadString ();
        int transferMode = _packet.ReadInt ();

        Server.clients [ _fromClient ].player.InventoryManager.DropItem ( fromSlotId, transferMode, out _ );
    }

    #endregion

    public static void PlayerKillSelf ( int _fromClient, Packet _packet )
    {
        int _clientIdCheck = _packet.ReadInt ();

        if ( _fromClient != _clientIdCheck )
        {
            Debug.Log ( $"Player [{_fromClient}] has assumed the wrong client ID ({_clientIdCheck})!" );
        }
        else
        {
            if ( Server.clients [ _fromClient ].player != null )
            {
                Server.clients [ _fromClient ].player.TakeDamage ( 9999, out bool _ );
            }
        }
    }

    #endregion
}
