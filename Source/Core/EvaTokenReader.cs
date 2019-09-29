using System;

namespace MSD.EvaFollower
{
  /// <summary>
  ///   Small parser for reading the EvaSetting's data.
  /// </summary>
  internal class EvaTokenReader
  {
    private int _startIndex = -1;
    private readonly string _msg;

    public EvaTokenReader(string content)
    {
      _startIndex = 0;
      _msg = content;
    }

    /// <summary>
    ///   End Of File
    /// </summary>
    public bool EOF
    {
      get { return !(_startIndex < _msg.Length - 1); }
    }

    /// <summary>
    ///   Get the next token.
    /// </summary>
    /// <param name="beginChar">The character when the token begins</param>
    /// <param name="endChar">The character at the end of the token.</param>
    /// <returns></returns>
    public string NextToken(char beginChar, char endChar)
    {
      return NextToken(ref _startIndex, beginChar, endChar);
    }

    /// <summary>
    ///   Get the next token
    /// </summary>
    /// <param name="endChar">The ending character of the token.</param>
    /// <returns></returns>
    public string NextTokenEnd(char endChar)
    {
      return NextTokenEnd(ref _startIndex, endChar);
    }

    internal void Consume()
    {
      ++_startIndex;
    }

    private string NextTokenEnd(ref int indexStart, char endChar)
    {
      return NextToken(ref indexStart, '^', endChar);
    }

    private string NextToken(ref int indexStart, char beginChar, char endChar)
    {
      if (beginChar != '^')
      {
        if (_msg[indexStart] != beginChar)
        {
          ParseException(_msg[indexStart], beginChar);
        }
      }

      var str = "";
      var counter = 0;

      var b0 = false;
      var b1 = true;

      var current = '^';
      do
      {
        current = _msg[_startIndex];

        //keep track of multiple begin tokens.
        if (beginChar != '^')
        {
          if (current == beginChar)
          {
            counter++;
          }

          if (current == endChar)
          {
            counter--;
          }
        }

        str += _msg[indexStart];
        indexStart++;

        b0 = current == endChar;
        b1 = counter == 0;
      } while (
        !(b0 & b1)
      );

      if (current != endChar)
      {
        ParseException(current, endChar);
      }

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
      {
        return;
      }

      var c = _msg[_startIndex];

      while (SkipChar(c))
      {
        _startIndex++;

        if (EOF)
        {
          return;
        }

        c = _msg[_startIndex];
      }
    }

    private bool SkipChar(char c)
    {
      return c == ' ' || c == '\t' || c == '\r' || c == '\n';
    }

    private void ParseException(char found, char expected)
    {
      throw new Exception("[EFX] ParseException: Expected: \'" + expected + "\'. Found:" + found);
    }
  }
}