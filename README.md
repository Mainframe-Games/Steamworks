# Steamworks Mainframe
Wrapper for [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET/tree/master). Inspired by [Facepunch](https://wiki.facepunch.com/steamworks/).

The reason for this is that the Facepunch library [doesn't work on Apple Silicon](https://github.com/Facepunch/Facepunch.Steamworks/issues/591). Steamworks.NET does, however, the API is more cumbersome to use. So I wanted to create a Facepunch-like experience using Steamworks.NET API.

## Install

Ensure you have the following packages

This package has a dependency on [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET/tree/master)

```json
"com.rlabrecque.steamworks.net": "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#20.2.0"
```

Install this package:

```json
"games.mainframe.steamworks": "https://github.com/Mainframe-Games/Steamworks.git"
```

(Optional) If you plan to use SteamP2P with Unity Netcode: 

```json
"com.community.netcode.transport.steamnetworkingsockets": "https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.steamnetworkingsockets"
```
