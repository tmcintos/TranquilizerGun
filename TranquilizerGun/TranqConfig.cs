using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Loader;
using UnityEngine;

namespace TranquilizerGun {
    public class TranqConfig : IConfig {

        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("If set to false, only the commands will be enabled.")]
        public bool IsEnabledCustom { get; set; } = true;

        #region Tranquilizer Settings
        [Description("Is the COM-15 treated as a Tranquilizer?")]
        public bool comIsTranquilizer { get; set; } = false;

        [Description("Is the USP treated as a Tranquilizer?")]
        public bool uspIsTranquilizer { get; set; } = true;

        [Description("Any of the pistols need a silencer on them to be a Tranquilizer?")]
        public bool silencerRequired { get; set; } = true;

        [Description("How much ammo is used per shot.")]
        public int ammoUsedPerShot { get; set; } = 9;

        [Description("How much damage does a Tranquilizer gun do.")]
        public int tranquilizerDamage { get; set; } = 1;

        [Description("For how long will the players be put to sleep.")]
        public float sleepDurationMax { get; set; } = 5;
        public float sleepDurationMin { get; set; } = 3;
        #endregion

        #region Broadcasts
        [Description("Whether broadcasts are cleared before doing one.")]
        public bool clearBroadcasts { get; set; } = true;

        [Description("Whether to use the new SCP:SL Hints System over broadcasts (The same variables below are used)")]
        public bool UseHintsSystem { get; set; } = false;

        [Description("Broadcast shown when the player is shot with a Tranquilizer. (Using %seconds in the text will be replaced by how many seconds the player will sleep for.")]
        public ushort tranquilizedBroadcastDuration { get; set; } = 3;
        public string tranquilizedBroadcast { get; set; } = "<color=red>You've been shot with a Tranquilizer...</color>";

        [Description("Broadcast shown when a Tranquilizer is picked up.")]
        public ushort pickedUpBroadcastDuration { get; set; } = 3;
        public string pickedUpBroadcast { get; set; } = "'<color=green><b>You picked up a tranquilizer gun!</b></color> \nEvery shot uses %ammo ammo, so count your bullets!'";

        [Description("Broadcast shown when a player is trying to shoot but has no ammo.")]
        public ushort notEnoughAmmoBroadcastDuration { get; set; } = 3;
        public string notEnoughAmmoBroadcast { get; set; } = "<color=red>You need at least %ammo ammo for the bullet to fire!</color>";
        #endregion

        #region Extra Settings
        [Description("Can you use the Tranquilizer effects on allies?")]
        public bool FriendlyFire { get; set; } = true;

        [Description("Where players are teleported when they are put to sleep. (usingEffects must be disabled)")]
        public float newPos_x { get; set; } = 2;
        public float newPos_y { get; set; } = -2;
        public float newPos_z { get; set; } = 3;

        [Description("Whether the player will be teleported away. (This + the effect below will give the effect that the old Tranquilizer had). (This will also apply Amnesia + Invisibility effects for the duration of the sleep effect)")]
        public bool teleportAway { get; set; } = false;

        [Description("Whether a Ragdoll is summoned when you're tranquilized.")]
        public bool SummonRagdoll { get; set; } = false;

        [Description("Should the player's inventory be dropped when shot.")]
        public bool dropItems { get; set; } = false;

        [Description("If Serpents Hand is enabled and you don't want friendly fire enabled, set this to true.")]
        public bool areTutorialSerpentsHand { get; set; } = false;

        [Description("Are COM-15s replaced with USPs at the start of the round? (2.3.1 Note: This doesn't work anymore as of 10.0.2, this feature will comeback at some point, but as of now, it's useless)")]
        public bool replaceCom { get; set; } = true;

        [Description("Chance for COM-15s to be replaced with USPs. (From 0 to 100)")]
        public ushort replaceChance { get; set; } = 100;

        [Description("List of roles which will be ignored by the Tranquilizer.")]
        public bool doBlacklist { get; set; } = true;
        public string blacklist { get; set; } = "Scp173, Scp106";
        [Description("List of roles which will require multiple shots to be put to sleep.")]
        public bool doSpecialRoles { get; set; } = false;
        public string specialRolesList { get; set; } = "Scp173:2, Scp106:5";

        internal List<RoleType> roleBlacklist;
        internal Dictionary<RoleType, ushort> specialRoles;
        #endregion

        #region PlayerEffects
        [Description("Whether the effects below will be used when the player is shot by a Tranquilizer.")]
        public bool usingEffects { get; set; } = true;

        public bool amnesia { get; set; } = true;
        public float amnesiaDuration { get; set; } = 2f;

        public bool disabled { get; set; } = false;
        public float disabledDuration { get; set; } = 3f;

        public bool flash { get; set; } = false;
        public float flashDuration { get; set; } = 3f;

        public bool blinded { get; set; } = false;
        public float blindedDuration { get; set; } = 3f;

        public bool concussed { get; set; } = false;
        public float concussedDuration { get; set; } = 3f;
                
        public bool deafened { get; set; } = false;
        public float deafenedDuration { get; set; } = 3f;

        public bool ensnared { get; set; } = true;
        public float ensnaredDuration { get; set; } = 1f;

        public bool bleeding { get; set; } = false;
        public float bleedingDuration { get; set; } = 3f;

        public bool poisoned { get; set; } = false;
        public float poisonedDuration { get; set; } = 3f;
                
        public bool asphyxiated { get; set; } = false;
        public float asphyxiatedDuration { get; set; } = 3f;

        public bool exhausted { get; set; } = false;
        public float exhaustedDuration { get; set; } = 3f;

        public bool sinkhole { get; set; } = true;
        public float sinkholeDuration { get; set; } = 3f;

        public bool hemorrhage { get; set; } = false;
        public float hemorrhageDuration { get; set; } = 3f;

        public bool decontaminating { get; set; } = false;
        public float decontaminatingDuration { get; set; } = 3f;

        public bool speed { get; set; } = false;
        public float speedDuration { get; set; } = 3f;

        public bool invisible { get; set; } = false;
        public float invisibleDuration { get; set; } = 3f;
        #endregion

        public List<RoleType> BlacklistedRoles() {
            List<RoleType> l = new List<RoleType>();
            if(doBlacklist) {
                try {
                    string[] bl = Regex.Replace(blacklist, @"\s+", "").Split(',');
                    foreach(string r in bl) {
                        if(Enum.TryParse(r, true, out RoleType role)) {
                            l.Add(role);
                        } else
                            Log.Error($"Couldn't parse role: {r}.");
                    }
                } catch(Exception e) {
                    e.Print("Loading Blacklisted Roles");
                }
            }
            return l;
        }

        public Dictionary<RoleType, ushort> SpecialRoles() {
            Dictionary<RoleType, ushort> l = new Dictionary<RoleType, ushort>();
            if(doSpecialRoles) {
                try {
                    string[] specialRoles = Regex.Replace(specialRolesList, @"\s+", "").Split(',');
                    foreach(string o in specialRoles) {
                        string[] option = Regex.Replace(o, @"\s+", "").Split(':');
                        if(Enum.TryParse(option[0], true, out RoleType role) && ushort.TryParse(option[1], out ushort shots)) {
                            l.Add(role, shots);
                        } else Log.Error($"Couldn't load {o}.");
                    }
                } catch(Exception e) {
                    e.Print("Loading Special Roles");
                }
            }
            return l;
        }
    }
}
