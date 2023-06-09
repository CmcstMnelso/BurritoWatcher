using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurritoWatcher
{
    /// <summary>
    /// Class for managing adb device connection on windows.
    /// Requires Windows OS with modern android device connected over usb-debugging
    /// </summary>

    public class ADBSMS:IDisposable
    {
        private String SMSBat = AppDomain.CurrentDomain.BaseDirectory+Setup.ADBDirectory+"\\platform-tools\\sendSMS.bat";
        private String ManageBat = AppDomain.CurrentDomain.BaseDirectory+Setup.ADBDirectory + "\\platform-tools\\manageADB.bat";
        public ADBSMS() {
            //start adb server
            Console.WriteLine("Starting SMS management through ADB..");
            manageADB("start");
            Console.WriteLine("SMS management started successfully.");
        }
        //section IDisposable
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Quit();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //sendSMS number:phone number to be sent to in 1234567890 format, message:text to send
        public void SendSMS(string number, string message)
        {
            callBatFile(SMSBat, $"\"+{number}\" \"{message}\"");
        }

        //provides method to interact with the provided batch files using their parameters
        private string callBatFile(string filePath, string parameters)
        {
            // Create a new process
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = parameters,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(filePath)
                }
            };

            // Start the process
            process.Start();

            // Read the output (optional)
            string output = process.StandardOutput.ReadToEnd();

            // Wait for the process to finish
            process.WaitForExit();

            // Output the result
            //Console.WriteLine(output);

            return output;
        }

        //starts or stops the ADB server via command: start/kill valid
        private void manageADB(string command)
        {
            callBatFile(ManageBat, $"\"{command}\"");
        }
        //properly dispose of objects
        public void Quit()
        {
            //kill adb
            manageADB("kill");
            Console.WriteLine("SMS management stopped.");
        }
    }
}
