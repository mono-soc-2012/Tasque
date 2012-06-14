/***************************************************************************
 *  Application.cs
 *
 *  Copyright (C) 2008 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Sockets;

using Gtk;
using Gdk;
using Mono.Unix;
using Mono.Unix.Native;
#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif
using Tasque.Backends;

namespace Tasque
{
	class Program
	{
		private static Application application;
#if !WIN32 && !OSX
		private RemoteControl remoteControl;
#endif
			
		private Preferences preferences;
		private IBackend backend;
		private TaskGroupModel overdue_tasks, today_tasks, tomorrow_tasks;
		private PreferencesDialog preferencesDialog;
		private bool quietStart = false;
		
		private DateTime currentDay = DateTime.Today;
		
		/// <value>
		/// Keep track of the available backends.  The key is the Type name of
		/// the backend.
		/// </value>
		private Dictionary<string, IBackend> availableBackends;
		
		private IBackend customBackend;


		public static IBackend Backend
		{ 
			get { return Program.Instance.backend; }
			set { Program.Instance.SetBackend (value); }
		}
		
		public static List<IBackend> AvailableBackends
		{
			get {
				return new List<IBackend> (Program.Instance.availableBackends.Values);
			}
//			get { return Application.Instance.availableBackends; }
		}
		
		public static Program Instance
		{
			get {

					if(application == null) {
						
							application = new Program();

					}
					return application;
			}
		}

		public static Application Application
		{
			get
			{
				return application;
			}
		}

		public static Preferences Preferences
		{
			get { return Program.Instance.preferences; }
		}

		static void Init(string[] args)
		{
#if OSX
			application = new OSXApplication ();
#elif WIN32 || GTKLINUX
			application = new GtkApplication ();
#else
			application = new GnomeApplication ();
#endif
			application.Initialize (args);
			
//			RegisterUIManager ();

			preferences = new Preferences (application.ConfDir);
			
#if !WIN32 && !OSX
			// Register Tasque RemoteControl
			try {
				remoteControl = RemoteControlProxy.Register ();
				if (remoteControl != null) {
					Logger.Debug ("Tasque remote control active.");
				} else {
					// If Tasque is already running, open the tasks window
					// so the user gets some sort of feedback when they
					// attempt to run Tasque again.
					RemoteControl remote = null;
					try {
						remote = RemoteControlProxy.GetInstance ();
						remote.ShowTasks ();
					} catch {}

					Logger.Debug ("Tasque is already running.  Exiting...");
					System.Environment.Exit (0);
				}
			} catch (Exception e) {
				Logger.Debug ("Tasque remote control disabled (DBus exception): {0}",
				            e.Message);
			}
#endif
			
			string potentialBackendClassName = null;
			
			for (int i = 0; i < args.Length; i++) {
				switch (args [i]) {
					
				case "--quiet":
					quietStart = true;
					Logger.Debug ("Starting quietly");
					break;
					
				case "--backend":
					if ( (i + 1 < args.Length) &&
					    !string.IsNullOrEmpty (args [i + 1]) &&
					    args [i + 1] [0] != '-') {
						potentialBackendClassName = args [++i];
					} // TODO: Else, print usage
					break;
					
				default:
					// Support old argument behavior
					if (!string.IsNullOrEmpty (args [i]))
						potentialBackendClassName = args [i];
					break;
				}
			}
			
			// See if a specific backend is specified
			if (potentialBackendClassName != null) {
				Logger.Debug ("Backend specified: " +
				              potentialBackendClassName);
				
				customBackend = null;
				Assembly asm = Assembly.GetCallingAssembly ();
				try {
					customBackend = (IBackend)
						asm.CreateInstance (potentialBackendClassName);
				} catch (Exception e) {
					Logger.Warn ("Backend specified on args not found: {0}\n\t{1}",
						potentialBackendClassName, e.Message);
				}
			}
			
			// Discover all available backends
			LoadAvailableBackends ();

			GLib.Idle.Add(InitializeIdle);
			GLib.Timeout.Add (60000, CheckForDaySwitch);
		}
		
		/// <summary>
		/// Load all the available backends that Tasque can find.  First look in
		/// Tasque.exe and then for other DLLs in the same directory Tasque.ex
		/// resides.
		/// </summary>
		private void LoadAvailableBackends ()
		{
			availableBackends = new Dictionary<string,IBackend> ();
			
			List<IBackend> backends = new List<IBackend> ();
			
			Assembly tasqueAssembly = Assembly.GetCallingAssembly ();
			
			// Look for other backends in Tasque.exe
			backends.AddRange (GetBackendsFromAssembly (tasqueAssembly));
			
			// Look through the assemblies located in the same directory as
			// Tasque.exe.
			Logger.Debug ("Tasque.exe location:  {0}", tasqueAssembly.Location);
			
			DirectoryInfo loadPathInfo =
				Directory.GetParent (tasqueAssembly.Location);
			Logger.Info ("Searching for Backend DLLs in: {0}", loadPathInfo.FullName);
			
			foreach (FileInfo fileInfo in loadPathInfo.GetFiles ("*.dll")) {
				Logger.Info ("\tReading {0}", fileInfo.FullName);
				Assembly asm = null;
				try {
					asm = Assembly.LoadFile (fileInfo.FullName);
				} catch (Exception e) {
					Logger.Debug ("Exception loading {0}: {1}",
								  fileInfo.FullName,
								  e.Message);
					continue;
				}
				
				backends.AddRange (GetBackendsFromAssembly (asm));
			}
			
			foreach (IBackend backend in backends) {
				string typeId = backend.GetType ().ToString ();
				if (availableBackends.ContainsKey (typeId))
					continue;
				
				Logger.Debug ("Storing '{0}' = '{1}'", typeId, backend.Name);
				availableBackends [typeId] = backend;
			}
		}
		
		private List<IBackend> GetBackendsFromAssembly (Assembly asm)
		{
			List<IBackend> backends = new List<IBackend> ();
			
			Type[] types = null;
			
			try {
				types = asm.GetTypes ();
			} catch (Exception e) {
				Logger.Warn ("Exception reading types from assembly '{0}': {1}",
					asm.ToString (), e.Message);
				return backends;
			}
			foreach (Type type in types) {
				if (!type.IsClass) {
					continue; // Skip non-class types
				}
				if (type.GetInterface ("Tasque.Backends.IBackend") == null) {
					continue;
				}
				Logger.Debug ("Found Available Backend: {0}", type.ToString ());
				
				IBackend availableBackend = null;
				try {
					availableBackend = (IBackend)
						asm.CreateInstance (type.ToString ());
				} catch (Exception e) {
					Logger.Warn ("Could not instantiate {0}: {1}",
								 type.ToString (),
								 e.Message);
					continue;
				}
				
				if (availableBackend != null) {
					backends.Add (availableBackend);
				}
			}
			
			return backends;
		}

		private void SetBackend (IBackend value)
		{
			bool changingBackend = false;
			if (this.backend != null) {
				UnhookFromTooltipTaskGroupModels ();
				changingBackend = true;
				// Cleanup the old backend
				try {
					Logger.Debug ("Cleaning up backend: {0}",
					              this.backend.Name);
					this.backend.Cleanup ();
				} catch (Exception e) {
					Logger.Warn ("Exception cleaning up '{0}': {1}",
					             this.backend.Name,
					             e);
				}
			}
				
			// Initialize the new backend
			this.backend = value;
			if (this.backend == null) {
//				RefreshTrayIconTooltip ();
				return;
			}
				
			Logger.Info ("Using backend: {0} ({1})",
			             this.backend.Name,
			             this.backend.GetType ().ToString ());
			this.backend.Initialize();
			
			if (!changingBackend) {
				TaskWindow.Reinitialize (!this.quietStart);
			} else {
				TaskWindow.Reinitialize (true);
			}

			RebuildTooltipTaskGroupModels ();
//			RefreshTrayIconTooltip ();
			
			Logger.Debug("Configuration status: {0}",
			             this.backend.Configured.ToString());
		}

		private bool InitializeIdle()
		{
			if (customBackend != null) {
				Program.Backend = customBackend;
			} else {
				// Check to see if the user has a preference of which backend
				// to use.  If so, use it, otherwise, pop open the preferences
				// dialog so they can choose one.
				string backendTypeString = Preferences.Get (Preferences.CurrentBackend);
				Logger.Debug ("CurrentBackend specified in Preferences: {0}", backendTypeString);
				if (backendTypeString != null
						&& availableBackends.ContainsKey (backendTypeString)) {
					Program.Backend = availableBackends [backendTypeString];
				}
			}
			
//			SetupTrayIcon ();
			
			if (backend == null) {
				// Pop open the preferences dialog so the user can choose a
				// backend service to use.
				Program.ShowPreferences ();
			} else if (!quietStart) {
				TaskWindow.ShowWindow ();
			}
			if (backend == null || !backend.Configured){
				GLib.Timeout.Add(1000, new GLib.TimeoutHandler(RetryBackend));
			}

			application.InitializeIdle ();
			
			return false;
		}
		private bool RetryBackend(){
			try {
				if (backend != null && !backend.Configured) {
					backend.Cleanup();
					backend.Initialize();
				}
			} catch (Exception e) {
				Logger.Error("{0}", e.Message);
			}
			if (backend == null || !backend.Configured) {
				return true;
			} else {
				return false;
			}
		}
		
		private bool CheckForDaySwitch ()
		{
			if (DateTime.Today != currentDay) {
				Logger.Debug ("Day has changed, reloading tasks");
				currentDay = DateTime.Today;
				// Reinitialize window according to new date
				if (TaskWindow.IsOpen)
					TaskWindow.Reinitialize (true);
				
				UnhookFromTooltipTaskGroupModels ();
				RebuildTooltipTaskGroupModels ();
//				RefreshTrayIconTooltip ();
			}
			
			return true;
		}



		private void UnhookFromTooltipTaskGroupModels ()
		{
			foreach (TaskGroupModel model in new TaskGroupModel[] { overdue_tasks, today_tasks, tomorrow_tasks })
			{
				if (model == null) {
					continue;
				}
				
				model.RowInserted -= OnTooltipModelChanged;
				model.RowChanged -= OnTooltipModelChanged;
				model.RowDeleted -= OnTooltipModelChanged;
			}
		}

		private void OnTooltipModelChanged (object o, EventArgs args)
		{
//			RefreshTrayIconTooltip ();
		}

		private void RebuildTooltipTaskGroupModels ()
		{
			if (backend == null || backend.Tasks == null) {
				overdue_tasks = null;
				today_tasks = null;
				tomorrow_tasks = null;
				
				return;
			}

			overdue_tasks = TaskGroupModelFactory.CreateOverdueModel (backend.Tasks);
			today_tasks = TaskGroupModelFactory.CreateTodayModel (backend.Tasks);
			tomorrow_tasks = TaskGroupModelFactory.CreateTomorrowModel (backend.Tasks);

			foreach (TaskGroupModel model in new TaskGroupModel[] { overdue_tasks, today_tasks, tomorrow_tasks })
			{
				if (model == null) {
					continue;
				}
				
				model.RowInserted += OnTooltipModelChanged;
				model.RowChanged += OnTooltipModelChanged;
				model.RowDeleted += OnTooltipModelChanged;
			}
		}

		private void OnPreferences (object sender, EventArgs args)
		{
			Logger.Info ("OnPreferences called");
			if (preferencesDialog == null) {
				preferencesDialog = new PreferencesDialog ();
				preferencesDialog.Hidden += OnPreferencesDialogHidden;
			}
			
			preferencesDialog.Present ();
		}
		
		private void OnPreferencesDialogHidden (object sender, EventArgs args)
		{
			preferencesDialog.Destroy ();
			preferencesDialog = null;
		}
		
		public static void ShowPreferences()
		{
			application.OnPreferences(null, EventArgs.Empty);
		}

		private void OnShowTaskWindow (object sender, EventArgs args)
		{
			TaskWindow.ShowWindow();
		}
		
		private void OnNewTask (object sender, EventArgs args)
		{
			// Show the TaskWindow and then cause a new task to be created
			TaskWindow.ShowWindow ();
			TaskWindow.GrabNewTaskEntryFocus ();
		}

		private void OnQuit (object sender, EventArgs args)
		{


			application.QuitMainLoop ();
		}
		
		private void OnRefreshAction (object sender, EventArgs args)
		{
			Program.Backend.Refresh();
		}

		public static void Main(string[] args)
		{
			try 
			{
				Init (args);
				application.StartMainLoop ();
			} 
			catch (Exception e)
			{
				Tasque.Logger.Debug("Exception is: {0}", e);
				Instance.NativeApplication.Exit (-1);
			}
		}

#if ENABLE_NOTIFY_SHARP
		public static void ShowAppNotification(Notification notification)
		{
			// TODO: Use this API for newer versions of notify-sharp
			//notification.AttachToStatusIcon(
			//		Tasque.Application.Instance.trayIcon);
			notification.Show();
		}
#endif

		public void StartMainLoop ()
		{
			application.StartMainLoop ();
		}

		public void Quit ()
		{
			OnQuit (null, null);
		}
	}
}
