using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.Util
{
    public class PrintToConsole
    {
        public static void Print(string message)
        {
            Console.WriteLine(message);
        }

        public static void Print(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintMultiColor(string message)
        {
            string[] words = message.Split(' ');

            foreach (string word in words)
            {
                if (CheckIsColorMacro(word))
                {
                    ConsoleColor color;
                    ConvertIntoColor(word, out color);
                    Console.ForegroundColor = color;
                    Console.Write(word + " ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write(word + " ");
                }
            }
        }

        private static bool CheckIsColorMacro(string word)
        {
            if (word.Contains("<color=") && word.Contains('>')) return true;
            else return false;
        }

        private static bool ConvertIntoColor(string word, out ConsoleColor color)
        {
            color = ConsoleColor.White;

            if (!CheckIsColorMacro(word)) return false;


            char[] chars = word.ToCharArray();

            bool onColor = false;

            string colorAsString = "";


            // get the color from the macro

            for (int i = 0; i < chars.Length; i++) 
            { 
                if (chars[i] == '=')
                {
                    onColor = true;
                    continue;
                }

                if (onColor)
                {
                    if (chars[i] == '>') continue;

                    colorAsString += chars[i];
                }
            }

           

            if (Enum.TryParse(colorAsString, out color))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
