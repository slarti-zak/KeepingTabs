using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace KeepingTabs
{
	internal class KeepingTabsOptionPage : DialogPage
	{
		[Category("Keep Tabs")]
		[DisplayName("Close Pinned Tabs")]
		[Description("Whether pinned Tabs should be closed on time out. If false pinned Tabs are kept open.")]
		public bool ClosePinnedTabs
		{
			get => Settings.ClosePinnedTabs;
			set => Settings.ClosePinnedTabs = value;
		}

		[Category("Keep Tabs")]
		[DisplayName("Timeout in Minutes")]
		[Description("After Tab was not used for this amount of time, it is queued for removal")]
		public uint TabTimeoutMinutes
		{
			get => Settings.TabTimeoutMinutes;
			set => Settings.TabTimeoutMinutes = value;
		}

		[Category("Keep Tabs")]
		[DisplayName("Clean on Build")]
		[Description("Will trigger a cleanup when the project is built.")]
		public bool CleanOnBuild
		{
			get => Settings.CleanOnBuild;
			set => Settings.CleanOnBuild = value;
		}

		[Category("User Activity")]
		[DisplayName("Track User Inactivity")]
		[Description("Will track when the user is absent. Will not expire tabs during that time when activated. Having Visual Studio in the Background or being absent for a time will count as inactive.")]
		public bool TrackUserInactivity
		{
			get => Settings.TrackUserInactivity;
			set => Settings.TrackUserInactivity = value;
		}

		[Category("User Activity")]
		[DisplayName("User Activity Timeout in Minutes")]
		[Description("The user is marked as inactive if he has not done an action after this amount of time. Needs \"Track User Inactivity\" to be enabled.")]
		public uint UserAbsentTimeoutMinutes
		{
			get => Settings.UserAbsentTimeoutMinutes;
			set => Settings.UserAbsentTimeoutMinutes = value;
		}

		private Settings settings;
		private Settings Settings => settings ?? (settings = SettingsProvider.Instance);
	}
}
