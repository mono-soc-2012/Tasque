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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CollectionTransforms;
using CrossCommand;

namespace Tasque.UIModel.Legacy
{
	public class MainWindowModel : ViewModel, ITimeAware
	{
		internal MainWindowModel (Preferences preferences)
		{
			if (preferences == null)
				throw new ArgumentNullException ("preferences");
			
			UpdateCompletionDateRangeCompareDates ();
			
			Preferences = preferences;
			showCompletedTasks = preferences.GetBool (Preferences.ShowCompletedTasksKey);
			preferences.SettingChanged += HandleSettingChanged;
			
			Tasks = new ListCollectionView<Task> (Backend.Tasks);
			using (Tasks.DeferRefresh ()) {
				Tasks.Filter = Filter;
				Tasks.GroupDescriptions.Add (new PropertyGroupDescription (null, new TaskGroupConverter ()));
				Tasks.CustomSort = new TaskComparer ();
			}
			
			topPanel = new MainWindowTopPanelModel (this);
		}
		
		public ReadOnlyObservableCollection<TaskGroupModel> Groups {
			get {
				if (groups == null) {
					privateGroups = new ObservableCollection<TaskGroupModel> ();
					foreach (CollectionViewGroup<Task> group in Tasks.Groups)
						privateGroups.Add (CreateGroupModel (group));
					
					groups = new ReadOnlyObservableCollection<TaskGroupModel> (privateGroups);
					((INotifyCollectionChanged)Tasks.Groups).CollectionChanged += HandleGroupsChanged;
				}
				
				return groups;
			}
		}
		
		void HandleSettingChanged (Preferences preferences, string settingKey)
		{
			switch (settingKey) {
			case Preferences.ShowCompletedTasksKey:
				showCompletedTasks = preferences.GetBool (settingKey);
				Tasks.Refresh ();
				OnPropertyChanged ("ShowCompletedTasks");
				break;
			}
		}
		
		bool isVisible;
		public bool IsVisible {
			get { return isVisible; }
			set {
				if (value != isVisible) {
					isVisible = value;
					OnPropertyChanged ("IsVisible");
				}
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
		
		public bool ShowCompletedTasks { get { return showCompletedTasks; } }
		
		public RelayCommand Hide { get { return hide ?? (hide = new RelayCommand ()); } }
		
		public RelayCommand Show {	get { return show ?? (show = new RelayCommand ());	} }
		
		public Point Position { get; set; }
		
		public ICommand ShowContextMenu { get { throw new NotImplementedException (); } }
		
		void ITimeAware.OnDayChanged ()
		{
			UpdateCompletionDateRangeCompareDates ();
			Tasks.Refresh ();
		}
		
		internal Backend Backend { get; set; }
		
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
			
			if (!showCompletedTasks)
				return false;
			
			// account for completion date range setting.
			//NOTE: the following code fails if showCompletedTasks is true. This is not good class design
			var complDateRange = completedTaskGroupModel.CompletionDateRange;
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
		
		TaskGroupModel CreateGroupModel (CollectionViewGroup<Task> group)
		{
			TaskGroupModel groupModel;
			var groupName = (TaskGroupName)group.Name;
			if (groupName == TaskGroupName.Completed)
				groupModel = new CompletedTaskGroupModel (group.Items, Preferences);
			else
				groupModel = new TaskGroupModel (groupName, group.Items);
			return groupModel;
		}
		
		void HandleGroupsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				var newGroup = (CollectionViewGroup<Task>)e.NewItems [0];
				var newIndex = e.NewStartingIndex < 0 ? privateGroups.Count : e.NewStartingIndex;
				privateGroups.Insert (newIndex, CreateGroupModel (newGroup));
				break;
			case NotifyCollectionChangedAction.Remove:
				var oldIndex = e.OldStartingIndex < 0 ? privateGroups.Count - 1 : e.OldStartingIndex;
				privateGroups.RemoveAt (oldIndex);
				break;
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
		
		DateTime aYearAgo, aMonthAgo, aWeekAgo, yesterday;
		
		string status;
		
		MainWindowTopPanelModel topPanel;
		TaskContextMenuModel contextMenu;
		TrayModel tray;
		AboutDialogModel aboutDialog;
		PreferencesDialogModel preferencesDialog;
		NotesDialogModel noteDialog;
		
		Task selectedTask;
		
		RelayCommand hide, show;
		
		bool showCompletedTasks;
		CompletedTaskGroupModel completedTaskGroupModel;
		
		ObservableCollection<TaskGroupModel> privateGroups;
		ReadOnlyObservableCollection<TaskGroupModel> groups;
	}
}
