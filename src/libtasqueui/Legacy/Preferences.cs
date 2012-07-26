// 
// Preferences.cs
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

namespace Tasque.UIModel.Legacy
{
	public class Preferences : INotifyPropertyChanged
	{
		public CompletionDateRange CompletionDateRange {
			get { return completionDateRange; }
			set {
				if (value != completionDateRange) {
					completionDateRange = value;
					OnPropertyChanged ("CompletionDateRange");
				}
			}
		}
		
		public bool ShowCompletedTasks {
			get { return showCompletedTasks; }
			set {
				if (value != showCompletedTasks) {
					showCompletedTasks = value;
					OnPropertyChanged ("ShowCompletedTasks");
				}
			}
		}
		
		public Category SelectedCategory {
			get { return selectedCategory; }
			set {
				if (value != selectedCategory) {
					selectedCategory = value;
					OnPropertyChanged ("SelectedCategory");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

		CompletionDateRange completionDateRange;
		bool showCompletedTasks;
		Category selectedCategory;
	}
}
