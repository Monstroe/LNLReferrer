using LiteNetLib;

namespace LiteNetLib_Referrer;

public class PacketSender
{
    public static PacketSender Instance { get; } = new PacketSender();

    public void ID(Client client, Guid id)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.ID);
        packet.Write(id.ToString());
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomCode(Client client, int roomCode)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.RoomCode);
        packet.Write(roomCode);
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomMembers(Client client, Room room)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.RoomMembers);
        packet.Write(room.ID);
        packet.Write(room.Members.Count);
        foreach (Client member in room.Members)
        {
            packet.Write(member.ID.ToString());
            packet.Write(member.Name ?? "");
        }
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void MemberJoined(List<Client> clients, Client member)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.MemberJoined);
        packet.Write(member.ID.ToString());
        packet.Write(member.Name ?? "");
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void MemberLeft(List<Client> clients, Client member)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.MemberLeft);
        packet.Write(member.ID.ToString());
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomStart(List<Client> clients)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.RoomStart);
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomClosed(List<Client> clients)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.RoomClosed);
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void Invalid(Client client, string errorMessage)
    {
        NetPacket packet = new NetPacket();
        packet.Write((byte)0); // Insert the command key at the start of the packet
        packet.Write((byte)ServiceSendType.Invalid);
        packet.Write(errorMessage);
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }
}