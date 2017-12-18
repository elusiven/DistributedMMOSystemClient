using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using Assets.Scripts;
using elusivenWorks;
using FlatBuffers;
using UnityEngine;
using UnityEngine.UI;

public class EventListener : MonoBehaviour
{
    public string Id { get; set; }
    public GameObject PlayerPrefab;

    public GameObject _idTextGo;
    private Text _idText;

    private StreamWriter _writer;
    private NetworkStream _stream;

    private Clients _networkPlayers;
    private GameObject _playerGo;

    private int _spawnCount;

	// Use this for initialization
	void Start ()
	{
	    if (_idText == null)
	    {
	        _idText = _idTextGo.GetComponent<Text>();
	    }

	    if (_networkPlayers == null)
	    {
	        _networkPlayers = GetComponent<Clients>();
	    }

	    _spawnCount = 0;

	    print("Connecting to Server...");
        TcpClient client = new TcpClient("127.0.0.1", 27800);
	    _stream = client.GetStream();
	    _stream.ReadTimeout = 10;

	    if (_stream.CanRead)
	    {
	        _writer = new StreamWriter(_stream);
	    }
	}

    public void SendInitialConnectionAck()
    {
        FlatBufferBuilder fbb = new FlatBufferBuilder(1024);

        var playerInfoOffset = FlatCreator.CreatePlayerInfo(fbb, Id, _playerGo.transform.position.x, _playerGo.transform.position.y,
            _playerGo.transform.position.z,
            _playerGo.transform.rotation.x, _playerGo.transform.rotation.y, _playerGo.transform.rotation.z,
            _playerGo.transform.rotation.w);

        InitialConnectCommand.StartInitialConnectCommand(fbb);
        InitialConnectCommand.AddPlayer(fbb, playerInfoOffset);
        var initialCommandOffset = InitialConnectCommand.EndInitialConnectCommand(fbb);

        MessageRoot.StartMessageRoot(fbb);
        MessageRoot.AddDataType(fbb, Data.InitialConnectCommand);
        MessageRoot.AddData(fbb, initialCommandOffset.Value);
        var msgRootOffset = MessageRoot.EndMessageRoot(fbb);
        MessageRoot.FinishMessageRootBuffer(fbb, msgRootOffset);


        byte[] buf = fbb.SizedByteArray();

        _writer.BaseStream.Write(buf, 0, buf.Length);
        _writer.Flush();
    }

    public void SendMovement(float pX, float pY, float pZ, 
        float rX, float rY, float rZ, float rW)
    {

        FlatBufferBuilder fbb = new FlatBufferBuilder(1024);

        var playerInfoOffset = FlatCreator.CreatePlayerInfo(fbb, Id, pX, pY, pZ, rX, rY, rZ, rW);

        MovementCommand.StartMovementCommand(fbb);
        MovementCommand.AddPlayer(fbb, playerInfoOffset);
        var movementCommandOffset = MovementCommand.EndMovementCommand(fbb);

        MessageRoot.StartMessageRoot(fbb);
        MessageRoot.AddDataType(fbb, Data.MovementCommand);
        MessageRoot.AddData(fbb, movementCommandOffset.Value);
        var msgRootOffset = MessageRoot.EndMessageRoot(fbb);
        MessageRoot.FinishMessageRootBuffer(fbb, msgRootOffset);

        byte[] buf = fbb.SizedByteArray();

        _writer.BaseStream.Write(buf, 0, buf.Length);
        _writer.Flush();
    }

    public void SendMeetPlayer(string id)
    {
        FlatBufferBuilder fbb = new FlatBufferBuilder(1024);

        var _id = fbb.CreateString(id);

        PlayerInfo.StartPlayerInfo(fbb);
        PlayerInfo.AddId(fbb, _id);
        var playerInfoOffset = PlayerInfo.EndPlayerInfo(fbb);

        MeetCommand.StartMeetCommand(fbb);
        MeetCommand.AddOtherPlayer(fbb, playerInfoOffset);
        var meetCommandOffset = MeetCommand.EndMeetCommand(fbb);

        MessageRoot.StartMessageRoot(fbb);
        MessageRoot.AddDataType(fbb, Data.MeetCommand);
        MessageRoot.AddData(fbb, meetCommandOffset.Value);
        var msgRootOffset = MessageRoot.EndMessageRoot(fbb);
        MessageRoot.FinishMessageRootBuffer(fbb, msgRootOffset);

        byte[] buf = fbb.SizedByteArray();

        _writer.BaseStream.Write(buf, 0, buf.Length);
        _writer.Flush();
    }

    void ReadData()
    {
        if (_stream.CanRead)
        {
            if (_stream.DataAvailable)
            {
                try
                {
                    byte[] bLen = new byte[4];
                    int data = _stream.Read(bLen, 0, 4);
                    if (data > 0)
                    {
                        int len = BitConverter.ToInt32(bLen, 0);
                        Byte[] buff = new byte[1024];
                        data = _stream.Read(buff, 0, len);
                        if (data > 0)
                        {
                            
                            ByteBuffer bb = new ByteBuffer(buff);
                            MessageRoot msg = MessageRoot.GetRootAsMessageRoot(bb);
                            switch (msg.DataType)
                            {
                                case Data.NONE:
                                    // Do nothing
                                    break;
                                case Data.InitialConnectCommand:
                                {
                                    var player = msg.Data<InitialConnectCommand>().Value.Player.Value;

                                    if (_spawnCount == 0)
                                    {
                                        // Spawn the player
                                        // Set player's ID
                                        Id = player.Id;
                                        _idText.text = player.Id;
                                        _playerGo = Instantiate(PlayerPrefab, new Vector3(player.Pos.Value.X, player.Pos.Value.Y, player.Pos.Value.Z),
                                            new Quaternion(0, 0, 0, 0));
                                        SendInitialConnectionAck();
                                    }
                                    else
                                    {
                                        _networkPlayers.AddPlayer(player.Id, new Vector3(player.Pos.Value.X, player.Pos.Value.Y, player.Pos.Value.Z),
                                            new Quaternion(0, 0, 0, 0));
                                    }
                                    
                                    Debug.Log(this.name + ": Command<" + msg.DataType + ">" + "   -  Player ID : " + player.Id);
                                    _spawnCount++;
                                }
                                    break;
                                case Data.MovementCommand:
                                {
                                    // Move the player
                                    var player = msg.Data<MovementCommand>().Value.Player.Value;
                                    _networkPlayers.MovePlayer(player.Id, new Vector3(player.Pos.Value.X, player.Pos.Value.Y, player.Pos.Value.Z),
                                        new Quaternion(player.Rot.Value.X, player.Rot.Value.Y, player.Rot.Value.Z, player.Rot.Value.W));
                                    Debug.Log(this.name + ": Command<" + msg.DataType + ">" + "   -  Player ID : " + player.Id);
                                }
                                    break;
                                case Data.MeetCommand:
                                {
                                    // Player entered another player's area of interest
                                    var otherPlayer = msg.Data<MeetCommand>().Value.OtherPlayer.Value;
                                    Debug.Log(this.name + ": Command<" + msg.DataType + ">" + "   -  Player ID : " + otherPlayer.Id);
                                    }
                                    break;
                                case Data.PlayerInfo:
                                {
                                    
                                }
                                    break;
                            }
                            _stream.Flush();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }

    void Update()
    {
        ReadData();
    }


}
