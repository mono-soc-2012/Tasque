// TaskTreeView.cs created with MonoDevelop
// User: boyd on 2/9/2008

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using System.Diagnostics;
using CollectionTransforms;
using System.Collections.Specialized;

namespace Tasque
{
	/// <summary>
	/// This is the main TreeView widget that is used to show tasks in Tasque's
	/// main window.
	/// </summary>
	public class TaskTreeView : Gtk.TreeView
	{
		private static Gdk.Pixbuf notePixbuf;
		
		private static Gdk.Pixbuf[] inactiveAnimPixbufs;
		
		CollectionView<Task> modelFilter;
		private Category filterCategory;	
		private Task taskBeingEdited = null;
		private bool toggled;

		internal static string status;
		
		static TaskTreeView ()
		{
			notePixbuf = Utilities.GetIcon ("hicolor_animations_16x16_notebook", 16);
			
			inactiveAnimPixbufs = new Gdk.Pixbuf [12];
			for (int i = 11; i >= 0; i--) {
				string iconName = string.Format ("hicolor_animations_16x16_countdown-{0}", i);
				inactiveAnimPixbufs [i] = Utilities.GetIcon (iconName, 16);
			}
		}
		
		public event EventHandler NumberOfTasksChanged;

		public TaskTreeView (CollectionView<Task> model)
			: base ()
		{		

			#if GTK_2_12
			// set up the timing for the tooltips
			this.Settings.SetLongProperty("gtk-tooltip-browse-mode-timeout", 0, "Tasque:TaskTreeView");
			this.Settings.SetLongProperty("gtk-tooltip-browse-timeout", 750, "Tasque:TaskTreeView");
			this.Settings.SetLongProperty("gtk-tooltip-timeout", 750, "Tasque:TaskTreeView");

			ConnectEvents();
			#endif
			
			// TODO: Modify the behavior of the TreeView so that it doesn't show
			// the highlighted row.  Then, also tie in with the mouse hovering
			// so that as you hover the mouse around, it will automatically
			// select the row that the mouse is hovered over.  By doing this,
			// we should be able to not require the user to click on a task
			// to select it and THEN have to click on the column item they want
			// to modify.
			
			filterCategory = null;
			
			modelFilter = new ListCollectionView<Task> (model);
			modelFilter.Filter = FilterFunc;
			modelFilter.CollectionChanged += HandleModelFilterChanged;
			Model = new TreeModelListAdapter<Task> (modelFilter);
			modelFilter.IsObserving = true;
			
			Selection.Mode = Gtk.SelectionMode.Single;
			RulesHint = false;
			HeadersVisible = false;
			HoverSelection = true;
			
			// TODO: Figure out how to turn off selection highlight
			
			Gtk.CellRenderer renderer;
			
			//
			// Checkbox Column
			//
			Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();
			// Title for Completed/Checkbox Column
			column.Title = Catalog.GetString ("Completed");
			column.Sizing = Gtk.TreeViewColumnSizing.Autosize;
			column.Resizable = false;
			column.Clickable = true;
			
			renderer = new Gtk.CellRendererToggle ();
			(renderer as Gtk.CellRendererToggle).Toggled += OnTaskToggled;
			column.PackStart (renderer, false);
			column.SetCellDataFunc (renderer, new Gtk.TreeCellDataFunc (TaskToggleCellDataFunc));
			AppendColumn (column);
			
			//
			// Priority Column
			//
			column = new Gtk.TreeViewColumn ();
			// Title for Priority Column
			column.Title = Catalog.GetString ("Priority");
			//column.Sizing = Gtk.TreeViewColumnSizing.Autosize;
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
			column.Alignment = 0.5f;
			column.FixedWidth = 30;
			column.Resizable = false;
			column.Clickable = true;

			renderer = new Gtk.CellRendererCombo ();
			(renderer as Gtk.CellRendererCombo).Editable = true;
			(renderer as Gtk.CellRendererCombo).HasEntry = false;
			SetCellRendererCallbacks ((CellRendererCombo) renderer, OnTaskPriorityEdited);
			Gtk.ListStore priorityStore = new Gtk.ListStore (typeof (string));
			priorityStore.AppendValues (Catalog.GetString ("1")); // High
			priorityStore.AppendValues (Catalog.GetString ("2")); // Medium
			priorityStore.AppendValues (Catalog.GetString ("3")); // Low
			priorityStore.AppendValues (Catalog.GetString ("-")); // None
			(renderer as Gtk.CellRendererCombo).Model = priorityStore;
			(renderer as Gtk.CellRendererCombo).TextColumn = 0;
			renderer.Xalign = 0.5f;
			column.PackStart (renderer, true);
			column.SetCellDataFunc (renderer,
					new Gtk.TreeCellDataFunc (TaskPriorityCellDataFunc));
			AppendColumn (column);

			//
			// Task Name Column
			//
			column = new Gtk.TreeViewColumn ();
			// Title for Task Name Column
			column.Title = Catalog.GetString ("Task Name");
//			column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
			column.Sizing = Gtk.TreeViewColumnSizing.Autosize;
			column.Expand = true;
			column.Resizable = true;
			
			// TODO: Add in code to determine how wide we should make the name
			// column.
			// TODO: Add in code to readjust the size of the name column if the
			// user resizes the Task Window.
			//column.FixedWidth = 250;
			
			renderer = new Gtk.CellRendererText ();
			column.PackStart (renderer, true);
			column.SetCellDataFunc (renderer,
				new Gtk.TreeCellDataFunc (TaskNameTextCellDataFunc));
			((Gtk.CellRendererText)renderer).Editable = true;
			SetCellRendererCallbacks ((CellRendererText) renderer, OnTaskNameEdited);
			
			AppendColumn (column);
			
			
			//
			// Due Date Column
			//

			//  2/11 - Today
			//  2/12 - Tomorrow
			//  2/13 - Wed
			//  2/14 - Thu
			//  2/15 - Fri
			//  2/16 - Sat
			//  2/17 - Sun
			// --------------
			//  2/18 - In 1 Week
			// --------------
			//  No Date
			// ---------------
			//  Choose Date...
			
			column = new Gtk.TreeViewColumn ();
			// Title for Due Date Column
			column.Title = Catalog.GetString ("Due Date");
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
			column.Alignment = 0f;
			column.FixedWidth = 90;
			column.Resizable = false;
			column.Clickable = true;

			renderer = new Gtk.CellRendererCombo ();
			(renderer as Gtk.CellRendererCombo).Editable = true;
			(renderer as Gtk.CellRendererCombo).HasEntry = false;
			SetCellRendererCallbacks ((CellRendererCombo) renderer, OnDateEdited);
			Gtk.ListStore dueDateStore = new Gtk.ListStore (typeof (string));
			DateTime today = DateTime.Now;
			dueDateStore.AppendValues (
				today.ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("Today"));
			dueDateStore.AppendValues (
				today.AddDays(1).ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("Tomorrow"));
			dueDateStore.AppendValues (
				today.AddDays(2).ToString(Catalog.GetString("M/d - ddd")));
			dueDateStore.AppendValues (
				today.AddDays(3).ToString(Catalog.GetString("M/d - ddd")));
			dueDateStore.AppendValues (
				today.AddDays(4).ToString(Catalog.GetString("M/d - ddd")));
			dueDateStore.AppendValues (
				today.AddDays(5).ToString(Catalog.GetString("M/d - ddd")));
			dueDateStore.AppendValues (
				today.AddDays(6).ToString(Catalog.GetString("M/d - ddd")));
			dueDateStore.AppendValues (
				today.AddDays(7).ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("In 1 Week"));			
			dueDateStore.AppendValues (Catalog.GetString ("No Date"));
			dueDateStore.AppendValues (Catalog.GetString ("Choose Date..."));
			(renderer as Gtk.CellRendererCombo).Model = dueDateStore;
			(renderer as Gtk.CellRendererCombo).TextColumn = 0;
			renderer.Xalign = 0.0f;
			column.PackStart (renderer, true);
			column.SetCellDataFunc (renderer,
					new Gtk.TreeCellDataFunc (DueDateCellDataFunc));
			AppendColumn (column);


			
			//
			// Notes Column
			//
			column = new Gtk.TreeViewColumn ();
			// Title for Notes Column
			column.Title = Catalog.GetString ("Notes");
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
			column.FixedWidth = 20;
			column.Resizable = false;
			
			renderer = new Gtk.CellRendererPixbuf ();
			column.PackStart (renderer, false);
			column.SetCellDataFunc (renderer,
				new Gtk.TreeCellDataFunc (TaskNotesCellDataFunc));
			
			AppendColumn (column);
			
			//
			// Timer Column
			//
			column = new Gtk.TreeViewColumn ();
			// Title for Timer Column
			column.Title = Catalog.GetString ("Timer");
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed;
			column.FixedWidth = 20;
			column.Resizable = false;
			
			renderer = new Gtk.CellRendererPixbuf ();
			renderer.Xalign = 0.5f;
			column.PackStart (renderer, false);
			column.SetCellDataFunc (renderer,
				new Gtk.TreeCellDataFunc (TaskTimerCellDataFunc));
			
			AppendColumn (column);
		}

