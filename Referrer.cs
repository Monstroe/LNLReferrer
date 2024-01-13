using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

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

    private Dictionary<int, Client> clients;
    private Dictionary<int, Room> rooms;
    private Dictionary<string, PacketHandler> packetHandlers;

    private Referrer()
    {
        listener = new EventBasedNetListener();
        netManager = new NetManager(listener);
        writer = new NetDataWriter();

        clients = new Dictionary<int, Client>();
        rooms = new Dictionary<int, Room>();
        packetHandlers = new Dictionary<string, PacketHandler>()
        {
            { "CREATEROOM", CreateRoom },
            { "JOINROOM", JoinRoom },
            { "LEAVEROOM", LeaveRoom },
            { "CLOSEROOM", CloseRoom },
            { "STARTROOM", StartRoom }
        };
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
        clients.Add(peer.Id, new Client(peer));
        Send(clients[peer.Id], IDPacket(peer.Id), DeliveryMethod.ReliableOrdered);
        Console.WriteLine("Client " + peer.ToString() + " Connected!");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine("Client " + peer.ToString() + " Disconnected: " + disconnectInfo.Reason.ToString());

        if (clients[peer.Id].CurrentRoom != null)
        {
            if (clients[peer.Id].IsHost)
            {
                CloseRoom(clients[peer.Id].CurrentRoom);
            }
            else
            {
                LeaveRoom(clients[peer.Id], clients[peer.Id].CurrentRoom);
            }
        }

        clients.Remove(peer.Id);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.Error.WriteLine("Network Error from " + endPoint + ": " + socketError.ToString());
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        string packet = reader.GetString();
        string command = packet.Split(':')[0];
        string message = packet.Split(':')[1];

        if (deliveryMethod != DeliveryMethod.Unreliable)
        {
            Console.WriteLine("Packet Received from " + peer.ToString() + ": " + command);
        }

        if (packetHandlers.ContainsKey(command))
        {
            packetHandlers[command](clients[peer.Id], message);
        }
        else
        {
            if (clients[peer.Id].CurrentRoom != null)
            {
                if (clients[peer.Id].IsHost)
                {
                    Send(clients[peer.Id].CurrentRoom.Guests, packet, deliveryMethod);
                }
                else
                {
                    Send(clients[peer.Id].CurrentRoom.Host, packet, deliveryMethod);
                }
            }
            else
            {
                Console.Error.WriteLine("Client " + peer.ToString() + " Sent Invalid Packet: " + packet);
            }
        }
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Console.WriteLine("Connection Request from " + request.RemoteEndPoint);
        request.AcceptIfKey("Bruh-Wizz-ArcGIS");
    }

    public void Send(Client client, string message, DeliveryMethod deliveryMethod)
    {
        try 
        {
            writer.Put(message);
            client.Peer.Send(writer, deliveryMethod);
            writer.Reset();
        }
        catch(SocketException e)
        {
            Console.Error.WriteLine("Socket Exception While Sending: " + e.SocketErrorCode.ToString());
        }
    }

    public void Send(List<Client> clients, string message, DeliveryMethod deliveryMethod)
    {
        foreach (Client client in clients)
        {
            Send(client, message, deliveryMethod);
        }
    }

    public void LeaveRoom(Client client, Room room)
    {
        Console.WriteLine("Client " + client.Peer.ToString() + " Left Room with Code: " + room.ID);
        Send(room.Members/*GetMembersExcept(client)*/, MemberLeftPacket(client.ID), DeliveryMethod.ReliableOrdered);
        room.Members.Remove(client);
    }

    public void CloseRoom(Room room)
    {
        Console.WriteLine("Closing Room with Code: " + room.ID);
        foreach (Client member in room.Members)
        {
            member.CurrentRoom = null;
        }
        Send(room.Members, RoomClosedPacket(), DeliveryMethod.ReliableOrdered);
        rooms.Remove(room.ID);
    }

    //----------------------------------PACKET HANDLERS----------------------------------//

    public void CreateRoom(Client client, string message)
    {
        Room room = new Room(GenerateRoomID());
        room.Members.Add(client);
        client.CurrentRoom = room;
        rooms.Add(room.ID, room);

        Console.WriteLine("Creating Room... New Room Code for Client " + client.Peer.ToString() + ": " + room.ID);
        Send(client, RoomCodePacket(room.ID), DeliveryMethod.ReliableOrdered);
    }

    public void JoinRoom(Client client, string message)
    {
        if (int.TryParse(message, out int roomID))
        {
            Room room = rooms[roomID];
            room.Members.Add(client);
            client.CurrentRoom = room;

            Console.WriteLine("Client " + client.Peer.ToString() + " Joining Room with Code: " + room.ID);
            foreach (Client member in room.Members)// room.GetMembersExcept(client))
            {
                Send(member, MemberJoinedPacket(client.ID), DeliveryMethod.ReliableOrdered);
            }
        }
        else
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Sent Invalid Room Code: " + roomID);
            Send(client, InvalidPacket("Invalid Room Code"), DeliveryMethod.ReliableOrdered);
            return;
        }
    }

    public void LeaveRoom(Client client, string message)
    {
        if (client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Leave Room Despite not Being in One");
            Send(client, InvalidPacket("Client Not in Room"), DeliveryMethod.ReliableOrdered);
            return;
        }

        if (client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Leave a Room as a Host");
            Send(client, InvalidPacket("Host Attempted to Leave Room"), DeliveryMethod.ReliableOrdered);
            return;
        }

        LeaveRoom(client, client.CurrentRoom);
    }

    public void CloseRoom(Client client, string message)
    {
        if (client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Leave Room Despite not Being in One");
            Send(client, InvalidPacket("Client Not in Room"), DeliveryMethod.ReliableOrdered);
            return;
        }

        if (!client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Close a Room as a Guest");
            Send(client, InvalidPacket("Guest Attempted to Close Room"), DeliveryMethod.ReliableOrdered);
        }

        CloseRoom(client.CurrentRoom);
    }

    public void StartRoom(Client client, string message) 
    {
        if(client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Start Room Despite not Being in One");
            Send(client, InvalidPacket("Client Not in Room"), DeliveryMethod.ReliableOrdered);
            return;
        }

        if(!client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.Peer.ToString() + " Attempted to Start a Room as a Guest");
            Send(client, InvalidPacket("Guest Attempted to Start Room"), DeliveryMethod.ReliableOrdered);
            return;
        }

        Console.WriteLine("Starting Room... With Code: " + client.CurrentRoom.ID);
        Send(client.CurrentRoom.Members, RoomStartPacket(), DeliveryMethod.ReliableOrdered);
    }

    //----------------------------------PACKET BUILDERS----------------------------------//

    public static string IDPacket(int id)
    {
        return "ID:" + id.ToString();
    }

    public string RoomCodePacket(int roomCode)
    {
        return "ROOMCODE:" + roomCode;
    }

    public string MemberJoinedPacket(int memberID)
    {
        return "MEMBERJOIN:" + memberID;
    }

    public string MemberLeftPacket(int memberID)
    {
        return "MEMBERLEFT:" + memberID;
    }

    public string RoomClosedPacket()
    {
        return "ROOMCLOSED:ACK";
    }

    public string RoomStartPacket()
    {
        return "ROOMSTART:ACK";
    }

    public string InvalidPacket(string errorMessage)
    {
        return "INVALID:" + errorMessage;
    }

    //-----------------------------------------------------------------------------------//

    public int GenerateRoomID()
    {
        var random = new Random();
        int randomNumber = random.Next(0, 10000);
        if (rooms.ContainsKey(randomNumber))
        {
            return GenerateRoomID();
        }
        return randomNumber;
    }

    static void Main(string[] args)
    {
        Referrer.Instance.Start(7777);
    }
}

public class Client
{
    public int ID { get { return Peer.Id; } }
    public NetPeer Peer { get; set; }
    public Room CurrentRoom { get; set; }
    public bool IsHost { get { return CurrentRoom != null && CurrentRoom.Host == this; } }

    public Client(NetPeer peer)
    {
        Peer = peer;
    }
}

public class Room
{
    public int ID { get; set; }
    public List<Client> Members { get; set; }
    public Client Host { get { return Members[0]; } }
    public List<Client> Guests { get { return Members.GetRange(1, Members.Count - 1); } }

    public Room(int id)
    {
        ID = id;
        Members = new List<Client>();
    }

    public List<Client> GetMembersExcept(Client clientToExclude)
    {
        List<Client> membersExcluding = new List<Client>();
        foreach (Client member in Members)
        {
            if (member != clientToExclude)
            {
                membersExcluding.Add(member);
            }
        }
        return membersExcluding;
    }
}