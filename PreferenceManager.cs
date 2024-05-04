using System;
using System.Collections.Generic;

namespace CleaningBonus {
	internal struct PreferenceDefinition {
		public Type Type;
		public object Value;
	}

	internal class PreferenceManager {
		internal static PreferenceWrapper Wrapper = null;

		internal static Dictionary<string, PreferenceDefinition> DefaultPreferences = new () {
			{ "dish_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 8 } },
			{ "floor_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 8 } },
			{ "trash_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 4 } },
			{ "bonus_cleaning_duration", new PreferenceDefinition() { Type = typeof(int), Value = 5 } },
		};

		internal static void Initialize() {
			if (KitchenMods.ModPreload.Mods.Exists(mod => {
				return mod.Name == "PreferenceSystem";
			})) {
				Wrapper = new PreferenceWrapper();
				Wrapper.SetupMenu();
			}
		}

		public static T Get<T>(string key) {
			if (Wrapper != null) {
				return Wrapper.Get<T>(key);
			}

			if (DefaultPreferences.TryGetValue(key, out PreferenceDefinition definition)) {
				return (T) definition.Value;
			} else {
				return default;
			}
		}

		public static bool Set<T>(string key, T value) {
			if (Wrapper != null) {
				return Wrapper.Set(key, value);
			}

			return false;
		}
	}
}
