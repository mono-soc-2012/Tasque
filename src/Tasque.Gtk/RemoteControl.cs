// RemoteControl.cs created with MonoDevelop
// User: sandy at 9:49 AM 2/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
// RemoteControl.cs
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
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Tasque
{
	[Interface ("org.gnome.Tasque.RemoteControl")]
	public class RemoteControl : MarshalByRefObject
	{
		public static RemoteControl GetInstance ()
		{
			BusG.Init ();

			if (!Bus.Session.NameHasOwner (Namespace))
				Bus.Session.StartServiceByName (Namespace);

			return Bus.Session.GetObject<RemoteControl> (Namespace, new ObjectPath (Path));
		}

		public static RemoteControl Register ()
		{
			BusG.Init ();

			var remoteControl = new RemoteControl ();
			Bus.Session.Register (new ObjectPath (Path), remoteControl);

			if (Bus.Session.RequestName (Namespace) != RequestNameReply.PrimaryOwner)
				return null;

			return remoteControl;
		}
		
		RemoteControl () {}
		
		public void KnockKnock ()
		{
			if (RemoteInstanceKnocked != null)
				RemoteInstanceKnocked ();
		}
		
		public Action RemoteInstanceKnocked { get; set; }
		
		const string Namespace = "org.gnome.Tasque";
		const string Path = "/org/gnome/Tasque/RemoteControl";
	}
}
