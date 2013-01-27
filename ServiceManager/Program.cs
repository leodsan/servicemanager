using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string command = args[0];
            string service = args[1];
            string hostname = args[2];

            string filename = null;

            if (args.Length == 4)
            {
                filename = args[3];
            }

            Console.WriteLine("Executing Command: " + command + " for Service: " + service + "(" + filename + ") on Machine: " + hostname);

            if (command == "start")
            {
                StartService(service, hostname,filename);
            }

            if (command == "stop")
            {
                StopService(service, hostname,filename);
            }

            if (command == "wait_for_delete")
            {
                WaitForDelete(filename);
            }
        }

        static void WaitForDelete(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("File Already Deleted");
                return;
            }

            while (true)
            {
                try
                {
                    File.Delete(filename);
                    break;
                } catch {
                    Console.WriteLine("Error While Deleting... Waiting 1 second");
                    Thread.Sleep(1000);
                }
            }
        }

        static void StartService(string service, string hostname, string filename)
        {
            ServiceController sc = new ServiceController(service, hostname);

            if (sc.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine("Service Already Started, Aborting");
                return;
            }

            try
            {
                Console.WriteLine("Starting Service");
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running);
                Console.WriteLine("Service Started");
            }
            catch (Exception)
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    return;
                }

                Console.WriteLine("Problem Starting Service");

            }
        }

        static void StopService(string service, string hostname, string filename)
        {
            ServiceController sc = new ServiceController(service, hostname);

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                Console.WriteLine("Service Already Stopped, Aborting");
                return;
            }

            try
            {
                Console.WriteLine("Stopping Service");
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                Console.WriteLine("Service Stopped");
            }
            catch (Exception)
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    return;
                }

                Console.WriteLine("Problem Stopping Service.");
            }

            var processes = Process.GetProcessesByName(filename, hostname);

            foreach (var process in processes)
            {
                Console.WriteLine("Aborting process: " + process.Id + " on Machine: " + hostname);

                process.Kill();
            }
        }
    }
}
