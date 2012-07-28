/***************************************************************************
 *  Utilities.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Copyright (C) 2010 Mario Carrion
 *  Copyright (C) 2012 Antonius Riha
 *  Written by:
 * 		Calvin Gaisford <calvinrg@gmail.com>
 *		Boyd Timothy <btimothy@gmail.com>
 * 		Mario Carrion <mario@carrion.mx>
 *  	Antonius Riha <antoniusriha@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using Gdk;
using Mono.Unix;

namespace Tasque
{
	static partial class Utilities
	{
		public static Gdk.Pixbuf GetIcon (string iconName, int size)
		{
			try {
				return Gtk.IconTheme.Default.LoadIcon (iconName, size, 0);
			} catch (GLib.GException) {}

			try {
				Gdk.Pixbuf ret = new Gdk.Pixbuf (null, iconName + ".png");
				return ret.ScaleSimple (size, size, Gdk.InterpType.Bilinear);
			} catch (ArgumentException) {}

			// TODO: This is a temporary fix to allow installing all icons as assembly
			//       resources. The proper thing to do is to ship the actual icons,
			//       and append to the default gtk+ icon theme path. See Tomboy.
			try {
				Gdk.Pixbuf ret = new Gdk.Pixbuf (null, String.Format ("{0}-{1}.png", iconName, size));
				return ret.ScaleSimple (size, size, Gdk.InterpType.Bilinear);
			} catch (ArgumentException) { }

			Debug.WriteLine ("Unable to load icon '{0}'.", iconName);
			return null;
		}
		
		/// <summary>
		/// Get a string that is more friendly/pretty for the specified date.
		/// For example, "Today, 3:00 PM", "4 days ago, 9:20 AM".
		/// <param name="date">The DateTime to evaluate</param>
		/// <param name="show_time">If true, output the time along with the
		/// date</param>
		/// </summary>
		public static string GetPrettyPrintDate (DateTime date, bool show_time)
		{
			string pretty_str = String.Empty;
			DateTime now = DateTime.Now;
			string short_time = date.ToShortTimeString ();

			if (date.Year == now.Year) {
				if (date.DayOfYear == now.DayOfYear)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Today, {0}"),
					                            short_time) :
					             Catalog.GetString ("Today");
				else if (date.DayOfYear < now.DayOfYear
				                && date.DayOfYear == now.DayOfYear - 1)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Yesterday, {0}"),
					                            short_time) :
					             Catalog.GetString ("Yesterday");
				else if (date.DayOfYear < now.DayOfYear
				                && date.DayOfYear > now.DayOfYear - 6)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("{0} days ago, {1}"),
					                            now.DayOfYear - date.DayOfYear, short_time) :
					             String.Format (Catalog.GetString ("{0} days ago"),
					                            now.DayOfYear - date.DayOfYear);
				else if (date.DayOfYear > now.DayOfYear
				                && date.DayOfYear == now.DayOfYear + 1)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Tomorrow, {0}"),
					                            short_time) :
					             Catalog.GetString ("Tomorrow");
				else if (date.DayOfYear > now.DayOfYear
				                && date.DayOfYear < now.DayOfYear + 6)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("In {0} days, {1}"),
					                            date.DayOfYear - now.DayOfYear, short_time) :
					             String.Format (Catalog.GetString ("In {0} days"),
					                            date.DayOfYear - now.DayOfYear);
				else
					pretty_str = show_time ?
					             date.ToString (Catalog.GetString ("MMMM d, h:mm tt")) :
					             date.ToString (Catalog.GetString ("MMMM d"));
			} else if (date == DateTime.MinValue)
				pretty_str = Catalog.GetString ("No Date");
			else
				pretty_str = show_time ?
				             date.ToString (Catalog.GetString ("MMMM d yyyy, h:mm tt")) :
				             date.ToString (Catalog.GetString ("MMMM d yyyy"));

			return pretty_str;
		}

		/// <summary>
		/// This returns the hexadecimal value of an GDK color.
		/// </summary>
		/// <param name="color">
		/// The color to convert to a hex string.
		/// </param>
		public static string ColorGetHex (Gdk.Color color)
		{
			return String.Format ("#{0:x2}{1:x2}{2:x2}",
			                      (byte)(color.Red >> 8),
			                      (byte)(color.Green >> 8),
			                      (byte)(color.Blue >> 8));
		}
	}
}
