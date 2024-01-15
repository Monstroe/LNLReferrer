# C# Referrer Using LiteNetLib

A simple C# referrer that can be used for creating lobbies/rooms for online multiplayer video games. It utilizes a host and guest setup; the host is the client that created the room and the guests are clients that join the host's room. All game logic is run on the host computer, and the host communicates with the guests via this referrer.

## Getting Started

### Dependencies

* [LiteNetLib](https://github.com/RevenantX/LiteNetLib) ([1.1.0](https://github.com/RevenantX/LiteNetLib/releases/tag/v1.1.0))
* .NET 7

### Installing and Executing Program

* Clone this repository (LiteNetLib is included)
* Build and run [Referrer.cs](https://github.com/Monstroe/LNLReferrer/blob/main/Referrer.cs)
* See code block below for Linux install
  
  ```
  sudo apt install dotnet-sdk-7.0
  git clone https://github.com/Monstroe/Referrer-LiteNetLib.git
  cd LNLReferrer
  dotnet run
  ```
## Version History
* 1.0
    * Initial Release
