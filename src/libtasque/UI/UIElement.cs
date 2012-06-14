// 
// UIElement.cs
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
using Tasque;

namespace Tasque.UI
{
	public abstract class UIElement
	{
		internal virtual void Initialize ()
		{
			OnInitialize ();

			foreach (var item in Children) {
				item.Parent = this;
				item.Initialize ();
			}
		}

		internal virtual void InitializeIdle ()
		{
			OnInitializeIdle ();

			foreach (var item in Children) {
				item.InitializeIdle ();
			}
		}

		protected internal UIElement Initialize (UIElement child)
		{
			if (child == null)
				throw new ArgumentNullException ("child");

			Children.Add (child);
			return child;
		}

		protected abstract void OnInitialize ();

		protected virtual void OnInitializeIdle ()
		{
		}

//		void Refresh ();

		protected internal Collection<UIElement> Children { get; private set; }

		protected internal UIElement Parent { get; private set; }

		internal Application Application {
			get {
				if (Parent != null)
					return Parent.Application;
				else
					return application;
			}
			set {
				if (Parent == null)
					application = value;
			}
		}

		Application application;
	}
}

