using System;
using System.Net;
using System.IO;
using SharpMonoInjector;
using System.Threading;

namespace REPOCheatLoader
{
    class Program
    {
        private static string downloadUrl = "https://github.com/D4rkks/r.e.p.o-cheat/releases/latest/download/r.e.p.o.cheat.dll";
        private static string dllPath = "r.e.p.o.cheat.dll";

        static void Main(string[] args)
        {
            Console.Title = "REPO Cheat Loader - Made by Zakarka";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("REPO Cheat Loader - Made by Zakarka\n");

            try
            {
                using (WebClient client = new WebClient())
                {
                    Console.Write("Downloading DLL");
                    ShowSpinner(3);
                    client.DownloadFile(downloadUrl, dllPath);
                    Console.WriteLine("\nDownload complete: " + dllPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading DLL: " + ex.Message);
                return;
            }

            if (!File.Exists(dllPath))
            {
                Console.WriteLine("DLL not found: " + dllPath);
                return;
            }

            byte[] rawAssembly = File.ReadAllBytes(dllPath);
            string targetProcess = "REPO";
            string namespaceName = "r.e.p.o_cheat";
            string className = "Loader";
            string methodName = "Init";

            try
            {
                Injector injector = new Injector(targetProcess);
                Console.Write("Injecting");
                ShowSpinner(3);
                injector.Inject(rawAssembly, namespaceName, className, methodName);
                Console.WriteLine("\nInjection completed successfully.");
                injector.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nInjection error: " + ex.Message);
            }

            try
            {
                if (File.Exists(dllPath))
                {
                    File.Delete(dllPath);
                    Console.WriteLine("DLL file deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting DLL: " + ex.Message);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void ShowSpinner(int seconds)
        {
            int iterations = seconds * 2;
            for (int i = 0; i < iterations; i++)
            {
                Console.Write(".");
                Thread.Sleep(500);
            }
        }
    }
}
