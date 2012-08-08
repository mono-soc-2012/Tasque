// 
// TaskModel.cs
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
using System.ComponentModel;
using CrossCommand;

namespace Tasque.UIModel.Legacy
{
	public class TaskModel : ViewModel
	{
		internal TaskModel (Task task, ViewModel parent) : base (parent)
		{
			if (task == null)
				throw new ArgumentNullException ("task");
			this.task = task;
			task.PropertyChanged += HandleTaskPropertyChanged;
		}
		
		public bool IsComplete {
			get { return task.IsComplete; }
			set {
				if (value == task.IsComplete)
					return;
				
				if (value)
					task.Complete ();
				else
					task.Activate ();
			}
		}
		
		public DateTime DueDate {
			get { return task.DueDate; }
		}
		
		public string Name { get { return task.Name; } set { task.Name = value; } }
		
		public int Priority { get { return (int)task.Priority; } set { task.Priority = value; } }
		
		public NotesDialogModel NotesDialogModel { get; private set; }
		
		public ICommand ShowNoteDialog {
			get {
				if (showNoteDialog == null) {
					showNoteDialog = new RelayCommand () {
						ExecuteAction = delegate {
							NotesDialogModel = new NotesDialogModel ();
						}
					};
				}
				return showNoteDialog;
			}
		} 
		
		void HandleTaskPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged (e.PropertyName);
		}
		
		RelayCommand showNoteDialog;
		Task task;
	}
}
