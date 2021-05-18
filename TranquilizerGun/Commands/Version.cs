using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace TranquilizerGun.Commands {
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
