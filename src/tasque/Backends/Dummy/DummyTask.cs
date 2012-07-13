// DummyTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008
// 
// DummyTask.cs
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
using Tasque;
using System.Collections.ObjectModel;

namespace Tasque.Backends.Dummy
{
	public class DummyTask : Task
	{
		public DummyTask(string name) : base (name)
		{
			notes = new ObservableCollection<TaskNote> ();
			Notes = new ReadOnlyObservableCollection<TaskNote> (notes);
		}
		
		#region Public Properties
		public override DateTime CompletionDate
		{
			get { return completionDate; }
			set {
				if (value != completionDate) {
					Logger.Debug ("Setting new task completion date");
					completionDate = value;
					OnPropertyChanged ("CompletionDate");
				}
			}
		}

		public override DateTime DueDate
		{
			get { return dueDate; }
			set {
				if (value != dueDate) {
					Logger.Debug ("Setting new task due date");
					dueDate = value;
					OnPropertyChanged ("DueDate");
				}
			}
		}

		public override ReadOnlyObservableCollection<TaskNote> Notes { get; private set; }

		public override TaskPriority Priority
		{
			get { return priority; }
			set {
				if (value != priority) {
					Logger.Debug ("Setting new task priority");
					priority = value;
					OnPropertyChanged ("Priority");
				}
			}
		}

		public override TaskState State {
			get { return state; }
			set {
				if (state != value) {
					state = value;
					OnPropertyChanged ("State");
				}
			}
		}

		public override TaskNoteSupport TaskNoteSupport { get { return TaskNoteSupport.Multiple; } }
		#endregion
		
		#region Public Methods
		public override void Activate ()
		{
			Logger.Debug ("DummyTask.Activate ()");
			completionDate = DateTime.MinValue;
			State = TaskState.Active;
		}
		
		public override void Complete ()
		{
			Logger.Debug ("DummyTask.Complete ()");
			CompletionDate = DateTime.Now;
			State = TaskState.Completed;
		}
		
		public override void Delete ()
		{
			Logger.Debug ("DummyTask.Delete ()");
			State = TaskState.Deleted;
		}
		
		public override TaskNote CreateNote(string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			var note = new TaskNote (text);
			notes.Add (note);
			return note;
		}
		
		public override void DeleteNote(TaskNote note)
		{
		}

		public override void SaveNote(TaskNote note)
		{
		}
		#endregion

		DateTime completionDate;
		DateTime dueDate;
		ObservableCollection<TaskNote> notes;
		TaskPriority priority;
		TaskState state;
	}
}
