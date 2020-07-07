using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

public class ServerSend
{
    #region ServerSend Methods
    private static void SendTCPData ( int _toClient, Packet _packet )
    {
        _packet.WriteLength ();
        Server.clients [ _toClient ].tcp.SendData ( _packet );
    }

    private static void SendUDPData ( int _toClient, Packet _packet )
    {
        _packet.WriteLength ();
        Server.clients [ _toClient ].udp.SendData ( _packet );
    }

    private static void SendTCPDataToAll ( Packet _packet )
    {
        _packet.WriteLength ();
        for ( int i = 1; i <= Server.MaxPlayers; i++ )
        {
            Server.clients [ i ].tcp.SendData ( _packet );
        }
    }

    private static void SendTCPDataToAll ( int _exceptClient, Packet _packet )
    {
        _packet.WriteLength ();
        for ( int i = 1; i <= Server.MaxPlayers; i++ )
        {
            if ( i != _exceptClient )
            {
                Server.clients [ i ].tcp.SendData ( _packet );
            }
        }
    }

    private static void SendUDPDataToAll ( Packet _packet )
    {
        _packet.WriteLength ();
        for ( int i = 1; i <= Server.MaxPlayers; i++ )
        {
            Server.clients [ i ].udp.SendData ( _packet );
        }
    }

    private static void SendUDPDataToAll ( int _exceptClient, Packet _packet )
    {
        _packet.WriteLength ();
        for ( int i = 1; i <= Server.MaxPlayers; i++ )
        {
            if ( i != _exceptClient )
            {
                Server.clients [ i ].udp.SendData ( _packet );
            }
        }
    }
    #endregion

    #region Packets
    public static void Welcome ( int _toClient, string _msg )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.welcome ) )
        {
            _packet.Write ( _msg );
            _packet.Write ( _toClient );

            SendTCPData ( _toClient, _packet );
        }
    }

    #region Player
    public static void ConnectPlayer ( int _toClient, int _clientId, string _username )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerConnected ) )
        {
            _packet.Write ( _clientId );
            _packet.Write ( _username );

            SendTCPData ( _toClient, _packet );
        }
    }

    public static void Ping ( int _toClient, int _elapsedTime, string _serverBounceTime )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.ping ) )
        {
            _packet.Write ( _elapsedTime );
            _packet.Write ( _serverBounceTime );

            SendTCPData ( _toClient, _packet );
        }
    }

    public static void SpawnPlayer ( int _toClient, Player _player )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.spawnPlayer ) )
        {
            _packet.Write ( _player.Id );
            _packet.Write ( _player.transform.position );
            _packet.Write ( _player.transform.rotation );

            SendTCPData ( _toClient, _packet );
        }
    }

    public static void PlayerPosition ( Player _player )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerPosition ) )
        {
            _packet.Write ( _player.Id );
            _packet.Write ( _player.transform.position );

            SendUDPDataToAll ( _packet );
        }
    }

    public static void PlayerRotation ( Player _player )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerRotation ) )
        {
            _packet.Write ( _player.Id );
            _packet.Write ( _player.transform.rotation );

            SendUDPDataToAll ( _player.Id, _packet );
        }
    }

    public static void PlayerMovementVector ( Player _player, Vector2 _movement )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerMovementVector ) )
        {
            _packet.Write ( _player.Id );
            _packet.Write ( _movement.x );
            _packet.Write ( _movement.y );

            SendUDPDataToAll ( _player.Id, _packet );
        }
    }

    public static void PlayerDisconnected ( int _playerId )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerDisconnected ) )
        {
            _packet.Write ( _playerId );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayerHealth ( Player _player )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerHealth ) )
        {
            _packet.Write ( _player.Id );
            _packet.Write ( _player.Health );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayerRespawned ( Player _player )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerRespawned ) )
        {
            _packet.Write ( _player.Id );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayerUpdateInventorySlot ( int _playerId, string _slotId, string _playerItemId, int _quantity )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerUpdateInventorySlot ) )
        {
            _packet.Write ( _playerId );

            _packet.Write ( _slotId );
            _packet.Write ( _playerItemId );
            _packet.Write ( _quantity );

            SendTCPData ( _playerId, _packet );
        }
    }

    #endregion

    #endregion
}
