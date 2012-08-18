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
using Tasque.Backends.Dummy.Gtk;

namespace Tasque.Backends.Dummy
{
	public class DummyBackend : Backend
	{
		public DummyBackend () : base ("Debugging System")
		{
			Configured = true;
		}
		
		#region Public Methods		
		public override void Refresh () {}
		
		public override void Initialize ()
		{			
			//
			// Add in some fake categories
			//
			var homeCategory = new Category ("Home");
			Categories.Add (homeCategory);
			
			var workCategory = new Category ("Work");
			Categories.Add (workCategory);
			
			var projectsCategory = new Category ("Projects");
			Categories.Add (projectsCategory);
			
			//
			// Add in some fake tasks
			//
			var task = CreateTask ("Buy some nails", projectsCategory);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			
			task = CreateTask ("Call Roger", homeCategory);
			task.DueDate = DateTime.Now.AddDays (-1);
			task.Complete ();
			task.CompletionDate = task.DueDate;
			
			task = CreateTask ("Replace burnt out lightbulb", homeCategory);
			task.DueDate = DateTime.Now;
			task.Priority = TaskPriority.Low;
			
			task = CreateTask ("File taxes", homeCategory);
			task.DueDate = new DateTime (2008, 4, 1);
			
			task = CreateTask ("Purchase lumber", projectsCategory);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
						
			task = CreateTask ("Estimate drywall requirements", new Category [] { projectsCategory, workCategory });
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Low;
			
			task = CreateTask ("Borrow framing nailer from Ben", new Category [] { projectsCategory, homeCategory });
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.High;
			
			task = CreateTask ("Call for an insulation estimate", projectsCategory);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.Medium;
			
			task = CreateTask ("Pay storage rental fee", homeCategory);
			task.DueDate = DateTime.Now.AddDays (1);
			task.Priority = TaskPriority.None;
			
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

		public override void Cleanup () {}
		
		public override IBackendPreferences Preferences
		{
			get {
				// TODO: Replace this with returning null once things are going
				// so that the Preferences Dialog doesn't waste space.
				return new DummyPreferences ();
			}
		}
		#endregion

		protected override Task CreateTaskCore (string taskName)
		{
			return new DummyTask (taskName);
		}
	}
}
