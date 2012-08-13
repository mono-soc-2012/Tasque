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
using CrossCommand;
using System.Collections.ObjectModel;
using System;
using CrossCommand.Generic;

namespace Tasque.UIModel.Legacy
{
	public class MainWindowTopPanelModel : ViewModel
	{
		internal MainWindowTopPanelModel (MainWindowModel mainWindowModel, ViewModel parent)
			: base (parent)
		{
			tasks = mainWindowModel.Tasks;
			
//			Categories = new ReadOnlyObservableSet<Category> (mainWindowModel.Backend.Categories);
			
			preferences = mainWindowModel.Preferences;
//			selectedCategory = GetSelectedCategory ();
			preferences.SettingChanged += HandleSettingChanged;
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
		
		#region Categories
		public OptionsModel<Category> Categories { get; private set; }
		
		public Category SelectedCategory {
			get { return selectedCategory; }
			private set {
				if (value != selectedCategory)
					preferences.Set (Preferences.SelectedCategoryKey, value.Name);
			}
		}
		
		public ICommand ShowCategories {
			get {
				return showCategories ?? (showCategories = new RelayCommand () {
					ExecuteAction = delegate {
						var cats = new Category [categories.Count + 1];
						cats [0] = null; // All category
						categories.CopyTo (cats, 1); // other categories
						Categories = new OptionsModel<Category> (cats, null, this);
						Categories.OptionSelected += delegate {
							SelectedCategory = Categories.SelectedOption.Value;
						};
					}
				});
			}
		}
		#endregion
		
		#region Add task
		public ICommand AddTask {
			get {
				return addTask ?? (addTask = new RelayCommand () {
					CanExecuteAction = delegate {
						return false;
					}
				});
			}
		}
		
		
		
		public ICommand<Category> AddTaskToCategory {
			get {
				throw new NotImplementedException ();
			}
		}
		
//		public string NewTaskName {
//			get { return addTaskCommand.TaskName; }
//			set {
//				if (value != addTaskCommand.TaskName) {
//					addTaskCommand.TaskName = value;
//					OnPropertyChanged ("NewTaskName");
//				}
//			}
//		}
		
//		public Category NewTaskCategory {
//			get { return addTaskCommand.Category; }
//			set {
//				if (value != addTaskCommand.Category) {
//					addTaskCommand.Category = value;
//					OnPropertyChanged ("NewTaskCategory");
//				}
//			}
//		}
		#endregion
		
		Category GetSelectedCategory ()
		{
			var selectedCategoryName = preferences.Get (Preferences.SelectedCategoryKey);
			return categories.SingleOrDefault (c => c.Name == selectedCategoryName);
		}
		
		Preferences preferences;
		ListCollectionView<Task> tasks;
		
		RelayCommand addTask;
		
		Category selectedCategory;
		ReadOnlyObservableCollection<Category> categories;
		RelayCommand showCategories;
	}
}
