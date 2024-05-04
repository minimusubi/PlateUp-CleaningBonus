using KitchenData;
using KitchenMods;

namespace CleaningBonus {
	public class Main : IModInitializer {
		public static readonly string MOD_GUID = "musubi.plateup.cleaningbonus";
		public static readonly string MOD_NAME = "Cleaning Bonus";

		public Main() { }

		public void PostActivate(Mod mod) {
			PreferenceManager.Initialize();
		}

		public void PreInject() {
			foreach (var id in Award.DirtyDishIDs) {
				Item dish = GameData.Main.Get<Item>(id);

				if (dish.Properties.Exists((Property) => {
					return Property.GetType() == typeof(CCountableDish);
				})) {
					break;
				}

				dish.Properties.Add(default(CCountableDish));
			}

			foreach (var id in Award.MessIDs) {
				Appliance mess = GameData.Main.Get<Appliance>(id);

				if (mess.Properties.Exists((Property) => {
					return Property.GetType() == typeof(CCountableMess);
				})) {
					break;
				}

				mess.Properties.Add(default(CCountableMess));
			}

			foreach (var id in Award.TrashBagIDs) {
				Item bag = GameData.Main.Get<Item>(id);

				if (bag.Properties.Exists((Property) => {
					return Property.GetType() == typeof(CCountableTrashBag);
				})) {
					break;
				}

				bag.Properties.Add(default(CCountableTrashBag));
			}
		}

		public void PostInject() { }
	}
}
