using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CleaningBonus {
	[UpdateInGroup(typeof(TimeManagementGroup))]
	[UpdateBefore(typeof(AdvanceTime))]
	public class TimeManager : RestaurantSystem, IModSystem {
		private EntityQuery CustomerGroupsQuery;
		private EntityQuery ApplianceQuery;

		protected override void Initialise() {
			base.Initialise();
			CustomerGroupsQuery = GetEntityQuery(typeof(CCustomerGroup));
			ApplianceQuery = GetEntityQuery(typeof(CAppliance));

			RequireSingletonForUpdate<SIsDayTime>();
		}

		protected override void OnUpdate() {
			STime time = GetSingleton<STime>();
			float dayLength = time.DayLength;

			if (!Has<SGameOver>() && !Has<SPracticeMode>()) {
				if (!time.ForcePause) {
					time.TimeOfDayUnbounded += Time.DeltaTime / dayLength;
				}

				if (!Has<SCleaningTimeActive>() && time.TimeOfDayUnbounded >= 1f && CustomerGroupsQuery.IsEmpty && NeedsBonusTime()) {
					if (ShouldActivateBonusTime()) {
						int bonusCleaningDuration = PreferenceManager.Get<int>("bonus_cleaning_duration");

						Log($"Extending day from {dayLength} to {(time.TimeOfDayUnbounded * dayLength) + bonusCleaningDuration}");
						Log($"Current TimeOfDayUnbounded is {time.TimeOfDayUnbounded} ({time.TimeOfDayUnbounded * dayLength})");
						time = GetSingleton<STime>();
						time.DayLength = (time.TimeOfDayUnbounded * dayLength) + bonusCleaningDuration;
						time.TimeOfDayUnbounded = time.SecondsSinceDayBegan / time.DayLength;
						SetSingleton(time);
						Set<SCleaningTimeActive>();

						// Disable booking desks
						using var entities = ApplianceQuery.ToEntityArray(Allocator.TempJob);
						foreach (var entity in entities) {
							CAppliance appliance = EntityManager.GetComponentData<CAppliance>(entity);

							if (appliance.ID == ApplianceID.BookingDesk) {
								EntityManager.AddComponentData<CIsBroken>(entity, default);
							}
						}
					}
				} else if (Has<SCleaningTimeActive>() && CustomerGroupsQuery.IsEmpty && !NeedsBonusTime()) {
					time = GetSingleton<STime>();

					time.DayLength = time.SecondsSinceDayBegan;
					time.TimeOfDayUnbounded = 1;
					SetSingleton(time);
				}
			}
		}

		private bool ShouldActivateBonusTime() {
			bool dishBonusEnabled = PreferenceManager.Get<int>("dish_bonus_percent") > 0;
			bool floorBonusEnabled = PreferenceManager.Get<int>("floor_bonus_percent") > 0;
			bool trashBonusEnabled = PreferenceManager.Get<int>("trash_bonus_percent") > 0;

			return (dishBonusEnabled && Has<SDishBonusActive>()) || (floorBonusEnabled && Has<SFloorBonusActive>()) || (trashBonusEnabled && Has<STrashBonusActive>());
		}

		private bool NeedsBonusTime() {
			bool dishBonusEnabled = PreferenceManager.Get<int>("dish_bonus_percent") > 0;
			bool floorBonusEnabled = PreferenceManager.Get<int>("floor_bonus_percent") > 0;
			bool trashBonusEnabled = PreferenceManager.Get<int>("trash_bonus_percent") > 0;

			return (dishBonusEnabled && Has<SDirtyDishes>()) || (floorBonusEnabled && Has<SFloorMesses>()) || (trashBonusEnabled && (Has<SOccupiedTrashBins>() || Has<STrashBags>()));
		}

		private void Log(string message) {
			Debug.Log($"[{Main.MOD_NAME}] [{GetType().Name} ] {message}");
		}
	}
}
