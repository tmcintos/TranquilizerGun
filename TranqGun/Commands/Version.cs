using System;
using CommandSystem;

namespace TranqGun.Commands {
    public class Version : ICommand {
        public string Command => "version";

        public string[] Aliases => new[] { "v" };

        public string Description => "Gives you a Tranquilier Gun.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            response = $"You're currently using {Plugin.Instance.Assembly.GetName().Name} ({Plugin.Instance.Version})";
            return true;
        }
    }
}
