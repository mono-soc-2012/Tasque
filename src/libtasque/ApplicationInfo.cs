//
// ApplicationInfo.cs
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

using System.Linq;
using Mono.Unix;
using System.IO;
using System.Collections.Generic;

namespace Tasque
{
	public sealed class ApplicationInfo
	{
		internal static ApplicationInfo GetInstance (Application application)
		{
			if (instance == null)
				instance = new ApplicationInfo (application);

			return instance;
		}

		ApplicationInfo (Application application)
		{
			this.application = application;
			Description = Catalog.GetString ("A Useful Task List");
		}

		public string[] Authors { get { return GlobalDefines.Authors.ToArray (); } }

		public string CopyrightInfo { get { return GlobalDefines.CopyrightInfo; } }

		public string Description { get; private set; }

		public string License { get { return GlobalDefines.License; } }

		public string[] Translators {
			get {
				var creditsString = Catalog.GetString ("translator-credits");

				var credits = new List<string> ();
				if (creditsString == "translator-credits")
					return null;

				using (var sr = new StringReader (creditsString)) {
					var line = sr.ReadLine ();
					if (!string.IsNullOrWhiteSpace (line))
						credits.Add (line);
				}

				return credits.ToArray ();
			}
		}

		public string Version { get { return GlobalDefines.Version; } }

		public string WebsiteUrl { get { return GlobalDefines.Website; } }

		static ApplicationInfo instance;

		Application application;
	}
}

