using LiteNetLib;

public class Client
{
    public Guid ID { get; }
    private NetPeer Peer { get; }

    public Client(int id, NetPeer peer)
    {
        ID = id;
        Peer = peer;
    }
}
