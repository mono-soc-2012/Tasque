// RtmCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using Tasque;
using RtmNet;

namespace Tasque.Backends.RtmBackend
{
	public class RtmCategory : Category
	{
		List list;

		public RtmCategory (List list) : base (list.Name)
		{
			this.list = list;
		}

		public string ID {
			get { return list.ID; }
		}
    
		public int Deleted {
			get { return list.Deleted; }
		}

		public int Locked {
			get { return list.Locked; }
		}
    
		public int Archived {
			get { return list.Archived; }
		}

		public int Position {
			get { return list.Position; }
		}

		public int Smart {
			get { return list.Smart; }
		}
	}
}
