﻿using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace LiteNetLib_Referrer;

class Referrer
{
    public static Referrer Instance { get; } = new Referrer();

    public const int POLL_RATE = 15;
    public const int MAX_ROOM_AMOUNT = 10000;

    public delegate void PacketHandler(Client client, Packet packet);

    public int Port { get; set; }
    public string ConnectionKey { get; set; }

    public NetManager NetManager { get; }
    public Dictionary<NetPeer, Client> Clients { get; }
    public Dictionary<int, Room> Rooms { get; }

    private EventBasedNetListener listener;
    private Dictionary<ServiceReceiveType, PacketHandler> packetHandlers;
    private bool running;

    private Referrer()
    {
        listener = new EventBasedNetListener();
        NetManager = new NetManager(listener);

        Clients = new Dictionary<NetPeer, Client>();
        Rooms = new Dictionary<int, Room>();
        packetHandlers = new Dictionary<ServiceReceiveType, PacketHandler>()
        {
            { ServiceReceiveType.CreateRoom, PacketReceiver.Instance.CreateRoom },
            { ServiceReceiveType.JoinRoom, PacketReceiver.Instance.JoinRoom },
            { ServiceReceiveType.LeaveRoom, PacketReceiver.Instance.LeaveRoom },
            { ServiceReceiveType.StartRoom, PacketReceiver.Instance.StartRoom },
            { ServiceReceiveType.CloseRoom, PacketReceiver.Instance.CloseRoom }
        };

        running = false;
    }

    public void Start(int port, string connectionKey)
    {
        Console.WriteLine("Starting Referrer...");
        Port = port;
        ConnectionKey = connectionKey;
        NetManager.Start(Port);
        running = true;
        Console.WriteLine("Referrer Started, waiting for connections...");

        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.PeerConnectedEvent += OnPeerConnected;
        listener.PeerDisconnectedEvent += OnPeerDisconnected;
        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.NetworkErrorEvent += OnNetworkError;

        while (running)
        {
            NetManager.PollEvents();
            Thread.Sleep(POLL_RATE);
        }

        Close();
    }

    public void Stop()
    {
        running = false;
    }

    public void Close()
    {
        Console.WriteLine("Closing Referrer...");
        NetManager.DisconnectAll();
        NetManager.Stop();
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Console.WriteLine("Connection Request from " + request.RemoteEndPoint.ToString());
        request.AcceptIfKey(ConnectionKey);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine("Client " + peer.ToString() + " Connected");
        var client = new Client(Guid.NewGuid(), peer);
        Clients.Add(peer, client);
        PacketSender.Instance.ID(client, client.ID);
        Console.WriteLine("Number of Clients Online: " + Clients.Count);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine("Client " + peer.ToString() + " Disconnected: " + disconnectInfo.Reason.ToString());

        if (Clients.ContainsKey(peer))
        {
            if (Clients[peer].CurrentRoom != null)
            {
                var client = Clients[peer];
                if (client.IsHost)
                {
                    CloseRoom(client.CurrentRoom);
                }
                else
                {
                    LeaveRoom(client, client.CurrentRoom);
                }
            }

            Clients.Remove(peer);
        }

        Console.WriteLine("Number of Clients Online: " + Clients.Count);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        byte[] data = reader.GetRemainingBytes();
        Packet packet = new Packet(data);

        if (packet.Length < 2)
        {
            Console.Error.WriteLine("Invalid Packet Received from " + peer.ToString());
            return;
        }

        ServiceReceiveType command = (ServiceReceiveType)packet.ReadShort();
        if (packetHandlers.TryGetValue(command, out PacketHandler? handler))
        {
            handler(Clients[peer], packet);
        }
        else
        {
            if (Clients[peer].CurrentRoom != null)
            {
                if (Clients[peer].IsHost)
                {
                    Send(Clients[peer].CurrentRoom.Guests, packet, deliveryMethod);
                }
                else
                {
                    Send(Clients[peer].CurrentRoom.Host, packet, deliveryMethod);
                }
            }
            else
            {
                Console.Error.WriteLine("Client " + peer.ToString() + " sent invalid packet");
            }
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.Error.WriteLine("Network Error from " + endPoint + ": " + socketError.ToString());
    }

    public void Send(Client client, Packet packet, DeliveryMethod method)
    {
        try
        {
            client.Peer.Send(packet.ByteArray, method);
        }
        catch (SocketException e)
        {
            Console.Error.WriteLine("Socket Exception While Sending: " + e.SocketErrorCode.ToString());
        }
    }

    public void Send(List<Client> Clients, Packet packet, DeliveryMethod method)
    {
        foreach (Client client in Clients)
        {
            Send(client, packet, method);
        }
    }

    public void LeaveRoom(Client client, Room room)
    {
        Console.WriteLine("Client " + client.Peer.ToString() + " left room with code: " + room.ID);
        PacketSender.Instance.MemberLeft(client, client.ID);
        room.Members.Remove(client);
    }

    public void CloseRoom(Room room)
    {
        Console.WriteLine("Closing room with code: " + room.ID);
        foreach (Client member in room.Members)
        {
            member.CurrentRoom = null;
        }
        PacketSender.Instance.RoomClosed(room.Members);
        Rooms.Remove(room.ID);
    }

    public int GenerateRoomID()
    {
        var random = new Random();
        int randomNumber = random.Next(0, 10000);
        if (Rooms.ContainsKey(randomNumber))
        {
            return GenerateRoomID();
        }
        return randomNumber;
    }

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: Referrer <port> <connectionKey>");
            return;
        }

        string connectionKey = args[1];
        if (int.TryParse(args[0], out int port))
        {
            Console.WriteLine("Passed Port: " + port);
            Console.WriteLine("Passed Connection Key: " + connectionKey + "\n");
            Referrer.Instance.Start(port, connectionKey);
        }
        else
        {
            Console.Error.WriteLine("Invalid Port");
        }
    }
}