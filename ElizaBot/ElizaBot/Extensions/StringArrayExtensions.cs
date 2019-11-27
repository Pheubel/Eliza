using System;
using System.Collections.Generic;
using System.Text;

namespace ElizaBot.Extensions
{
    public static class StringArrayExtensions
    {
        public static string[] ToUpper(this string[] array)
        {
            var upperArray = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                upperArray[i] = array[i].ToUpper();
            }

            return upperArray;
        }

        public static string[] ToLower(this string[] array)
        {
            var lowerArray = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                lowerArray[i] = array[i].ToLower();
            }

            return lowerArray;
        }
    }
}
