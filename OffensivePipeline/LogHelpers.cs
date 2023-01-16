using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffensivePipeline
{
    public static class LogHelpers
    {
        public static void PrintError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {text}");
            Console.ResetColor();
        }

        public static void PrintOk(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintYellow(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        public static void PrintBlue(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
            Console.ResetColor();
        } 
        
        public static void PrintMagenta(string text)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        
        public static void PrintGray(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void LogToFile(string source, string logType, string messsage)
        {
            try
            {
                DateTime localDate = DateTime.Now;
                using var fileStream = new FileStream(Conf.logFile, FileMode.Append);
                using var writter = new StreamWriter(fileStream);
                writter.WriteLine($"{localDate} [{logType}] {source} -- {messsage}");
            } catch (Exception e)
            {
                PrintError($"LogToFile - {e}");
            }
        }
    }
}
