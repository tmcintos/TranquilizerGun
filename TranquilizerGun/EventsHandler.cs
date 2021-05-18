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
        public bool allArmorEnabled = false;

        public EventsHandler(Plugin plugin) {
            this.plugin = plugin;
            tranquilized = new List<string>();
            armored = new List<string>();
            scpShots = new Dictionary<string, int>();
        }

        public void RoundEnd(RoundEndedEventArgs _) {
            tranquilized.Clear();
            armored.Clear();
            scpShots.Clear();
        } 

        public void ShootEvent(ShootingEventArgs ev) {
            try {
                if((ev.Shooter.CurrentItem.id == ItemType.GunCOM15 && plugin.Config.comIsTranquilizer)
                    || (ev.Shooter.CurrentItem.id == ItemType.GunUSP && plugin.Config.uspIsTranquilizer)) {

                    if(plugin.Config.silencerRequired && !ev.Shooter.HasSilencer())
                        return;

                    if(ev.Shooter.CurrentItem.durability < plugin.Config.ammoUsedPerShot - 1) {
                        if(plugin.Config.notEnoughAmmoBroadcastDuration > 0) {
                            if(plugin.Config.UseHintsSystem)
                                ev.Shooter.ShowHint(plugin.Config.notEnoughAmmoBroadcastDuration, plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                            else {
                                if(plugin.Config.clearBroadcasts)
                                    ev.Shooter.ClearBroadcasts();
                                ev.Shooter.Broadcast(plugin.Config.notEnoughAmmoBroadcastDuration, plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                            }
                        }
                        ev.IsAllowed = false;
                        return;
                    }
                    ev.Shooter.RemoveWeaponAmmo(plugin.Config.ammoUsedPerShot - 1);
                }
            } catch(Exception e) {
                e.Print("ShootEvent");
            }
        }

        public void OnPickupEvent(PickingUpItemEventArgs ev) {
            if(IsTranquilizer(ev.Pickup.ItemId) && plugin.Config.pickedUpBroadcastDuration > 0) {
                if(!ev.Pickup.ItemId.IsPistol() || (plugin.Config.silencerRequired && ev.Pickup.weaponMods.Barrel != 1))
                    return;
                if(plugin.Config.UseHintsSystem)
                    ev.Player.ShowHint(plugin.Config.pickedUpBroadcastDuration, plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                else {
                    if(plugin.Config.clearBroadcasts)
                        ev.Player.ClearBroadcasts();
                    ev.Player.Broadcast(plugin.Config.pickedUpBroadcastDuration, plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                }
            }
        }

        public void HurtEvent(HurtingEventArgs ev) {
            try {
                if(ev.Attacker == null || ev.Attacker == ev.Target || plugin.Config.roleBlacklist.Contains(ev.Target.Role))
                    return;
                else if(tranquilized.Contains(ev.Target.UserId)
                    && (ev.DamageType == DamageTypes.Decont || ev.DamageType == DamageTypes.Nuke || ev.DamageType == DamageTypes.Scp939) 
                    && (plugin.Config.teleportAway || plugin.Config.SummonRagdoll)) {
                    ev.Amount = 0;
                    return;
                } else if(IsTranquilizerDamage(ev.DamageType) && !tranquilized.Contains(ev.Target.UserId)) {
                    if(plugin.Config.silencerRequired && !ev.Attacker.HasSilencer()) return;

                    ev.Amount = plugin.Config.tranquilizerDamage;

                    if(!plugin.Config.FriendlyFire && (ev.Target.Side == ev.Attacker.Side
                        || (plugin.Config.areTutorialSerpentsHand && ev.Attacker.Side == Side.ChaosInsurgency && ev.Target.Role == RoleType.Tutorial)))
                        return;

                    string id = ev.Target.UserId;
                    if(plugin.Config.specialRoles.Keys.Contains(ev.Target.Role)) {
                        if(!scpShots.ContainsKey(id))
                            scpShots.Add(id, 0);
                        scpShots[id] += 1;
                        if(scpShots[id] >= plugin.Config.specialRoles[ev.Target.Role]) {
                            Sleep(ev.Target);
                            scpShots[id] = 0;
                        }
                        return;
                    }

                    Sleep(ev.Target);
                }
            } catch(Exception e) {
                e.Print("HurtEvent (TranqHandler)");
            }
        }

        public void Sleep(Player player) {
            try {
                // Initialize variables & add player to list
                Vector3 oldPos = player.Position;
                PlayerEffectsController controller = player.ReferenceHub.playerEffectsController;
                tranquilized.Add(player.Nickname);
                float sleepDuration = UnityEngine.Random.Range(plugin.Config.sleepDurationMin, plugin.Config.sleepDurationMax), bdd = controller.GetEffect<Bleeding>().Duration;
                bool pd = controller.GetEffect<Corroding>().Enabled, bd = controller.GetEffect<Bleeding>().Enabled;

                // Broadcast message (if enabled)
                if(plugin.Config.tranquilizedBroadcastDuration > 0) {
                    if(plugin.Config.UseHintsSystem)
                        player.ShowHint(plugin.Config.tranquilizedBroadcastDuration, plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    else {
                        if(plugin.Config.clearBroadcasts)
                            player.ClearBroadcasts();
                        player.Broadcast(plugin.Config.tranquilizedBroadcastDuration, plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    }
                }

                if(plugin.Config.dropItems)
                    player.Inventory.ServerDropAll();

                if(plugin.Config.usingEffects) {
                    EnableEffects(controller);

                    if(plugin.Config.invisible) {
                        Invisible(player, true);
                        Timing.CallDelayed(plugin.Config.invisibleDuration, () => Invisible(player, false));
                    }
                }

                if(plugin.Config.SummonRagdoll) {
                    // Spawn a Ragdoll
                    PlayerStats.HitInfo hitInfo = new PlayerStats.HitInfo(1000f, player.UserId, DamageTypes.Usp, player.Id);

                    player.GameObject.GetComponent<RagdollManager>().SpawnRagdoll(
                        oldPos, player.GameObject.transform.rotation, player.ReferenceHub.playerMovementSync.PlayerVelocity,
                        (int) player.Role, hitInfo, false, player.Nickname, player.Nickname, 0);
                    
                }

                if(plugin.Config.teleportAway) {
                    // Apply effects
                    controller.EnableEffect<Amnesia>(sleepDuration);
                    controller.EnableEffect<Scp268>(sleepDuration);

                    player.Position = new Vector3(plugin.Config.newPos_x, plugin.Config.newPos_y, plugin.Config.newPos_z);
                    Timing.CallDelayed(1f, () => player.ReferenceHub.playerEffectsController.DisableEffect<Decontaminating>());
                }

                Timing.CallDelayed(sleepDuration, () => Wake(player, oldPos, pd, bd, bdd));

            } catch(Exception e) {
                e.Print($"Sleeping {player.Nickname} {e.StackTrace}");
            }
        }

        public void Wake(Player player, Vector3 oldPos, bool inPd = false, bool bleeding = false, float bleedingDur = 3) {
            try {
                tranquilized.Remove(player.UserId);

                if(plugin.Config.SummonRagdoll)
                foreach(Ragdoll doll in Object.FindObjectsOfType<Ragdoll>()) {
                    if(doll.owner.ownerHLAPI_id == player.Nickname) {
                        NetworkServer.Destroy(doll.gameObject);
                    }
                }

                if(plugin.Config.teleportAway) {
                    player.Position = oldPos;

                    Timing.CallDelayed(1.5f, () => {
                        // Attempt number 27 of fixing this issue
                        if(player.TryGetEffect(EffectType.Decontaminating, out var effect)) {
                            (effect as Decontaminating).TimeLeft = 99f;
                        }
                        player.ReferenceHub.playerEffectsController.DisableEffect<Decontaminating>();

                        if(inPd)
                            player.ReferenceHub.playerEffectsController.EnableEffect<Corroding>();

                        if(bleeding)
                            player.ReferenceHub.playerEffectsController.EnableEffect<Bleeding>(bleedingDur);

                        if(Warhead.IsDetonated) {
                            if(player.CurrentRoom.Zone != ZoneType.Surface)
                                player.Kill();
                            else {
                                foreach(Lift l in Map.Lifts) {
                                    if(l.Type() == ElevatorType.GateA || l.Type() == ElevatorType.GateB) {
                                        foreach(Lift.Elevator e in l.elevators) {
                                            if(Vector3.Distance(player.Position, e.target.position) <= 3.6f) {
                                                player.Kill();
                                                return;
                                            }
                                        }
                                    }
                                }
                            }  
                        }
                    });
                }
            } catch(Exception e) {
                e.Print("Sleeping " + player.Nickname);
            }
        }

        public void EnableEffects(PlayerEffectsController controller) {
            if(plugin.Config.amnesia) {
                controller.EnableEffect<Amnesia>(plugin.Config.amnesiaDuration);
            }

            if(plugin.Config.asphyxiated) {
                controller.EnableEffect<Asphyxiated>(plugin.Config.asphyxiatedDuration);
            }

            if(plugin.Config.blinded) {
                controller.EnableEffect<Blinded>(plugin.Config.blindedDuration);
            }

            if(plugin.Config.concussed) {
                controller.EnableEffect<Concussed>(plugin.Config.concussedDuration);
            }

            if(plugin.Config.deafened) {
                controller.EnableEffect<Deafened>(plugin.Config.deafenedDuration);
            }

            if(plugin.Config.disabled) {
                controller.EnableEffect<Disabled>(plugin.Config.disabledDuration);
            }

            if(plugin.Config.ensnared) {
                controller.EnableEffect<Ensnared>(plugin.Config.ensnaredDuration);
            }

            if(plugin.Config.exhausted) {
                controller.EnableEffect<Exhausted>(plugin.Config.exhaustedDuration);
            }

            if(plugin.Config.flash) {
                controller.EnableEffect<Flashed>(plugin.Config.flashDuration);
            }

            if(plugin.Config.poisoned) {
                controller.EnableEffect<Poisoned>(plugin.Config.poisonedDuration);
            }

            if(plugin.Config.bleeding) {
                controller.EnableEffect<Bleeding>(plugin.Config.bleedingDuration);
            }

            if(plugin.Config.sinkhole) {
                controller.EnableEffect<SinkHole>(plugin.Config.sinkholeDuration);
            }

            if(plugin.Config.speed) {
                controller.EnableEffect<Scp207>(plugin.Config.speedDuration);
            }

            if(plugin.Config.hemorrhage) {
                //hemrorrrogohgage
                controller.EnableEffect<Hemorrhage>(plugin.Config.hemorrhageDuration);
            }

            if(plugin.Config.decontaminating) {
                controller.EnableEffect<Decontaminating>(plugin.Config.decontaminatingDuration, true);
            }
        }

        public void Invisible(Player p, bool toggle) {
            if(toggle) {
                foreach(Player ply in Player.List) {
                    p.TargetGhostsHashSet.Add(ply.Id);
                }
            } else {
                foreach(Player ply in Player.List) {
                    if(p.TargetGhostsHashSet.Contains(ply.Id))
                        p.TargetGhostsHashSet.Remove(ply.Id);
                }
            }
        }

        public bool IsTranquilizerDamage(DamageTypes.DamageType damageType) => 
            (plugin.Config.comIsTranquilizer && damageType == DamageTypes.Com15) || (plugin.Config.uspIsTranquilizer && damageType == DamageTypes.Usp);

        public bool IsTranquilizer(ItemType type) =>
            (type == ItemType.GunCOM15 && plugin.Config.comIsTranquilizer)
                || (type == ItemType.GunUSP && plugin.Config.uspIsTranquilizer);

        // I'm fucking lazy 
        public void ToggleArmor(Player p, out string ReplyMessage) {
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
