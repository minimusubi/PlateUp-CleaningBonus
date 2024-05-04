using System.Runtime.InteropServices;
using KitchenData;

namespace CleaningBonus {
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableDish : IItemProperty {
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableMess : IApplianceProperty {
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableTrashBag : IItemProperty {
	}
}
