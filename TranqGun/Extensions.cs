using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem;
using InventorySystem.Items.Firearms.Attachments;

namespace TranqGun {
    public static class Extensions {
		// uniq = unique id - modBarrel == 1 = silencer
		public static bool HasSilencer(this Player p) {
			try {
				return p.CurrentItem is Firearm gun && gun.Attachments.Any(attachment => attachment.Name == AttachmentNameTranslation.SoundSuppressor);
			} catch(Exception e) {
				e.Print("HasSilencer");
			}	
			return false;
		}

		public static void ShowHint(this Player p, float duration, string text) => p.ShowHint(text, duration);

		public static void RemoveWeaponAmmo(this Player player, int amount)
		{
			player.Ammo.Remove(ItemType.Ammo9x19 - amount);
		}

		public static void Print(this Exception e, string type) {
            Log.Error($"{type}: {e.Message}\n{e.StackTrace}");
        }

		// Gonna wait for attachments API to be le finished
		
		/*public static Item GetTranquilizerItem()
		{
			var tranqType = Plugin.Instance.Config.comIsTranquilizer ? ItemType.GunCOM15 : ItemType.GunCOM18;
			var tranqItem = new Firearm(tranqType);
			
			tranqItem.Attachments[37].Slot == AttachmentSlot.Barrel


			var tempGun = new Firearm(tranqItem);
			// This is still to do
			return tempGun;
		}*/

    }
}
