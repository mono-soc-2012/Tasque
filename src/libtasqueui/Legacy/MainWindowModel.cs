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
		internal MainWindowModel ()
		{
			if (Backend == null)
				throw new ArgumentNullException ("backend");
			if (Preferences == null)
				throw new ArgumentNullException ("preferences");
			
			UpdateCompletionDateRangeCompareDates ();
			
			this.Backend = Backend;
			this.Preferences = Preferences;
			
			Tasks = new ListCollectionView<Task> (Backend.Tasks);
			Tasks.Filter = Filter;
			Tasks.GroupDescriptions.Add (new PropertyGroupDescription ("IsComplete"));
			var dueDateDesc = new PropertyGroupDescription("DueDate");
            dueDateDesc.Converter = new DueDateConverter();
            Tasks.GroupDescriptions.Add(dueDateDesc);
			Tasks.CustomSort = new TaskComparer(new TaskCompletionDateComparer());
			
			topPanel = new MainWindowTopPanelModel (this);
		}
		
		public CompletionDateRange CompletionDateRange {
			get { return Preferences.CompletionDateRange; }
			set {
				Preferences.CompletionDateRange = value;
				Tasks.Refresh ();
			}
		}
		
		public string Status {
			get { return status; }
			private set {
				if (value != status) {
					status = value;
					OnPropertyChanged ("Status");
				}
			}
		}
		
		public Task SelectedTask {
			get { return selectedTask; }
			set {
				if (value != selectedTask) {
					selectedTask = value;
					OnPropertyChanged ("SelectedTask");
				}
			}
		}
		
		public ICommand ShowContextMenu { get { throw new NotImplementedException (); } }
		
		public void OnDayChanged ()
		{
			UpdateCompletionDateRangeCompareDates ();
			Tasks.Refresh ();
		}
		
		internal Backend Backend { get; private set; }
		
		internal Preferences Preferences { get; private set; }
		
		internal ListCollectionView<Task> Tasks { get; private set; }
		
		bool Filter (Task task)
		{
			// account for selected category
			var selectedCategory = topPanel.SelectedCategory;
			if (selectedCategory != null && !selectedCategory.Contains (task))
				return false;
			
			// account for show completed tasks setting
			if (!task.IsComplete)
				return true;
			
			if (!Preferences.ShowCompletedTasks)
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
		
		DateTime aYearAgo;
		DateTime aMonthAgo;
		DateTime aWeekAgo;
		DateTime yesterday;
		
		string status;
		
		MainWindowTopPanelModel topPanel;
		MainWindowContextMenuModel contextMenu;
		
		Task selectedTask;
	}
}
