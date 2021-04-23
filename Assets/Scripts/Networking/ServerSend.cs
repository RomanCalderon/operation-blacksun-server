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

    public static void Ping ( int _toClient )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.ping ) )
        {
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

    public static void PlayerPosition ( int _playerId, Vector3 _position )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerPosition ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _position );

            SendUDPDataToAll ( _playerId, _packet );
        }
    }

    public static void PlayerRotation ( int _playerId, Quaternion _rotation )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerRotation ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _rotation );

            SendUDPDataToAll ( _playerId, _packet );
        }
    }

    public static void PlayerMovement ( int _playerId, int moveInputX, int moveInputY, float moveSpeed, bool _run, bool _crouch )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerMovement ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( moveInputX );
            _packet.Write ( moveInputY );
            _packet.Write ( moveSpeed );
            _packet.Write ( _run );
            _packet.Write ( _crouch );

            SendUDPDataToAll ( _packet );
        }
    }

    public static void PlayerInputProcessed ( int _playerId, byte [] _requestProcessed )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerInputProcessed ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _requestProcessed.Length );
            _packet.Write ( _requestProcessed );

            SendTCPData ( _playerId, _packet );
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

    public static void PlayerHealth ( int _playerId, float _health )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerHealth ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _health );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayerRespawned ( int _playerId )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playerRespawned ) )
        {
            _packet.Write ( _playerId );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayerUpdateInventorySlot ( int _playerId, string _slotId )
    {
        PlayerUpdateInventorySlot ( _playerId, _slotId, 0 );
    }

    public static void PlayerUpdateInventorySlot ( int _playerId, string _slotId, int _quantity )
    {
        PlayerUpdateInventorySlot ( _playerId, _slotId, string.Empty, _quantity );
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

    public static void SpawnHitObject ( int _hitType, Vector3 _hitPosition, Vector3 _hitNormal )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.spawnHitObject ) )
        {
            _packet.Write ( _hitType );
            _packet.Write ( _hitPosition );
            _packet.Write ( _hitNormal );

            SendTCPDataToAll ( _packet );
        }
    }

    public static void PlayAudioClip ( int _playerId, string _audioClipName, float _volume, Vector3 _location, float _minDistance, float _maxDistance )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.playAudioClip ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _audioClipName );
            _packet.Write ( _volume );
            _packet.Write ( _location );
            _packet.Write ( _minDistance );
            _packet.Write ( _maxDistance );

            SendTCPDataToAll ( _playerId, _packet );
        }
    }

    public static void Hitmarker ( int _playerId, int _hitmarkerType )
    {
        using ( Packet _packet = new Packet ( ( int ) ServerPackets.hitmarker ) )
        {
            _packet.Write ( _playerId );
            _packet.Write ( _hitmarkerType );

            SendTCPData ( _playerId, _packet );
        }
    }

    #endregion
}
