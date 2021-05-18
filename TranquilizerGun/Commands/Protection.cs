using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace TranquilizerGun.Commands {
    public class Protection : ICommand {
        public string Command => "protect";

        public string[] Aliases => new[] { "prt", "protection", "armor" };

        public string Description => "Grants you special protection against Tranquilizers.";

        private EventsHandler Handler => Plugin.Instance.handler;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            if(!sender.CheckPermission("tgun.armor")) {
                response = "<color=red>Permission denied.</color>";
                return false;
            }

            if(arguments.Count > 0) {
                string argument = arguments.At(0);
                if(argument.Equals("all", StringComparison.OrdinalIgnoreCase) || argument == "*") {
                    int amountArmored = 0;
                    foreach(Player p in Player.List) {
                        if(Handler.allArmorEnabled && Handler.armored.Contains(p.UserId)) {
                            Handler.armored.Remove(p.UserId);
                            amountArmored++;
                        } else if(!Handler.allArmorEnabled && !Handler.armored.Contains(p.UserId)) {
                            Handler.armored.Add(p.UserId);
                            amountArmored++;
                        }
                    }
                    response = Handler.allArmorEnabled ? $"<color=#4ce300>Tranquilizer protection has been disabled for {amountArmored} players.</color>" : $"<color=#4ce300>Tranquilizer protection has been enabled for {amountArmored} players.</color>";
                    Handler.allArmorEnabled = !Handler.allArmorEnabled;
                } else {
                    Player p = Player.Get(argument);

                    if(p == null) {
                        response = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                        return false;
                    }

                    Handler.ToggleArmor(p, out string newMessage);
                    response = newMessage;
                    return false;
                }
            } else {
                Handler.ToggleArmor(Player.Get((CommandSender) sender), out string newMessage);
                response = newMessage;
            }
            return false;
        }
    }
}
