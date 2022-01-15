using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Utilities
{
    internal static class VariableBuilder
    {
        internal static List<char> GetLowerCaseAlphabetCharaterSet()
        {
            return new List<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        }
        internal static List<char> GetUpperCaseAlphabetCharaterSet()
        {
            return new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        }

        internal static List<char> GetNumberCharaterSet()
        {
            return new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        }

        internal static List<char> GetSpecialCharaterSet()
        {
            return new List<char>() { '~', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '|', '\\', '{', '}', '[', ']', ':', ';', '\"', '\'', '<', ',', '>', '.', '?', '/' };
        }

        internal static List<char> GetCharacterSet(bool includeSpecials = false)
        {
            List<char> characterSet = new List<char>();
            characterSet.AddRange(GetLowerCaseAlphabetCharaterSet());
            characterSet.AddRange(GetUpperCaseAlphabetCharaterSet());
            characterSet.AddRange(GetNumberCharaterSet());

            if (includeSpecials)
            {
                characterSet.AddRange(GetSpecialCharaterSet());
            }

            return characterSet;
        }
    }
}
