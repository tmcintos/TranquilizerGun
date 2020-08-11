using System.Collections.Generic;
using Exiled.API.Features;
using System;
using Exiled.API.Interfaces;
using Exiled.Events.Handlers;
using Exiled.Loader;
using UnityEngine;
using Server = Exiled.Events.Handlers.Server;
using Player = Exiled.Events.Handlers.Player;
using MEC;

namespace TranquilizerGun {
    public class Plugin : Plugin<TranqConfig> {

        public override string Prefix => "tranquilizergun";
        public override string Name => "TranquilizerGun";
        public override string Author => "Beryl";
        public override Version Version { get; } = new Version(2, 2, 2);

        public EventsHandler handler;

        public override void OnEnabled() {
            handler = new EventsHandler(this);

            if(Config.IsEnabledCustom)
                RegisterEvents();
            Server.SendingRemoteAdminCommand += handler.OnCommand;

            Timing.CallDelayed(1f, () => {
                try {
                    Config.roleBlacklist = Config.BlacklistedRoles();
                    Config.specialRoles = Config.SpecialRoles();
                } catch(Exception e) {
                    Log.Error("Exception caused while loading Blacklisted/Special roles: " + e.Message + " - " + e.StackTrace);
                }
            });

            Log.Info($"{Name} has been enabled!");
        }

        public override void OnDisabled() {

            Server.SendingRemoteAdminCommand -= handler.OnCommand;
            if(Config.IsEnabledCustom)
                UnregisterEvents();

            handler = null;
            Log.Info($"{Name} has been disabled!");
        }

        public override void OnReloaded() => Log.Info($"{Name} has been reloaded!");

        public void RegisterEvents() {
            Player.PickingUpItem += handler.OnPickupEvent;
            Player.Shooting += handler.ShootEvent;
            Player.Hurting += handler.HurtEvent;
            Server.RoundStarted += handler.RoundStart;
            Server.RoundEnded += handler.RoundEnd;
        }

        public void UnregisterEvents() {
            Player.PickingUpItem -= handler.OnPickupEvent;
            Player.Shooting -= handler.ShootEvent;
            Player.Hurting -= handler.HurtEvent;
            Server.RoundStarted -= handler.RoundStart;
            Server.RoundEnded -= handler.RoundEnd;
        }

    }
}
