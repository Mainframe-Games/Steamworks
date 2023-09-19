using System;
using UnityEngine;

namespace Steamworks.Mainframe
{
	public readonly struct SteamFriend : IEquatable<SteamFriend>, IComparable<SteamFriend>
	{
		public readonly ulong SteamId;
		public readonly string Username;
		
		public bool IsMe => SteamId == Steam.SteamId;

		public SteamFriend(ulong steamId, string username)
		{
			SteamId = steamId;
			Username = username;
		}

		public override string ToString()
		{
			return $"Username: {Username}, SteamId: {SteamId}";
		}

		public bool Equals(SteamFriend other)
		{
			return SteamId == other.SteamId;
		}

		public override bool Equals(object obj)
		{
			return obj is SteamFriend other && Equals(other);
		}

		public override int GetHashCode()
		{
			return SteamId.GetHashCode();
		}

		public int CompareTo(SteamFriend other)
		{
			return SteamId.CompareTo(other.SteamId);
		}

		public Sprite GetAvatarSprite()
		{
			var sprite = SteamAvatar.GetAvatar(SteamId);
			if (!sprite)
				Debug.Log($"Failed to get avatar for: {ToString()}");
			return sprite;
		}
	}
}