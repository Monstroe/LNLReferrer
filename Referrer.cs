using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using LiteNetLib;
using LiteNetLib.Utils;

public enum PacketType
{
    ID,
}

class Referrer
{
    private static Referrer instance;
    public static Referrer Instance 
    { 
        get 
        {
            if (instance == null)
                instance = new Referrer();
            return instance;
        }
    }

    public int Port { get; set; }

    public delegate void PacketHandler(Client client, string message);

    private EventBasedNetListener listener;
    private NetManager netManager;
    private NetDataWriter writer;

    private Dictionary<int, NetPeer> clients;

    private Referrer()
    {
        listener = new EventBasedNetListener();
        netManager = new NetManager(listener);

        clients = new Dictionary<int, NetPeer>();
    }

    public void Start(int port) 
    {
        Console.WriteLine("Starting Referrer...");
        Port = port;
        netManager.Start(Port);
        while (true)
        {
            netManager.PollEvents();
            Thread.Sleep(15);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        clients.Add(peer.Id, peer);
        Send(peer, PacketBuilder.ID(peer.Id), DeliveryMethod.ReliableOrdered);
        Console.WriteLine("Client " + peer.ToString() + " Connected!");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        throw new NotImplementedException();
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        throw new NotImplementedException();
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        string packet = reader.GetString();
        string command = packet.Split(':')[0];
        string message = packet.Split(':')[1];
        
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Console.WriteLine("Connection Request from " + request.RemoteEndPoint);
        request.AcceptIfKey("Bruh-Wizz-ArcGIS");
    }

    public void Send(NetPeer peer, string message, DeliveryMethod deliveryMethod)
    {
        writer.Put(message);
        peer.Send(writer, deliveryMethod);
        writer.Reset();
    }

    static void Main(string[] args)
    {
        Referrer.Instance.Start();
    }
}

