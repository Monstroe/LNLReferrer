namespace LiteNetLib_Referrer;

public enum ServiceSendType
{
    ID = 0,
    RoomCode = 1,
    RoomMembers = 2,
    MemberJoined = 3,
    MemberLeft = 4,
    RoomStart = 5,
    RoomClosed = 6,
    Invalid = 7
}

public enum ServiceReceiveType
{
    CreateRoom,
    JoinRoom,
    LeaveRoom,
    StartRoom,
    CloseRoom
}