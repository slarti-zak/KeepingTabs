using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using TasksTask = System.Threading.Tasks.Task;

namespace KeepingTabs
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(CleanTabsCommandPackage.PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideOptionPage(typeof(KeepingTabsOptionPage), "Keeping Tabs", "Options", 1000, 1001, false)]
	public sealed class CleanTabsCommandPackage : AsyncPackage
	{
		/// <summary>
		/// CleanTabsCommandPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "e4f66d9c-6e89-4258-b8bb-c18373ef7746";
		private Settings _settings;
		private DocumentManager _documentManager;

		#region Events

		private DTE _dte;
		private DTE2 _visualStudio;
		private WindowEvents _windowEvents;
		private DocumentEvents _documentEvents;
		private TextEditorEvents _textEditorEvents;
		private SolutionEvents _solutionEvents;
		private BuildEvents _buildEvents;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="CleanTabsCommand"/> class.
		/// </summary>
		public CleanTabsCommandPackage()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
		/// <param name="progress">A provider for progress updates.</param>
		/// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
		protected override async TasksTask InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress).ConfigureAwait(false);

			try
			{
				var commandService = await GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(false) as OleMenuCommandService;
				_dte = (DTE)await GetServiceAsync(typeof(DTE)).ConfigureAwait(false);
				_visualStudio = await GetServiceAsync(typeof(SDTE)).ConfigureAwait(false) as DTE2;

				// When initialized asynchronously, the current thread may be a background thread at this point.
				// Do any initialization that requires the UI thread after switching to the UI thread.
				await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				_settings = SettingsProvider.Initialize(GetWritableSettingsStore(this));

				_documentManager = new DocumentManager(this, _visualStudio, _settings);
				CleanTabsCommand.Initialize(_documentManager, commandService);


				_windowEvents = _dte.Events.WindowEvents;
				_documentEvents = _dte.Events.DocumentEvents;
				_textEditorEvents = _dte.Events.TextEditorEvents;
				_solutionEvents = _dte.Events.SolutionEvents;
				_buildEvents = _dte.Events.BuildEvents;

				_documentEvents.DocumentOpened += OnDocumentEvent;
				_documentEvents.DocumentSaved += OnDocumentEvent;
				_windowEvents.WindowActivated += OnWindowActivated;
				_textEditorEvents.LineChanged += OnLineChanged;
				_solutionEvents.Opened += OnSolutionOpened;
				_solutionEvents.BeforeClosing += OnSolutionClosing;
				_buildEvents.OnBuildBegin += OnBuildBegin;

				LoadSolution();
			}
			catch (Exception e)
			{
				ActivityLog.TryLogError("KeepingTabs", e.Message);
				Console.WriteLine(e.ToString());
				throw;
			}
		}

		public static WritableSettingsStore GetWritableSettingsStore(IServiceProvider vsServiceProvider)
		{
			var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
			return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
		}

		private void LoadSolution()
		{
			_documentManager.Clear();
			foreach (var window in _visualStudio.Windows)
			{
				SaveWindowLastSeen(window as Window);
			}
		}

		private void SaveDocumentLastSeen(Document document)
		{
			_documentManager.SetLastSeen(document);
		}

		private void SaveWindowLastSeen(Window window)
		{
			// Ignore tool windows
			if (window == null || window.Linkable)
			{
				return;
			}
			SaveDocumentLastSeen(window.Document);
		}

		#region Events

		private void OnDocumentEvent(Document Document)
		{
			_documentManager.UpdateUserActivity();
			SaveDocumentLastSeen(Document);
		}

		private void OnLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
		{
			_documentManager.UpdateUserActivity();
		}

		private void OnWindowActivated(Window gotFocus, Window lostFocus)
		{
			_documentManager.UpdateUserActivity();
			SaveWindowLastSeen(lostFocus);
			SaveWindowLastSeen(gotFocus);
		}

		private void OnSolutionOpened()
		{
			_documentManager.UpdateUserActivity();
			LoadSolution();
		}

		private void OnSolutionClosing()
		{
			_documentManager.Clear();
		}

		private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
		{
			_documentManager.UpdateUserActivity();
			if (_settings.CleanOnBuild)
			{
				_documentManager.CleanOldTabs();
			}
		}

		#endregion
	}
}
