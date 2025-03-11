using System;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace REPOCheatLoader
{
    public class MonoProcess
    {
        public IntPtr MonoModule;
        public int Id;
        public string Name;
    }

    class Program
    {
        private static string downloadUrl = "https://github.com/D4rkks/r.e.p.o-cheat/releases/latest/download/r.e.p.o.cheat.dll";
        private static string dllPath = "r.e.p.o.cheat.dll";

        static void Main(string[] args)
        {
            Console.Title = "REPO Cheat Loader - Made by Zakarka";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("REPO Cheat Loader - Made by Zakarka\n");

            // 1. Search for the game process "REPO"
            Console.Write("Searching for game process 'REPO'... ");
            Process[] targetProcesses = Process.GetProcessesByName("REPO");
            if (targetProcesses.Length == 0)
            {
                Console.WriteLine("Not found.");
                Console.WriteLine("The game 'REPO' is not running. Please start the game and try again.");
                PromptExit();
                return;
            }
            Process targetProcess = targetProcesses[0];
            Console.WriteLine("Found: " + targetProcess.ProcessName + " (ID: " + targetProcess.Id + ")\n");

            // 2. Download the cheat DLL
            Console.Write("Downloading cheat DLL... ");
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, dllPath);
                    Console.WriteLine("Download complete: " + dllPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError downloading DLL: " + ex.Message);
                Console.WriteLine("Please check your internet connection and try again.");
                PromptExit();
                return;
            }

            if (!File.Exists(dllPath))
            {
                Console.WriteLine("\nDLL file not found: " + dllPath);
                PromptExit();
                return;
            }

            byte[] rawAssembly = File.ReadAllBytes(dllPath);

            // 3. Open the process with full access rights
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, targetProcess.Id);
                if (handle == IntPtr.Zero)
                {
                    Console.WriteLine("\nFailed to open process " + targetProcess.ProcessName);
                    Console.WriteLine("Please run the game with sufficient privileges and try again.");
                    PromptExit();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError opening process: " + ex.Message);
                PromptExit();
                return;
            }

            // 4. Check for the Mono module in the process
            if (!ProcessUtils.GetMonoModule(handle, out IntPtr monoModule))
            {
                Console.WriteLine("\nMono module not found in process " + targetProcess.ProcessName);
                Console.WriteLine("Make sure the game uses Mono and try again.");
                Native.CloseHandle(handle);
                PromptExit();
                return;
            }

            // 5. Perform injection
            string injectNamespace = "r.e.p.o_cheat";
            string injectClassName = "Loader";
            string injectMethodName = "Init";

            Console.Write("Injecting... ");
            try
            {
                using (Injector injector = new Injector(handle, monoModule))
                {
                    IntPtr asmAddress = injector.Inject(rawAssembly, injectNamespace, injectClassName, injectMethodName);
                    Console.WriteLine("\nInjection completed successfully.");
                    Console.WriteLine("Injected assembly address: 0x" + asmAddress.ToString("X"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nInjection error: " + ex.Message);
                Console.WriteLine("Make sure the game is running and try again.");
            }
            finally
            {
                Native.CloseHandle(handle);
            }

            // 6. Delete the downloaded DLL file
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

            PromptExit();
        }

        // Displays a message and waits for a key press before exit
        static void PromptExit()
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
