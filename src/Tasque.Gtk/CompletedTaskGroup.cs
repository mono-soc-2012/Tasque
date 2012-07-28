// CompletedTaskGroup.cs created with MonoDevelop
// User: boyd at 12:34 AM 3/1/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
// 
// CompletedTaskGroup.cs
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
using Gtk;
using Mono.Unix;
using System.Linq;
using System.Collections;
using CollectionTransforms;
using System.ComponentModel;
using Tasque.UIModel.Legacy;
using System.Collections.Generic;

namespace Tasque
{
	public class CompletedTaskGroup : TaskGroup
	{
		/// <summary>
		/// The purpose of this method is to allow the CompletedTaskGroup to show
		/// completed tasks in reverse order (i.e., most recently completed tasks
		/// at the top of the list).
		/// </summary>
		static IEnumerable<Task> GetSortedTasks (IEnumerable<Task> tasks)
		{
			var cv = new CollectionView<Task> (tasks);
			cv.SortDescriptions.Add (new SortDescription ("CompletionDate", ListSortDirection.Descending));
			return cv;
		}
		
		Category selectedCategory;
		HScale rangeSlider;
		CompletionDateRange currentRange;
		
		public CompletedTaskGroup (string groupName, DateTime rangeStart,
								   DateTime rangeEnd, IEnumerable<Task> tasks)
			: base (groupName, rangeStart, rangeEnd, GetSortedTasks (tasks))
		{
			// Don't hide this group when it's empty because then the range
			// slider won't appear and the user won't be able to customize the
			// range.
			this.HideWhenEmpty = false;
			
			selectedCategory = GetSelectedCategory ();
			Application.Preferences.SettingChanged += OnSelectedCategorySettingChanged;
			
			CreateRangeSlider ();
			UpdateDateRanges ();
		}
		
		/// <summary>
		/// Create and show a slider (HScale) that will allow the user to
		/// customize how far in the past to show completed items.
		/// </summary>
		private void CreateRangeSlider ()
		{
			// There are five (5) different values allowed here:
			// "Yesterday", "Last7Days", "LastMonth", "LastYear", or "All"
			// Create the slider with 5 distinct "stops"
			rangeSlider = new HScale (0, 4, 1);
			rangeSlider.SetIncrements (1, 1);
			rangeSlider.WidthRequest = 100;
			rangeSlider.DrawValue = true;
			
			// TODO: Set the initial value and range
			string rangeStr =
				Application.Preferences.Get (Preferences.CompletedTasksRange);
			if (rangeStr == null) {
				// Set a default value of All
				rangeStr = CompletionDateRange.All.ToString ();
				Application.Preferences.Set (Preferences.CompletedTasksRange,
											 rangeStr);
			}
			
			currentRange = ParseRange (rangeStr);
			rangeSlider.Value = (double)currentRange;
			rangeSlider.FormatValue += OnFormatRangeSliderValue;
			rangeSlider.ValueChanged += OnRangeSliderChanged;
			rangeSlider.Show ();
			
			this.ExtraWidget = rangeSlider;
		}

		protected override TaskGroupModel CreateModel (DateTime rangeStart,
		                                               DateTime rangeEnd,
		                                               IEnumerable<Task> tasks)
		{
			return new CompletedTaskGroupModel (rangeStart, rangeEnd, tasks);
		}
		
		protected void OnSelectedCategorySettingChanged (
								Preferences preferences,
								string settingKey)
		{
			if (settingKey.CompareTo (Preferences.SelectedCategoryKey) != 0)
				return;
			
			selectedCategory = GetSelectedCategory ();
			Refilter (selectedCategory);
		}
		
		protected Category GetSelectedCategory ()
		{
			Category foundCategory = null;
			
			string cat = Application.Preferences.Get (Preferences.SelectedCategoryKey);
			if (cat != null) {
				var categories = Application.Backend.Categories;
				foundCategory = categories.SingleOrDefault (c => c.Name == cat);
			}
			
			return foundCategory;
		}
		
		private void OnRangeSliderChanged (object sender, EventArgs args)
		{
			CompletionDateRange range = (CompletionDateRange)(uint)rangeSlider.Value;
			
			// If the value is different than what we already have, adjust it in
			// the UI and set the preference.
			if (range == this.currentRange)
				return;
			
			this.currentRange = range;
			Application.Preferences.Set (Preferences.CompletedTasksRange,
										 range.ToString ());
			
			UpdateDateRanges ();
		}
		
		private void OnFormatRangeSliderValue (object sender,
											   FormatValueArgs args)
		{
			CompletionDateRange range = (CompletionDateRange)args.Value;
			args.RetVal = GetTranslatedRangeValue (range);
		}
		
		private CompletionDateRange ParseRange (string rangeStr)
		{
			switch (rangeStr) {
			case "Yesterday":
				return CompletionDateRange.Yesterday;
			case "Last7Days":
				return CompletionDateRange.Last7Days;
			case "LastMonth":
				return CompletionDateRange.LastMonth;
			case "LastYear":
				return CompletionDateRange.LastYear;
			}
			
			// If the string doesn't match for some reason just return the
			// default, which is All.
			return CompletionDateRange.All;
		}
		
		private string GetTranslatedRangeValue (CompletionDateRange range)
		{
			switch (range) {
			case CompletionDateRange.Yesterday:
				return Catalog.GetString ("Yesterday");
			case CompletionDateRange.Last7Days:
				return Catalog.GetString ("Last 7 Days");
			case CompletionDateRange.LastMonth:
				return Catalog.GetString ("Last Month");
			case CompletionDateRange.LastYear:
				return Catalog.GetString ("Last Year");
			}
			
			return Catalog.GetString ("All");
		}
		
		private void UpdateDateRanges ()
		{
			DateTime date = DateTime.MinValue;
			DateTime today = DateTime.Now;
			
			switch (currentRange) {
			case CompletionDateRange.Yesterday:
				date = today.AddDays (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case CompletionDateRange.Last7Days:
				date = today.AddDays (-7);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case CompletionDateRange.LastMonth:
				date = today.AddMonths (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case CompletionDateRange.LastYear:
				date = today.AddYears (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			}
			
			this.TimeRangeStart = date;
		}
	}
}
