// 
// MainWindowTopPanelModel.cs
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
using System.Linq;
using CollectionTransforms;

namespace Tasque.UIModel.Legacy
{
	public class MainWindowTopPanelModel : ViewModelBase
	{
		internal MainWindowTopPanelModel (MainWindowModel mainWindowModel)
		{
			tasks = mainWindowModel.Tasks;
			
			Categories = new ReadOnlySortedNotifyCollection<Category> (mainWindowModel.Backend.Categories);
			
			preferences = mainWindowModel.Preferences;
			selectedCategory = GetSelectedCategory ();
			preferences.SettingChanged += HandleSettingChanged;
			
			addTaskCommand = new AddTaskCommand (mainWindowModel.Backend);
		}

		void HandleSettingChanged (Preferences preferences, string settingKey)
		{
			switch (settingKey) {
			case Preferences.SelectedCategoryKey:
				selectedCategory = GetSelectedCategory ();
				tasks.Refresh ();
				OnPropertyChanged ("SelectedCategory");
				break;
			}
		}
		
		public ReadOnlySortedNotifyCollection<Category> Categories { get; private set; }
		
		public Category SelectedCategory {
			get { return selectedCategory; }
			set {
				if (value != selectedCategory)
					preferences.Set (Preferences.SelectedCategoryKey, value.Name);
			}
		}
		
		public ICommand AddTask { get { return addTaskCommand; } }
		
		public string NewTaskName {
			get { return addTaskCommand.TaskName; }
			set {
				if (value != addTaskCommand.TaskName) {
					addTaskCommand.TaskName = value;
					OnPropertyChanged ("NewTaskName");
				}
			}
		}
		
		public Category NewTaskCategory {
			get { return addTaskCommand.Category; }
			set {
				if (value != addTaskCommand.Category) {
					addTaskCommand.Category = value;
					OnPropertyChanged ("NewTaskCategory");
				}
			}
		}
		
		void GetSelectedCategory ()
		{
			var selectedCategoryName = preferences.Get (Preferences.SelectedCategoryKey);
			return Categories.SingleOrDefault (c => c.Name == selectedCategoryName);
		}
		
		Preferences preferences;
		ListCollectionView<Task> tasks;
		
		AddTaskCommand addTaskCommand;
		
		Category selectedCategory;
	}
}
