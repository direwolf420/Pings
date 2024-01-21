﻿using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Pings
{
	public class ServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();

		public const int PingsPerPlayerMin = 1;
		public const int PingsPerPlayerMax = 10;

		[Slider]
		[SliderColor(50, 50, 255)]
		[Range(PingsPerPlayerMin, PingsPerPlayerMax)]
		[DefaultValue(3)]
		public int PingsPerPlayer;

		public const int PingDurationMin = 10; //Seconds!
		public const int PingDurationMax = 30 * 60; //Half an hour = Infinite in code

		[Slider]
		[SliderColor(50, 50, 255)]
		[Range(PingDurationMin, PingDurationMax)]
		[DefaultValue(5 * 60)] //5 minutes
		public int PingDuration;

		public const int PingCooldownMin = 1; //Seconds!
		public const int PingCooldownMax = 60; //A minute

		[Slider]
		[SliderColor(50, 50, 255)]
		[Range(PingCooldownMin, PingCooldownMax)]
		[DefaultValue(1)] //1 second
		public int PingCooldown;

		public static bool IsPlayerLocalServerOwner(int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				RemoteClient client = Netplay.Clients[i];
				if (client.State == 10 && i == whoAmI && client.Socket.GetRemoteAddress().IsLocalHost())
				{
					return true;
				}
			}
			return false;
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost");
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			PingsPerPlayer = Utils.Clamp(PingsPerPlayer, PingsPerPlayerMin, PingsPerPlayerMax);

			PingDuration = Utils.Clamp(PingDuration, PingDurationMin, PingDurationMax);

			PingCooldown = Utils.Clamp(PingCooldown, PingCooldownMin, PingCooldownMax);
		}
	}
}
