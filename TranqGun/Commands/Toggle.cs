using System;
using CommandSystem;

namespace TranqGun.Commands {
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
