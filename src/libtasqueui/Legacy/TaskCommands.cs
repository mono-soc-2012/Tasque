// 
// TaskCommands.cs
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
using System.Text;

namespace Tasque.UIModel.Legacy
{
	class AddTaskCommand : CommandBase
	{
		public AddTaskCommand (Backend backend)
		{
			if (backend == null)
				throw new ArgumentNullException ("backend");
			this.backend = backend;
			category = backend.DefaultCategory;
			UpdateCanExecute ();
		}
		
		public string TaskName {
			get { return taskName; }
			set {
				if (value != taskName) {
					taskName = value;
					UpdateCanExecute ();
				}
			}
		}
		
		public Category Category {
			get { return category; }
			set {
				if (value != category) {
					category = value ?? backend.DefaultCategory;
					UpdateCanExecute ();
				}
			}
		}
		
		public override void Execute ()
		{
			if (CanExecute)
				backend.CreateTask (taskName, category);
		}
		
		void UpdateCanExecute ()
		{
			var isTaskNameValid = !string.IsNullOrWhiteSpace (taskName);
			var isCategoryValid = backend.Categories.Contains (category);
			
			if (isTaskNameValid && isCategoryValid)
				SetCanExecute ();
			else {
				var errMsg = new StringBuilder ();
				if (!isTaskNameValid) {
					errMsg.AppendLine ("TaskName must not be null or white space.");
					if (!isCategoryValid)
						errMsg.AppendLine ("Category doesn't belong to backend " + backend.Name);
				}
				UnsetCanExecute (errMsg.ToString ());
			}
		}
		
		Backend backend;
		Category category;
		string taskName;
	}
	
	class RemoveTaskCommand : CommandBase
	{
		public RemoveTaskCommand (Backend backend)
		{
			if (backend == null)
				throw new ArgumentNullException ("backend");
			this.backend = backend;
		}
		
		public Task Task {
			get { return task; }
			set {
				if (value == task)
					return;
				
				task = value;
				if (value == null || !backend.Tasks.Contains (value))
					UnsetCanExecute ("Task doesn't belong to backend " + backend.Name);
				else
					SetCanExecute ();
			}
		}
		
		public override void Execute ()
		{
			if (CanExecute)
				backend.DeleteTask (task);
		}
		
		Backend backend;
		Task task;
	}
}
