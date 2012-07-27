// RemoteControl.cs created with MonoDevelop
// User: sandy at 9:49 AM 2/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Unix; // for Catalog.GetString ()

#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif

using org.freedesktop.DBus;
using NDesk.DBus;

namespace Tasque
{
	[Interface ("org.gnome.Tasque.RemoteControl")]
	public class RemoteControl : MarshalByRefObject
	{
		static Gdk.Pixbuf tasqueIcon;
		static RemoteControl ()
		{
			tasqueIcon = Utilities.GetIcon ("tasque-48", 48);
		}
		
		public RemoteControl()
		{
		}
				
		/// <summary>
		/// Create a new task in Tasque using the given categoryName and name.
		/// Will not attempt to parse due date information.
		/// </summary>
		/// <param name="categoryName">
		/// A <see cref="System.String"/>.  The name of an existing category.
		/// Matches are not case-sensitive.
		/// </param>
		/// <param name="taskName">
		/// A <see cref="System.String"/>.  The name of the task to be created.
		/// </param>
		/// <param name="enterEditMode">
		/// A <see cref="System.Boolean"/>.  Specify true if the TaskWindow
		/// should be shown, the new task scrolled to, and have it be put into
		/// edit mode immediately.
		/// </param>
		/// <returns>
		/// A unique <see cref="System.String"/> which can be used to reference
		/// the task later.
		/// </returns>
		public string CreateTask (string categoryName, string taskName,
						bool enterEditMode)
		{
			return CreateTask (categoryName, taskName, enterEditMode, false);
		}

		/// <summary>
		/// Create a new task in Tasque using the given categoryName and name.
		/// </summary>
		/// <param name="categoryName">
		/// A <see cref="System.String"/>.  The name of an existing category.
		/// Matches are not case-sensitive.
		/// </param>
		/// <param name="taskName">
		/// A <see cref="System.String"/>.  The name of the task to be created.
		/// </param>
		/// <param name="enterEditMode">
		/// A <see cref="System.Boolean"/>.  Specify true if the TaskWindow
		/// should be shown, the new task scrolled to, and have it be put into
		/// edit mode immediately.
		/// </param>
		/// <param name="parseDate">
		/// A <see cref="System.Boolean"/>.  Specify true if the 
		/// date should be parsed out of the taskName (in case 
		/// Preferences.ParseDateEnabledKey is true as well).
		/// </param>
		/// <returns>
		/// A unique <see cref="System.String"/> which can be used to reference
		/// the task later.
		/// </returns>
		public string CreateTask (string categoryName, string taskName,
						bool enterEditMode, bool parseDate)
		{
			var categories = Application.Backend.Categories2;
			
			//
			// Validate the input parameters.  Don't allow null or empty strings
			// be passed-in.
			//
			if (string.IsNullOrWhiteSpace (categoryName) || string.IsNullOrWhiteSpace (taskName))
				return string.Empty;
			
			//
			// Look for the specified category
			//
			Category category = null;
			categories.SingleOrDefault (c => c.Name.ToLower () == categoryName.ToLower ());
			
			if (category == null)
				return string.Empty;
			
			// If enabled, attempt to parse due date information
			// out of the taskName.
			DateTime taskDueDate = DateTime.MinValue;
			if (parseDate && Application.Preferences.GetBool (Preferences.ParseDateEnabledKey))
				TaskParser.Instance.TryParse (
				                         taskName,
				                         out taskName,
				                         out taskDueDate);
			Task task = null;
			try {
				task = Application.Backend.CreateTask (taskName, category);
				if (taskDueDate != DateTime.MinValue)
					task.DueDate = taskDueDate;
			} catch (Exception e) {
				Logger.Error ("Exception calling Application.Backend.CreateTask from RemoteControl: {0}", e.Message);
				return string.Empty;
			}
			
			if (task == null) {
				return string.Empty;
			}
			
			if (enterEditMode) {
				TaskWindow.SelectAndEdit (task);
			}
			
			#if ENABLE_NOTIFY_SHARP
			// Use notify-sharp to alert the user that a new task has been
			// created successfully.
			Notification notify =
				new Notification (
					Catalog.GetString ("New task created."), // summary
					Catalog.GetString (taskName), // body
					tasqueIcon);
			Application.ShowAppNotification (notify);
			#endif
			
			
			return task.Id;
		}
		
		/// <summary>
		/// Return an array of Category names.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string[] GetCategoryNames ()
		{
			List<string> categories = new List<string> ();
			var model = Application.Backend.Categories;
			
			foreach (var item in model) {
				if (item is AllCategory)
					continue;
				
				categories.Add (((Category)item).Name);
			}
			
			return categories.ToArray ();
		}
		
		public void ShowTasks ()
		{
			TaskWindow.ShowWindow ();
		}
		
