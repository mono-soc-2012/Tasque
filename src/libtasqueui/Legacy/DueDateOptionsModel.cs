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
using Mono.Unix;

namespace Tasque.UIModel.Legacy
{
	public class DueDateOptionsModel : OptionsModel<DateTime>, ITimeAware
	{
		internal DueDateOptionsModel (ViewModel parent) : base (parent)
		{
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
		
		protected override bool CanExecuteSelectOption (OptionItem parameter)
		{
			return parameter != Options [9] || customDate != DateTime.MinValue;
		}
		
		protected override void OnClose ()
		{
			customDate = DateTime.MinValue;
			base.OnClose ();
		}
		
		void ITimeAware.OnDayChanged ()
		{
			UpdateDateOptions ();
		}
		
		void UpdateDateOptions ()
		{
			ProtectedOptions.Clear ();
			
			var today = DateTime.Today;
			ProtectedOptions.Add (new DueDateOption (today, today.ToString (
				Catalog.GetString ("M/d - ")) + Catalog.GetString ("Today")));
			
			var tomorrow = today.AddDays (1);
			ProtectedOptions.Add (new DueDateOption (tomorrow, tomorrow.ToString (
				Catalog.GetString ("M/d - ")) + Catalog.GetString ("Tomorrow")));
			
			for (int i = 2; i < 7; i++) {
				var date = today.AddDays (i);
				ProtectedOptions.Add (new DueDateOption (date, date.ToString (
					Catalog.GetString ("M/d - ddd"))));
			}
			
			var nextWeek = today.AddDays (7);
			ProtectedOptions.Add (new DueDateOption (nextWeek, nextWeek.ToString (
				Catalog.GetString ("M/d - ")) + Catalog.GetString ("In 1 Week")));
			
			ProtectedOptions.Add (new DueDateOption (DateTime.MinValue, Catalog.GetString ("No Date")));
			ProtectedOptions.Add (new DueDateOption (customDate, Catalog.GetString ("Choose Date...")));
		}
		
		DateTime customDate;
		
		public class DueDateOption : OptionItem
		{
			internal DueDateOption (DateTime date, string text) : base (date, null)
			{
				this.text = text;
			}
			
			public override string Text { get { return text; } }
			
			string text;
		}
	}
}
