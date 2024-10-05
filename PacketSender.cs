using LiteNetLib;

namespace LiteNetLib_Referrer;

public class PacketSender
{
    public static PacketSender Instance { get; } = new PacketSender();

    public void ID(Client client, Guid id)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.ID);
        packet.Write(id.ToString());
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomCode(Client client, int roomCode)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.RoomCode);
        packet.Write(roomCode);
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomMembers(Client client, List<Client> members)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.RoomMembers);
        packet.Write(members.Count);
        foreach (Client member in members)
        {
            packet.Write(member.ID.ToString());
        }
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void MemberJoined(List<Client> clients, Guid memberID)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.MemberJoined);
        packet.Write(memberID.ToString());
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void MemberLeft(Client client, Guid memberID)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.MemberLeft);
        packet.Write(memberID.ToString());
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomStart(List<Client> clients)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.RoomStart);
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void RoomClosed(List<Client> clients)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.RoomClosed);
        Referrer.Instance.Send(clients, packet, DeliveryMethod.ReliableOrdered);
    }

    public void Invalid(Client client, string errorMessage)
    {
        Packet packet = new Packet();
        packet.Write((short)ServiceSendType.Invalid);
        packet.Write(errorMessage);
        Referrer.Instance.Send(client, packet, DeliveryMethod.ReliableOrdered);
    }
}