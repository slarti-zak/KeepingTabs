using System;
using System.Collections.Concurrent;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace KeepingTabs
{
	internal class DocumentManager
	{
		private readonly ConcurrentDictionary<Document, DateTime> _lastSeenMap = new ConcurrentDictionary<Document, DateTime>();

		private readonly IServiceProvider _serviceProvider;
		private readonly DTE2 _visualStudio;
		private readonly Settings _settings;

		private DateTime _lastUserAction = DateTime.Now;

		public DocumentManager(IServiceProvider serviceProvider, DTE2 visualStudio, Settings settings)
		{
			_serviceProvider = serviceProvider;
			_visualStudio = visualStudio;
			_settings = settings;
		}

		internal void SetLastSeen(Document document)
		{
			if (document != null)
			{
				_lastSeenMap[document] = DateTime.Now;
			}
		}

		internal void CleanOldTabs()
		{
			var now = DateTime.Now;
			var timeout = TimeSpan.FromMinutes(_settings.TabTimeoutMinutes);
			var mayClosePinnedTabs = _settings.ClosePinnedTabs;

			var activeDocument = _visualStudio.ActiveDocument;
			foreach (var entry in _lastSeenMap)
			{
				var value = entry.Value;
				var document = entry.Key;

				var closeIfPinned = mayClosePinnedTabs || !IsPinned(document);

				if (activeDocument != document
					&& document.Saved
					&& closeIfPinned
					&& IsTooOld(now, value, timeout))
				{
					CloseDocument(document);
				}
			}
		}

		private bool IsPinned(Document document)
		{
			if (VsShellUtilities.IsDocumentOpen(_serviceProvider, document.FullName, VSConstants.LOGVIEWID_Primary, out var hierarchy, out var itemId, out var frame))
			{
				ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID5.VSFPROPID_IsPinned, out var propVal));

				if (bool.TryParse(propVal.ToString(), out var isPinned))
				{
					return isPinned;
				}
			}

			return false;
		}

		private void CloseDocument(Document document)
		{
			if (_lastSeenMap.TryRemove(document, out _))
			{
				document.Close();
			}
		}

		private bool IsTooOld(DateTime now, DateTime value, TimeSpan timeout)
		{
			return (now - value) > timeout;
		}

		internal void Clear()
		{
			_lastSeenMap.Clear();
		}

		internal void UpdateUserActivity()
		{
			var now = DateTime.Now;
			TrackUserInactivity(now);
			_lastUserAction = now;
		}

		private void TrackUserInactivity(DateTime now)
		{
			if (_settings.TrackUserInactivity)
			{
				var delta = now - _lastUserAction;
				if (UserWasAbsent(delta))
				{
					var entries = _lastSeenMap.ToArray();
					foreach (var entry in entries)
					{
						_lastSeenMap[entry.Key] = entry.Value + delta;
					}
				}
			}
		}

		private bool UserWasAbsent(TimeSpan timespan)
		{
			return timespan > TimeSpan.FromMinutes(_settings.UserAbsentTimeoutMinutes);
		}
	}
}
