using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using TranqGun;

namespace TranqGun.Commands {
    public class Givegun : ICommand {
        public string Command => "receivegun";

        public string[] Aliases => new[] { "givegun", "gg", "addgun" };

        public string Description => "Gives you a Tranquilizer Gun.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            if(!sender.CheckPermission("tgun.sleep")) {
                response = "<color=red>Permission denied.</color>";
                return false;
            }

            if(arguments.Count > 0) {
                string argument = arguments.At(0);
                if(argument.Equals("all", StringComparison.OrdinalIgnoreCase) || argument == "*") {
                    int playerAmount = 0;
                    foreach(Player p in Player.List) {
                        if(p.Role.Side != Side.None) {
                            p.AddItem(Extensions.GetTranquilizerItem());
                            playerAmount++;
                        }
                    }
                    response = $"<color=#4ce300>A total of {playerAmount} players have been given a TranqGun.</color>";
                } else {
                    Player p = Player.Get(argument);

                    if(p == null) {
                        response = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                        return false;
                    }

                    p.AddItem(Extensions.GetTranquilizerItem());
                    response = $"<color=#4ce300>{p.Nickname} has been given a TranqGun!</color>";
                    return false;
                }
            } else {
                Player.Get((CommandSender) sender).AddItem(Extensions.GetTranquilizerItem());
                response = $"<color=#4ce300>You've received a TranqGun!</color>";
            }
            return false;
        }
    }
}
