using LiteNetLib;

namespace LiteNetLib_Referrer;

public class PacketReceiver
{
    public static PacketReceiver Instance { get; } = new PacketReceiver();

    public void Name(Client client, NetPacket packet)
    {
        if (client.Name != null)
        {
            Console.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to set their name despite already having one");
            PacketSender.Instance.Invalid(client, "Client already has name");
            return;
        }

        string name = packet.ReadString();
        client.Name = name;

        Console.WriteLine("Client " + client.RemotePeer.ToString() + " set their name to: " + name);
    }

    public void CreateRoom(Client client, NetPacket packet)
    {
        Room room = new Room(Referrer.Instance.GenerateRoomID());
        room.Members.Add(client);
        client.CurrentRoom = room;
        Referrer.Instance.Rooms.Add(room.ID, room);

        Console.WriteLine("Creating Room... New room code for client " + client.RemotePeer.ToString() + ": " + room.ID);
        PacketSender.Instance.RoomCode(client, room.ID);
    }

    public void JoinRoom(Client client, NetPacket packet)
    {
        if (client.CurrentRoom != null)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to join a room despite already being in one");
            PacketSender.Instance.Invalid(client, "Client already in room");
            return;
        }

        int roomID = packet.ReadInt();
        if (Referrer.Instance.Rooms.TryGetValue(roomID, out Room? room))
        {
            PacketSender.Instance.MemberJoined(room.Members, client);
            room.Members.Add(client);
            PacketSender.Instance.RoomMembers(client, room);

            client.CurrentRoom = room;

            Console.WriteLine("Client " + client.RemotePeer.ToString() + " joining room with code: " + room.ID);
        }
        else
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " sent invalid room code: " + roomID);
            PacketSender.Instance.Invalid(client, "Invalid room code");
        }
    }

    public void LeaveRoom(Client client, NetPacket packet)
    {
        if (client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to leave a room despite not being in one");
            PacketSender.Instance.Invalid(client, "Client not in room");
            return;
        }

        if (client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to leave a room as a host");
            PacketSender.Instance.Invalid(client, "Host attempted to leave room");
            return;
        }

        Referrer.Instance.LeaveRoom(client, client.CurrentRoom);
    }

    public void StartRoom(Client client, NetPacket packet)
    {
        if (client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to start a room despite not being in one");
            PacketSender.Instance.Invalid(client, "Client not in room");
            return;
        }

        if (!client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to start a room as a guest");
            PacketSender.Instance.Invalid(client, "Guest attempted to start room");
            return;
        }

        Console.WriteLine("Starting Room... With Code: " + client.CurrentRoom.ID);
        PacketSender.Instance.RoomStart(client.CurrentRoom.Members);
    }

    public void CloseRoom(Client client, NetPacket packet)
    {
        if (client.CurrentRoom == null)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to leave a room despite not being in one");
            PacketSender.Instance.Invalid(client, "Client not in room");
            return;
        }

        if (!client.IsHost)
        {
            Console.Error.WriteLine("Client " + client.RemotePeer.ToString() + " attempted to close a room as a guest");
            PacketSender.Instance.Invalid(client, "Guest attempted to close room");
        }

        Referrer.Instance.CloseRoom(client.CurrentRoom);
    }
}