// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 
//  
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// 
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
// Copyright (c) 2012 Antonius Riha
// 
// Authors: 
//      Sandy Armstrong <sanfordarmstrong@gmail.com>
//      Antonius Riha <antoniusriha@gmail.com>
// 
using System;
using System.Diagnostics;
using Mono.Unix;

#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif

namespace Tasque
{
	public abstract class GtkApplication : Application
	{
		public new static GtkApplication Instance {
			get {
				if (instance == null)
					throw new InvalidOperationException ("No instance has been created yet.");
				return instance;
			}
		}
		
		static GtkApplication instance;
		
		protected GtkApplication ()
		{
			instance = this;
		}
		
		protected override void OnInitialize ()
		{
			Catalog.Init ("tasque", GlobalDefines.LocaleDir);
			Trace.WriteLine ("Locale dir: " + GlobalDefines.LocaleDir);
			
			Gtk.Application.Init ();
			GLib.Idle.Add (delegate {
				InitializeIdle ();
				return false;
			});
			
			GLib.Timeout.Add (60000, delegate {
				CheckForDaySwitch ();
				return true;
			});
			
			base.OnInitialize ();
		}
		
		protected override void OnInitializeIdle ()
		{
			trayIcon = GtkTray.CreateTray ();
			
			if (Backend == null) {
				// Pop open the preferences dialog so the user can choose a
				// backend service to use.
				ShowPreferences ();
			} else if (!QuietStart)
				TaskWindow.ShowWindow ();
			
			if (Backend == null || !Backend.Configured) {
				GLib.Timeout.Add (1000, new GLib.TimeoutHandler (delegate {
					RetryBackend ();
					return Backend == null || !Backend.Configured;
				}));
			}
			
			base.OnInitializeIdle ();
		}
		
		public override void StartMainLoop ()
		{
			Gtk.Application.Run ();
		}
		
		public override void QuitMainLoop ()
		{
			Gtk.Application.Quit ();
		}
		
#if ENABLE_NOTIFY_SHARP
		public void ShowAppNotification (Notification notification)
		{
			// TODO: Use this API for newer versions of notify-sharp
//			notification.AttachToStatusIcon (Tasque.GtkApplication.Instance.Instance.trayIcon);
			notification.Show ();
		}
#endif
		
		public void ShowPreferences ()
		{
			Trace.TraceInformation ("OnPreferences called");
			if (preferencesDialog == null) {
				preferencesDialog = new PreferencesDialog ();
				preferencesDialog.Hidden += OnPreferencesDialogHidden;
			}
			
			preferencesDialog.Present ();
		}
		
		protected override void ShowMainWindow ()
		{
			TaskWindow.ShowWindow ();
		}
		
		protected override void OnBackendChanged ()
		{
			if (backendWasNullBeforeChange)
				TaskWindow.Reinitialize (!QuietStart);
			else
				TaskWindow.Reinitialize (true);
			
			Debug.WriteLine ("Configuration status: {0}", Backend.Configured.ToString ());
			
			RebuildTooltipTaskGroupModels ();
			if (trayIcon != null)
				trayIcon.RefreshTrayIconTooltip ();
			
			base.OnBackendChanged ();
		}
		
		protected override void OnBackendChanging ()
		{
			if (Backend != null)
				UnhookFromTooltipTaskGroupModels ();
			
			backendWasNullBeforeChange = Backend == null;
			
			base.OnBackendChanging ();
		}
		
		protected override void OnDaySwitched ()
		{
			// Reinitialize window according to new date
			if (TaskWindow.IsOpen)
				TaskWindow.Reinitialize (true);
				
			UnhookFromTooltipTaskGroupModels ();
			RebuildTooltipTaskGroupModels ();
			if (trayIcon != null)
				trayIcon.RefreshTrayIconTooltip ();
		}
		
		protected override void OnQuitting ()
		{
			if (Backend != null)
				UnhookFromTooltipTaskGroupModels ();
			
			TaskWindow.SavePosition ();
			
			base.OnQuitting ();
		}
		
		protected void OnRemoteInstanceKnocked ()
		{
			if (RemoteInstanceKnocked != null)
				RemoteInstanceKnocked (this, EventArgs.Empty);
		}
		
		protected override event EventHandler RemoteInstanceKnocked;
		
		void OnPreferencesDialogHidden (object sender, EventArgs args)
		{
			preferencesDialog.Destroy ();
			preferencesDialog.Hidden -= OnPreferencesDialogHidden;
			preferencesDialog = null;
		}
		
		void OnTooltipModelChanged (object sender, EventArgs args)
		{
			if (trayIcon != null)
				trayIcon.RefreshTrayIconTooltip ();
		}
		
		void RebuildTooltipTaskGroupModels ()
		{
			if (Backend == null || Backend.Tasks == null) {
				OverdueTasks = null;
				TodayTasks = null;
				TomorrowTasks = null;
				return;
			}

			OverdueTasks = TaskGroupModelFactory.CreateOverdueModel (Backend.Tasks);
			TodayTasks = TaskGroupModelFactory.CreateTodayModel (Backend.Tasks);
			TomorrowTasks = TaskGroupModelFactory.CreateTomorrowModel (Backend.Tasks);

			foreach (var model in new TaskGroupModel [] { OverdueTasks, TodayTasks, TomorrowTasks })
			{
				if (model != null)
					model.CollectionChanged += OnTooltipModelChanged;
			}
		}
		
		void UnhookFromTooltipTaskGroupModels ()
		{
			foreach (var model in new TaskGroupModel[] { OverdueTasks, TodayTasks, TomorrowTasks })
			{
				if (model == null)
					model.CollectionChanged -= OnTooltipModelChanged;
			}
		}
		
		bool backendWasNullBeforeChange;
		PreferencesDialog preferencesDialog;
		GtkTray trayIcon;
	}
}
