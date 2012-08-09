// 
// ViewModel.cs
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
using System.ComponentModel;
using CrossCommand;

namespace Tasque.UIModel
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		internal ViewModel (ViewModel parent)
		{
			this.parent = parent;
			if (parent != null)
				parent.Children.Add (this);
		}
		
		public ICommand Close {
			get {
				if (close == null) {
					close = new RelayCommand () {
						ExecuteAction = delegate {
							foreach (var child in Children)
								child.Close.Execute (null);
							
							parent.Children.Remove (this);
							Dispose ();
							if (Closed != null)
								Closed (this, EventArgs.Empty);
						}
					};
				}
				return close;
			}
		}
		
		protected virtual void OnClose () {}
		
		void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		protected virtual void Dispose (bool disposing) {}
		
		~ViewModel ()
		{
			Dispose (false);
		}

		
		#region Object Service
		//DOCS: Provides a service for descendants to retrieve an object instance
		// of a class, which has a one-to-one relation with the MainWindow, though
		// it is used multiple times by various descendants (better solution appreciated).
		protected object GetObjectFromAncestor (Type objectType)
		{
			if (registeredObjects.ContainsKey (objectType))
				return registeredObjects [objectType];
			
			return parent == null ? null : parent.GetObjectFromAncestor (objectType);
		}
		
		protected void AddObjectToObjectService (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			
			if (registeredObjects == null)
				registeredObjects = new Dictionary<Type, object> ();
			registeredObjects.Add (obj.GetType (), obj);
		}
		
		protected bool RemoveObjectFromObjectService (object obj)
		{
			return obj == null ? false : registeredObjects.Remove (obj);
		}
		#endregion
		
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
		
		internal Collection<ViewModel> Children {
			get { return children ?? (children = new Collection<ViewModel> ()); }
		}
		
		internal event EventHandler Closed;
		
		Collection<ViewModel> children;
		RelayCommand close;
		ViewModel parent;
		Dictionary<Type, object> registeredObjects;
	}
}
