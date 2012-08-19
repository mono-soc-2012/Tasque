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
using System.Collections.Generic;
using Tasque.Backends.Dummy.Gtk;

namespace Tasque.Backends.Dummy
{
	public class DummyBackend : Backend
	{
		public DummyBackend () : base ("Debugging System")
		{
			Configured = true;
		}
		
		public override Category DefaultCategory {
			get {
				if (!Initialized)
					throw new InvalidOperationException ("Backend not initialized");
				return defaultCategory;
			}
			set {
				if (!Initialized)
					throw new InvalidOperationException ("Backend not initialized");
				if (value == null)
					throw new ArgumentNullException ("value");
				if (!Categories.Contains (value))
					throw new ArgumentException ("Default category must be one of Categories.");
				defaultCategory = value;
			}
		}
		
		#region Public Methods		
		public override void Refresh () {}
		
		public override void Initialize ()
		{			
			//
			// Add in some fake categories
			//
			var homeCategory = new DummyCategory ("Home");
			Categories.Add (homeCategory);
			
			var workCategory = new DummyCategory ("Work");
			Categories.Add (workCategory);
			defaultCategory = workCategory;
			
			var projectsCategory = new DummyCategory ("Projects");
			Categories.Add (projectsCategory);
			
			//
			// Add in some fake tasks
			//
			var task = CreateTask ("Buy some nails");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			projectsCategory.Add (task);
			
			task = CreateTask ("Call Roger");
			task.DueDate = DateTime.Now.AddDays (-1);
			task.Complete ();
			homeCategory.Add (task);
			
			task = CreateTask ("Replace burnt out lightbulb");
			task.DueDate = DateTime.Now;
			task.Priority = TaskPriority.Low;
			homeCategory.Add (task);
			
			task = CreateTask ("File taxes");
			task.DueDate = new DateTime (2008, 4, 1);
			homeCategory.Add (task);
			
			task = CreateTask ("Purchase lumber");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			projectsCategory.Add (task);
			
			task = CreateTask ("Estimate drywall requirements");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Low;
			projectsCategory.Add (task);
			workCategory.Add (task);
			
			task = CreateTask ("Borrow framing nailer from Ben");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			projectsCategory.Add (task);
			homeCategory.Add (task);
			
			task = CreateTask ("Call for an insulation estimate");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			projectsCategory.Add (task);
			
			task = CreateTask ("Pay storage rental fee");
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.None;
			homeCategory.Add (task);
			
			task = new DummyTask ("Place carpet order");
			projectsCategory.Add (task);
			task.Priority = TaskPriority.None;
			
			task = new DummyTask ("Test task overdue");
			workCategory.Add (task);
			projectsCategory.Add (task);
			task.DueDate = DateTime.Now.AddDays (-89);
			task.Priority = TaskPriority.None;
			task.Complete ();
			
			Initialized = true;
		}
		
		public override IBackendPreferences Preferences
		{
			get {
				// TODO: Replace this with returning null once things are going
				// so that the Preferences Dialog doesn't waste space.
				return new DummyPreferences ();
			}
		}
		#endregion

		public override Task CreateTask (string taskName)
		{
			return new DummyTask (taskName);
		}
		
		Category defaultCategory;
	}
}
