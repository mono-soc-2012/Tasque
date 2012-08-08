// 
// NoteModel.cs
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
using CrossCommand;

namespace Tasque.UIModel.Legacy
{
	public class NoteModel : ViewModel
	{
		internal NoteModel (TaskNote note, ViewModel parent) : base (parent)
		{
			if (note == null)
				throw new ArgumentNullException ("note");
			Note = note;
			Text = note.Text;
		}
		
		public bool IsEditable { get; private set; }
		
		public string Text {
			get { return text; }
			set {
				if (value != text) {
					text = value;
					OnPropertyChanged ("Text");
				}
			}
		}
		
		public ICommand Edit {
			get {
				if (edit == null) {
					edit = new RelayCommand () {
						CanExecuteAction = delegate { return !IsEditable; },
						ExecuteAction = delegate {
							IsEditable = true;
							OnPropertyChanged ("IsEditable");
						}
					};
				}
				return edit;
			}
		}
		
		public ICommand Remove {
			get {
				if (remove == null) {
					remove = new RelayCommand () {
						CanExecuteAction = delegate { return !IsEditable; },
						ExecuteAction = delegate { OnClose (); }
					};
				}
				return remove;
			}
		}
		
		public ICommand Cancel {
			get {
				if (cancel == null) {
					cancel = new RelayCommand () {
						CanExecuteAction = delegate { return IsEditable; },
						ExecuteAction = delegate {
							Text = Note.Text;
							IsEditable = false;
							OnPropertyChanged ("IsEditable");
						}
					};
				}
				return cancel;
			}
		}
		
		public ICommand Save {
			get {
				if (save == null) {
					save = new RelayCommand () {
						CanExecuteAction = delegate { return IsEditable; },
						ExecuteAction = delegate {
							Note.Text = Text;
							IsEditable = false;
							OnPropertyChanged ("IsEditable");
						}
					};
				}
				return save;
			}
		}
		
		protected override void OnClose ()
		{
			if (Removed != null)
				Removed (this, EventArgs.Empty);
			base.OnClose ();
		}
		
		internal TaskNote Note { get; private set; }
		
		internal event EventHandler Removed;
		
		string text;
		RelayCommand cancel, edit, remove, save;
	}
}
