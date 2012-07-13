// 
// NotifyCollection.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Tasque
{
	public class NotifyCollection<T> : ICollection<T>, INotifyCollectionChanged
		where T : INotifyPropertyChanged, IComparable<T>
	{
		public NotifyCollection ()
		{
			items = new ObservableCollection<T> ();
		}

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		#endregion

		#region ICollection implementation
		public void Add (T item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");

			if (items.Contains (item))
				return;

			item.PropertyChanged += HandlePropertyChanged;

			OnAdd (item);
			var index = GetItemIndex (item);
			items.Insert (index, item);
		}

		public void Clear ()
		{
			OnClear ();

			foreach (var item in items)
				item.PropertyChanged -= HandlePropertyChanged;

			items.Clear ();
		}

		public bool Contains (T item)
		{
			return items.Contains (item);
		}

		public void CopyTo (T[] array, int arrayIndex)
		{
			items.CopyTo (array, arrayIndex);
		}

		public bool Remove (T item)
		{
			if (Contains (item)) {
				OnRemove (item);
				item.PropertyChanged -= HandlePropertyChanged;
			}

			return items.Remove (item);
		}

		public int Count { get { return items.Count; } }

		public bool IsReadOnly { get { return false; } }
		#endregion

		#region INotifyCollectionChanged implementation
		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add { items.CollectionChanged += value; }
			remove { items.CollectionChanged -= value; }
		}
		#endregion

		protected virtual void OnAdd (T item) {}

		protected virtual void OnClear () {}

		protected virtual void OnRemove (T item) {}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var item = (T)sender;
			var oldIndex = items.IndexOf (item);
			var newIndex = GetItemIndex (item);

			if (oldIndex != newIndex)
				items.Move (oldIndex, newIndex);
		}

		int GetItemIndex (T item)
		{
			var index = 0;
			foreach (var i in items) {
				if (item.CompareTo (i) == -1)
					break;
				index++;
			}
			return index;
		}

		ObservableCollection<T> items;
	}
}
