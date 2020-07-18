using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace TranquilizerGun {
    public static class Extensions {
		// uniq = unique id - modBarrel == 1 = silencer
		public static bool HasSilencer(this ReferenceHub hub) {
			try {
				if(hub.inventory == null || hub.weaponManager == null || hub.weaponManager.curWeapon < 0 ||
					hub.weaponManager.curWeapon >= hub.weaponManager.weapons.Length || !hub.inventory.GetItemInHand().id.IsPistol())
					return false;
				else if(hub.inventory.GetItemInHand().modBarrel == 1)
					return true;
			} catch(Exception e) {
				e.Print("HasSilencer");
			}	
			return false;
		}

		public static void RemoveWeaponAmmo(this Player player, int amount) {
			player.Inventory.items.ModifyDuration(
			player.Inventory.items.IndexOf(player.CurrentItem),
			player.CurrentItem.durability - amount);
		}

		public static bool IsPistol(this ItemType type) => type == ItemType.GunCOM15 || type == ItemType.GunUSP;

		public static void Print(this Exception e, string type) {
            Log.Error($"{type}: {e.Message}\n{e.StackTrace}");
        }

		public static Inventory.SyncItemInfo GetTranquilizerItem() {
			Inventory.SyncItemInfo _tempGun = new Inventory.SyncItemInfo();
			_tempGun.modBarrel = 1;
			_tempGun.durability = 18;
			_tempGun.id = ItemType.GunUSP;
			// This is still todo
			return _tempGun;
		}

    }
}
