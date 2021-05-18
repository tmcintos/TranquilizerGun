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
    public class Toggle : ICommand {
        public string Command => "toggle";

        public string[] Aliases => new[] { "t" };

        public string Description => "Toggles the plugin's main functionalities.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {

            if(Plugin.Instance.Config.IsEnabledCustom) {
                Plugin.Instance.UnregisterEvents();
                Plugin.Instance.Config.IsEnabledCustom = false;
                response = $"<color=#4ce300>The plugin has now been disabled!</color>";
            } else {
                Plugin.Instance.RegisterEvents();
                Plugin.Instance.Config.IsEnabledCustom = true;
                response = $"<color=#4ce300>The plugin has now been enabled!</color>";
            }

            return true;
        }
    }
}
