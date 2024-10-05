using LiteNetLib;

namespace LiteNetLib_Referrer;

public class Client
{
    public Guid ID { get; }
    public NetPeer Peer { get; }
    public Room CurrentRoom { get; set; }
    public bool IsHost { get { return CurrentRoom != null && CurrentRoom.Host == this; } }

    public Client(Guid id, NetPeer peer)
    {
        ID = id;
        Peer = peer;
    }
}