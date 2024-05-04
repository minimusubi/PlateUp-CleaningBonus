using System.Collections.Generic;
using System.Reflection;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CleaningBonus {
	[UpdateBefore(typeof(UpdateMoneyTracker))]
	[UpdateBefore(typeof(DestroyAppliancesAtNight))]
	[UpdateBefore(typeof(EndOfDayProgressionGroup))]
	public class Award : RestaurantSystem, IModSystem {
		internal static readonly List<int> DirtyDishIDs = [
			ItemID.PlateDirty,
			ItemID.PlateDirtyWithFood,
			ItemID.PlateDirtyWithBone
		];
		internal static readonly List<int> MessIDs = [
			ApplianceID.MessCustomer1,
			ApplianceID.MessCustomer2,
			ApplianceID.MessCustomer3,
			ApplianceID.MessKitchen1,
			ApplianceID.MessKitchen2,
			ApplianceID.MessKitchen3
		];
		internal static readonly List<int> TrashBagIDs = [
			ItemID.BinBag,
			ItemID.FlammableBinBag,
		];
		private EntityQuery DirtyDishQuery;
		private EntityQuery ItemProviderQuery;
		private EntityQuery MessQuery;
		private EntityQuery TrashBinQuery;
		private EntityQuery TrashBagQuery;
		private SMoney StartingMoney;

		protected override void Initialise() {
			base.Initialise();
			//DirtyDishQuery = GetEntityQuery(typeof(CItem));
			DirtyDishQuery = GetEntityQuery(typeof(CCountableDish));
			ItemProviderQuery = GetEntityQuery(typeof(CItemProvider));
			MessQuery = GetEntityQuery(typeof(CCountableMess));
			TrashBinQuery = GetEntityQuery(typeof(CApplianceBin));
			TrashBagQuery = GetEntityQuery(typeof(CCountableTrashBag));
		}

		protected override void OnUpdate() {
			if (HasSingleton<SIsDayFirstUpdate>()) {
				StartingMoney = GetSingleton<SMoney>();
			} else if (HasSingleton<SIsNightFirstUpdate>()) {
				SMoney currentMoney = GetSingleton<SMoney>();
				int difference = currentMoney - StartingMoney;

				int dishBonusPercent = PreferenceManager.Get<int>("dish_bonus_percent");
				int floorBonusPercent = PreferenceManager.Get<int>("floor_bonus_percent");
				int trashBonusPercent = PreferenceManager.Get<int>("trash_bonus_percent");

				Log($"Starting balance: {(int) StartingMoney}, current balance: {(int) currentMoney}, earned today: {difference}");

				if (dishBonusPercent > 0) {
					AwardBonus<SDishBonusActive>(!Has<SDirtyDishes>(), difference, ApplianceID.DishWasher, dishBonusPercent / 100f);
				}
				if (floorBonusPercent > 0) {
					AwardBonus<SFloorBonusActive>(!Has<SFloorMesses>(), difference, ApplianceID.FloorBufferStation, floorBonusPercent / 100f);
				}
				if (trashBonusPercent > 0) {
					AwardBonus<STrashBonusActive>(!Has<SOccupiedTrashBins>() && !Has<STrashBags>(), difference, ApplianceID.BinCompactor, trashBonusPercent / 100f);
				}
			} else if (HasSingleton<SIsDayTime>()) {
				SetCountableSingleton<SDirtyDishes>(GetDirtyDishCount());
				SetCountableSingleton<SFloorMesses>(GetMessCount());
				SetCountableSingleton<SOccupiedTrashBins>(GetOccupiedTrashBinCount());
				SetCountableSingleton<STrashBags>(GetTrashBagCount());

				TryActivateBonus<SDishBonusActive>(Has<SDirtyDishes>());
				TryActivateBonus<SFloorBonusActive>(Has<SFloorMesses>());
				TryActivateBonus<STrashBonusActive>(Has<SOccupiedTrashBins>() || Has<STrashBags>());
			}
		}

		private void SetCountableSingleton<Countable>(int count) where Countable : struct, CountableSingleton {
			if (count == 0) {
				EntityManager.DestroyEntity(GetEntityQuery(typeof(Countable)));
			} else {
				Countable singleton = default;
				singleton.Count = count;
				Set(singleton);
			}
		}

		private void TryActivateBonus<BonusType>(bool shouldActivate) where BonusType : struct, IComponentData {
			if (!Has<BonusType>()) {
				if (shouldActivate) {
					Log($"Setting {typeof(BonusType).Name}");
					Set<BonusType>();
				}
			}
		}

		private void AwardBonus<SBonusActive>(bool shouldAward, int earningsToday, int applianceID, float percent) where SBonusActive : struct, IComponentData {
			if (Has<SBonusActive>()) {
				Log($"{typeof(SBonusActive).Name} is active");
				if (shouldAward) {
					if (earningsToday > 0) {
						SMoney money = GetSingleton<SMoney>();
						int bonusAmount = Mathf.CeilToInt(earningsToday * percent);

						Log($"{bonusAmount} earned for {typeof(SBonusActive).Name}");
						Set<SMoney>(money + bonusAmount);
						//CreateMoneyPopup(applianceID, bonusAmount);
						TrackMoney(applianceID, bonusAmount);
					} else {
						Log($"Earnings was negative, skipping {typeof(SBonusActive).Name} bonus");
					}
				} else {
					Log($"{typeof(SBonusActive).Name} shouldAward is false, no bonus awarded");
				}
			} else {
				Log($"{typeof(SBonusActive).Name} is not present");
			}
		}

		private int GetDirtyDishCount() {
			//int count = DirtyDishQuery.CalculateEntityCount();
			int count = 0;

			using var entities = DirtyDishQuery.ToEntityArray(Allocator.TempJob);
			foreach (var entity in entities) {
				CItem item = EntityManager.GetComponentData<CItem>(entity);

				if (DirtyDishIDs.Contains(item.ID)) {
					count++;
				}
			}

			// Add dirty plates within wash basins/dish washers
			using var providers = ItemProviderQuery.ToComponentDataArray<CItemProvider>(Allocator.TempJob);
			foreach (var provider in providers) {
				if (DirtyDishIDs.Contains(provider.ProvidedItem)) {
					count += provider.Available;
				}
			}

			return count;
		}

		private int GetMessCount() {
			return MessQuery.CalculateEntityCount();
		}

		private int GetOccupiedTrashBinCount() {
			int count = 0;

			using var bins = TrashBinQuery.ToComponentDataArray<CApplianceBin>(Allocator.TempJob);
			foreach (var bin in bins) {
				if (bin.EmptyBinItem == ItemID.BinBag) {
					if (bin.CurrentAmount > 0) {
						count++;
					}
				}
			}

			return count;
		}

		private int GetTrashBagCount() {
			int count = TrashBagQuery.CalculateEntityCount();

			// Add items within item providers
			using var providers = ItemProviderQuery.ToComponentDataArray<CItemProvider>(Allocator.TempJob);
			foreach (var provider in providers) {
				if (TrashBagIDs.Contains(provider.ProvidedItem)) {
					count += provider.Available;
				}
			}

			return count;
		}

		// From https://github.com/propstg/plateup-overcooked-patience/blob/01e9e20611570bafef8a343027dbb672749f596e/OvercookedPatience/MoneyPopup.cs
		public void CreateMoneyPopup(int applianceID, int money) {
			FieldInfo field = GetType().GetField("ECBs", BindingFlags.Instance | BindingFlags.NonPublic);
			Dictionary<ECB, EntityCommandBufferSystem> ecbs = (Dictionary<ECB, EntityCommandBufferSystem>) field.GetValue(this);

			EntityCommandBuffer buffer = ecbs[ECB.End].CreateCommandBuffer();
			Entity entity = buffer.CreateEntity();
			buffer.AddComponent(entity, new CMoneyPopup() { Change = money });
			buffer.AddComponent(entity, new CPosition(new Vector3()));
			buffer.AddComponent(entity, new CLifetime(1f));
			buffer.AddComponent(entity, new CRequiresView() { Type = ViewType.MoneyPopup });
			MoneyTracker.AddEvent(new EntityContext(EntityManager, buffer), applianceID, money);
		}

		// From https://github.com/StarFluxMods/CPPEffectPack/blob/master/Systems/AdjustMoney.cs
		// See https://discord.com/channels/1027159977040826408/1035906388284690453/1236168710595547257
		private void TrackMoney(int applianceID, int money) {
			Entity e = EntityManager.CreateEntity(typeof(CMoneyTrackEvent));
			EntityManager.SetComponentData(e, new CMoneyTrackEvent {
				Identifier = applianceID,
				Amount = money
			});
		}

		private void Log(string message) {
			Debug.Log($"[{Main.MOD_NAME}] [{GetType().Name} ] {message}");
		}
	}
}
