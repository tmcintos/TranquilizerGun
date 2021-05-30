using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;

namespace TranquilizerGun.Commands {
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Main : ParentCommand {
        public override string Command => "tranquilizergun";

        public override string[] Aliases => new[] { "tgun", "tranqgun" };

        public override string Description => "Tranquilizer Gun's main command.";

        public Main() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands() {
            RegisterCommand(new Protection());
            RegisterCommand(new Givegun());
            RegisterCommand(new Sleep());
            RegisterCommand(new Toggle());
            RegisterCommand(new Version());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            response =
                $"\n<color=#4ce300>--- [ TranqGun Help ] ---</color>" +
                $"\n<color=#006eff>Protection:</color> <color=#f7ff9c>Grants you special protection against Tranquilizers.</color>" +
                $"\n<color=#006eff>Sleep:</color> <color=#f7ff9c>Forces the sleep method on someone.</color>" +
                $"\n<color=#006eff>AddGun:</color> <color=#f7ff9c>Add a Tranquilizer to your inventory.</color>" +
                $"\n<color=#006eff>Toggle:</color> <color=#f7ff9c>Toggles the plugin's features on/off.</color>" +
                $"\n<color=#006eff>Version:</color> <color=#f7ff9c>Check the installed version of this plugin.</color>";
            return false;
        }
    }
}
