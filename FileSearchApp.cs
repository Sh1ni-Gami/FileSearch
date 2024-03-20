using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileSearchApp
{
    class Program
    {
        static bool searching;
        static string startDirectory;
        static Regex fileNamePattern;
        static int totalFilesFound;
        static int totalFilesSearched;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to File Search App!");

            while (true)
            {
                Console.WriteLine("\nEnter start directory:");
                startDirectory = Console.ReadLine();

                Console.WriteLine("\nEnter file name pattern (regex):");
                var pattern = Console.ReadLine();
                fileNamePattern = new Regex(pattern);

                StartSearch();

                Console.WriteLine("\nDo you want to perform another search? (yes/no)");
                var response = Console.ReadLine().ToLower();
                if (response != "yes")
                    break;
            }
        }

        static void StartSearch()
        {
            searching = true;
            totalFilesFound = 0;
            totalFilesSearched = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Search(startDirectory);
                stopwatch.Stop();

                searching = false;

                Console.WriteLine($"\nSearch finished. Found {totalFilesFound} file(s) out of {totalFilesSearched} in {stopwatch.Elapsed.TotalSeconds} seconds.");
            });

            DisplayProgress();
        }

        static void Search(string directory)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    totalFilesSearched++;

                    if (fileNamePattern.IsMatch(Path.GetFileName(file)))
                    {
                        totalFilesFound++;
                        Console.WriteLine($"Found: {file}");
                    }
                }

                foreach (var subdir in Directory.GetDirectories(directory))
                {
                    if (!searching)
                        break;

                    Search(subdir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error searching in {directory}: {e.Message}");
            }
        }

        static void DisplayProgress()
        {
            while (searching)
            {
                Console.Clear();
                Console.WriteLine($"Searching in: {startDirectory}");
                Console.WriteLine($"Files found: {totalFilesFound} / {totalFilesSearched}");
                Thread.Sleep(1000);
            }
        }
    }
}
