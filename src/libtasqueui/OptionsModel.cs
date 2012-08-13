// 
// DropDownModel.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CrossCommand.Generic;

namespace Tasque.UIModel
{
	public class OptionsModel<T> : ViewModel
	{
		internal OptionsModel (ViewModel parent) : this (null, parent) {}
		
		internal OptionsModel (IValueConverter<T, string> converter, ViewModel parent)
			: this (new OptionItem [] {}, converter, parent) {}
		
		internal OptionsModel (IEnumerable<T> optionValues, IValueConverter<T, string> converter,
		                       ViewModel parent) : base (parent)
		{
			Converter = converter;
			
			ProtectedOptions = new ObservableCollection<OptionItem> ();
			foreach (var item in optionValues)
				ProtectedOptions.Add (new OptionItem (item, converter));
			
			Options = new ReadOnlyObservableCollection<OptionItem> (ProtectedOptions);
		}
		
		public IValueConverter<T, string> Converter { get; protected set; }
		
		public ReadOnlyObservableCollection<OptionItem> Options { get; private set; }
		
		public OptionItem SelectedOption { get; private set; }
		
		public ICommand<OptionItem> SelectOption {
			get {
				return selectOption ?? (selectOption = new RelayCommand<OptionItem> () {
					CanExecuteAction = CanExecuteSelectOption,
					ExecuteAction = ExecuteSelectOption
				});
			}
		}
		
		public event EventHandler OptionSelected;
		
		protected ObservableCollection<OptionItem> ProtectedOptions { get; private set; }
		
		protected virtual bool CanExecuteSelectOption (OptionItem parameter)
		{
			return true;
		}
		
		protected virtual void ExecuteSelectOption (OptionItem parameter)
		{
			SelectedOption = parameter;
			if (OptionSelected != null)
				OptionSelected (this, EventArgs.Empty);
			Close.Execute (null);			
		}
		
		protected override void OnClose ()
		{
			SelectedOption = null;
			base.OnClose ();
		}
		
		RelayCommand<OptionItem> selectOption;
		
		public class OptionItem
		{
			internal OptionItem (T value, IValueConverter<T, string> converter)
			{
				if (converter == null)
					Debug.WriteLine ("DropDownModel: No converter specified. " +
						"Using ToString() to convert.");
				Value = value;
				this.converter = converter;
			}
			
			public virtual string Text {
				get { return converter == null ? Value.ToString () : converter.Convert (Value); }
			}
			
			internal T Value { get; private set; }
			
			IValueConverter<T, string> converter;
		}
	}
}
