using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace CleaningBonus {
	[UpdateAfter(typeof(Award))]
	public class ResetAtPrep : RestaurantSystem, IModSystem {
		protected override void Initialise() {
			base.Initialise();

			RequireSingletonForUpdate<SIsNightFirstUpdate>();
		}

		protected override void OnUpdate() {
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SDishBonusActive)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SFloorBonusActive)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(STrashBonusActive)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SDirtyDishes)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SFloorMesses)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SOccupiedTrashBins)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(STrashBags)));
			EntityManager.DestroyEntity(GetEntityQuery(typeof(SCleaningTimeActive)));
		}
	}
}
