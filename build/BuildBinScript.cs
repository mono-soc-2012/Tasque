// 
// BuildBinScript.cs
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
	public class BuildBinScript : Task
	{
		[Required]
		public ITaskItem Prefix { get; set; }
		
		[Required]
		public ITaskItem BinScriptIn { get; set; }
		
		public override bool Execute ()
		{
			try {
				var text = File.ReadAllText (BinScriptIn.GetMetadata ("FullPath"));
				text = text.Replace ("@prefix@", Prefix.ItemSpec);
				
				var dirPath = Path.Combine (Prefix.ItemSpec, "bin");
				if (!Directory.Exists (dirPath))
					Directory.CreateDirectory (dirPath);
				
				var path = Path.Combine (dirPath, "tasque");
				File.WriteAllText (path, text);
			} catch (Exception ex) {
				Log.LogErrorFromException (ex, true);
				return false;
			}
			return true;
		}
	}
}