		/// <summary>
		/// Retreives the IDs of all tasks for the current backend.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> array containing the ID of all tasks
		/// in the current backend.
		/// </returns>
		public string[] GetTaskIds ()
		{
			var model = Application.Backend.Tasks2;

			if (model == null)
				return new string[0];

			List<string> ids = new List<string> ();
			foreach (var task in model) {
				ids.Add (task.Id);
			}

			return ids.ToArray ();

		}
		
		/// <summary>
		/// Gets the name of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> the name of the task
		/// </returns>
		public string GetNameForTaskById (string id)
		{
			Task task = GetTaskById (id);
			return task != null ? task.Name : string.Empty;
		}
		
		/// <summary>
		/// Sets the name of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <param>
		/// A <see cref="System.String"/> the name of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for success, false
		/// for failure.
		/// </returns>
		public bool SetNameForTaskById (string id, string name)
		{
			Task task = GetTaskById (id);
			if (task == null)
			{
				return false;
			}
			task.Name = name;
			return true;
		}
		
		/// <summary>
		/// Gets the category of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> the category of the task
		/// </returns>
		public string GetCategoryForTaskById (string id)
		{
			Task task = GetTaskById (id);
			return task != null ? task.Category.Name : string.Empty;
		}
		
		/// <summary>
		/// Sets the category of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <param name="category">
		/// A <see cref="System.String"/> the category of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for success, false
		/// for failure.
		/// </returns>
		public bool SetCategoryForTaskById (string id, string categoryName)
		{
			Task task = GetTaskById (id);
			if (task == null)
				return false;
			
			var categories = Application.Backend.Categories;
			var cat = categories.SingleOrDefault (c => c.Name == categoryName);
			if (cat != null) {
				task.Category = cat;
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// Get the due date of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> containing the POSIX time
		/// of the due date
		/// </returns>
		public int GetDueDateForTaskById (string id)
		{
			Task task = GetTaskById (id);
			if (task == null)
				return -1;
			if (task.DueDate == DateTime.MinValue)
				return 0;
			return (int)(task.DueDate - new DateTime(1970,1,1)).TotalSeconds;
		}
		
		/// <summary>
		/// Set the due date of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <param name="duedate">
		/// A <see cref="System.Int32"/> containing the POSIX time
		/// of the due date
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for success, false
		/// for failure.
		/// <returns>
		public bool SetDueDateForTaskById (string id, int dueDate)
		{
			Task task = GetTaskById (id);
			if (task == null)
			{
				return false;
			}
			if (dueDate == 0)
				task.DueDate = DateTime.MinValue;
			else
				task.DueDate = new DateTime(1970,1,1).AddSeconds(dueDate);
			return true;
		}
		
		/// <summary>
		/// Gets the state of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> the state of the task
		/// </returns>
		public int GetStateForTaskById (string id)
		{
			Task task = GetTaskById (id);
			return task != null ? (int) task.State : -1;
		}

		/// <summary>
		/// Gets the priority of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> the priority of the task
		/// </returns>
		public int GetPriorityForTaskById (string id)
		{
			Task task = GetTaskById (id);
			return task != null ? (int) task.Priority : -1;
		}
		
		/// <summary>
		/// Sets the priority of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <param name="priority">
		/// A <see cref="System.Int32"/> the priority of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for success, false
		/// for failure.
		/// </returns>
		public bool SetPriorityForTaskById (string id, int priority)
		{
			Task task = GetTaskById (id);
			if (task == null)
			{
				return false;
			}
			task.Priority = (TaskPriority) priority;
			return true;
		}
		
		/// <summary>
		/// Marks a task active
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for success, false
		/// for failure.
		/// </returns>
		public bool MarkTaskAsActiveById (string id)
		{
			Task task = GetTaskById (id);
			if (task == null)
				return false;
				
			task.Activate ();
			return true;
		}
		
		/// <summary>
		/// Marks a task complete
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		public void MarkTaskAsCompleteById (string id)
		{
			Task task = GetTaskById (id);
			if (task == null)
				return;
				
			if (task.State == TaskState.Active) {
				// Complete immediately; no timeout or fancy
				// GUI stuff.
				task.Complete ();
			}
		}
		
		/// <summary>
		/// Deletes a task
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true for sucess, false
		/// for failure.
		/// </returns>
		public bool DeleteTaskById (string id)
		{
			Task task = GetTaskById (id);
			if (task == null)
				return false;
				
			task.Delete ();
			return true;
		}

		
		/// <summary>
		/// Looks up a task by ID in the backend
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="ITask"/> having the given ID
		/// </returns>
		private Task GetTaskById (string id)
		{
			return Application.Backend.Tasks2.SingleOrDefault (f => f.Id == id);
		}
	}
}
