// 
// NativeApplication.cs
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
using System.Diagnostics;
using Tasque.UI.Desktop;

namespace Tasque
{
	public abstract class Application
	{
		public abstract string ConfDir { get; }

		public void Exit (int exitcode)
		{
			OnExit (exitcode);

			if (Exiting != null)
				Exiting (this, EventArgs.Empty);

			Environment.Exit (exitcode);
		}

		public void Initialize (string[] args)
		{
			OnInitialize (args);

			var rootVisual = InitializeMainWindow ();
			if (rootVisual == null)
				throw new ApplicationException ("The main window was not properly initialized. "
				                                + "NativeApplication.InitializeMainWindow () "
				                                + "must not return null.");
			RootVisual = rootVisual;
			RootVisual.Application = this;
			RootVisual.Initialize ();
		}

		public virtual void InitializeIdle () {}

		protected abstract MainWindow InitializeMainWindow ();

		protected virtual void OnExit (int exitCode) {}

		protected virtual void OnInitialize (string[] args) {}

		public virtual void OpenUrlInBrowser (string url)
		{
			Process.Start (url);
		}

		public abstract void QuitMainLoop ();

		public abstract void StartMainLoop ();

		public event EventHandler Exiting;

		internal MainWindow RootVisual { get; private set; }

		public ApplicationInfo ApplicationInfo {
			get {
				return ApplicationInfo.GetInstance (this);
			}
		}
	}
}
