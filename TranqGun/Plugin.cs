using System;
using Exiled.API.Features;
using MEC;
using TranquilizerGun;
using Server = Exiled.Events.Handlers.Server;
using Player = Exiled.Events.Handlers.Player;

namespace TranqGun {
    public class Plugin : Plugin<TranqConfig> {

        public override string Prefix => "tranq_gun";
        public override string Name => "TranquilizerGun";
        public override string Author => "Beryl";
        public override Version RequiredExiledVersion => new Version(4, 1, 3);
        public override Version Version { get; } = new Version(2, 5, 1);
        public static Plugin Instance { get; private set;  }

        public EventsHandler Handler;

        public override void OnEnabled() {
            Instance = this;
            Handler = new EventsHandler(this);

            if(Config.IsEnabledCustom)
                RegisterEvents();

            Timing.CallDelayed(1f, () => {
                try {
                    Config.roleBlacklist = Config.BlacklistedRoles();
                    Config.specialRoles = Config.SpecialRoles();
                } catch(Exception e) {
                    Log.Error("Exception caused while loading Blacklisted/Special roles: " + e.Message + " - " + e.StackTrace);
                }
            });

            Log.Info($"{Name} has been enabled!");
            base.OnEnabled();
        }

        public override void OnDisabled() {

            if(Config.IsEnabledCustom)
                UnregisterEvents();

            Handler = null;
            Log.Info($"{Name} has been disabled!");
            base.OnDisabled();
        }

        public override void OnReloaded() => Log.Info($"{Name} has been reloaded!");

        public void RegisterEvents() {
            Player.ChangingItem += Handler.OnChangingItem;
            Player.Shooting += Handler.ShootEvent;
            Player.Hurting += Handler.HurtEvent;
            Server.RoundEnded += Handler.RoundEnd;
        }

        public void UnregisterEvents() {
            Player.ChangingItem -= Handler.OnChangingItem;
            Player.Shooting -= Handler.ShootEvent;
            Player.Hurting -= Handler.HurtEvent;
            Server.RoundEnded -= Handler.RoundEnd;
        }

    }
}
