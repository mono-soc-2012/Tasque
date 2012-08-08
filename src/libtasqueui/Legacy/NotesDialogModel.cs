// 
// NotesDialogModel.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CrossCommand;

namespace Tasque.UIModel.Legacy
{
	public class NotesDialogModel : ViewModel
	{
		public NotesDialogModel (Task task, ViewModel parent) : base (parent)
		{
			if (task == null)
				throw new ArgumentNullException ("task");
			this.task = task;
			
			Title = "Notes for: " + task.Name;
			
			notes = new ObservableCollection<NoteModel> ();
			foreach (var note in task.Notes)
				AddNote (note);
			Notes = new ReadOnlyObservableCollection<NoteModel> (notes);
			
			((INotifyCollectionChanged)task.Notes).CollectionChanged += HandleNoteCollectionChanged;
		}

		public string Title { get; private set; }
		
		public ReadOnlyObservableCollection<NoteModel> Notes { get; private set; }
		
		public ICommand Add {
			get {
				if (add == null) {
					add = new RelayCommand () {
						ExecuteAction = delegate { task.AddNote (new TaskNote ()); }
					};
				}
				return add;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing)
				((INotifyCollectionChanged)task.Notes).CollectionChanged -= HandleNoteCollectionChanged;
			base.Dispose (disposing);
		}
		
		void AddNote (TaskNote note)
		{
			var noteModel = new NoteModel (note, this);
			noteModel.Removed += (sender, e) => task.RemoveNote (((NoteModel)sender).Note);
			notes.Add (noteModel);
		}
		
		void HandleNoteCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddNote ((TaskNote)e.NewItems [0]);
				break;
			case NotifyCollectionChangedAction.Remove:
				notes.Remove (notes.Single (n => n.Note == (TaskNote)e.OldItems [0]));
				break;
			}
		}
		
		ObservableCollection<NoteModel> notes;
		Task task;
		
		RelayCommand add, close;
	}
}
