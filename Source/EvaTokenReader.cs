using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSD.EvaFollower
{
    /// <summary>
    /// Small parser for reading the EvaSetting's data.
    /// </summary>
    class EvaTokenReader
    {
         int startIndex = -1;
         string msg = "";

        public EvaTokenReader(string content)
        {
            startIndex = 0;
            this.msg = content;
        }

        /// <summary>
        /// End Of File
        /// </summary>
        public bool EOF
        {
            get{
                return !(startIndex < msg.Length-1); 
            }
        }

        /// <summary>
        /// Get the next token.
        /// </summary>
        /// <param name="beginChar">The character when the token begins</param>
        /// <param name="endChar">The character at the end of the token.</param>
        /// <returns></returns>
        public string NextToken(char beginChar, char endChar)
        {
            return NextToken(ref startIndex, beginChar, endChar);
        }

        /// <summary>
        /// Get the next token
        /// </summary>
        /// <param name="endChar">The ending character of the token.</param>
        /// <returns></returns>
        public string NextTokenEnd(char endChar)
        {
            return NextTokenEnd(ref startIndex, endChar);
        }

        private string NextTokenEnd(ref int indexStart, char endChar)
        {
            return NextToken(ref indexStart, '^', endChar);
        }

        private string NextToken(ref int indexStart, char beginChar, char endChar)
        {
            if (beginChar != '^')
            {
                if (msg[indexStart] != beginChar)
                    ParseException(msg[indexStart], beginChar);
            }

            string str = "";
            int counter = 0;

            bool b0 = false;
            bool b1 = true;

            char current = '^';
            do
            {
                current = msg[startIndex];

                //keep track of multiple begin tokens.
                if (beginChar != '^')
                {
                    if (current == beginChar)
                        counter++;

                    if (current == endChar)
                        counter--;
                }
                
                str += msg[indexStart];
                indexStart++;
                
                b0 = (current == endChar);
                b1 = (counter == 0);

             } while (                 
                !( b0 & b1 )
            );   

            if (current != endChar)
                ParseException(current, endChar);
            
            Skip();

            //strip token.
            str = str.Remove(str.Length - 1, 1);

            if (beginChar != '^')
            {
                str = str.Remove(0, 1);
            }

            return str;
        }


        private void Skip()
        {
            if (EOF)
                return;

            char c = msg[startIndex];

            while (SkipChar(c))
            {
                startIndex++;
 
                if (EOF)
                    return;

                c = msg[startIndex];
            }
        }

        private bool SkipChar(char c)
        {
            return (c == ' ' || c == '\t' || c == '\r' || c == '\n');
        }

        private void ParseException(char found, char expected)
        {
            throw new Exception("[EFX] ParseException: Expected: \'" + expected + "\'. Found:" + found); // NO_LOCALIZATION
        }

        internal void Consume()
        {
            ++startIndex;
        }
    }
}
