// 
// AboutDialog.cs
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

namespace Tasque.UI.Desktop
{
	public abstract class AboutDialog : UIElement, IDialog
	{
		protected string CopyrightInfo { get { return AppInfo.CopyrightInfo; } }

		protected string License { get { return AppInfo.License; } }
		
		protected string Description { get { return AppInfo.Description; } }
		
		protected string LogoName { get { throw new NotImplementedException (); } }
		
		protected string Version { get { return AppInfo.Version; } }
		
		protected string WebsiteUrl { get { return AppInfo.WebsiteUrl; } }
		
		protected string[] Translators { get { return AppInfo.Translators; } }
		
		protected string[] Authors { get { return AppInfo.Authors; } }

		protected override void OnInitialize () { }

		public abstract void Show ();

		ApplicationInfo AppInfo { get { return Application.ApplicationInfo;	} }
	}
}
