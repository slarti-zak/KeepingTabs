using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace KeepingTabs
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class CleanTabsCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("9bbfe385-f901-4ceb-8660-ea10ae6db8ec");

		private readonly DocumentManager _documentManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="CleanTabsCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="documentManager"></param>
		private CleanTabsCommand(DocumentManager documentManager)
		{
			_documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
		}

		public static void Initialize(DocumentManager documentManager, OleMenuCommandService commandService)
		{
			var command = new CleanTabsCommand(documentManager);

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(command.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event args.</param>
		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			_documentManager.CleanOldTabs();
		}
	}
}
