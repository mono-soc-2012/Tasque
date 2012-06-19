// DummyBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008

using System;
using System.Collections.Generic;
using Mono.Unix;
using Tasque.Backends;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tasque.Backends.Dummy
{
	public class DummyBackend : IBackend
	{
		/// <summary>
		/// Keep track of the Gtk.TreeIters for the tasks so that they can
		/// be referenced later.
		///
		/// Key   = Task ID
		/// Value = Gtk.TreeIter in taskStore
		/// </summary>
		private Dictionary<int, Gtk.TreeIter> taskIters;
		private int newTaskId;
		private bool initialized;
		private bool configured = true;
		
		private Gtk.ListStore categoryListStore;
		private Gtk.TreeModelSort sortedCategoriesModel;

		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;
		
		DummyCategory homeCategory;
		DummyCategory workCategory;
		DummyCategory projectsCategory;
		
		public DummyBackend ()
		{
			initialized = false;
			newTaskId = 0;
			taskIters = new Dictionary<int, Gtk.TreeIter> (); 
			Tasks = new ObservableCollection<ITask> ();
			
			categoryListStore = new Gtk.ListStore (typeof(ICategory));
			
			sortedCategoriesModel = new Gtk.TreeModelSort (categoryListStore);
			sortedCategoriesModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareCategorySortFunc));
			sortedCategoriesModel.SetSortColumnId (0, Gtk.SortType.Ascending);
		}
		
		#region Public Properties
		public string Name {
			get { return "Debugging System"; }
		}
		
		/// <value>
		/// All the tasks including ITaskDivider items.
		/// </value>
		public IEnumerable<ITask> SortedTasks {
			get { return Tasks.OrderBy (t => t, Comparer<ITask>.Default); }
		}

		public ObservableCollection<ITask> Tasks { get; private set; }
		
		/// <value>
		/// This returns all the task lists (categories) that exist.
		/// </value>
		public Gtk.TreeModel Categories {
			get { return sortedCategoriesModel; }
		}
		
		/// <value>
		/// Indication that the dummy backend is configured
		/// </value>
		public bool Configured {
			get { return configured; }
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized {
			get { return initialized; }
		}		
		#endregion // Public Properties
		
		#region Public Methods
		public ITask CreateTask (string taskName, ICategory category)
		{
			// not sure what to do here with the category
			DummyTask task = new DummyTask (this, newTaskId, taskName);
			
			// Determine and set the task category
			if (category == null || category is Tasque.AllCategory)
				task.Category = workCategory; // Default to work
			else
				task.Category = category;

			Tasks.Add (task);
			newTaskId++;
			
			return task;
		}
		
		public void DeleteTask (ITask task)
		{
		}
		
		public void Refresh ()
		{
		}
		
		public void Initialize ()
		{
			Gtk.TreeIter iter;
			
			//
			// Add in the "All" Category
			//
			AllCategory allCategory = new Tasque.AllCategory ();
			iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, allCategory);
			
			//
			// Add in some fake categories
			//
			homeCategory = new DummyCategory ("Home");
			iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, homeCategory);
			
			workCategory = new DummyCategory ("Work");
			iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, workCategory);
			
			projectsCategory = new DummyCategory ("Projects");
			iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, projectsCategory);
			
			//
			// Add in some fake tasks
			//
			
			DummyTask task = new DummyTask (this, newTaskId, "Buy some nails");
			task.Category = projectsCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Call Roger");
			task.Category = homeCategory;
			task.DueDate = DateTime.Now.AddDays (-1);
			task.Complete ();
			task.CompletionDate = task.DueDate;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Replace burnt out lightbulb");
			task.Category = homeCategory;
			task.DueDate = DateTime.Now;
			task.Priority = TaskPriority.Low;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "File taxes");
			task.Category = homeCategory;
			task.DueDate = new DateTime (2008, 4, 1);
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Purchase lumber");
			task.Category = projectsCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			Tasks.Add (task);
			newTaskId++;
						
			task = new DummyTask (this, newTaskId, "Estimate drywall requirements");
			task.Category = projectsCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Low;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Borrow framing nailer from Ben");
			task.Category = projectsCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Call for an insulation estimate");
			task.Category = projectsCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Pay storage rental fee");
			task.Category = homeCategory;
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.None;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Place carpet order");
			task.Category = projectsCategory;
			task.Priority = TaskPriority.None;
			Tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Test task overdue");
			task.Category = workCategory;
			task.DueDate = DateTime.Now.AddDays (-89);
			task.Priority = TaskPriority.None;
			task.Complete ();
			Tasks.Add (task);
			newTaskId++;
			
			initialized = true;
			if (BackendInitialized != null) {
				BackendInitialized ();
			}		
		}

		public void Cleanup ()
		{
		}
		
		public Gtk.Widget GetPreferencesWidget ()
		{
			// TODO: Replace this with returning null once things are going
			// so that the Preferences Dialog doesn't waste space.
			return new Gtk.Label ("Debugging System (this message is a test)");
		}
		#endregion // Public Methods
		
		#region Private Methods

		static int CompareCategorySortFunc (Gtk.TreeModel model,
											Gtk.TreeIter a,
											Gtk.TreeIter b)
		{
			ICategory categoryA = model.GetValue (a, 0) as ICategory;
			ICategory categoryB = model.GetValue (b, 0) as ICategory;
			
			if (categoryA == null || categoryB == null)
				return 0;
			
			if (categoryA is Tasque.AllCategory)
				return -1;
			else if (categoryB is Tasque.AllCategory)
				return 1;
			
			return (categoryA.Name.CompareTo (categoryB.Name));
		}

		public void UpdateTask (DummyTask task)
		{
			if (!Tasks.Contains (task))
				return;

			if (task.State == TaskState.Deleted) {
				Tasks.Remove (task);
				Logger.Debug ("Successfully deleted from taskStore: {0}", task.Name);
			} else {
				// TODO: Notify UI
				Logger.Debug ("The UI should be notified here.");
			}

			// Set the task in the store so the model will update the UI.
//			Gtk.TreeIter iter;
//			
//			if (!taskIters.ContainsKey (task.DummyId))
//				return;
//				
//			iter = taskIters [task.DummyId];
//			
//			if (task.State == TaskState.Deleted) {
//				taskIters.Remove (task.DummyId);
//				if (!taskStore.Remove (ref iter)) {
//					Logger.Debug ("Successfully deleted from taskStore: {0}",
//						task.Name);
//				} else {
//					Logger.Debug ("Problem removing from taskStore: {0}",
//						task.Name);
//				}
//			} else {
//				taskStore.SetValue (iter, 0, task);
//			}
		}
		#endregion // Private Methods
		
		#region Event Handlers
		#endregion // Event Handlers
	}
}
