using Microsoft.VisualStudio.Settings;

namespace KeepingTabs
{
	internal class Settings
	{
		private readonly WritableSettingsStore _settings;

		public Settings(WritableSettingsStore settings)
		{
			_settings = settings;
		}

		private readonly BoolSettingEntry _closePinnedTabs = new BoolSettingEntry("KeepingTabs", "ClosePinnedTabs", false);

		public bool ClosePinnedTabs
		{
			get => _closePinnedTabs.Get(_settings);
			set => _closePinnedTabs.Set(_settings, value);
		}

		private readonly UInt32SettingEntry _tabTimeoutMinutes = new UInt32SettingEntry("KeepingTabs", "TimeoutMinutes", 30);

		public uint TabTimeoutMinutes
		{
			get => _tabTimeoutMinutes.Get(_settings);
			set => _tabTimeoutMinutes.Set(_settings, value);
		}

		private readonly BoolSettingEntry _cleanOnBuild = new BoolSettingEntry("KeepingTabs", "CleanOnBuild", true);

		public bool CleanOnBuild
		{
			get => _cleanOnBuild.Get(_settings);
			set => _cleanOnBuild.Set(_settings, value);
		}

		private readonly UInt32SettingEntry _userAbsentTimeoutMinutes = new UInt32SettingEntry("KeepingTabs", "UserAbsentTimeoutMinutes", 5);

		public uint UserAbsentTimeoutMinutes
		{
			get => _userAbsentTimeoutMinutes.Get(_settings);
			set => _userAbsentTimeoutMinutes.Set(_settings, value);
		}

		private readonly BoolSettingEntry _trackUserInactivity = new BoolSettingEntry("KeepingTabs", "UserAbsentTimeoutMinutes", true);

		public bool TrackUserInactivity
		{
			get => _trackUserInactivity.Get(_settings);
			set => _trackUserInactivity.Set(_settings, value);
		}

		internal abstract class BaseSettingEntry
		{
			protected string _collectionPath;
			protected string _propertyName;

			protected BaseSettingEntry(string collectionPath, string propertyName)
			{
				_collectionPath = collectionPath;
				_propertyName = propertyName;
			}
		}

		internal abstract class BaseSettingEntry<T> : BaseSettingEntry
		{
			protected readonly T _defaultValue;

			protected BaseSettingEntry(string collectionPath, string propertyName, T defaultValue)
				: base(collectionPath, propertyName)
			{
				_defaultValue = defaultValue;
			}

			public abstract T Get(WritableSettingsStore settings);

			public void Set(WritableSettingsStore settings, T value)
			{
				settings.CreateCollection(_collectionPath);
				SetValue(settings, value);
			}

			protected abstract void SetValue(WritableSettingsStore settings, T value);
		}

		internal class UInt32SettingEntry : BaseSettingEntry<uint>
		{
			public UInt32SettingEntry(string collectionPath, string propertyName, uint defaultValue)
				: base(collectionPath, propertyName, defaultValue)
			{
			}

			public override uint Get(WritableSettingsStore settings)
				=> settings.GetUInt32(_collectionPath, _propertyName, _defaultValue);

			protected override void SetValue(WritableSettingsStore settings, uint value)
				=> settings.SetUInt32(_collectionPath, _propertyName, value);
		}

		internal class BoolSettingEntry : BaseSettingEntry<bool>
		{
			public BoolSettingEntry(string collectionPath, string propertyName, bool defaultValue)
				: base(collectionPath, propertyName, defaultValue)
			{
			}

			public override bool Get(WritableSettingsStore settings)
				=> settings.GetBoolean(_collectionPath, _propertyName, _defaultValue);

			protected override void SetValue(WritableSettingsStore settings, bool value)
				=> settings.SetBoolean(_collectionPath, _propertyName, value);
		}
	}
}
