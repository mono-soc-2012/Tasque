//
// SetDataAtBuildTime.cs
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
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tasque.Build
{
	public class SetDataAtBuildTime : Task
	{
		[Required]
		public ITaskItem AuthorsFile {
			get {
				return authorsFile;
			}
			set {
				authorsFile = value;

				try {
					using (var fs = new FileStream (value.GetMetadata ("FullPath"), FileMode.Open)) {
						using (var sr = new StreamReader (fs)) {

							var authorsStrBuilder = new StringBuilder ();

							while (!sr.EndOfStream) {
								var line = sr.ReadLine ().Trim ();
								if (!string.IsNullOrWhiteSpace (line))
									authorsStrBuilder.Append ("\"" + line + "\", ");
							}

							authors = authorsStrBuilder.ToString ();
						}
					} 
				} catch (Exception ex) {
					Log.LogErrorFromException (ex);
				}
			}
		}

		[Required]
		public ITaskItem CopyingFile {
			get {
				return copyingFile;
			}
			set {
				copyingFile = value;

				try {
					using (var fs = new FileStream (value.GetMetadata ("FullPath"), FileMode.Open)) {
						using (var sr = new StreamReader (fs)) {

							/*
							 * Parsing states:
							 * 0 ... nothing parsed yet
							 * 1 ... parsing copyright notice
							 * 2 ... before parsing license text
							 * 3 ... parsing license text
							 * 
							 * The text format to be parsed is:
							 * [empty line]*
							 * [paragraph with copyright notices]
							 * [empty line]*
							 * [paragraphs with license text]
							 */
							int state = 0;

							var copyrightStrBuilder = new StringBuilder ();
							var licenseStrBuilder = new StringBuilder ();

							while (!sr.EndOfStream) {
								var line = sr.ReadLine ().Trim ();
								switch (state) {
								case 0:
									if (string.IsNullOrWhiteSpace (line))
										break;
									else {
										state = 1;
										goto case 1;
									}
								case 1:
									if (!string.IsNullOrWhiteSpace (line)) {
										copyrightStrBuilder.AppendLine (line);
										break;
									} else {
										state = 2;
										break;
									}
								case 2:
									if (string.IsNullOrWhiteSpace (line))
										break;
									else {
										state = 3;
										goto case 3;
									}
								case 3:
									licenseStrBuilder.AppendLine (line);
									break;
								}
							}

							/* 
							 * Replace double quotes with double double quotes, which is
							 * necessary to escape double quotes in verbatim C# string constants.							 * 
							 */
							copyrightStrBuilder.Replace ("\"", "\"\"");
							licenseStrBuilder.Replace ("\"", "\"\"");

							copyrightInfo = copyrightStrBuilder.ToString ();
							license = licenseStrBuilder.ToString ();
						}
					}
				} catch (Exception ex) {
					Log.LogErrorFromException (ex);
				}
			}
		}

		[Required]
		public ITaskItem Prefix { get; set; }
		
		[Required]
		public ITaskItem GlobalDefinesFile { get; set; }

		[Required]
		public ITaskItem GlobalDefinesFileIn { get; set; }

		[Required]
		public ITaskItem Version { get; set; }

		[Required]
		public ITaskItem Website { get; set; }

		public override bool Execute ()
		{
			if (!ReadGlobalDefines ())
				return false;

			SetValue ("@version@", Version.ItemSpec);

			SetValue ("@copyrightinfo@", copyrightInfo);

			SetValue ("@license@", license);

			SetValue ("@website@", Website.ItemSpec);

			SetValue ("@authors@", authors);
			
			SetValue ("@prefix@", Prefix.ItemSpec);

			if (!WriteGlobalDefines ())
				return false;

			return true;
		}

		bool ReadGlobalDefines ()
		{
			try {
				globalDefines = File.ReadAllText (GlobalDefinesFileIn.GetMetadata ("FullPath"));
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}
			return true;
		}

		void SetValue (string key, string value)
		{
			globalDefines = globalDefines.Replace (key, value);
		}

		bool WriteGlobalDefines ()
		{
			try {
				File.WriteAllText (GlobalDefinesFile.GetMetadata ("FullPath"), globalDefines);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				return false;
			}
			return true;
		}

		ITaskItem authorsFile;
		string authors;
		ITaskItem copyingFile;
		string copyrightInfo;
		string globalDefines;
		string license;
	}
}
