using Microsoft.VisualStudio.Settings;

namespace KeepingTabs
{
	internal class SettingsProvider
	{
		internal static Settings Instance
		{
			get;
			private set;
		}

		internal static Settings Initialize(WritableSettingsStore settings)
		{
			return Instance = new Settings(settings);
		}
	}
}
