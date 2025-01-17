using LiteNetLib;

namespace LiteNetLib_Referrer;

public class Client
{
    public Guid ID { get; }
    public string Name { get; set; }
    public NetPeer RemotePeer { get; }
    public Room CurrentRoom { get; set; }
    public bool IsHost { get { return CurrentRoom != null && CurrentRoom.Host == this; } }

    public Client(Guid id, NetPeer peer)
    {
        ID = id;
        RemotePeer = peer;
    }
}