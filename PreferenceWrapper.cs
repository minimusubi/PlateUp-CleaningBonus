using System;
using System.Collections.Generic;
using Kitchen;
using PreferenceSystem;

namespace CleaningBonus {
	internal class PreferenceWrapper {
		internal object PrefManager = null;

		public PreferenceWrapper() {
			PrefManager = new PreferenceSystemManager(Main.MOD_GUID, Main.MOD_NAME);
		}

		public void SetupMenu() {
			if (PrefManager != null) {
				try {
					int[] percentOptions = GetInts(0, 15, 1);
					string[] percentLabels = GetLabels(percentOptions, (value) => {
						if (value == 0) {
							return "Disabled";
						}

						return value + "%";
					});

					PreferenceSystemManager manager = (PreferenceSystemManager) PrefManager;
					manager
						.AddLabel("Cleaning Bonus")
						.AddSpacer()
						.AddLabel("Dish Cleaning Bonus")
						.AddOption("dish_bonus_percent", (int) PreferenceManager.DefaultPreferences["dish_bonus_percent"].Value, percentOptions, percentLabels)
						.AddLabel("Floor Cleaning Bonus")
						.AddOption("floor_bonus_percent", (int) PreferenceManager.DefaultPreferences["floor_bonus_percent"].Value, percentOptions, percentLabels)
						.AddLabel("Trash Cleaning Bonus")
						.AddOption("trash_bonus_percent", (int) PreferenceManager.DefaultPreferences["trash_bonus_percent"].Value, percentOptions, percentLabels)
						.AddLabel("Bonus Cleaning Time")
						.AddOption("bonus_cleaning_duration", (int) PreferenceManager.DefaultPreferences["bonus_cleaning_duration"].Value, GetInts(0, 15, 1), GetLabels(percentOptions, (value) => {
							if (value == 0) {
								return "Disabled";
							}

							return value + " second" + (value == 1 ? "" : "s");
						}))
						.AddSpacer()
						.AddButtonWithConfirm("Reset to Defaults", "Reset all Cleaning Bonus settings to the recommended defaults?", (GenericChoiceDecision decision) => 
						{
							if (decision == GenericChoiceDecision.Accept) {
								ResetPreferences();
							}
						})
						.AddSpacer()
						.AddSpacer();
					manager.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);
					manager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
				} catch { }
			}
		}

		public T Get<T>(string key) {
			if (PrefManager != null) {
				return ((PreferenceSystemManager) PrefManager).Get<T>(key);
			}

			return default;
		}

		public bool Set<T>(string key, T value) {
			if (PrefManager != null) {
				((PreferenceSystemManager) PrefManager).Set(key, value);
				return true;
			}

			return false;
		}

		private void ResetPreferences() {
			foreach (var preference in PreferenceManager.DefaultPreferences) {
				Type type = preference.Value.Type;

				if (type == typeof(bool)) {
					Set(preference.Key, (bool) preference.Value.Value);
				} else if (type == typeof(int)) {
					Set(preference.Key, (int) preference.Value.Value);
				} else if (type == typeof(float)) {
					Set(preference.Key, (float) preference.Value.Value);
				} else if (type == typeof(string)) {
					Set(preference.Key, (string) preference.Value.Value);
				} else {
					throw new ArgumentException($"Default preference with type {type.Name} is not supported!");
				}
			}
		}

		private int[] GetInts(int start, int end, int step) {
			List<int> result = [];

			for (int i = start; i <= end; i += step) {
				result.Add(i);
			}

			return [.. result];
		}

		private string[] GetLabels<T>(T[] list, Func<T, string> func) {
			string[] result = new string[list.Length];

			for (int i = 0; i < list.Length; i++) {
				result[i] = func(list[i]);
			}

			return result;
		}
	}
}
