using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace TranqGun.Commands {
    public class Sleep : ICommand {
        public string Command => "forcesleep";

        public string[] Aliases => new[] { "sleep", "fs" };

        public string Description => "Forces the sleep method on someone.";

        private EventsHandler Handler => Plugin.Instance.Handler;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            if(!sender.CheckPermission("tgun.sleep")) {
                response = "<color=red>Permission denied.</color>";
                return false;
            }

            if(arguments.Count > 0) {
                string argument = arguments.At(0);
                if(argument.Equals("all", StringComparison.OrdinalIgnoreCase) || argument == "*") {
                    int amountSleeping = 0;
                    foreach(Player p in Player.List) {
                        if(p.Side != Side.None && !Handler.Tranquilized.Contains(p.UserId)) {
                            Handler.Sleep(p);
                            amountSleeping++;
                        }
                    }
                    response = $"<color=#4ce300>A total of {amountSleeping} players have been put to sleep.</color>";
                } else {
                    Player p = Player.Get(argument);

                    if(p == null) {
                        response = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                        return false;
                    }

                    Handler.Sleep(p);
                    response = $"<color=#4ce300>{p.Nickname} has been forced to sleep. Tell him sweet dreams!</color>";
                    return false;
                }
            } else {
                Handler.Sleep(Player.Get((CommandSender) sender));
                response = $"<color=#4ce300>You've been forced to sleep. Sweet dreams!</color>";
            }
            return false;
        }
    }
}
