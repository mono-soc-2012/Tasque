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
		
		#region DueDate
		public DateTime DueDate { get { return task.DueDate; } }
		
		public DueDateOptionsModel DueDateOptions { get; private set; }
		
		public ICommand ShowDueDateOptions {
			get {
				if (showDueDateOptions == null) {
					showDueDateOptions = new RelayCommand () {
						CanExecuteAction = delegate { return !task.IsComplete; },
						ExecuteAction = delegate {
							var optionsModel = GetObjectFromAncestor (
								typeof (DueDateOptionsModel)) as DueDateOptionsModel;
							DueDateOptions = optionsModel ?? new DueDateOptionsModel (this);
							DueDateOptions.OptionSelected += HandleDueDateOptionSelected;
							OnPropertyChanged ("DueDateOptions");
						}
					};
				}
				return showDueDateOptions;
			}
		}

		void HandleDueDateOptionSelected (object sender, EventArgs e)
		{
			DueDateOptions.OptionSelected -= HandleDueDateOptionSelected;
			task.DueDate = DueDateOptions.SelectedOption.Value;
		}
		#endregion
		
		#region IsComplete
		public ICommand ToogleIsComplete {
			get {
				if (toggleIsComplete == null) {
					toggleIsComplete = new RelayCommand () {
						ExecuteAction = delegate {
							if (task.IsComplete)
								task.Activate ();
							else
								task.Complete ();
						}
					};
				}
				return toggleIsComplete;
			}
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
		#endregion
		
		#region Name
		public string Name {
			get { return name; }
			set {
				if (value != name) {
					name = value;
					OnPropertyChanged ("Name");
				}
			}
		}
		
		public ICommand SaveName {
			get {
				if (saveName == null) {
					saveName = new RelayCommand () {
						CanExecuteAction = delegate { return !string.IsNullOrWhiteSpace (Name); },
						ExecuteAction = delegate { task.Name = name; }
					};
				}
				return saveName;
			}
		}
		
		public ICommand CancelChangeName {
			get {
				if (cancelChangeName == null) {
					cancelChangeName = new RelayCommand () {
						ExecuteAction = delegate { Name = task.Name; }
					};
				}
				return cancelChangeName;
			}
		}
		#endregion
		
		#region Notes
		public NotesDialogModel NotesDialogModel { get; private set; }
		
		public string NotesIcon { get { return "hicolor_animations_16x16_notebook"; } }
		
		public ICommand ShowNotesDialog {
			get {
				if (showNotesDialog == null) {
					showNotesDialog = new RelayCommand () {
						ExecuteAction = delegate {
							NotesDialogModel = new NotesDialogModel (task, this);
							NotesDialogModel.Closed += delegate { NotesDialogModel = null; };
							OnPropertyChanged ("NotesDialogModel");
						}
					};
				}
				return showNotesDialog;
			}
		} 
		#endregion
		
		#region Priority
		public OptionsModel<TaskPriority> PriorityOptions { get; private set; }
		
		public int Priority { get { return (int)task.Priority; } }
		
		public ICommand ShowPriorityOptions {
			get {
				return setPriority ?? (setPriority = new RelayCommand () {
					CanExecuteAction = delegate { return !task.IsComplete; },
					ExecuteAction = delegate {
						var optionsModel = GetObjectFromAncestor (
							typeof (OptionsModel<TaskPriority>)) as OptionsModel<TaskPriority>;
						PriorityOptions = optionsModel ?? new OptionsModel<TaskPriority> (this);
						PriorityOptions.OptionSelected += HandlePriorityOptionSelected;
						OnPropertyChanged ("PriorityOptions");
					}
				});
			}
		}
		
		void HandlePriorityOptionSelected (object sender, EventArgs e)
		{
			PriorityOptions.OptionSelected -= HandlePriorityOptionSelected;
			task.Priority = PriorityOptions.SelectedOption.Value;			
		}
		#endregion
		
		void HandleTaskPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged (e.PropertyName);
		}
		
		string name;
		int priority;
		RelayCommand toggleIsComplete, saveName, setPriority,
			cancelChangeName, showNotesDialog, showDueDateOptions;
		Task task;
	}
}
