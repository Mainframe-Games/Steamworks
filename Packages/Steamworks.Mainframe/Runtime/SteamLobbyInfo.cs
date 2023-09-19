using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Steamworks.Mainframe.Core
{
	public class SteamLobbyInfo
	{
		public ulong LobbyId { get; }

		public ulong HostId
		{
			get => ParseUlong(nameof(HostId));
			set => SetData(nameof(HostId), value.ToString());
		}

		public string LobbyName
		{
			get => GetData(nameof(LobbyName));
			set => SetData(nameof(LobbyName), value);
		}

		public int PlayerCount
		{
			get => ParseInt(nameof(PlayerCount));
			set => SetData(nameof(PlayerCount), value.ToString());
		}

		public string AppVersion
		{
			get => GetData(nameof(AppVersion));
			set => SetData(nameof(AppVersion), value);
		}

		public bool IsActive
		{
			get => GetData(nameof(IsActive)) == true.ToString();
			set => SetData(nameof(IsActive), value.ToString());
		}

		public int MaxConnections 
		{
			get => ParseInt(nameof(MaxConnections));
			set => SetData(nameof(MaxConnections), value.ToString());
		}

		public SteamLobbyInfo(ulong lobbyId)
		{
			LobbyId = lobbyId;
		}

		private int ParseInt(string key)
		{
			var rawVal = GetData(key);

			if (int.TryParse(rawVal, out var id))
				return id;

			Debug.LogError($"Could not parse '{rawVal}' to {nameof(UInt32)}. Key: {key}");
			return 0;
		}
		
		private ulong ParseUlong(string key)
		{
			var rawVal = GetData(key);

			if (ulong.TryParse(rawVal, out var id))
				return id;

			Debug.LogError($"Could not parse '{rawVal}' to {nameof(UInt64)}. Key: {key}");
			return 0;
		}

		public override string ToString()
		{
			return JObject.FromObject(this).ToString();
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