using System;
using System.Collections.Generic;
using KitchenMods;
using UnityEngine;

namespace CleaningBonus {
	internal struct PreferenceDefinition {
		public Type Type;
		public object Value;
	}

	internal class PreferenceManager {
		private const string PreferenceSystemManagerTypeName = "PreferenceSystem.PreferenceSystemManager";
		internal static PreferenceWrapper Wrapper = null;

		internal static Dictionary<string, PreferenceDefinition> DefaultPreferences = new() {
			{ "dish_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 8 } },
			{ "floor_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 8 } },
			{ "trash_bonus_percent", new PreferenceDefinition() { Type = typeof(int), Value = 4 } },
			{ "bonus_cleaning_duration", new PreferenceDefinition() { Type = typeof(int), Value = 7 } },
		};

		internal static void Initialize() {
			if (!IsPreferenceSystemAvailable()) {
				Log("PreferenceSystem was not found, using only default preferences.");
				return;
			}

			Log("PreferenceSystem was found, setting up preference menu.");
			Wrapper = new PreferenceWrapper();
			Wrapper.SetupMenu();
		}

		private static bool IsPreferenceSystemAvailable() {
			return ModPreload.Mods.Exists(mod => {
				return mod.GetPacks<AssemblyModPack>().Exists(pack => {
					return pack.Asm?.GetType(PreferenceSystemManagerTypeName, throwOnError: false) != null;
				});
			});
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

		private static void Log(string message) {
			Debug.Log($"[{Main.MOD_NAME}] [PreferenceManager] {message}");
		}
	}
}
