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
    Name = 0,
    CreateRoom = 1,
    JoinRoom = 2,
    LeaveRoom = 3,
    StartRoom = 4,
    CloseRoom = 5
}