﻿using System;
using OTA;
using OTA.Command;
using Microsoft.Xna.Framework;

namespace TDSM.Core.Command.Commands
{
    public class WhiteListCommand : CoreCommand
    {
        public const String Prefix_WhitelistName = "NAME:";
        public const String Prefix_WhitelistIp = "IP:";

        public override void Initialise()
        {
            Core.AddCommand("whitelist")
                .WithAccessLevel(AccessLevel.OP)
                .WithPermissionNode("tdsm.whitelist")
                .WithDescription("Manages the whitelist")
                .WithHelpText("enable|disable|reload|?")
                .WithHelpText("addplayer|removeplayer <name>")
                .WithHelpText("addip|removeip <ip>")
                .Calls(this.WhitelistMan);
        }

        void WhitelistMan(ISender sender, ArgumentList args)
        {
            var index = 0;
            var cmd = args.GetString(index++);
            string name, ip;

            switch (cmd)
            {
                case "status":
                case "current":
                case "?":
                    sender.Message("The whitelist is currently " + (Core.Config.WhitelistEnabled ? "enabled" : "disabled"));
                    break;
                case "reload":
                    Core.Whitelist.Load();
                    Tools.NotifyAllOps("The whitelist was reloaded");
                    if (!sender.Op) sender.Message("The whitelist was reloaded", Color.Green);
                    break;
                case "enable":
                    if (!Core.Config.WhitelistEnabled)
                    {
                        Core.Config.WhitelistEnabled = true;

                        if (!ConfigUpdater.IsAvailable || ConfigUpdater.Set("whitelist", Core.Config.WhitelistEnabled))
                        {
                            Tools.NotifyAllOps("The whitelist was enabled");
                            if (!sender.Op) sender.Message("The whitelist was enabled", Color.Green);
                        }
                        else sender.Message("Failed to save to config, whitelist is only enabled this session.", Color.Red);
                    }
                    else sender.Message("The whitelist is already enabled", Color.Red);
                    break;
                case "disable":
                    if (Core.Config.WhitelistEnabled)
                    {
                        Core.Config.WhitelistEnabled = false;

                        if (!ConfigUpdater.IsAvailable || ConfigUpdater.Set("whitelist", Core.Config.WhitelistEnabled))
                        {
                            Tools.NotifyAllOps("The whitelist was disabled");
                            if (!sender.Op) sender.Message("The whitelist was disabled", Color.Green);
                        }
                        else sender.Message("Failed to save to config, whitelist is only disabled this session.", Color.Red);
                    }
                    else sender.Message("The whitelist is already disabled", Color.Red);
                    break;

                case "addplayer":
                    if (!args.TryGetString(index++, out name))
                    {
                        throw new CommandError("Expected player name after [addplayer]");
                    }

                    var addName = Prefix_WhitelistName + name;
                    if (Core.Whitelist.Add(addName))
                    {
                        Tools.NotifyAllOps(String.Format("Player {0} was added to the whitelist", name));
                        if (!sender.Op) sender.Message(String.Format("Player {0} was added to the whitelist", name), Color.Green);

                        if (!Core.Config.WhitelistEnabled) sender.Message("Note, the whitelist is not enabled", Color.Orange);
                    }
                    else sender.Message("Failed to add " + name + " to the whitelist", Color.Red);
                    break;
                case "removeplayer":
                    if (!args.TryGetString(index++, out name))
                    {
                        throw new CommandError("Expected player name after [removeplayer]");
                    }

                    var removeName = Prefix_WhitelistName + name;
                    if (Core.Whitelist.Remove(removeName))
                    {
                        Tools.NotifyAllOps(String.Format("Player {0} was removed from the whitelist", name));
                        if (!sender.Op) sender.Message(String.Format("Player {0} was removed from the whitelist", name), Color.Green);

                        if (!Core.Config.WhitelistEnabled) sender.Message("Note, the whitelist is not enabled", Color.Orange);
                    }
                    else sender.Message("Failed to remove " + name + " from the whitelist", Color.Red);
                    break;

                case "addip":
                    if (!args.TryGetString(index++, out ip))
                    {
                        throw new CommandError("Expected IP after [addip]");
                    }

                    var addIP = Prefix_WhitelistIp + ip;
                    if (Core.Whitelist.Add(addIP))
                    {
                        Tools.NotifyAllOps(String.Format("IP {0} was added to the whitelist", ip));
                        if (!sender.Op) sender.Message(String.Format("IP {0} was added to the whitelist", ip), Color.Green);

                        if (!Core.Config.WhitelistEnabled) sender.Message("Note, the whitelist is not enabled", Color.Orange);
                    }
                    else sender.Message("Failed to add " + ip + " to the whitelist", Color.Red);
                    break;
                case "removeip":
                    if (!args.TryGetString(index++, out ip))
                    {
                        throw new CommandError("Expected IP after [removeip]");
                    }

                    var removeIP = Prefix_WhitelistIp + ip;
                    if (Core.Whitelist.Remove(removeIP))
                    {
                        Tools.NotifyAllOps(String.Format("IP {0} was removed from the whitelist", ip));
                        if (!sender.Op) sender.Message(String.Format("IP {0} was removed from the whitelist", ip), Color.Green);

                        if (!Core.Config.WhitelistEnabled) sender.Message("Note, the whitelist is not enabled", Color.Orange);
                    }
                    else sender.Message("Failed to remove " + ip + " from the whitelist", Color.Red);
                    break;

                default:
                    throw new CommandError("Unknown whitelist command: " + cmd);
            }
        }
    }
}

