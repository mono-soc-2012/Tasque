//
// TreeViewModelImplementor.cs
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
using System.Runtime.InteropServices;
using GLib;
using Gtk;

namespace Tasque
{
	public class TreeViewModelImplementor<T> : GLib.Object, TreeModelImplementor
	{
		public TreeViewModelImplementor (Collection<T> collection)
		{
			stamp = new Random ().Next (int.MinValue, int.MaxValue);
			items = collection;
//			adapter = new TreeModelAdapter (this);
			gcHandles = new List<GCHandle> ();
		}

		public TreeModelFlags Flags {
			get {
				return TreeModelFlags.ItersPersist | TreeModelFlags.ListOnly;
			}
		}

		public int NColumns { get { return 1; } }

		public override void Dispose ()
		{
			foreach (var item in gcHandles)
				item.Free ();

			base.Dispose ();
		}

		public GLib.GType GetColumnType (int index_)
		{
			return (GType)typeof(T);
		}

		public bool GetIter (out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Depth != 1 || path.Indices [0] > items.Count) {
				iter = TreeIter.Zero;
				return false;
			}

			var item = items [path.Indices [0]];

			if (item == null) {
				iter = TreeIter.Zero;
				return false;
			}

			var gch = GCHandle.Alloc (item);
			iter = new TreeIter () { Stamp = stamp, UserData = (IntPtr)gch };
			gcHandles.Add (gch);
			return true;
		}

		public TreePath GetPath (TreeIter iter)
		{
			if (iter.Stamp != stamp)
				return null;

			var gch = (GCHandle)iter.UserData;
			object target = gch.Target;

			if (target == null)
				return new TreePath ();

			var index = items.IndexOf ((T)target);
			if (index != -1)
				return new TreePath (new int[] { index });
			else
				return new TreePath ();
		}

		public void GetValue (TreeIter iter, int column, ref Value value)
		{
			value.Init ((GType)typeof(T));

			if (iter.Stamp != stamp || column != 0) {
				value.Val = null;
				return;
			}

			var gch = (GCHandle)iter.UserData;
			value.Val = gch.Target is T ? gch.Target : null;
		}

		public bool IterChildren (out TreeIter iter, TreeIter parent)
		{
			return IterNthChild (out iter, parent, 0);
		}

		public bool IterHasChild (TreeIter iter)
		{
			return false;
		}

		public int IterNChildren (TreeIter iter)
		{
			if (iter.Stamp != stamp)
				return -1;

			// No iter has children
			return 0;
		}

		public bool IterNext (ref TreeIter iter)
		{
			if (iter.Stamp != stamp)
				return false;

			var gch = (GCHandle)iter.UserData;
			var target = gch.Target;

			if (target == null)
				return false;

			var index = items.IndexOf ((T)target);
			if (index == -1)
				return false;

			if (index >= items.Count - 1) {
				iter = TreeIter.Zero;
				return false;
			}

			GetIter (out iter, new TreePath (new int [] { ++index }));
			return true;
		}

		public bool IterNthChild (out TreeIter iter, TreeIter parent, int n)
		{
			iter = TreeIter.Zero;
			return false;
		}

		public bool IterParent (out TreeIter iter, TreeIter child)
		{
			iter = TreeIter.Zero;
			return false;
		}

		void TreeModelImplementor.RefNode (TreeIter iter)
		{
		}

		void TreeModelImplementor.UnrefNode (TreeIter iter)
		{
		}

//		TreeModelAdapter adapter;
		List<GCHandle> gcHandles;
		Collection<T> items;
		readonly int stamp;
	}
}

