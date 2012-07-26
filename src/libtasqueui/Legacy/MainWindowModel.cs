// 
// MainWindowModel.cs
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
using CollectionTransforms;

namespace Tasque.UIModel.Legacy
{
	public class MainWindowModel : ViewModelBase, ITimeAware
	{
		public MainWindowModel (Backend backend, Preferences preferences)
		{
			if (backend == null)
				throw new ArgumentNullException ("backend");
			if (preferences == null)
				throw new ArgumentNullException ("preferences");
			
			UpdateCompletionDateRangeCompareDates ();
			
			this.backend = backend;
			this.preferences = preferences;
			
			Categories = new ReadOnlySortedNotifyCollection<Category> (backend.Categories);
			
			tasks = new ListCollectionView<Task> (backend.Tasks);
			tasks.Filter = Filter;
			tasks.GroupDescriptions.Add (new PropertyGroupDescription ("IsComplete"));
			var dueDateDesc = new PropertyGroupDescription("DueDate");
            dueDateDesc.Converter = new DueDateConverter();
            tasks.GroupDescriptions.Add(dueDateDesc);
			tasks.CustomSort = new TaskComparer(new TaskCompletionDateComparer());
			
			addTaskCommand = new AddTaskCommand (backend);
			
		}
		
		public CompletionDateRange CompletionDateRange {
			get { return preferences.CompletionDateRange; }
			set {
				preferences.CompletionDateRange = value;
				tasks.Refresh ();
			}
		}
		
		public ReadOnlySortedNotifyCollection<Category> Categories { get; private set; }
		
		public Category SelectedCategory {
			get { return preferences.SelectedCategory; }
			set {
				if (value != null && !Categories.Contains (value))
					value = null;
				preferences.SelectedCategory = value;
				tasks.Refresh ();
				OnPropertyChanged ("SelectedCategory");
			}
		}
		
		public ICommand AddTask { get { return addTaskCommand; } }
		
		public string NewTaskName {
			get { return newTaskName; }
			set {
				if (value != newTaskName) {
					newTaskName = value;
					addTaskCommand.TaskName = value;
					OnPropertyChanged ("NewTaskName");
				}
			}
		}
		
		public Category NewTaskCategory {
			get { return newTaskCategory; }
			set {
				if (value != newTaskCategory) {
					newTaskCategory = value;
					addTaskCommand.Category = value;
					OnPropertyChanged ("NewTaskCategory");
				}
			}
		}
		
		public ICommand RemoveTask { get { throw new NotImplementedException (); } }
		
		public string Status {
			get { return status; }
			private set {
				if (value != status) {
					status = value;
					OnPropertyChanged ("Status");
				}
			}
		}
		
		public void OnDayChanged ()
		{
			UpdateCompletionDateRangeCompareDates ();
			tasks.Refresh ();
		}
		
		bool Filter (Task task)
		{
			// account for selected category
			var selectedCategory = SelectedCategory;
			if (selectedCategory != null && !selectedCategory.Contains (task))
				return false;
			
			// account for show completed tasks setting
			if (!task.IsComplete)
				return true;
			
			if (!preferences.ShowCompletedTasks)
				return false;
			
			// account for completion date range setting
			var complDateRange = CompletionDateRange;
			switch (complDateRange) {
			case CompletionDateRange.All:
				return true;
			case CompletionDateRange.LastYear:
				return task.CompletionDate >= aYearAgo;
			case CompletionDateRange.LastMonth:
				return task.CompletionDate >= aMonthAgo;
			case CompletionDateRange.Last7Days:
				return task.CompletionDate >= aWeekAgo;
			case CompletionDateRange.Yesterday:
				return task.CompletionDate >= yesterday;
			default:
				return true;
			}
		}
		
		void UpdateCompletionDateRangeCompareDates ()
		{
			var today = DateTime.Today.Date;
			aYearAgo = today.AddYears (-1);
			aMonthAgo = today.AddMonths (-1);
			aWeekAgo = today.AddDays (-7);
			yesterday = today.AddDays (-1);
		}
		
		Backend backend;
		Preferences preferences;
		ListCollectionView<Task> tasks;
		
		DateTime aYearAgo;
		DateTime aMonthAgo;
		DateTime aWeekAgo;
		DateTime yesterday;
		
		string status;
		
		AddTaskCommand addTaskCommand;
		string newTaskName;
		Category newTaskCategory;
		
		RemoveTaskCommand removeTaskCommand;
	}
}
