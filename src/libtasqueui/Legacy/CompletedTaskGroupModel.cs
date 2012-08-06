// 
// CompletedTaskGroupModel.cs
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
using System.Globalization;
using CollectionTransforms;

namespace Tasque.UIModel.Legacy
{
	public class CompletedTaskGroupModel : TaskGroupModel, IValueConverter<CompletionDateRange, string>
	{
		public CompletedTaskGroupModel (ReadOnlyObservableCollection<Task> tasks,
		    Preferences preferences) : base (TaskGroupName.Completed, tasks)
		{
			if (preferences == null)
				throw new ArgumentNullException ("preferences");
			this.preferences = preferences;
			completionDateRange = GetComplDateRangeFromPrefs ();
			preferences.SettingChanged += HandleSettingChanged;
			
			complDateRangeConverter = new CompletionDateRangeConverter ();
		}
		
		public CompletionDateRange CompletionDateRange {
			get { return completionDateRange; }
			set {
				if (value != completionDateRange) {
					completionDateRange = value;
					preferences.Set (Preferences.CompletedTasksRange, value.ToString ());
					OnPropertyChanged ("CompletionDateRange");
				}
			}
		}
		
		public string Convert (CompletionDateRange value, object parameter, CultureInfo culture)
		{
			return complDateRangeConverter.Convert (value, parameter, culture);
		}

		public CompletionDateRange ConvertBack (string value, object parameter, CultureInfo culture)
		{
			return complDateRangeConverter.ConvertBack (value, parameter, culture);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing)
				preferences.SettingChanged -= HandleSettingChanged;
			base.Dispose (disposing);
		}
		
		object IValueConverter.Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((IValueConverter)complDateRangeConverter).Convert (value, targetType, parameter, culture);
		}
		
		object IValueConverter.ConvertBack (object value, Type targetType,
		                                    object parameter, CultureInfo culture)
		{
			return ((IValueConverter)complDateRangeConverter).ConvertBack (
				value, targetType, parameter, culture);
		}
		
		CompletionDateRange GetComplDateRangeFromPrefs ()
		{
			var val = preferences.Get (Preferences.CompletedTasksRange);
			return (CompletionDateRange)Enum.Parse (typeof (CompletionDateRange), val);
		}
		
		void HandleSettingChanged (Preferences preferences, string settingKey)
		{
			if (settingKey == Preferences.CompletedTasksRange)
				CompletionDateRange = GetComplDateRangeFromPrefs ();
		}
		
		CompletionDateRange completionDateRange;
		CompletionDateRangeConverter complDateRangeConverter;
		Preferences preferences;
	}
}
