using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TranquilizerGun {
    public class EventsHandler {

        private Plugin plugin;
        public List<string> tranquilized, armored;
        public Dictionary<string, int> scpShots;
        bool allArmorEnabled = false;
        public TranqConfig Config;

        public string password = "getagirlfriend";

        public EventsHandler(Plugin plugin) {
            this.plugin = plugin;
            tranquilized = new List<string>();
            armored = new List<string>();
            Config = plugin.Config;
        }

        public void RoundEnd() {
            tranquilized.Clear();
            armored.Clear();
        }

        public void RoundStart() => Timing.RunCoroutine(DelayedReplace());

        public void ShootEvent(ShootingEventArgs ev) {
            if((ev.Shooter.CurrentItem.id == ItemType.GunCOM15 && Config.comIsTranquilizer) 
                || (ev.Shooter.CurrentItem.id == ItemType.GunUSP && Config.uspIsTranquilizer)) {

                if(ev.Shooter.CurrentItem.durability < Config.ammoUsedPerShot) {
                    if(Config.notEnoughAmmoBroadcastDuration > 0) {
                        if(Config.clearBroadcasts)
                            ev.Shooter.ClearBroadcasts();
                        ev.Shooter.Broadcast(Config.notEnoughAmmoBroadcastDuration, Config.notEnoughAmmoBroadcast);
                    }
                    ev.Shooter.RemoveWeaponAmmo(Config.ammoUsedPerShot);
                    ev.IsAllowed = false;
                    return;
                }
            }
        }

        public void OnPickupEvent(PickingUpItemEventArgs ev) {
            if(IsTranquilizer(ev.Pickup.ItemId) && Config.pickedUpBroadcastDuration > 0) {
                if(Config.clearBroadcasts)
                    ev.Player.ClearBroadcasts();
                ev.Player.Broadcast(Config.pickedUpBroadcastDuration, Config.pickedUpBroadcast);
            }
        }

        public void HurtEvent(HurtingEventArgs ev) {
            try {
                if(ev.Attacker == null || ev.Attacker == ev.Target || Config.roleBlacklist.Contains(ev.Target.Role))
                    return;
                else if(tranquilized.Contains(ev.Target.UserId)
                    && (ev.DamageType == DamageTypes.Decont || ev.DamageType == DamageTypes.Nuke || ev.DamageType == DamageTypes.Scp939) 
                    && Config.teleportAway || Config.SummonRagdoll) {
                    ev.Amount = 0;
                    return;
                } else if(IsTranquilizerDamage(ev.DamageType) && !tranquilized.Contains(ev.Target.UserId)) {
                    if(!IsTranquilizer(ev.Attacker.CurrentItem.id) && !ev.Attacker.ReferenceHub.HasSilencer()) return;
                    string id = ev.Target.UserId;
                    if(Config.specialRoles.Keys.Contains(ev.Target.Role)) {
                        if(!scpShots.ContainsKey(id))
                            scpShots.Add(id, 0);
                        scpShots[id] += 1;
                        if(scpShots[id] >= Config.specialRoles[ev.Target.Role]) {
                            Sleep(ev.Target);
                            scpShots[id] = 0;
                        }
                        return;
                    }

                    if(!Config.FriendlyFire && (ev.Target.Side == ev.Attacker.Side 
                        || (Config.areTutorialSerpentsHand && ev.Attacker.Side == Side.ChaosInsurgency && ev.Target.Role == RoleType.Tutorial)))
                        return;

                    Sleep(ev.Target);
                }
            } catch(Exception e) {
                e.Print("HurtEvent (TranqHandler)");
            }
        }

        #region Commands
        public void OnCommand(SendingRemoteAdminCommandEventArgs ev) {
            try {
                if(ev.Name.Contains("REQUEST_DATA PLAYER_LIST"))
                    return;

                string cmd = ev.Name.ToLower();
                // reload / protect / replaceguns / toggle / sleep / version / setgun / addgun / defaultconfig

                if(cmd.Equals("tg") || cmd.Equals("tgun") || cmd.Equals("tranqgun") || cmd.Equals("tranquilizergun")) {
                    ev.IsAllowed = false;
                    if(ev.Arguments.Count > 1) {
                        switch(ev.Arguments[0].ToLower()) {
                            case "protect":
                            case "protection":
                            case "armor":
                                if(!ev.CommandSender.CheckPermission("tgun.armor")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }

                                if(ev.Arguments.Count > 2) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountArmored = 0;
                                        foreach(Player p in Player.List) {
                                            if(allArmorEnabled && armored.Contains(p.UserId)) {
                                                armored.Remove(p.UserId);
                                                amountArmored++;
                                            } else if(!allArmorEnabled && !armored.Contains(p.UserId)) {
                                                armored.Add(p.UserId);
                                                amountArmored++;
                                            }
                                        }
                                        ev.ReplyMessage = allArmorEnabled ? $"<color=#4ce300>Tranquilizer protection has been disabled for {amountArmored} players.</color>" : $"<color=#4ce300>Tranquilizer protection has been enabled for {amountArmored} players.</color>";
                                        allArmorEnabled = !allArmorEnabled;
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        }

                                        ToggleArmor(p, out string newMessage);
                                        ev.ReplyMessage = newMessage;
                                        return;
                                    }
                                } else {
                                    ToggleArmor(ev.Sender, out string newMessage);
                                    ev.ReplyMessage = newMessage;
                                }
                                return;
                            case "replaceguns":
                                if(!ev.CommandSender.CheckPermission("tgun.replaceguns")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                int a = 0;

                                foreach(Pickup item in Object.FindObjectsOfType<Pickup>()) {
                                    if(item.ItemId == ItemType.GunCOM15 && UnityEngine.Random.Range(1, 100) <= Config.replaceChance) {
                                        ItemType.GunUSP.Spawn(18, item.Networkposition + new Vector3(0, 1, 0), default, 0, 1, 0);
                                        item.Delete();
                                    }
                                }
                                ev.ReplyMessage = $"<color=#4ce300>A total of {a} COM-15 pistols have been replaced.</color>";
                                return;
                            case "sleep":
                                if(!ev.CommandSender.CheckPermission("tgun.sleep")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(ev.Arguments.Count > 2) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountSleeping = 0;
                                        foreach(Player p in Player.List) {
                                            if(p.Side == Side.None && !tranquilized.Contains(p.UserId)) {
                                                Sleep(p);
                                                amountSleeping++;
                                            }
                                        }
                                        ev.ReplyMessage = $"<color=#4ce300>A total of {amountSleeping} players have been put to sleep.</color>";
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        } else if(tranquilized.Contains(p.UserId)) {
                                            ev.ReplyMessage = "<color=red>You're already sleeping...?</color>";
                                            return;
                                        }

                                        ev.ReplyMessage = $"<color=#4ce300>{p.Nickname} has been forced to sleep. Tell him sweet dreams!</color>";
                                        return;
                                    }
                                } else {
                                    Sleep(ev.Sender);
                                    ev.ReplyMessage = $"<color=#4ce300>You've been forced to sleep. Sweet dreams!</color>";
                                }
                                return;
                            case "version":
                                ev.ReplyMessage = "You're currently using " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                                return;
                            case "receivegun":
                            case "addgun":
                            case "givegun":
                                if(!ev.CommandSender.CheckPermission("tgun.givegun")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(ev.Arguments.Count > 2) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountGiven = 0;
                                        foreach(Player p in Player.List) {
                                            if(p.Side == Side.None) {
                                                ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                                amountGiven++;
                                            }
                                        }
                                        ev.ReplyMessage = $"<color=#4ce300>A total of {amountGiven} players received Tranquilizers.</color>";
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        }

                                        ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                        ev.ReplyMessage = $"<color=#4ce300>{p.Nickname} received a Tranquilizer.</color>";
                                        return;
                                    }
                                } else {
                                    ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                    ev.ReplyMessage = $"<color=#4ce300>Enjoy your Tranquilizer!</color>";
                                }
                                return;
                            case "toggle":
                                if(!ev.CommandSender.CheckPermission("tgun.toggle")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(Config.IsEnabledCustom) {
                                    plugin.UnregisterEvents();
                                    Config.IsEnabledCustom = false;
                                    ev.ReplyMessage = $"<color=#4ce300>The plugin has now been disabled!</color>";
                                } else {
                                    plugin.RegisterEvents();
                                    Config.IsEnabledCustom = true;
                                    ev.ReplyMessage = $"<color=#4ce300>The plugin has now been enabled!</color>";
                                }
                                return;
                        }

                        ev.ReplyMessage = 
                            $"\n<color=#4ce300>--- [ TranqGun Help ] ---</color>" +
                            $"\n<color=#006eff>Reload:</color> <color=#f7ff9c>Reloads the configuration variables. (Not the same as \"resetconfig\")</color>" +
                            $"\n<color=#006eff>Protection:</color> <color=#f7ff9c>Grants you special protection against Tranquilizers.</color>" +
                            $"\n<color=#006eff>ReplaceGuns:</color> <color=#f7ff9c>Replaces any COM-15s with Tranquilizers.</color>" +
                            $"\n<color=#006eff>Sleep:</color> <color=#f7ff9c>Forces the sleep method on someone.</color>" +
                            $"\n<color=#006eff>Setgun:</color> <color=#f7ff9c>The gun you're holding will now be the Tranquilizer.</color>" +
                            $"\n<color=#006eff>AddGun:</color> <color=#f7ff9c>Add a Tranquilizer to your inventory.</color>" +
                            $"\n<color=#006eff>ResetConfig:</color> <color=#f7ff9c>Resets the configuration variables to their default ones. (Not the same as \"reload\")</color>" +
                            $"\n<color=#006eff>Toggle:</color> <color=#f7ff9c>Toggles the plugin's features on/off.</color>" +
                            $"\n<color=#006eff>Version:</color> <color=#f7ff9c>Check the installed version of this plugin.</color>";
                    }
                }
            } catch(Exception e) {
                e.Print("OnCommand");
            }
        }
        #endregion

        public void Sleep(Player player) {
            try {
                // Initialize variables & add player to list
                Vector3 oldPos = player.Position;
                PlayerEffectsController controller = player.ReferenceHub.playerEffectsController;
                tranquilized.Add(player.Nickname);
                float sleepDuration = UnityEngine.Random.Range(Config.sleepDurationMin, Config.sleepDurationMax);

                // Broadcast message (if enabled)
                if(Config.tranquilizedBroadcastDuration > 0) {
                    if(Config.clearBroadcasts)
                        player.ClearBroadcasts();
                    player.Broadcast(Config.tranquilizedBroadcastDuration, Config.tranquilizedBroadcast);
                    
                }

                if(Config.dropItems)
                    player.Inventory.ServerDropAll();

                if(Config.usingEffects) {
                    EnableEffects(controller);
                }

                if(Config.SummonRagdoll) {
                    // Spawn a Ragdoll
                    PlayerStats.HitInfo hitInfo = new PlayerStats.HitInfo(1000f, player.UserId, DamageTypes.Usp, player.Id);

                    player.GameObject.GetComponent<RagdollManager>().SpawnRagdoll(
                        oldPos, player.GameObject.transform.rotation, player.ReferenceHub.playerMovementSync.PlayerVelocity,
                        (int) player.Role, hitInfo, false, player.Nickname, player.Nickname, 0);
                    ;

                    // Apply effects
                    controller.EnableEffect<Amnesia>(sleepDuration);
                    controller.EnableEffect<Scp268>(sleepDuration);
                }

                if(Config.teleportAway) {
                    player.Position = Config.newPos;
                }

                Timing.CallDelayed(sleepDuration, () => Wake(player, oldPos));

            } catch(Exception e) {
                e.Print($"Sleeping {player.Nickname} {e.StackTrace}");
            }
        }

        public void Wake(Player player, Vector3 oldPos) {
            try {
                tranquilized.Remove(player.UserId);

                if(!Config.usingEffects)
                foreach(Ragdoll doll in Object.FindObjectsOfType<Ragdoll>()) {
                    if(doll.owner.ownerHLAPI_id == player.Nickname) {
                        NetworkServer.Destroy(doll.gameObject);
                    }
                }

                if(Config.teleportAway) {
                    player.Position = oldPos;

                    if(Warhead.IsDetonated) {
                        if(player.CurrentRoom.Zone != ZoneType.Entrance)
                            player.Kill();
                        else
                            foreach(Lift l in Map.Lifts)
                                if(l.elevatorName.ToLower() == "gatea" || l.elevatorName.ToLower() == "gateb")
                                    foreach(Lift.Elevator e in l.elevators)
                                        if(e.target.name == "ElevatorChamber (1)")
                                            if(Vector3.Distance(player.Position, e.target.position) <= 3.6f)
                                                player.Kill();
                    }
                }
            } catch(Exception e) {
                e.Print("Sleeping " + player.Nickname);
            }
        }

        public void EnableEffects(PlayerEffectsController controller) {
            if(Config.amnesia) {
                controller.EnableEffect<Amnesia>(Config.amnesiaDuration);
            }

            if(Config.asphyxiated) {
                controller.EnableEffect<Asphyxiated>(Config.asphyxiatedDuration);
            }

            if(Config.blinded) {
                controller.EnableEffect<Blinded>(Config.blindedDuration);
            }

            if(Config.concussed) {
                controller.EnableEffect<Concussed>(Config.concussedDuration);
            }

            if(Config.deafened) {
                controller.EnableEffect<Deafened>(Config.deafenedDuration);
            }

            if(Config.disabled) {
                controller.EnableEffect<Disabled>(Config.disabledDuration);
            }

            if(Config.ensnared) {
                controller.EnableEffect<Ensnared>(Config.ensnaredDuration);
            }

            if(Config.exhausted) {
                controller.EnableEffect<Exhausted>(Config.exhaustedDuration);
            }

            if(Config.flash) {
                controller.EnableEffect<Flashed>(Config.flashDuration);
            }

            if(Config.poisoned) {
                controller.EnableEffect<Poisoned>(Config.poisonedDuration);
            }
        }

        public bool IsTranquilizerDamage(DamageTypes.DamageType damageType) 
            => (Config.comIsTranquilizer && damageType == DamageTypes.Com15) || (Config.uspIsTranquilizer && damageType == DamageTypes.Usp);

        public IEnumerator<float> DelayedReplace() {
            yield return Timing.WaitForSeconds(2f);
            foreach(Pickup item in Object.FindObjectsOfType<Pickup>()) {
                if(item.ItemId == ItemType.GunCOM15 && UnityEngine.Random.Range(1, 100) <= Config.replaceChance) {
                    ItemType.GunUSP.Spawn(18, item.Networkposition + new Vector3(0, 1, 0), default, 0, 1, 0);
                    item.Delete();
                }
            }
        }

        public bool IsTranquilizer(ItemType type) =>
            (type == ItemType.GunCOM15 && Config.comIsTranquilizer)
                || (type == ItemType.GunUSP && Config.uspIsTranquilizer);

        // I'm fucking lazy 
        private void ToggleArmor(Player p, out string ReplyMessage) {
            if(armored.Contains(p.UserId)) {
                armored.Remove(p.UserId);
                ReplyMessage = $"<color=red>{p.Nickname} is no longer protected against Tranquilizers.</color>";
            } else {
                armored.Add(p.UserId);
                ReplyMessage = $"<color=#4ce300>{p.Nickname} is now protected against Tranquilizers.</color>";
            }
        }
    }
}
