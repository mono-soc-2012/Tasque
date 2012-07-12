// DummyBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008
// 
// DummyBackend.cs
//  
// Author:
//       Antonius Riha <antoniusriha@gmail.com>
// 
// Copyright (c) 2012 Antonius Riha
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using Tasque.Backends;
using Tasque.Backends.Dummy.Gtk;

namespace Tasque.Backends.Dummy
{
	public class DummyBackend : Backend
	{
		/// <summary>
		/// Keep track of the Gtk.TreeIters for the tasks so that they can
		/// be referenced later.
		///
		/// Key   = Task ID
		/// Value = Gtk.TreeIter in taskStore
		/// </summary>
		int newTaskId;
		bool initialized;
		bool configured = true;

		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;
		
		Category homeCategory;
		Category workCategory;
		Category projectsCategory;
		
		public DummyBackend () : base ()
		{
			initialized = false;
			newTaskId = 0;
		}
		
		#region Public Properties
		public string Name { get { return "Debugging System"; } }
		
		/// <value>
		/// All the tasks including ITaskDivider items.
		/// </value>
		public IEnumerable SortedTasks { get; private set; }
		
		/// <value>
		/// This returns all the task lists (categories) that exist.
		/// </value>
		public IEnumerable SortedCategories { get; private set; }
		
		public ObservableCollection<ICategory> Categories { get; private set; }
		
		/// <value>
		/// Indication that the dummy backend is configured
		/// </value>
		public bool Configured { get { return configured; } }
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized { get { return initialized; } }		
		#endregion // Public Properties
		
		#region Public Methods
		public Task CreateTask (string taskName, ICategory category)
		{
			// not sure what to do here with the category
			DummyTask task = new DummyTask (this, newTaskId, taskName);
			
			// Determine and set the task category
			if (category == null || category is AllCategory)
				task.Category = workCategory; // Default to work
			else
				task.Category = category;
			
			tasks.Add (task);
			newTaskId++;
			
			return task;
		}
		
		public void DeleteTask (Task task)
		{
		}
		
		public void Refresh ()
		{
		}
		
		public void Initialize ()
		{
			//
			// Add in the "All" Category
			//
			AllCategory allCategory = new AllCategory ();
			Categories.Add (allCategory);
			
			//
			// Add in some fake categories
			//
			homeCategory = new Category ("Home");
			Categories.Add (homeCategory);
			
			workCategory = new Category ("Work");
			Categories.Add (workCategory);
			
			projectsCategory = new Category ("Projects");
			Categories.Add (projectsCategory);
			
			//
			// Add in some fake tasks
			//
			
			DummyTask task = new DummyTask (this, newTaskId, "Buy some nails");
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Call Roger");
			homeCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (-1);
			task.Complete ();
			task.CompletionDate = task.DueDate;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Replace burnt out lightbulb");
			homeCategory.Add (task);
			task.DueDate = DateTime.Now;
			task.Priority = TaskPriority.Low;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "File taxes");
			homeCategory.Add (task);
			task.DueDate = new DateTime (2008, 4, 1);
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Purchase lumber");
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			tasks.Add (task);
			newTaskId++;
						
			task = new DummyTask (this, newTaskId, "Estimate drywall requirements");
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Low;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Borrow framing nailer from Ben");
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Call for an insulation estimate");
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Pay storage rental fee");
			homeCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.None;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Place carpet order");
			projectsCategory.Add (task);
			task.Priority = TaskPriority.None;
			tasks.Add (task);
			newTaskId++;
			
			task = new DummyTask (this, newTaskId, "Test task overdue");
			workCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (-89);
			task.Priority = TaskPriority.None;
			task.Complete ();
			tasks.Add (task);
			newTaskId++;
			
			initialized = true;
			if (BackendInitialized != null) {
				BackendInitialized ();
			}		
		}

		public void Cleanup ()
		{
		}
		
		public IBackendPreferences Preferences
		{
			get {
				// TODO: Replace this with returning null once things are going
				// so that the Preferences Dialog doesn't waste space.
				return new DummyPreferences ();
			}
		}
		#endregion // Public Methods
		
		ObservableCollection<Task> tasks;
	}
}
