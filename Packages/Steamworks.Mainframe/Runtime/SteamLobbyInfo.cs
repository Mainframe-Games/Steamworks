using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Steamworks.Mainframe
{
	public class SteamLobbyInfo
	{
		public ulong LobbyId { get; }
		public bool IsHost => HostId == Steam.SteamId;
		public ulong HostId => SteamMatchmaking.GetLobbyOwner((CSteamID)LobbyId).m_SteamID;
		public int PlayerCount => SteamMatchmaking.GetNumLobbyMembers((CSteamID)LobbyId);
		public int MaxPlayers => SteamMatchmaking.GetLobbyMemberLimit((CSteamID)LobbyId);
		
		public string LobbyName
		{
			get => GetData(nameof(LobbyName));
			set => SetData(nameof(LobbyName), value);
		}
		
		public string AppVersion
		{
			get => GetData(nameof(AppVersion));
			set => SetData(nameof(AppVersion), value);
		}

		public bool IsAdvertising
		{
			get => GetData(nameof(IsAdvertising)) == true.ToString();
			set => SetData(nameof(IsAdvertising), value.ToString());
		}

		public SteamLobbyInfo(ulong lobbyId)
		{
			LobbyId = lobbyId;
		}

		public override string ToString()
		{
			return JObject.FromObject(this).ToString();
		}

		public IEnumerable<SteamFriend> GetPlayers()
		{
			var id = (CSteamID)LobbyId;
			var count = SteamMatchmaking.GetNumLobbyMembers(id);
			for (int i = 0; i < count; i++)
			{
				var memberId = SteamMatchmaking.GetLobbyMemberByIndex(id, i);
				var username = SteamFriends.GetFriendPersonaName(memberId);
				var player = new SteamFriend(memberId.m_SteamID, username);
				yield return player;
			}
		}

		private string GetData(string key)
		{
			return SteamMatchmaking.GetLobbyData((CSteamID)LobbyId, key);
		}

		private void SetData(string key, string value)
		{
			if (!SteamMatchmaking.SetLobbyData((CSteamID)LobbyId, key, value))
				Debug.LogError($"Failed to set lobby data. {key}: {value}");
		}
	}
}