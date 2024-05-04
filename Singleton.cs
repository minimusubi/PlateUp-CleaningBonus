using Unity.Entities;

namespace CleaningBonus {
	public struct SDishBonusActive : IComponentData {
	}

	public struct SFloorBonusActive : IComponentData {
	}

	public struct STrashBonusActive : IComponentData {
	}

	internal interface CountableSingleton : IComponentData {
		int Count { get; set; }
	}

	public struct SDirtyDishes : CountableSingleton, IComponentData {
		public int Count { get; set; }
	}

	public struct SFloorMesses : CountableSingleton {
		public int Count { get; set; }
	}

	public struct SOccupiedTrashBins : CountableSingleton {
		public int Count { get; set; }
	}

	public struct STrashBags : CountableSingleton {
		public int Count { get; set; }
	}

	public struct SCleaningTimeActive : IComponentData {
	}
}
