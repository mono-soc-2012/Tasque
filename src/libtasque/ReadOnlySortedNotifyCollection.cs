// 
// ReadOnlyNotifyCollection.cs
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
using System.Collections.Specialized;
using System.ComponentModel;

namespace Tasque
{
	public class ReadOnlySortedNotifyCollection<T> : ICollection<T>, INotifyCollectionChanged
		where T : INotifyPropertyChanged, IComparable<T>
	{
		public ReadOnlySortedNotifyCollection (SortedNotifyCollection<T> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			items = source;
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
		void ICollection<T>.Add (T item)
		{
			throw new NotSupportedException ("This collection is read-only.");
		}

		void ICollection<T>.Clear ()
		{
			throw new NotSupportedException ("This collection is read-only.");
		}

		public bool Contains (T item)
		{
			return items.Contains (item);
		}

		public void CopyTo (T[] array, int arrayIndex)
		{
			items.CopyTo (array, arrayIndex);
		}

		bool ICollection<T>.Remove (T item)
		{
			throw new NotSupportedException ("This collection is read-only.");
		}

		public int Count { get { return items.Count; } }

		public bool IsReadOnly { get { return true; } }
		#endregion

		#region INotifyCollectionChanged implementation
		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add { items.CollectionChanged += value; }
			remove { items.CollectionChanged -= value; }
		}
		#endregion

		SortedNotifyCollection<T> items;
	}
}
