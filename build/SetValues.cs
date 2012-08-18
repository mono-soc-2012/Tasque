// 
// SetValues.cs
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tasque.Build
{
	public class SetValues : Task
	{
		[Required]
		public ITaskItem[] SourceFiles { get; set; }
		
		[Required]
		public ITaskItem[] TargetFiles { get; set; }
		
		[Required]
		public ITaskItem[] Patterns { get; set; }
		
		[Required]
		public ITaskItem[] Values { get; set; }
		
		public override bool Execute ()
		{
			try {
				if (Patterns.Length != Values.Length)
					throw new Exception ("The number of provided patterns must be equal to" +
						" the number of provided values."
					);
				
				if (SourceFiles.Length != TargetFiles.Length)
					throw new Exception ("The number of provided source files must be equal to" +
						" the number of provided target files."
					);
				
				for (int i = 0; i < SourceFiles.Length; i++) {
					fileText = File.ReadAllText (SourceFiles [i].GetMetadata ("FullPath"));
					
					for (int j = 0; j < Values.Length; j++) {
						// if the input value is actually a path, take file content as new value
						string newVal;
						var path = Values [j].GetMetadata ("FullPath");
						if (File.Exists (path)) {
							newVal = File.ReadAllText (path);
							newVal = newVal.Replace ("\"", "\"\"");
						} else
							newVal = Values [j].ItemSpec;
						fileText = fileText.Replace (Patterns [j].ItemSpec, newVal);
					}
					var newPath = TargetFiles [i].GetMetadata ("FullPath");
					var dirNewPath = Path.GetDirectoryName (newPath);
					if (!Directory.Exists (dirNewPath))
						Directory.CreateDirectory (dirNewPath);
					File.WriteAllText (newPath, fileText);
				}
			} catch (Exception ex) {
				Log.LogErrorFromException (ex, true);
				return false;
			}
			return true;
		}
		
		string fileText;
	}
}
