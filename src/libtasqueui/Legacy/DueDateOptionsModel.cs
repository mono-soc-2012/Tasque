// 
// DueDateOptionsModel.cs
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
using System.Collections;
using System.Collections.Generic;
using Mono.Unix;
using CrossCommand.Generic;

namespace Tasque.UIModel.Legacy
{
	public class DueDateOptionsModel : ViewModel, IEnumerable<string>, ITimeAware
	{
		internal DueDateOptionsModel (ViewModel parent) : base (parent)
		{
			options = new Option [10];
			options [8] = new Option (Catalog.GetString ("No Date"), DateTime.MinValue);
			options [9] = new Option (Catalog.GetString ("Choose Date..."), CustomDate);
			UpdateDateOptions ();
		}
		
		public DateTime CustomDate {
			get { return customDate; }
			set {
				if (value != customDate) {
					customDate = value;
					OnPropertyChanged ("CustomDate");
				}
			}
		}
		
		public ICommand<Option> SelectOption {
			get {
				if (selectOption == null) {
					selectOption = new RelayCommand<Option> () {
						CanExecuteAction = (option) => {
							return option != options [9] || customDate != DateTime.MinValue;
						},
						ExecuteAction = (option) => {
							SelectedOption = option;
							if (OptionSelected != null)
								OptionSelected (this, EventArgs.Empty);
							Close.Execute (null);
						}
					};
				}
				return selectOption;
			}
		}
		
		public IEnumerator<string> GetEnumerator ()
		{
			foreach (var item in options)
				yield return item;
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		protected override void OnClose ()
		{
			customDate = DateTime.MinValue;
			SelectedOption = null;
			base.OnClose ();
		}
		
		internal Option SelectedOption { get; private set; }
		
		internal event EventHandler OptionSelected;
		
		void ITimeAware.OnDayChanged ()
		{
			UpdateDateOptions ();
		}
		
		void UpdateDateOptions ()
		{
			var today = DateTime.Today;	
			options [0] = new Option (today.ToString (Catalog.GetString ("M/d - "))
				+ Catalog.GetString ("Today"), today);
			
			var tomorrow = today.AddDays (1);
			options [1] = new Option (tomorrow.ToString (Catalog.GetString ("M/d - "))
				+ Catalog.GetString ("Tomorrow"), tomorrow);
			
			var nextWeek = today.AddDays (7);
			options [7] = new Option (nextWeek.ToString (Catalog.GetString ("M/d - "))
				+ Catalog.GetString ("In 1 Week"), nextWeek);
			
			for (int i = 2; i < 7; i++) {
				var date = today.AddDays (i);
				options [i] = new Option (date.ToString (Catalog.GetString ("M/d - ddd")), date);
			}
		}
		
		DateTime customDate;
		Option [] options;
		RelayCommand<Option> selectOption;
		
		public class Option 
		{
			internal Option (string text, DateTime date)
			{
				Text = text;
				Date = date;
			}
			
			internal DateTime Date { get; private set; }
			
			public string Text { get; private set; }
		}
	}
}
