using System.Runtime.InteropServices;
using KitchenData;
using KitchenMods;

namespace CleaningBonus {
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableDish : IModComponent, IItemProperty {
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableMess : IModComponent, IApplianceProperty {
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CCountableTrashBag : IModComponent, IItemProperty {
	}
}
