using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem;
using InventorySystem.Items.Firearms.Attachments;

namespace TranqGun {
    public static class Extensions {
		// uniq = unique id - modBarrel == 1 = silencer
		public static bool HasSilencer(this Player p)
		{
			return p.CurrentItem is Firearm gun && gun.Attachments.Any(attachment =>
				attachment.Name == AttachmentName.SoundSuppressor);
		}

		public static void ShowHint(this Player p, float duration, string text) => p.ShowHint(text, duration);

		/// <summary>
		/// Removes an given amount of 9X19 ammo (Nonfunctional)
		/// </summary>
		/// <param name="player"></param>
		/// <param name="amount"></param>
		public static void RemoveWeaponAmmo(this Player player, int amount)
		{
			player.Ammo[ItemType.Ammo9x19] -= Convert.ToUInt16(amount);
		}

		public static void Print(this Exception e, string type) {
            Log.Error($"{type}: {e.Message}\n{e.StackTrace}");
        }

		public static Item GetTranquilizerItem()
		{
			var tranqType = Plugin.Instance.Config.comIsTranquilizer ? FirearmType.Com15 : FirearmType.Com18;
			var tranqItem = Firearm.Create(tranqType);

            if ( Plugin.Instance.Config.silencerRequired ) {
                tranqItem.AddAttachment(AttachmentName.SoundSuppressor);
            }

            return tranqItem;
		}

    }
}
