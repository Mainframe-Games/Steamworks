using System;

namespace Steamworks.Mainframe
{
	[Serializable]
	public struct SteamFriend : IEquatable<SteamFriend>, IComparable<SteamFriend>
	{
		public ulong SteamId;
		public string Username;
		
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
	}
}