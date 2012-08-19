/***************************************************************************
 *  TargetWindow.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Copyright (C) 2012 Antonius Riha
 *  Written by:
 *		Calvin Gaisford <calvinrg@gmail.com>
 *		Boyd Timothy <btimothy@gmail.com>
 *		Antonius Riha <antoniusriha@gmail.com>
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Mono.Unix;
using Gtk;
using CollectionTransforms;

namespace Tasque
{
	public class TaskWindow : Gtk.Window 
	{
		private static TaskWindow taskWindow = null;
		private static int lastXPos;
		private static int lastYPos;
		private static Gdk.Pixbuf noteIcon;
		
		private Backend backend;
		private ScrolledWindow scrolledWindow;
		
		private Entry addTaskEntry;
		private MenuToolButton addTaskButton;
		private Gtk.ComboBox categoryComboBox;
		private Gtk.VBox targetVBox;
		
		private TaskGroup overdueGroup;
		private TaskGroup todayGroup;
		private TaskGroup tomorrowGroup;
		private TaskGroup nextSevenDaysGroup;
		private TaskGroup futureGroup;
		private CompletedTaskGroup completedTaskGroup;
		private EventBox innerEb;

		private List<TaskGroup> taskGroups;
		
		private Dictionary<Task, NoteDialog> noteDialogs;
		
		private Gtk.Statusbar statusbar;
		private uint statusContext;
		private uint currentStatusMessageId;
		private static uint ShowOriginalStatusId;
		private static string status;
		private static string lastLoadedTime;
		private const uint DWELL_TIME_MS = 8000;
		
		private Task clickedTask;
		
		private Gtk.AccelGroup accelGroup;
		private GlobalKeybinder globalKeys;
		
		static TaskWindow ()
		{
			noteIcon = Utilities.GetIcon ("hicolor_animations_16x16_notebook", 16);
		}
		
		public TaskWindow (Backend aBackend) : base (Gtk.WindowType.Toplevel)
		{
			this.backend = aBackend;
			taskGroups = new List<TaskGroup> ();
			noteDialogs = new Dictionary<Task, NoteDialog> ();
			InitWindow();
			
			Realized += OnRealized;
		}

		void InitWindow()
		{
			int height;
			int width;
			
			this.Icon = Utilities.GetIcon ("tasque-48", 48);
			// Update the window title
			Title = string.Format ("Tasque");	

			width = GtkApplication.Instance.Preferences.GetInt("MainWindowWidth");
			height = GtkApplication.Instance.Preferences.GetInt("MainWindowHeight");
			
			if(width == -1)
				width = 600;
			if(height == -1)
				height = 600;
			
			this.DefaultSize = new Gdk.Size( width, height);
			
			accelGroup = new AccelGroup ();
			AddAccelGroup (accelGroup);
			globalKeys = new GlobalKeybinder (accelGroup);

			VBox mainVBox = new VBox();
			mainVBox.BorderWidth = 0;
			mainVBox.Show ();
			this.Add (mainVBox);
			
			HBox topHBox = new HBox (false, 0);
			topHBox.BorderWidth = 4;
			
			categoryComboBox = new ComboBox ();
			categoryComboBox.Accessible.Description = "Category Selection";
			categoryComboBox.WidthRequest = 150;
			categoryComboBox.WrapWidth = 1;
			categoryComboBox.Sensitive = false;
			CellRendererText comboBoxRenderer = new Gtk.CellRendererText ();
			comboBoxRenderer.WidthChars = 20;
			comboBoxRenderer.Ellipsize = Pango.EllipsizeMode.End;
			categoryComboBox.PackStart (comboBoxRenderer, true);
			categoryComboBox.SetCellDataFunc (comboBoxRenderer,
				new Gtk.CellLayoutDataFunc (CategoryComboBoxDataFunc));
			
			categoryComboBox.Show ();
			topHBox.PackStart (categoryComboBox, false, false, 0);
			
			// Space the addTaskButton and the categoryComboBox
			// far apart by using a blank label that expands
			Label spacer = new Label (string.Empty);
			spacer.Show ();
			topHBox.PackStart (spacer, true, true, 0);
			
			// The new task entry widget
			addTaskEntry = new Entry (Catalog.GetString ("New task..."));
			addTaskEntry.Sensitive = false;
			addTaskEntry.Focused += OnAddTaskEntryFocused;
			addTaskEntry.Changed += OnAddTaskEntryChanged;
			addTaskEntry.Activated += OnAddTaskEntryActivated;
			addTaskEntry.FocusInEvent += OnAddTaskEntryFocused;
			addTaskEntry.FocusOutEvent += OnAddTaskEntryUnfocused;
			addTaskEntry.DragDataReceived += OnAddTaskEntryDragDataReceived;
			addTaskEntry.Show ();
			topHBox.PackStart (addTaskEntry, true, true, 0);
			
			// Use a small add icon so the button isn't mammoth-sized
			HBox buttonHBox = new HBox (false, 6);
			Gtk.Image addImage = new Gtk.Image (Gtk.Stock.Add, IconSize.Menu);
			addImage.Show ();
			buttonHBox.PackStart (addImage, false, false, 0);
			Label l = new Label (Catalog.GetString ("_Add"));
			l.Show ();
			buttonHBox.PackStart (l, true, true, 0);
			buttonHBox.Show ();
			addTaskButton = 
				new MenuToolButton (buttonHBox, Catalog.GetString ("_Add Task"));
			addTaskButton.UseUnderline = true;
			// Disactivate the button until the backend is initialized
			addTaskButton.Sensitive = false;
			Gtk.Menu addTaskMenu = new Gtk.Menu ();
			addTaskButton.Menu = addTaskMenu;
			addTaskButton.Clicked += OnAddTask;
			addTaskButton.Show ();
			topHBox.PackStart (addTaskButton, false, false, 0);
			
			globalKeys.AddAccelerator (OnGrabEntryFocus,
						(uint) Gdk.Key.n,
						Gdk.ModifierType.ControlMask,
						Gtk.AccelFlags.Visible);
			
			globalKeys.AddAccelerator (delegate (object sender, EventArgs e) {
				GtkApplication.Instance.Quit (); },
						(uint) Gdk.Key.q,
						Gdk.ModifierType.ControlMask,
						Gtk.AccelFlags.Visible);

			this.KeyPressEvent += KeyPressed;
			
			topHBox.Show ();
			mainVBox.PackStart (topHBox, false, false, 0);

			scrolledWindow = new ScrolledWindow ();
			scrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.HscrollbarPolicy = PolicyType.Never;

			scrolledWindow.BorderWidth = 0;
			scrolledWindow.CanFocus = true;
			scrolledWindow.Show ();
			mainVBox.PackStart (scrolledWindow, true, true, 0);

			innerEb = new EventBox();
			innerEb.BorderWidth = 0;
			Gdk.Color backgroundColor = GetBackgroundColor ();
			innerEb.ModifyBg (StateType.Normal, backgroundColor);
			innerEb.ModifyBase (StateType.Normal, backgroundColor);
			
			targetVBox = new VBox();
			targetVBox.BorderWidth = 5;
			targetVBox.Show ();
			innerEb.Add(targetVBox);

			scrolledWindow.AddWithViewport(innerEb);
			
			statusbar = new Gtk.Statusbar ();
			statusbar.HasResizeGrip = true;
			statusbar.Show ();

			mainVBox.PackEnd (statusbar, false, false, 0);
			
			//
			// Delay adding in the TaskGroups until the backend is initialized
			//
			
			Shown += OnWindowShown;
			DeleteEvent += WindowDeleted;
			
			backend.BackendInitialized += OnBackendInitialized;
			backend.BackendSyncStarted += OnBackendSyncStarted;
			backend.BackendSyncFinished += OnBackendSyncFinished;
			// if the backend is already initialized, go ahead... initialize
			if(backend.Initialized) {
				OnBackendInitialized(null, null);
			}
			
			GtkApplication.Instance.Preferences.SettingChanged += OnSettingChanged;
		}

		void PopulateWindow ()
		{
			// Add in the groups
			
			//
			// Overdue Group
			//
			DateTime rangeStart;
			DateTime rangeEnd;
			
			rangeStart = DateTime.MinValue;
			rangeEnd = DateTime.Now.AddDays (-1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month, rangeEnd.Day,
									 23, 59, 59);
			
			overdueGroup = new TaskGroup (Catalog.GetString ("Overdue"),
										  rangeStart, rangeEnd,
										  backend.Tasks);
			overdueGroup.RowActivated += OnRowActivated;
			overdueGroup.ButtonPressed += OnButtonPressed;
			overdueGroup.Show ();
			targetVBox.PackStart (overdueGroup, false, false, 0);
			taskGroups.Add (overdueGroup);
			
			//
			// Today Group
			//
			rangeStart = DateTime.Now;
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now;
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			todayGroup = new TaskGroup (Catalog.GetString ("Today"),
										rangeStart, rangeEnd,
										backend.Tasks);
			todayGroup.RowActivated += OnRowActivated;
			todayGroup.ButtonPressed += OnButtonPressed;
			todayGroup.Show ();
			targetVBox.PackStart (todayGroup, false, false, 0);
			taskGroups.Add (todayGroup);
			
			//
			// Tomorrow Group
			//
			rangeStart = DateTime.Now.AddDays (1);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now.AddDays (1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			tomorrowGroup = new TaskGroup (Catalog.GetString ("Tomorrow"),
										   rangeStart, rangeEnd,
										   backend.Tasks);
			tomorrowGroup.RowActivated += OnRowActivated;
			tomorrowGroup.ButtonPressed += OnButtonPressed;			
			tomorrowGroup.Show ();
			targetVBox.PackStart (tomorrowGroup, false, false, 0);
			taskGroups.Add (tomorrowGroup);
			
			//
			// Next 7 Days Group
			//
			rangeStart = DateTime.Now.AddDays (2);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now.AddDays (6);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			nextSevenDaysGroup = new TaskGroup (Catalog.GetString ("Next 7 Days"),
										   rangeStart, rangeEnd,
										   backend.Tasks);
			nextSevenDaysGroup.RowActivated += OnRowActivated;
			nextSevenDaysGroup.ButtonPressed += OnButtonPressed;				
			nextSevenDaysGroup.Show ();
			targetVBox.PackStart (nextSevenDaysGroup, false, false, 0);
			taskGroups.Add (nextSevenDaysGroup);
			
			//
			// Future Group
			//
			rangeStart = DateTime.Now.AddDays (7);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.MaxValue;
			futureGroup = new TaskGroup (Catalog.GetString ("Future"),
										 rangeStart, rangeEnd,
										 backend.Tasks);
			futureGroup.RowActivated += OnRowActivated;
			futureGroup.ButtonPressed += OnButtonPressed;			
			futureGroup.Show ();
			targetVBox.PackStart (futureGroup, false, false, 0);
			taskGroups.Add (futureGroup);
			
			//
			// Completed Tasks Group
			//
			rangeStart = DateTime.MinValue;
			rangeEnd = DateTime.MaxValue;
			completedTaskGroup = new CompletedTaskGroup (
					Catalog.GetString ("Completed"),
					rangeStart, rangeEnd,
					backend.Tasks);
			completedTaskGroup.RowActivated += OnRowActivated;
			completedTaskGroup.ButtonPressed += OnButtonPressed;
			completedTaskGroup.Show ();
			targetVBox.PackStart (completedTaskGroup, false, false, 0);
			taskGroups.Add (completedTaskGroup);
			

			//manualTarget = new TargetService();
			//manualTarget.Show ();
			//mainVBox.PackStart(manualTarget, false, false, 0);
			
			
			// Set up the combo box (after the above to set the current filter)

			var categoryComboStore = new ListStore (typeof(string));
			categoryComboStore.AppendValues (Catalog.GetString ("All"));
			foreach (var item in GtkApplication.Instance.Backend.Categories) {
				categoryComboStore.AppendValues (item.Name);
				item.CollectionChanged += HandleCategoryTaskCollectionChanged;
			}
			
			categoryComboBox.Model = categoryComboStore;	

			// Read preferences for the last-selected category and select it
			string selectedCategoryName =
				GtkApplication.Instance.Preferences.Get (Preferences.SelectedCategoryKey);
			
			categoryComboBox.Changed += OnCategoryChanged;
			
			SelectCategory (selectedCategoryName);
		}

		void HandleCategoryTaskCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			categoryComboBox.QueueDraw ();
		}
		
		#region Public Methods
		/// <summary>
		/// Method to allow other classes to "click" on the "Add Task" button.
		/// </summary>
		public static void AddTask ()
		{
			if (taskWindow == null)
				TaskWindow.ShowWindow ();
			
			taskWindow.OnAddTask (null, EventArgs.Empty);
		}

		public static void SavePosition()
		{
			if(taskWindow != null) {
				int x;
				int y;
				int width;
				int height;

				taskWindow.GetPosition(out x, out y);
				taskWindow.GetSize(out width, out height);

				lastXPos = x;
				lastYPos = y;
				
				GtkApplication.Instance.Preferences.SetInt("MainWindowLastXPos", lastXPos);
				GtkApplication.Instance.Preferences.SetInt("MainWindowLastYPos", lastYPos);
				GtkApplication.Instance.Preferences.SetInt("MainWindowWidth", width);
				GtkApplication.Instance.Preferences.SetInt("MainWindowHeight", height);	
			}

		}
		
		public static void ShowWindow ()
		{
			ShowWindow (false);
		}
		
		public static void ToggleWindowVisible ()
		{
			ShowWindow (true);
		}
		
		private static void ShowWindow(bool supportToggle)
		{
			if(taskWindow != null) {
				if(taskWindow.IsActive && supportToggle) {
					int x;
					int y;

					taskWindow.GetPosition(out x, out y);

					lastXPos = x;
					lastYPos = y;

					taskWindow.Hide();
				} else {
					if(!taskWindow.Visible) {
						int x = lastXPos;
						int y = lastYPos;

						if (x >= 0 && y >= 0)
							taskWindow.Move(x, y);						
					}
					taskWindow.Present();
				}
			} else if (GtkApplication.Instance.Backend != null) {
				TaskWindow.taskWindow = new TaskWindow(GtkApplication.Instance.Backend);
				if(lastXPos == 0 || lastYPos == 0)
				{
					lastXPos = GtkApplication.Instance.Preferences.GetInt("MainWindowLastXPos");
					lastYPos = GtkApplication.Instance.Preferences.GetInt("MainWindowLastYPos");				
				}

				int x = lastXPos;
				int y = lastYPos;

				if (x >= 0 && y >= 0)
					taskWindow.Move(x, y);						

				taskWindow.ShowAll();
			}
		}
		
		public static void GrabNewTaskEntryFocus ()
		{
			if (taskWindow == null)
				TaskWindow.ShowWindow ();
			
			taskWindow.addTaskEntry.GrabFocus ();
		}
		
		public static void SelectAndEdit (Task task)
		{
			ShowWindow ();
			taskWindow.EnterEditMode (task, true);
			taskWindow.Present ();
		}

		public static bool ShowOriginalStatus ()
		{
			// Translators: This status shows the date and time when the task list was last refreshed
			status = string.Format (Catalog.GetString ("Tasks loaded: {0}"),
			                        TaskWindow.lastLoadedTime);
			TaskWindow.ShowStatus (status);
			return false;
		}
		
		public static void ShowStatus (string statusText)
		{
			// By default show the new status for 8 seconds
			ShowStatus (statusText, DWELL_TIME_MS);
		}

		public static void ShowStatus (string statusText, uint dwellTime)
		{
			if (taskWindow == null) {
				Trace.TraceWarning ("Cannot set status when taskWindow is null");
				return;
			}

			// remove old timer to show original status and then start another one
			if (ShowOriginalStatusId > 0)
				GLib.Source.Remove (ShowOriginalStatusId);
			// any status will dwell for <dwellTime> seconds and then the original
			//status will be shown
			ShowOriginalStatusId = GLib.Timeout.Add (dwellTime, ShowOriginalStatus);
			
			if (taskWindow.currentStatusMessageId != 0) {
				// Pop the old message
				taskWindow.statusbar.Remove (taskWindow.statusContext,
								taskWindow.currentStatusMessageId);
				taskWindow.currentStatusMessageId = 0;
			}
			
			taskWindow.currentStatusMessageId =
				taskWindow.statusbar.Push (taskWindow.statusContext,
								statusText);
		}

		public static bool IsOpen
		{
			get {
				return taskWindow != null && taskWindow.IsRealized;
			}
		}
		
		/// <summary>
		/// This should be called after a new IBackend has been set
		/// </summary>
		public static void Reinitialize (bool show)
		{
			if (TaskWindow.taskWindow != null) {
				TaskWindow.taskWindow.Hide ();
				TaskWindow.taskWindow.Destroy ();
				TaskWindow.taskWindow = null;
			}

			if (show)
				TaskWindow.ShowWindow ();
		}
		
		public void HighlightTask (Task task)
		{
			Gtk.TreeIter iter;
			
			// Make sure we've waited around for the new task to fully
			// be added to the TreeModel before continuing.  Some
			// backends might be threaded and will have used something
			// like Gtk.Idle.Add () to actually store the new Task in
			// their TreeModel.
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			foreach (TaskGroup taskGroup in taskGroups) {
				if (taskGroup.ContainsTask (task, out iter)) {
					taskGroup.TreeView.Selection.SelectIter (iter);
					break;
				}
			}
		}
		
		/// <summary>
		/// Search through the TaskGroups looking for the specified task and
		/// adjust the window so the new task is showing.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		public void ScrollToTask (Task task)
		{
			// TODO: NEED to add something to NOT scroll the window if the new
			// task is already showing in the window!
			
			// Make sure we've waited around for the new task to fully
			// be added to the TreeModel before continuing.  Some
			// backends might be threaded and will have used something
			// like Gtk.Idle.Add () to actually store the new Task in
			// their TreeModel.
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
			
			Gtk.TreeIter iter;
			
			// Make sure we've waited around for the new task to fully
			// be added to the TreeModel before continuing.  Some
			// backends might be threaded and will have used something
			// like Gtk.Idle.Add () to actually store the new Task in
			// their TreeModel.
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			int taskGroupHeights = 0;
			
			foreach (TaskGroup taskGroup in taskGroups) {
				
				//Debug.WriteLine("taskGroupHeights: {0}", taskGroupHeights);
				TreePath start;
				TreePath end;
				if (taskGroup.TreeView.GetVisibleRange (out start, out end)) {
					Debug.WriteLine ("TaskGroup '{0}' range: {1} - {2}",
						taskGroup.DisplayName,
						start.ToString (),
						end.ToString ());
				} else {
					Debug.WriteLine ("TaskGroup range not visible: {0}", taskGroup.DisplayName);
				}
				
				if (taskGroup.ContainsTask (task, out iter)) {
					Debug.WriteLine ("Found new task group: {0}", taskGroup.DisplayName);
					
					// Get the header height
					int headerHeight = taskGroup.HeaderHeight;
				
					// Get the total number items in the TaskGroup
					int nChildren = taskGroup.GetNChildren(iter);
					//Debug.WriteLine("n children: {0}", nChildren);

					// Calculate size of each item
					double itemSize = (double)(taskGroup.Requisition.Height-headerHeight) / nChildren;
					//Debug.WriteLine("item size: {0}", itemSize);
				
					// Get the index of the new item within the TaskGroup
					int newTaskIndex = taskGroup.GetIterIndex (iter);
					//Debug.WriteLine("new task index: {0}", newTaskIndex);
						
					// Calculate the scrolling distance
					double scrollDistance = (itemSize*newTaskIndex)+taskGroupHeights;
					//Debug.WriteLine("Scroll distance = ({0}*{1})+{2}+{3}: {4}", itemSize, newTaskIndex, taskGroupHeights, headerHeight, scrollDistance);
	
					//scroll to the new task
					scrolledWindow.Vadjustment.Value = scrollDistance;
					taskGroup.TreeView.Selection.SelectIter (iter);
				}
				if (taskGroup.Visible) {
					taskGroupHeights += taskGroup.Requisition.Height;
				}
			}
		}
		#endregion // Public Methods
		
		#region Private Methods
		void CategoryComboBoxDataFunc (CellLayout layout, CellRenderer renderer, TreeModel model, TreeIter iter)
		{
			var crt = renderer as CellRendererText;
			var catName = model.GetValue (iter, 0) as string;
			var backend = GtkApplication.Instance.Backend;
			
			int taskCount = 0;
			if (model.GetPath (iter).Indices [0] == TreePath.NewFirst ().Indices [0])
				taskCount = backend.Tasks.Count (t => !t.IsComplete);
			else {
				var cat = backend.Categories.SingleOrDefault (c => c.Name == catName);
				if (cat != null)
					taskCount = cat.Count (t => !t.IsComplete);
				else
					catName = null;
			}
			
			if (catName != null)
				crt.Text = string.Format ("{0} ({1})", catName, taskCount);
			else
				crt.Text = "unknown";
		}
		
		/// <summary>
		/// Search through the TaskGroups looking for the specified task and:
		/// 1) scroll the window to its location, 2) enter directly into edit
		/// mode.  This method should be called right after a new task is
		/// created.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		/// <param name="adjustScrolledWindow">
		/// A <see cref="bool"/> which indicates whether the task should be
		/// scrolled to.
		/// </param>
		private void EnterEditMode (Task task, bool adjustScrolledWindow)
		{
			// Make sure we've waited around for the new task to fully
			// be added to the TreeModel before continuing.  Some
			// backends might be threaded and will have used something
			// like Gtk.Idle.Add () to actually store the new Task in
			// their TreeModel.
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
			
			if (adjustScrolledWindow)
				ScrollToTask (task);
			
			
			Gtk.TreeIter iter;
			foreach (TaskGroup taskGroup in taskGroups) {
				if (taskGroup.ContainsTask (task, out iter)) {
					Debug.WriteLine ("Found new task group: {0}", taskGroup.DisplayName);
					
					// Get the header height
					taskGroup.EnterEditMode (task, iter);
					return;
				}
			}
		}
		
		void RebuildAddTaskMenu (IEnumerable<Category> categories)
		{
			Menu menu = new Menu ();
			
			foreach (var cat in categories) {
				if (cat == null)
					continue;
				
				var item = new CategoryMenuItem ((Category)cat);
				item.Activated += OnNewTaskByCategory;
				item.ShowAll ();
				menu.Add (item);
			}
			
			addTaskButton.Menu = menu;
		}
		
		void SelectCategory (string categoryName)
		{
			var model = categoryComboBox.Model;

			var cats = GtkApplication.Instance.Backend.Categories;
			var cat = cats.SingleOrDefault (c => c.Name == categoryName);
			int index = 0;
			foreach (var item in cats) {
				if (item == cat)
					break;
				index++;
			}
			
			TreeIter iter;
			bool success = true;
			if (cat != null)
				success = model.GetIter (out iter, new TreePath (new int [] { index + 1 }));
			else
				model.GetIterFirst (out iter);
			
			if (success)
				categoryComboBox.SetActiveIter (iter);
		}
		
		private void ShowTaskNotes (Task task)
		{
			NoteDialog dialog = null;
			if (!noteDialogs.ContainsKey (task)) {
				dialog = new NoteDialog (this, task);
				dialog.Hidden += OnNoteDialogHidden;
				noteDialogs [task] = dialog;
			} else {
				dialog = noteDialogs [task];
			}
			
			if (!task.HasNotes) {
				dialog.CreateNewNote();
			}
			dialog.Present ();
		}
		
		private Task CreateTask (string taskText, Category category)
		{
			Task task = backend.CreateTask (taskText);
			if (category == null)
				category = GtkApplication.Instance.Backend.DefaultCategory;
			category.Add (task);
			
			if (task == null) {
				Debug.WriteLine ("Error creating a new task!");
				// Show error status
				status = Catalog.GetString ("Error creating a new task");
				TaskWindow.ShowStatus (status);
			} else {
				// Show successful status
				status = Catalog.GetString ("Task created successfully");	
				TaskWindow.ShowStatus (status);
				// Clear out the entry
				addTaskEntry.Text = string.Empty;
				addTaskEntry.GrabFocus ();
			}
			
			return task;
		}
		
		/// <summary>
		/// This returns the current input widget color from the GTK theme
		/// </summary>
		/// <returns>
		/// A Gdk.Color
		/// </returns>
		private Gdk.Color GetBackgroundColor ()
		{
			using (Gtk.Style style = Gtk.Rc.GetStyle (this)) 
				return style.Base (StateType.Normal);
		}
		
		#endregion // Private Methods

		#region Event Handlers
		protected override void OnStyleSet (Gtk.Style previous_style)
		{
			base.OnStyleSet (previous_style);
			Gdk.Color backgroundColor = GetBackgroundColor ();
			innerEb.ModifyBg (StateType.Normal, backgroundColor);
			innerEb.ModifyBase (StateType.Normal, backgroundColor);
			
			if (addTaskEntry.Text == Catalog.GetString ("New task...")) {
				Gdk.Color insensitiveColor =
					addTaskEntry.Style.Text (Gtk.StateType.Insensitive);
				addTaskEntry.ModifyText (Gtk.StateType.Normal, insensitiveColor);
			}

		}
		
		private void OnRealized (object sender, EventArgs args)
		{
			addTaskEntry.GrabFocus ();
		}
		
		private void WindowDeleted (object sender, DeleteEventArgs args)
		{
			int x;
			int y;
			int width;
			int height;

			this.GetPosition(out x, out y);
			this.GetSize(out width, out height);

			lastXPos = x;
			lastYPos = y;
			
			GtkApplication.Instance.Preferences.SetInt("MainWindowLastXPos", lastXPos);
			GtkApplication.Instance.Preferences.SetInt("MainWindowLastYPos", lastYPos);
			GtkApplication.Instance.Preferences.SetInt("MainWindowWidth", width);
			GtkApplication.Instance.Preferences.SetInt("MainWindowHeight", height);

			Debug.WriteLine("WindowDeleted was called");
			taskWindow = null;
		}

		private void OnWindowShown (object sender, EventArgs args)
		{

		}
		
		void OnSettingChanged (Preferences preferences, string settingKey)
		{
			if (settingKey.CompareTo (Preferences.HideInAllCategory) != 0)
				return;
			
			OnCategoryChanged (this, EventArgs.Empty);
		}
		
		void OnGrabEntryFocus (object sender, EventArgs args)
		{
			addTaskEntry.GrabFocus ();
		}
		
		void OnAddTaskEntryFocused (object sender, EventArgs args)
		{
			// Clear the entry if it contains the default text
			if (addTaskEntry.Text == Catalog.GetString ("New task...")) {
				addTaskEntry.Text = string.Empty;
				addTaskEntry.ModifyText (Gtk.StateType.Normal);
			}
		}
		
		void OnAddTaskEntryUnfocused (object sender, EventArgs args)
		{
			// Restore the default text if nothing is entered
			if (addTaskEntry.Text == string.Empty) {
				addTaskEntry.Text = Catalog.GetString ("New task...");
				Gdk.Color insensitiveColor =
					addTaskEntry.Style.Text (Gtk.StateType.Insensitive);
				addTaskEntry.ModifyText (Gtk.StateType.Normal, insensitiveColor);
			}
		}
		
		void OnAddTaskEntryChanged (object sender, EventArgs args)
		{
			string text = addTaskEntry.Text.Trim ();
			if (text.Length == 0
					|| text.CompareTo (Catalog.GetString ("New task...")) == 0) {
				addTaskButton.Sensitive = false;
			} else {
				addTaskButton.Sensitive = true;
			}
		}
		
		void OnAddTaskEntryActivated (object sender, EventArgs args)
		{
			string newTaskText = addTaskEntry.Text.Trim ();
			if (newTaskText.Length == 0)
				return;
			
			OnAddTask (sender, args);
		}

		void OnAddTaskEntryDragDataReceived(object sender, DragDataReceivedArgs args)
		{
			// Change the text directly to the dropped text
			addTaskEntry.Text = args.SelectionData.Text;
			addTaskEntry.ModifyText (Gtk.StateType.Normal);
		}

		void OnAddTask (object sender, EventArgs args)
		{
			var enteredTaskText = addTaskEntry.Text.Trim ();
			if (enteredTaskText.Length == 0)
				return;
			
			TreeIter iter;
			if (!categoryComboBox.GetActiveIter (out iter))
				return;
			
			var catName = categoryComboBox.Model.GetValue (iter, 0) as string;
			Category cat = null;
			if (categoryComboBox.Model.GetPath (iter).Indices [0] != TreePath.NewFirst ().Indices [0])
				cat = GtkApplication.Instance.Backend.Categories.SingleOrDefault (c => c.Name == catName);
				
			// If enabled, attempt to parse due date information out of the entered task text.
			var taskDueDate = DateTime.MinValue;
			string taskName;
			if (GtkApplication.Instance.Preferences.GetBool (Preferences.ParseDateEnabledKey))
				TaskParser.Instance.TryParse (enteredTaskText, out taskName, out taskDueDate);
			else
				taskName = enteredTaskText;
			
			var task = CreateTask (taskName, cat);
			if (task == null)
				return; // TODO: Explain error to user!
			
			if (taskDueDate != DateTime.MinValue)
				task.DueDate = taskDueDate;
			
			HighlightTask (task);
		}
		
		void OnNewTaskByCategory (object sender, EventArgs args)
		{
			var newTaskText = addTaskEntry.Text.Trim ();
			if (newTaskText.Length == 0)
				return;
			
			var item = sender as CategoryMenuItem;
			if (item == null)
				return;
			
			// Determine if the selected category is currently shown in the
			// task window.  If we're in a specific cateogory or on the All
			// category and the selected category is not showing, we've got
			// to switch the category first so the user will be able to edit
			// the title of the task.
			TreeIter iter;
			if (categoryComboBox.GetActiveIter (out iter)) {
				
				// Check to see if "All" is selected
				if (categoryComboBox.Model.GetPath (iter).Indices [0] == TreePath.NewFirst ().Indices [0]) {
					// See if the item.Category is currently being shown in the "All" category
					// and if not, select the category specifically.
					var categoriesToHide = GtkApplication.Instance.Preferences.GetStringList (
						Preferences.HideInAllCategory);
					if (categoriesToHide != null && categoriesToHide.Contains (item.Category.Name))
						SelectCategory (item.Category.Name);
				} else {
					var selCatName = categoryComboBox.Model.GetValue (iter, 0) as string;
					if (selCatName != item.Category.Name)
						SelectCategory (item.Category.Name);
				}
			}
			
			var task = CreateTask (newTaskText, item.Category);
			HighlightTask (task);
		}
		
		void OnCategoryChanged (object sender, EventArgs args)
		{
			TreeIter iter;
			if (!categoryComboBox.GetActiveIter (out iter))
				return;
			
			Category category = null;
			if (categoryComboBox.Model.GetPath (iter) != TreePath.NewFirst ()) {
				var catName = categoryComboBox.Model.GetValue (iter, 0) as string;
				category = GtkApplication.Instance.Backend.Categories.SingleOrDefault (c => c.Name == catName);
			}
				
			// Update the TaskGroups so they can filter accordingly
			overdueGroup.Refilter (category);
			todayGroup.Refilter (category);
			tomorrowGroup.Refilter (category);
			nextSevenDaysGroup.Refilter (category);
			futureGroup.Refilter (category);
			completedTaskGroup.Refilter (category);
			
			// Save the selected category in preferences
			string prefCatName = category == null ? "All" : category.Name;
			GtkApplication.Instance.Preferences.Set (Preferences.SelectedCategoryKey, prefCatName);
		}
		
		void OnRowActivated (object sender, Gtk.RowActivatedArgs args)
		{
			// Check to see if a note dialog is already open for the activated
			// task.  If so, just bring it forward.  Otherwise, open a new one.
			Gtk.TreeView tv = sender as Gtk.TreeView;
			if (tv == null)
				return;
			
			Gtk.TreeModel model = tv.Model;
			
			Gtk.TreeIter iter;
			
			if (!model.GetIter (out iter, args.Path))
				return;
			
			Task task = model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			ShowTaskNotes (task);
		}
		

		[GLib.ConnectBefore]
		void OnButtonPressed (object sender, Gtk.ButtonPressEventArgs args)
		{
	        switch (args.Event.Button) {
	            case 3: // third mouse button (right-click)
		            clickedTask = null;

					Gtk.TreeView tv = sender as Gtk.TreeView;
					if (tv == null)
						return;
					
					Gtk.TreeModel model = tv.Model;
					
					Gtk.TreeIter iter;
					Gtk.TreePath path;
					Gtk.TreeViewColumn column = null;

					if (!tv.GetPathAtPos ((int) args.Event.X,
									(int) args.Event.Y, out path, out column))
						return;

					if (!model.GetIter (out iter, path))
						return;
					
					clickedTask = model.GetValue (iter, 0) as Task;
					if (clickedTask == null)
						return;
					
					Menu popupMenu = new Menu ();
					ImageMenuItem item;
					
					item = new ImageMenuItem (Catalog.GetString ("_Notes..."));
					item.Image = new Gtk.Image (noteIcon);
					item.Activated += OnShowTaskNotes;
					popupMenu.Add (item);
					
					popupMenu.Add (new SeparatorMenuItem ());

					item = new ImageMenuItem (Catalog.GetString ("_Delete task"));
					item.Image = new Gtk.Image(Gtk.Stock.Delete, IconSize.Menu);
					item.Activated += OnDeleteTask;
					popupMenu.Add (item);

					item = new ImageMenuItem(Catalog.GetString ("_Edit task"));
					item.Image = new Gtk.Image(Gtk.Stock.Edit, IconSize.Menu);
					item.Activated += OnEditTask;
					popupMenu.Add (item);

					/*
					 * Depending on the currently selected task's category, we create a context popup
					 * here in order to enable changing categories. The list of available categories
					 * is pre-filtered as to not contain the current category and the AllCategory.
					 */
					var cvCategories = new CollectionView<Category> (GtkApplication.Instance.Backend.Categories);
					cvCategories.Filter = c => c != null && !c.Contains (clickedTask);
					cvCategories.IsObserving = true;
					
					// The categories submenu is only created in case we actually provide at least one category.
					if (cvCategories.Count > 0) {
						Menu categoryMenu = new Menu();
						CategoryMenuItem categoryItem;

						foreach (var cat in cvCategories) {
							categoryItem = new CategoryMenuItem((Category)cat);
							categoryItem.Activated += OnChangeCategory;
							categoryMenu.Add(categoryItem);
						}
					
						// TODO Needs translation.
						item = new ImageMenuItem(Catalog.GetString("_Change category"));
						item.Image = new Gtk.Image(Gtk.Stock.Convert, IconSize.Menu);
						item.Submenu = categoryMenu;
						popupMenu.Add(item);
					}
				
					popupMenu.ShowAll();
					popupMenu.Popup ();
				
					// Debug.WriteLine ("Right clicked on task: " + task.Name);
					break;
			}
		}
		
		private void OnShowTaskNotes (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			ShowTaskNotes (clickedTask);
		}
		
		private void OnDeleteTask (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
		
			GtkApplication.Instance.Backend.DeleteTask(clickedTask);
			
			status = Catalog.GetString ("Task deleted");
			TaskWindow.ShowStatus (status);
		}


		private void OnEditTask (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			EnterEditMode (clickedTask, false);
		}
		
		
		void OnNoteDialogHidden (object sender, EventArgs args)
		{
			NoteDialog dialog = sender as NoteDialog;
			if (dialog == null) {
				Trace.TraceWarning ("OnNoteDialogHidden (), sender is not NoteDialog, it's: {0}", sender.GetType ().ToString ());
				return;
			}
			
			if (!noteDialogs.ContainsKey (dialog.Task)) {
				Trace.TraceWarning ("Closed NoteDialog not found in noteDialogs");
				return;
			}
			
			Debug.WriteLine ("Removing NoteDialog from noteDialogs");
			noteDialogs.Remove (dialog.Task);
			
			dialog.Destroy ();
		}
		
		private void OnBackendInitialized(object sender, EventArgs e)
		{		
			backend.BackendInitialized -= OnBackendInitialized;
			PopulateWindow();
			OnBackendSyncFinished (null, null); // To update the statusbar
		}
		
		private void OnBackendSyncStarted (object sender, EventArgs e)
		{
			TaskWindow.ShowStatus (Catalog.GetString ("Loading tasks..."));
		}
		
		private void OnBackendSyncFinished (object sender, EventArgs e)
		{
			Debug.WriteLine("Backend sync finished");
			if (GtkApplication.Instance.Backend.Configured) {
				string now = DateTime.Now.ToString ();
				// Translators: This status shows the date and time when the task list was last refreshed
				status = string.Format (Catalog.GetString ("Tasks loaded: {0}"), now);
				TaskWindow.lastLoadedTime = now;
				TaskWindow.ShowStatus (status);
				RebuildAddTaskMenu (GtkApplication.Instance.Backend.Categories);
				addTaskEntry.Sensitive = true;
				categoryComboBox.Sensitive = true;
				// Keep insensitive text color
				Gdk.Color insensitiveColor =
					addTaskEntry.Style.Text (Gtk.StateType.Insensitive);
				addTaskEntry.ModifyText (Gtk.StateType.Normal, insensitiveColor);
			} else {
				string status =
					string.Format (Catalog.GetString ("Not connected."));
				TaskWindow.ShowStatus (status);
			}
		}

		void KeyPressed (object sender, Gtk.KeyPressEventArgs args)
		{
			args.RetVal = true;
			if (args.Event.Key == Gdk.Key.Escape) {
				if ((GdkWindow.State & Gdk.WindowState.Maximized) > 0)
					Unmaximize ();
				Hide ();
				return;
			}
			args.RetVal = false;
		}

		private void OnChangeCategory (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			var oldCategory = GtkApplication.Instance.Backend.Categories.Single (c => c.Contains (clickedTask));
			oldCategory.Remove (clickedTask);
			(((CategoryMenuItem)sender).Category).Add (clickedTask);
		}
		#endregion // Event Handlers
		
		#region Private Classes
		class CategoryMenuItem : Gtk.MenuItem
		{
			private Category cat;
			
			public CategoryMenuItem (Category category) : base (category.Name)
			{
				cat = category;
			}
			
			public Category Category
			{
				get { return cat; }
			}
		}
		#endregion // Private Classes
	}
	
	/// <summary>
	/// Provide keybindings via a fake Gtk.Menu.
	/// </summary>
	public class GlobalKeybinder
	{
		Gtk.AccelGroup accel_group;
		Gtk.Menu fake_menu;

		/// <summary>
		/// Create a global keybinder for the given Gtk.AccelGroup.
		/// </summary>
		/// </param>
		public GlobalKeybinder (Gtk.AccelGroup accel_group)
		{
			this.accel_group = accel_group;

			fake_menu = new Gtk.Menu ();
			fake_menu.AccelGroup = accel_group;
		}

		/// <summary>
		/// Add a keybinding for this keybinder's AccelGroup.
		/// </summary>
		/// <param name="handler">
		/// A <see cref="EventHandler"/> for when the keybinding is
		/// activated.
		/// </param>
		/// <param name="key">
		/// A <see cref="System.UInt32"/> specifying the key that will
		/// be bound (see the Gdk.Key enumeration for common values).
		/// </param>
		/// <param name="modifiers">
		/// The <see cref="Gdk.ModifierType"/> to be used on key
		/// for this binding.
		/// </param>
		/// <param name="flags">
		/// The <see cref="Gtk.AccelFlags"/> for this binding.
		/// </param>
		public void AddAccelerator (EventHandler handler,
		                            uint key,
		                            Gdk.ModifierType modifiers,
		                            Gtk.AccelFlags flags)
		{
			Gtk.MenuItem foo = new Gtk.MenuItem ();
			foo.Activated += handler;
			foo.AddAccelerator ("activate",
			                    accel_group,
			                    key,
			                    modifiers,
			                    flags);
			foo.Show ();

			fake_menu.Append (foo);
		}
	}
}
