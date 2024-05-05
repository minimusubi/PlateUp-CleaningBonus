using CleaningBonus;
using KitchenMods;
using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(CountableSingleton))]
namespace CleaningBonus {
	public struct SDishBonusActive : IModComponent, IComponentData {
	}

	public struct SFloorBonusActive : IModComponent, IComponentData {
	}

	public struct STrashBonusActive : IModComponent, IComponentData {
	}

	internal interface CountableSingleton : IModComponent, IComponentData {
		int Count { get; set; }
	}

	public struct SDirtyDishes : CountableSingleton, IModComponent, IComponentData {
		public int Count { get; set; }
	}

	public struct SFloorMesses : CountableSingleton, IModComponent, IComponentData {
		public int Count { get; set; }
	}

	public struct SOccupiedTrashBins : CountableSingleton, IModComponent, IComponentData {
		public int Count { get; set; }
	}

	public struct STrashBags : CountableSingleton, IModComponent, IComponentData {
		public int Count { get; set; }
	}

	public struct SCleaningTimeActive : IModComponent, IComponentData {
	}
}
