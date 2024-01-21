using Pings.Netcode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Pings
{
	//TODO light level check threshold (everything, configurable)
	//TODO tile pings dont appear in MP for other players if too far away
	//TODO fix whoami regression/ping going invisible when different player joins
	public class PingsMod : Mod
	{
		public static ModKeybind PingKeybind { internal set; get; }

		public static PingsMod Mod { internal set; get; }

		public static HashSet<ushort> Ores { internal set; get; }

		public static HashSet<ushort> GemOres { internal set; get; }

		public static bool IsCluster(ushort type) => type <= TileLoader.TileCount && (TileID.Sets.Ore[type] || Ores.Contains(type) || GemOres.Contains(type));

		public override void Load()
		{
			Mod = this;
			NetHandler.Load();
			Ping.Load();
			PingKeybind = KeybindLoader.RegisterKeybind(this, "PingObjectAtCursor", "Mouse3");
		}

		public override void PostSetupContent()
		{
			Ores = new HashSet<ushort>
			{
				TileID.DesertFossil,
				TileID.Silt,
				TileID.Slush
			};

			GemOres = new HashSet<ushort>
			{
				TileID.Sapphire,
				TileID.Ruby,
				TileID.Emerald,
				TileID.Topaz,
				TileID.Amethyst,
				TileID.Diamond,
			};
		}

		public override void Unload()
		{
			Mod = null;
			NetHandler.Unload();
			Ping.Unload();
			Ores = null;
			GemOres = null;
			PingKeybind = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetHandler.HandlePackets(reader, whoAmI);
		}

		/// <summary>
		/// Logs a message in the respective log (client or server). Optionally, displays it in the respective output (chat or console)
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="chat">If true, displays the message in the respective output</param>
		public static void Log(object message, bool chat = false, bool localTime = false)
		{
			string msg = message.ToString();

			if (localTime)
			{
				string time = (DateTime.Now.Ticks).ToString();
				msg = $"{time,16} {msg}";
			}

			Mod.Logger.Info(msg);

			if (!chat) return;

			if (Main.netMode == NetmodeID.Server)
			{
				Console.WriteLine(msg);
			}
			else
			{
				Main.NewText(msg);
			}
		}

		private static readonly Regex sWhitespaceRegex = new Regex(@"\s+");
		private static readonly Regex sSplitStringRegex = new Regex("(?<!^)(?=[A-Z](?![A-Z]|$))");
		public static string SplitCapitalString(string text)
		{
			text = sWhitespaceRegex.Replace(text, "");
			return SplitCamelCase(text);
		}

		public static string SplitCamelCase(string input, string delimiter = " ")
		{
			return input.Any(char.IsUpper) ? string.Join(delimiter, sSplitStringRegex.Split(input)) : input;
		}
	}
}
