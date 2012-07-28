// 
// WindowsSingleInstanceSolution.cs
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
using System.Threading;

namespace Tasque.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckIfRunning();

            Console.WriteLine("Press ENTER to begin exiting");
            Console.ReadLine();
            exiting = true;
            Console.WriteLine("Exiting has begun");

            waitHandle.Set();
            Console.WriteLine("Porter thread has been released.");
            waitHandle.Dispose();
            waitHandle = null;

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        static void CheckIfRunning()
        {
            try
            {
                waitHandle = EventWaitHandle.OpenExisting(waitHandleName);
                waitHandle.Set();
                Console.WriteLine("Running instance found and notified.");
                Console.WriteLine("Exiting");
                Environment.Exit(0);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, waitHandleName);
                Console.WriteLine("Event wait handle created");

                var portier = new Thread(new ThreadStart(WaitForAnotherInstance));
                portier.Start();
            }
        }

        static void WaitForAnotherInstance()
        {
            while (!exiting)
            {
                waitHandle.WaitOne();
                if (!exiting)
                {
                    Console.WriteLine("Another app instance has just knocked on the door.");
                    Console.WriteLine("Show window!");
                    Thread.Sleep(2000);
                }
            }
        }

        static bool exiting;
        static EventWaitHandle waitHandle;
        static readonly string waitHandleName = "Tasque." + Environment.UserName;
    }
}
