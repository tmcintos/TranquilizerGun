using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Firearms.Attachments;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Object = UnityEngine.Object;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Interfaces;
using static MapGeneration.ImageGenerator;
using System.Xml.Linq;

namespace TranqGun {
    public class EventsHandler {

        private readonly Plugin _plugin;
        public readonly List<string> Tranquilized;
        public readonly List<string> Armored;
        public readonly Dictionary<string, int> ScpShots;
        public bool AllArmorEnabled = false;

        public EventsHandler(Plugin plugin) {
            this._plugin = plugin;
            Tranquilized = new List<string>();
            Armored = new List<string>();
            ScpShots = new Dictionary<string, int>();
        }

        public void RoundEnd(RoundEndedEventArgs _) {
            Tranquilized.Clear();
            Armored.Clear();
            ScpShots.Clear();
        } 

        public void ShootEvent(ShootingEventArgs ev)
        {
            if (!IsTranquilizer(ev.Player.CurrentItem.Type))
                return;
            
            if(_plugin.Config.silencerRequired && !ev.Player.HasSilencer())
                return;
            
            if (!(ev.Player.CurrentItem is Firearm tranqGun)) return;
            if (tranqGun.Ammo < _plugin.Config.ammoUsedPerShot - 1)
            {
                ev.IsAllowed = false;
                    
                if (_plugin.Config.stopClientDesync)
                {
                    tranqGun.Ammo = 0;
                }

                if(_plugin.Config.notEnoughAmmoBroadcastDuration > 0) {
                    if(_plugin.Config.UseHintsSystem)
                        ev.Player.ShowHint(_plugin.Config.notEnoughAmmoBroadcastDuration, _plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{_plugin.Config.ammoUsedPerShot}"));
                    else {
                        if(_plugin.Config.clearBroadcasts)
                            ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(_plugin.Config.notEnoughAmmoBroadcastDuration, _plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{_plugin.Config.ammoUsedPerShot}"));
                    }
                }

                return;   
            }
                
            ev.Player.ShowHitMarker(2f);
            tranqGun.Ammo = (byte) (tranqGun.Ammo - (_plugin.Config.ammoUsedPerShot - 1));
            if (tranqGun.Ammo < _plugin.Config.ammoUsedPerShot && _plugin.Config.stopClientDesync)
            {
                tranqGun.Ammo = 0;
            }
        }

        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.NewItem == null || !IsTranquilizer(ev.NewItem.Type) || _plugin.Config.pickedUpBroadcastDuration <= 0) return;

            if (!(ev.NewItem is Firearm gun &&
                  gun.Attachments.Any(attachment => attachment.Name == AttachmentName.SoundSuppressor)) &&
                _plugin.Config.silencerRequired) 
                return;
            
            if(_plugin.Config.UseHintsSystem)
                ev.Player.ShowHint(_plugin.Config.pickedUpBroadcastDuration, _plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{_plugin.Config.ammoUsedPerShot}"));
            else {
                if(_plugin.Config.clearBroadcasts)
                    ev.Player.ClearBroadcasts();
                ev.Player.Broadcast(_plugin.Config.pickedUpBroadcastDuration, _plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{_plugin.Config.ammoUsedPerShot}"));
            }
        }

        public void HurtEvent(HurtingEventArgs ev) {
            try
            {
                if(ev.Attacker == null || ev.Player == null || ev.Attacker == ev.Player || _plugin.Config.roleBlacklist.Contains(ev.Player.Role))
                    return;

                if (Tranquilized.Contains(ev.Player.UserId)
                    && (ev.DamageHandler.Type == DamageType.Decontamination || ev.DamageHandler.Type == DamageType.Warhead || ev.DamageHandler.Type == DamageType.Scp939) 
                    && (_plugin.Config.teleportAway || _plugin.Config.SummonRagdoll)) 
                {
                    ev.Amount = 0;
                }

                if (!IsTranquilizerDamage(ev.DamageHandler.Type) || Tranquilized.Contains(ev.Player.UserId)) return;

                if(_plugin.Config.silencerRequired && !ev.Attacker.HasSilencer()) return;

                ev.Amount = _plugin.Config.tranquilizerDamage;

                if(!_plugin.Config.FriendlyFire && (ev.Player.Role.Side == ev.Attacker.Role.Side || _plugin.Config.areTutorialSerpentsHand && ev.Attacker.Role.Side == Side.ChaosInsurgency && ev.Player.Role == RoleTypeId.Tutorial)) return;

                var id = ev.Player.UserId;
                if(_plugin.Config.specialRoles.Keys.Contains(ev.Player.Role)) {
                    if(!ScpShots.ContainsKey(id))
                        ScpShots.Add(id, 0);
                    ScpShots[id] += 1;
                    if (ScpShots[id] < _plugin.Config.specialRoles[ev.Player.Role]) return;
                    Sleep(ev.Player);
                    ScpShots[id] = 0;
                    return;
                }
                
                ev.Attacker.ShowHitMarker(2f);
                Sleep(ev.Player);
            } catch(Exception e) {
                e.Print("HurtEvent (TranqHandler)");
            }
        }

        public void Sleep(Player player) {
            try {
                // Initialize variables & add player to list
                var oldPos = player.Position;
                var controller = player.ReferenceHub.playerEffectsController;
                Tranquilized.Add(player.Nickname);
                float sleepDuration = UnityEngine.Random.Range(_plugin.Config.sleepDurationMin, _plugin.Config.sleepDurationMax), bdd = controller.GetEffect<Bleeding>().Duration;
                bool pd = controller.GetEffect<Corroding>().IsEnabled, bd = controller.GetEffect<Bleeding>().IsEnabled;

                // Broadcast message (if enabled)
                if(_plugin.Config.tranquilizedBroadcastDuration > 0) {
                    if(_plugin.Config.UseHintsSystem)
                        player.ShowHint(_plugin.Config.tranquilizedBroadcastDuration, _plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    else {
                        if(_plugin.Config.clearBroadcasts)
                            player.ClearBroadcasts();
                        player.Broadcast(_plugin.Config.tranquilizedBroadcastDuration, _plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    }
                }

                if(_plugin.Config.dropItems)
                    player.DropItems();

                if(_plugin.Config.usingEffects) {
                    EnableEffects(controller);

                    if(_plugin.Config.invisible) {
                        Invisible(player, true);
                        Timing.CallDelayed(_plugin.Config.invisibleDuration, () => Invisible(player, false));
                    }
                }
                if(_plugin.Config.SummonRagdoll) {
                    // Spawn a Ragdoll

                    Exiled.API.Features.Ragdoll.CreateAndSpawn(player.Role, player.DisplayNickname, new CustomReasonDamageHandler("Under the influence of a tranquilizer"), player.Position, default);
                }
                if(_plugin.Config.teleportAway) {
                    // Apply effects
                    controller.EnableEffect<AmnesiaVision>(sleepDuration);
                    controller.EnableEffect<Invisible>(sleepDuration);

                    player.Position = new Vector3(_plugin.Config.newPos_x, _plugin.Config.newPos_y, _plugin.Config.newPos_z);
                    Timing.CallDelayed(1f, () => player.ReferenceHub.playerEffectsController.DisableEffect<Decontaminating>());
                }

                Timing.CallDelayed(sleepDuration, () => Wake(player, oldPos, pd, bd, bdd));

            } catch(Exception e) {
                e.Print($"Sleeping {player.Nickname} {e.StackTrace}");
            }
        }

        public void Wake(Player player, Vector3 oldPos, bool inPd = false, bool bleeding = false, float bleedingDur = 3) {
            try {
                Tranquilized.Remove(player.UserId);

                if(_plugin.Config.SummonRagdoll)
                    foreach(var doll in Ragdoll.List) {
                        if(doll.NetworkInfo.OwnerHub.nicknameSync == player.ReferenceHub.nicknameSync) {
                            NetworkServer.Destroy(doll.GameObject);
                        }
                    }

                if (!_plugin.Config.teleportAway) return;
                player.Position = oldPos;

                Timing.CallDelayed(1.5f, () => {
                    // Attempt number 27 of fixing this issue
                    if(player.TryGetEffect(EffectType.Decontaminating, out var effect)) {
                        ((Decontaminating) effect).TimeLeft = 99f;
                    }
                    player.DisableEffect<Decontaminating>();

                    if(inPd)
                        player.EnableEffect<Corroding>();

                    if(bleeding)
                        player.EnableEffect<Bleeding>(bleedingDur);

                    if (!Warhead.IsDetonated) return;
                        
                    if(player.CurrentRoom.Zone != ZoneType.Surface)
                        player.Kill("The warhead was detonated and your were exploded while you were tranquilized!");
                    else
                    {
                        if (!Lift.List.Where(l => l.Type == ElevatorType.GateA || l.Type == ElevatorType.GateB)
                                .Any(l => l.IsInElevator(player.Position))) return;
                            
                        player.Kill("You were in an elevator!");
                    }
                });
            } catch(Exception e) {
                e.Print("Sleeping " + player.Nickname);
            }
        }

        public void EnableEffects(PlayerEffectsController controller) {
            if(_plugin.Config.amnesia) {
                controller.EnableEffect<AmnesiaVision>(_plugin.Config.amnesiaDuration);
            }

            if(_plugin.Config.asphyxiated) {
                controller.EnableEffect<Asphyxiated>(_plugin.Config.asphyxiatedDuration);
            }

            if(_plugin.Config.blinded) {
                controller.EnableEffect<Blinded>(_plugin.Config.blindedDuration);
            }

            if(_plugin.Config.concussed) {
                controller.EnableEffect<Concussed>(_plugin.Config.concussedDuration);
            }

            if(_plugin.Config.deafened) {
                controller.EnableEffect<Deafened>(_plugin.Config.deafenedDuration);
            }

            if(_plugin.Config.disabled) {
                controller.EnableEffect<Disabled>(_plugin.Config.disabledDuration);
            }

            if(_plugin.Config.ensnared) {
                controller.EnableEffect<Ensnared>(_plugin.Config.ensnaredDuration);
            }

            if(_plugin.Config.exhausted) {
                controller.EnableEffect<Exhausted>(_plugin.Config.exhaustedDuration);
            }

            if(_plugin.Config.flash) {
                controller.EnableEffect<Flashed>(_plugin.Config.flashDuration);
            }

            if(_plugin.Config.poisoned) {
                controller.EnableEffect<Poisoned>(_plugin.Config.poisonedDuration);
            }

            if(_plugin.Config.bleeding) {
                controller.EnableEffect<Bleeding>(_plugin.Config.bleedingDuration);
            }

            if(_plugin.Config.sinkhole) {
                controller.EnableEffect<Sinkhole>(_plugin.Config.sinkholeDuration);
            }

            if(_plugin.Config.speed) {
                controller.EnableEffect<Scp207>(_plugin.Config.speedDuration);
            }

            if(_plugin.Config.hemorrhage) {
                //hemrorrrogohgage
                controller.EnableEffect<Hemorrhage>(_plugin.Config.hemorrhageDuration);
            }

            if(_plugin.Config.decontaminating) {
                controller.EnableEffect<Decontaminating>(_plugin.Config.decontaminatingDuration, true);
            }
        }

        public static void Invisible(Player player, bool toggle) {
            if(toggle) {
                foreach(var item in Player.List) {
                    if(item == player)
                        continue;

                    item.EnableEffect<Invisible>();
                }
            } else {
                foreach(var item in Player.List) {
                    item.DisableEffect<Invisible>();
                }
            }
        }

        public bool IsTranquilizerDamage(DamageType damageType) => 
            (_plugin.Config.comIsTranquilizer && damageType == DamageType.Com15) || (_plugin.Config.uspIsTranquilizer && damageType == DamageType.Com18);

        /// <summary>
        /// Check whether or not the gun is a valid tranquilizer gun
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTranquilizer(ItemType type)
        {
            switch (type)
            {
                case ItemType.GunCOM15 when _plugin.Config.comIsTranquilizer:
                case ItemType.GunCOM18 when _plugin.Config.uspIsTranquilizer:
                    return true;
                default:
                    return false;
            }
        }
            

        // I'm fucking lazy 
        public void ToggleArmor(Player p, out string replyMessage) {
            if(Armored.Contains(p.UserId)) {
                Armored.Remove(p.UserId);
                replyMessage = $"<color=red>{p.Nickname} is no longer protected against Tranquilizers.</color>";
            } else {
                Armored.Add(p.UserId);
                replyMessage = $"<color=#4ce300>{p.Nickname} is now protected against Tranquilizers.</color>";
            }
        }
    }
}
