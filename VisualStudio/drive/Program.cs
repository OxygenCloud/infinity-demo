using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Example
{
    class Program
    {
        // System Dependencies
        private static readonly string oxygenCloudAgentServerUrl = "https://agent.grid.oxygencloud.com";
        private static readonly string oxygenCloudAuthServerUrl = "https://auth.grid.oxygencloud.com";
        private static readonly string infinityDriveServerUrl = "http://127.0.0.1:26701";

        // Mount Path
        private static readonly string localPathToMount = "<your local path>";
        private static readonly string remotePathToMount = "/";

        static void Main()
        {
            // Clear console for output
            Console.Clear();

            //
            // Get agent token.
            //

            // Authenticate developer key.  
            Dictionary<String, Object> authentication = Infinity.Agent.AuthenticateKey(
                oxygenCloudAgentServerUrl,
                "<replace with your oxygen developer key>"
            );
            if (authentication == null)
            {
                // Handle invalid Developer Key
                Console.WriteLine("Unable authenticate key. Invalid Developer Key.");
                return;
            }
            string agentToken = (string)authentication["agentToken"];

            //
            // Get access token.
            //

            // Authorize access
            Dictionary<String, Object> authorization = Infinity.Auth.AuthorizeAgent(
                oxygenCloudAuthServerUrl,
                agentToken
            );
            if (authorization == null)
            {
                // Handle invalid developer auth token
                Console.WriteLine("Unable to get access token. Access not authorized.");
                return;
            }
            String accessToken = (string)authorization["accessToken"];

            //
            // Print current mounts
            //

            // Get mounts.
            System.Collections.ArrayList mounts = Infinity.Mount.GetMounts(
                infinityDriveServerUrl,
                accessToken
            );

            // Print mounts.
            if (mounts != null)
            {
                Console.Write("Existing Mounts:");
                foreach (Dictionary<string, object> mount in mounts)
                {
                    string localPath = mount["localPath"].ToString();
                    string remotePath = mount["remotePath"].ToString();
                    Console.WriteLine(localPath + " = " + remotePath);
                }
            }

            //
            // Create new mount
            //

            // Validate new local folder does not exist
            if (Directory.Exists(localPathToMount))
            {
                // Handle conflict
                Console.WriteLine("\r\n\r\nError. Local mount path already exists.");
                return;
            }

            // Create local folder
            Directory.CreateDirectory(localPathToMount);

            // Create mount
            Dictionary<String, Object> newMount = Infinity.Mount.CreateMount(
                infinityDriveServerUrl,
                accessToken,
                localPathToMount,
                remotePathToMount
            );
            if (newMount == null)
            {
                // Handle failure to mount
                Console.WriteLine("\r\n\r\nUnable to create mount.");
                return;
            }
            Console.WriteLine("New mount created. " + localPathToMount + "=" + remotePathToMount);

            // Refresh new mount directory
            Infinity.Refresh.RefreshDirectory(
                infinityDriveServerUrl,
                accessToken,
                localPathToMount
            );

            // Open file manager to new mount path
            Process.Start(localPathToMount);

            //
            // Monitor Progress and Errors
            //

            // Keep monitoring until user presses a key.
            Console.WriteLine("Monitor (hit any key to exit) :");
            int startRow = Console.CursorTop;

            while (!Console.KeyAvailable)
            {
                // Print progress
                System.Collections.ArrayList progressList = Infinity.Progress.ListProgress(
                    infinityDriveServerUrl,
                    accessToken
                );
                foreach (Dictionary<string, object> progress in progressList)
                {
                    if (Console.CursorTop == Console.BufferHeight)
                    {
                        // stop if bottom of screen
                        Console.Write(".");
                        break;
                    }
                    Console.WriteLine(progress["message"]);
                }

                // Print errors
                System.Collections.ArrayList errorList = Infinity.Error.ListError(
                    infinityDriveServerUrl,
                    accessToken,
                    true,
                    true,
                    true,
                    true,
                    true
                );
                foreach (Dictionary<string, object> error in errorList)
                {
                    if (Console.CursorTop == Console.BufferHeight)
                    {
                        // stop if bottom of screen
                        Console.Write(".");
                        break;
                    }
                    Console.WriteLine(error["message"]);
                }

                // Wait for next status update
                Thread.Sleep(1000);

                // clear progress
                int consoleCursorTop = Console.CursorTop;
                while (consoleCursorTop >= startRow)
                {
                    // clear current row
                    Console.SetCursorPosition(0, consoleCursorTop--);
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }

            //
            //  Cleanup Demo
            //

            // Delete the mount to stop Infinity Drive 
            if (!Infinity.Mount.DeleteMount(
                    infinityDriveServerUrl,
                    accessToken,
                    localPathToMount,
                    remotePathToMount
            ))
            {
                // Handle drive error
                Console.WriteLine("Unable to delete mount.");
                return;
            }
            Console.WriteLine("Mount deleted.");


            // Delete local mount
            Directory.Delete(localPathToMount, true);
            Console.WriteLine("Local mount folder deleted.");

            Console.WriteLine("Bye bye.");
        }
    }
}