		void HandleModelFilterChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				OnRowInsertedHandler ();
				break;
			case NotifyCollectionChangedAction.Remove:
				OnRowDeletedHandler ();
				break;
			}
		}

		void CellRenderer_EditingStarted (object o, EditingStartedArgs args)
		{
			if (!toggled)
				return;

			Gtk.TreeIter iter;
			Gtk.TreePath path = new Gtk.TreePath (args.Path);
			if (!Model.GetIter (out iter, path))
				return;

			Task task = Model.GetValue (iter, 0) as Task;
			if (task == null)
				return;

			taskBeingEdited = task;
			InactivateTimer.ToggleTimer (taskBeingEdited);
		}
		
		void SetCellRendererCallbacks (CellRendererText renderer, EditedHandler handler)
		{
			// The user is going to "edit" or "cancel", timer can't continue.
			renderer.EditingStarted += CellRenderer_EditingStarted;
			// Canceled: timer can continue.
			renderer.EditingCanceled += (o, args) => {
				if (toggled && taskBeingEdited != null) {
					taskBeingEdited.Inactivate ();
					InactivateTimer.ToggleTimer (taskBeingEdited);
					taskBeingEdited = null;
					toggled = false;
				}
			};
			// Edited: after calling the delegate the timer can continue.
			renderer.Edited += (o, args) => {
				if (handler != null)
					handler (o, args);

				if (toggled && taskBeingEdited != null) {
					taskBeingEdited.Inactivate ();
					InactivateTimer.ToggleTimer (taskBeingEdited);
					taskBeingEdited = null;
					toggled = false;
				}
			};
		}

		#region Public Methods
		public void Refilter ()
		{
			Refilter (filterCategory);
		}
		
		public void Refilter (Category selectedCategory)
		{
			this.filterCategory = selectedCategory;
			modelFilter.Refresh ();
		}
		
		public int GetNumberOfTasks ()
		{
			return modelFilter.Count;
		}
		#endregion // Public Methods
		
		#region Private Methods
		protected override void OnRealized ()
		{
			base.OnRealized ();
			
			// Not sure why we need this, but without it, completed items are
			// initially appearing in the view.
			Refilter (filterCategory);
		}

		internal static void ShowCompletedTaskStatus ()
		{
			status = Catalog.GetString ("Task Completed");
			TaskWindow.ShowStatus (status);
		}
		
		private void TaskToggleCellDataFunc (Gtk.TreeViewColumn column,
										Gtk.CellRenderer cell,
										Gtk.TreeModel model,
										Gtk.TreeIter iter)
		{
			Gtk.CellRendererToggle crt = cell as Gtk.CellRendererToggle;
			Task task = model.GetValue (iter, 0) as Task;
			if (task == null)
				crt.Active = false;
			else {
				crt.Active =
					task.State == TaskState.Active ? false : true;
			}
		}

		#if GTK_2_12
		private void ConnectEvents()
		{
			this.CursorChanged += delegate(object o, EventArgs args) {			
			int toolTipMaxLength = 250;
			string snipText = "...";
			int maxNumNotes = 3;
			int notesAdded = 0;
			TooltipText = null;
			TriggerTooltipQuery();
			TreeModel m;
			TreeIter iter;
			List<String> list = new List<String>();
	
			if(Selection.GetSelected(out m, out iter)) {
				ITask task = Model.GetValue (iter, 0) as ITask;							      
				if (task != null && task.HasNotes && task.Notes != null) {
					foreach (INote note in task.Notes) {
						// for the tooltip, truncate any notes longer than 250 characters.
						if (note.Text.Length > toolTipMaxLength)
							list.Add(note.Text.Substring(0, toolTipMaxLength - snipText.Length) + 
											snipText);
						else
							list.Add(note.Text);
						notesAdded++;
						// stop iterating once we reach maxNumNotes
						if (notesAdded >= maxNumNotes) {
							break;
						}
					}
				}			      		
		
				HasTooltip = list.Count > 0;
				if (HasTooltip) {
					// if there are more than maxNumNotes, append a notice to the tooltip
					if (notesAdded < task.Notes.Count) {
						int nMoreNotes = task.Notes.Count - notesAdded;
						if (nMoreNotes > 1)
							list.Add(String.Format("[{0} more notes]", nMoreNotes));
						else
							list.Add("[1 more note]");
					}
					TooltipText = String.Join("\n\n", list.ToArray());
					TriggerTooltipQuery();
				}
			}
			};
		}
		#endif

		void TaskPriorityCellDataFunc (Gtk.TreeViewColumn tree_column,
									   Gtk.CellRenderer cell,
									   Gtk.TreeModel tree_model,
									   Gtk.TreeIter iter)
		{
			// TODO: Add bold (for high), light (for None), and also colors to priority?
			Gtk.CellRendererCombo crc = cell as Gtk.CellRendererCombo;
			Task task = Model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			switch (task.Priority) {
			case TaskPriority.Low:
				crc.Text = Catalog.GetString ("3");
				break;
			case TaskPriority.Medium:
				crc.Text = Catalog.GetString ("2");
				break;
			case TaskPriority.High:
				crc.Text = Catalog.GetString ("1");
				break;
			default:
				crc.Text = Catalog.GetString ("-");
				break;
			}
		}
		
		private void TaskNameTextCellDataFunc (Gtk.TreeViewColumn treeColumn,
				Gtk.CellRenderer renderer, Gtk.TreeModel model,
				Gtk.TreeIter iter)
		{
			Gtk.CellRendererText crt = renderer as Gtk.CellRendererText;
			crt.Ellipsize = Pango.EllipsizeMode.End;
			Task task = model.GetValue (iter, 0) as Task;
			if (task == null) {
				crt.Text = string.Empty;
				return;
			}
			
			string formatString = "{0}";

			Preferences prefs = GtkApplication.Instance.Preferences;
			string todayTasksColor = prefs.Get (Preferences.TodayTaskTextColor);
			string overdueTaskColor = prefs.Get (Preferences.OverdueTaskTextColor);

			if (task.IsComplete)
				; // Completed tasks colored below
			else if (task.DueDate.Date == DateTime.Today.Date)
				crt.Foreground = todayTasksColor;
			// Overdue and the task has a date assigned to it.
			else if (task.DueDate < DateTime.Today && task.DueDate != DateTime.MinValue)
				crt.Foreground = overdueTaskColor;

			switch (task.State) {
			case TaskState.Inactive:
				// Strikeout the text
				formatString = "<span strikethrough=\"true\">{0}</span>";
				break;
			case TaskState.Deleted:
			case TaskState.Completed:
				// Gray out the text and add strikeout
				// TODO: Determine the grayed-out text color appropriate for the current theme
				formatString =
					"<span strikethrough=\"true\">{0}</span>";
				crt.Foreground = "#AAAAAA";
				break;
			}
			
			crt.Markup = string.Format (formatString,
				GLib.Markup.EscapeText (task.Name));
		}
		
		protected virtual void DueDateCellDataFunc (Gtk.TreeViewColumn treeColumn,
				Gtk.CellRenderer renderer, Gtk.TreeModel model,
				Gtk.TreeIter iter)
		{
			Gtk.CellRendererCombo crc = renderer as Gtk.CellRendererCombo;
			Task task = Model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			DateTime date = task.State == TaskState.Completed ?
									task.CompletionDate :
									task.DueDate;
			if (date == DateTime.MinValue || date == DateTime.MaxValue) {
				crc.Text = "-";
				return;
			}
			
			if (date.Year == DateTime.Today.Year)
				crc.Text = date.ToString(Catalog.GetString("M/d - ddd"));
			else
				crc.Text = date.ToString(Catalog.GetString("M/d/yy - ddd"));
			//Utilities.GetPrettyPrintDate (task.DueDate, false);
		}
		
		private void TaskNotesCellDataFunc (Gtk.TreeViewColumn treeColumn,
				Gtk.CellRenderer renderer, Gtk.TreeModel model,
				Gtk.TreeIter iter)
		{
			Gtk.CellRendererPixbuf crp = renderer as Gtk.CellRendererPixbuf;
			Task task = model.GetValue (iter, 0) as Task;
			if (task == null) {
				crp.Pixbuf = null;
				return;
			}
			
			crp.Pixbuf = task.HasNotes ? notePixbuf : null;
		}
		
		private void TaskTimerCellDataFunc (Gtk.TreeViewColumn treeColumn,
				Gtk.CellRenderer renderer, Gtk.TreeModel model,
				Gtk.TreeIter iter)
		{
			Gtk.CellRendererPixbuf crp = renderer as Gtk.CellRendererPixbuf;
			Task task = model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			if (task.State != TaskState.Inactive) {
				// The task is not in the inactive state so don't show any icon
				crp.Pixbuf = null;
				return;
			}
			
			Preferences prefs = GtkApplication.Instance.Preferences;
			int timerSeconds = prefs.GetInt (Preferences.InactivateTimeoutKey);
			// convert to milliseconds for more granularity
			long timeout = timerSeconds * 1000;
			
			//Debug.WriteLine ("TaskTimerCellDataFunc ()\n\tNow.Ticks: {0}\n\tCompletionDate.Ticks: {1}",
			//				DateTime.Now.Ticks, task.CompletionDate.Ticks);
			long elapsedTicks = DateTime.Now.Ticks - task.CompletionDate.Ticks;
			//Debug.WriteLine ("\tElapsed Ticks: {0}", elapsedTicks);
			long elapsedMillis = elapsedTicks / 10000;
			//Debug.WriteLine ("\tElapsed Milliseconds: {0}", elapsedMillis);
			
			double percentComplete = (double)elapsedMillis / (double)timeout;
			//Debug.WriteLine ("\tPercent Complete: {0}", percentComplete);
			
			Gdk.Pixbuf pixbuf = GetIconForPercentage (percentComplete * 100);
			crp.Pixbuf = pixbuf;
		}
		
		protected static Gdk.Pixbuf GetIconForPercentage (double timeoutPercent)
		{
			int iconNum = GetIconNumForPercentage (timeoutPercent);
			if (iconNum == -1 || iconNum > 11)
				return null;
			
			return inactiveAnimPixbufs [iconNum];
		}
		
		protected static int GetIconNumForPercentage (double timeoutPercent)
		{
			//Debug.WriteLine ("GetIconNumForPercentage: {0}", timeoutPercent);
			int numOfIcons = 12;
			double percentIncrement = (double)100 / (double)numOfIcons;
			//Debug.WriteLine ("\tpercentIncrement: {0}", percentIncrement);
			
			if (timeoutPercent < percentIncrement)
				return 0;
			if (timeoutPercent < percentIncrement * 2)
				return 1;
			if (timeoutPercent < percentIncrement * 3)
				return 2;
			if (timeoutPercent < percentIncrement * 4)
				return 3;
			if (timeoutPercent < percentIncrement * 5)
				return 4;
			if (timeoutPercent < percentIncrement * 6)
				return 5;
			if (timeoutPercent < percentIncrement * 7)
				return 6;
			if (timeoutPercent < percentIncrement * 8)
				return 7;
			if (timeoutPercent < percentIncrement * 9)
				return 8;
			if (timeoutPercent < percentIncrement * 10)
				return 9;
			if (timeoutPercent < percentIncrement * 11)
				return 10;
			if (timeoutPercent < percentIncrement * 12)
				return 11;
			
			return -1;
		}
		
		protected virtual bool FilterFunc (Task task)
		{
			// Filter out deleted tasks
			if (task == null) {
				Trace.TraceError ("FilterFunc: task at iter was null");
				return false;
			}
			
			if (task.State == TaskState.Deleted) {
				//Debug.WriteLine ("TaskTreeView.FilterFunc:\n\t{0}\n\t{1}\n\tReturning false", task.Name, task.State);  
				return false;
			}
			
			if (filterCategory == null)
				return true;
			
			return filterCategory.Contains (task);
		}
		#endregion // Private Methods
		
		#region EventHandlers
		void OnTaskToggled (object sender, ToggledArgs args)
		{
			Debug.WriteLine ("OnTaskToggled");
			TreeIter iter;
			var path = new TreePath (args.Path);
			if (!Model.GetIter (out iter, path))
				return; // Do nothing
			
			var task = Model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			// remove any timer set up on this task
			InactivateTimer.CancelTimer(task);
			
			if (task.State == TaskState.Active) {
				bool showCompletedTasks =
					GtkApplication.Instance.Preferences.GetBool (
						Preferences.ShowCompletedTasksKey);
				
				// When showCompletedTasks is true, complete the tasks right
				// away.  Otherwise, set a timer and show the timer animation
				// before marking the task completed.
				if (showCompletedTasks) {
					task.Complete ();
					ShowCompletedTaskStatus ();
				} else {
					task.Inactivate ();
					
					// Read the inactivate timeout from a preference
					int timeout =
						GtkApplication.Instance.Preferences.GetInt (Preferences.InactivateTimeoutKey);
					Debug.WriteLine ("Read timeout from prefs: {0}", timeout);
					var timer = new InactivateTimer (this, iter, task, (uint) timeout);
					timer.StartTimer ();
					toggled = true;
				}
			} else {
				status = Catalog.GetString ("Action Canceled");
				TaskWindow.ShowStatus (status);
				task.Activate ();
			}
		}

		void OnTaskPriorityEdited (object sender, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			Gtk.TreePath path = new TreePath (args.Path);
			if (!Model.GetIter (out iter, path))
				return;

			TaskPriority newPriority;
			if (args.NewText.CompareTo (Catalog.GetString ("3")) == 0)
				newPriority = TaskPriority.Low;
			else if (args.NewText.CompareTo (Catalog.GetString ("2")) == 0)
				newPriority = TaskPriority.Medium;
			else if (args.NewText.CompareTo (Catalog.GetString ("1")) == 0)
				newPriority = TaskPriority.High;
			else
				newPriority = TaskPriority.None;

			// Update the priority if it's different
			Task task = Model.GetValue (iter, 0) as Task;
			if (task.Priority != newPriority)
				task.Priority = newPriority;
		}
		
		void OnTaskNameEdited (object sender, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			Gtk.TreePath path = new TreePath (args.Path);
			if (!Model.GetIter (out iter, path))
				return;
			
			Task task = Model.GetValue (iter, 0) as Task;
			if (task == null)
				return;
			
			string newText = args.NewText;
			
			// Attempt to derive due date information from text.
			if (GtkApplication.Instance.Preferences.GetBool (Preferences.ParseDateEnabledKey) &&
			    task.State == TaskState.Active &&
			    task.DueDate == DateTime.MinValue) {
				
				string parsedTaskText;
				DateTime parsedDueDate;
				TaskParser.Instance.TryParse (newText, out parsedTaskText, out parsedDueDate);
				
				if (parsedDueDate != DateTime.MinValue)
					task.DueDate = parsedDueDate;
				newText = parsedTaskText;
			}
			
			task.Name = newText;
		}
		
		/// <summary>
		/// Modify the due date or completion date depending on whether the
		/// task being modified is completed or active.
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="Gtk.EditedArgs"/>
		/// </param>
		void OnDateEdited (object sender, Gtk.EditedArgs args)
		{
			if (args.NewText == null) {
				Debug.WriteLine ("New date text null, not setting date");
				return;
			}
			
			Gtk.TreeIter iter;
			Gtk.TreePath path = new TreePath (args.Path);
			if (!Model.GetIter (out iter, path))
				return;
			
			//  2/11 - Today
			//  2/12 - Tomorrow
			//  2/13 - Wed
			//  2/14 - Thu
			//  2/15 - Fri
			//  2/16 - Sat
			//  2/17 - Sun
			// --------------
			//  2/18 - In 1 Week
			// --------------
			//  No Date
			// ---------------
			//  Choose Date...
			
			DateTime newDate = DateTime.MinValue;
			DateTime today = DateTime.Now;
			Task task = Model.GetValue (iter, 0) as Task;			
			
			if (args.NewText.CompareTo (
							today.ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("Today") ) == 0)
				newDate = today;
			else if (args.NewText.CompareTo (
						today.AddDays(1).ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("Tomorrow") ) == 0)
				newDate = today.AddDays (1);
			else if (args.NewText.CompareTo (Catalog.GetString ("No Date")) == 0)
				newDate = DateTime.MinValue;
			else if (args.NewText.CompareTo (
				today.AddDays(7).ToString(Catalog.GetString("M/d - ")) + Catalog.GetString("In 1 Week")	) == 0)
				newDate = today.AddDays (7);
			else if (args.NewText.CompareTo (Catalog.GetString ("Choose Date...")) == 0) {
				TaskCalendar tc = new TaskCalendar(task, this.Parent);
				tc.ShowCalendar();
				return;
			} else {
				for (int i = 2; i <= 6; i++) {
					DateTime testDate = today.AddDays (i);
					if (testDate.ToString(Catalog.GetString("M/d - ddd")).CompareTo (
							args.NewText) == 0) {
						newDate = testDate;
						break;
					}
				}
			}
			
			Console.WriteLine ("task.State {0}", task.State);
			
			// Modify the due date
			task.DueDate = newDate;
		}
		
		void OnRowInsertedHandler ()
		{
			if (NumberOfTasksChanged == null)
				return;
			
			NumberOfTasksChanged (this, EventArgs.Empty);
		}
		
		void OnRowDeletedHandler ()
		{
			if (NumberOfTasksChanged == null)
				return;
			
			NumberOfTasksChanged (this, EventArgs.Empty);
		}
		#endregion // EventHandlers
	}
}
