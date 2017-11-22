using System;
using System.IO;
using System.Security;
using System.Text;

namespace Utilities
{
    public class Files
    {
        /// <summary>
        /// Read a file, handling exceptions
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Read(string name)
        {
            string result = null;
            if (name != null)
            {
                try
                {
                    result = File.ReadAllText(name, Encoding.UTF8);
                }
                catch (ArgumentException ex)
                {
                    Logging.Error("Failed to read from " + name, ex);
                }
                catch (PathTooLongException ex)
                {
                    Logging.Error("Path " + name + " too long", ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Logging.Error("Directory for " + name + " not found", ex);
                }
                catch (FileNotFoundException ex)
                {
                    Logging.Error("File" + name + " not found", ex);
                }
                catch (IOException ex)
                {
                    Logging.Error("IO exception for " + name, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logging.Error("Not allowed to read from " + name, ex);
                }
                catch (NotSupportedException ex)
                {
                    Logging.Error("Not supported reading from " + name, ex);
                }
                catch (SecurityException ex)
                {
                    Logging.Error("Security exception reading from " + name, ex);
                }

            }
            return result;
        }

        /// <summary>
        /// Write a file, handling exceptions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public static void Write(string name, string content)
        {
            if (name != null && content != null)
            {
                // Attempt to write the file
                try
                {
                    File.WriteAllText(name, content, Encoding.UTF8);
                }
                catch (ArgumentException ex)
                {
                    Logging.Error("Failed to write to " + name, ex);
                }
                catch (PathTooLongException ex)
                {
                    Logging.Error("Path " + name + " too long", ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Logging.Error("Directory for " + name + " not found", ex);
                }
                catch (IOException ex)
                {
                    Logging.Error("IO exception for " + name, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logging.Error("Not allowed to write to " + name, ex);
                }
                catch (NotSupportedException ex)
                {
                    Logging.Error("Not supported writing to " + name, ex);
                }
                catch (SecurityException ex)
                {
                    Logging.Error("Security exception writing to " + name, ex);
                }
            }
        }
    }
}
