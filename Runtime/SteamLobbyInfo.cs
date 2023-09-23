﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Steamworks.Mainframe
{
	public class SteamLobbyInfo : IEquatable<SteamLobbyInfo>, IComparable<SteamLobbyInfo>
	{
		public ulong LobbyId { get; }
		public bool IsHost => HostId == Steam.SteamId;
		public int PlayerCount => SteamMatchmaking.GetNumLobbyMembers((CSteamID)LobbyId);
		public int MaxPlayers => SteamMatchmaking.GetLobbyMemberLimit((CSteamID)LobbyId);
		/// <summary>
		/// Returns lobby owner id, Unlike <see cref="HostId"/> you need to be in the lobby to get this value.
		/// </summary>
		public ulong OwnerId => SteamMatchmaking.GetLobbyOwner((CSteamID)LobbyId).m_SteamID;
		
		public ulong HostId
		{
			get => ulong.Parse(GetData(nameof(HostId)));
			set => SetData(nameof(HostId), value.ToString());
		}

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

		public string Country
		{
			get => GetData(nameof(Country));
			set => SetData(nameof(Country), value);
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

		public bool Equals(SteamLobbyInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return LobbyId == other.LobbyId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((SteamLobbyInfo)obj);
		}

		public override int GetHashCode()
		{
			return LobbyId.GetHashCode();
		}

		public int CompareTo(SteamLobbyInfo other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			return LobbyId.CompareTo(other.LobbyId);
		}

		public static bool operator ==(SteamLobbyInfo x, SteamLobbyInfo y)
		{
			if (x is null && y is null)
				return true;
			
			return x?.Equals(y) ?? false;
		}
		public static bool operator !=(SteamLobbyInfo x, SteamLobbyInfo y) 
		{
			return !(x == y);
		}
	}
}