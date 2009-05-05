using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace NModel.Conformance
{
    /// <summary>
    /// Type of text line: Header or an indented line
    /// </summary>
    public enum TextType { HeaderOpenBlock, Header, Indent1, CloseBlock, EmptyLine };

    class LogMetrics
    {
        
        private StreamWriter _sw;        
        private Sequence<string> _text;
        private Sequence<TextType> _type;

        public LogMetrics(StreamWriter sw)
        {
            this._sw = sw;
            this._text = Sequence<string>.EmptySequence;
            this._type = Sequence<TextType>.EmptySequence;

            WriteLine("Test Metrics(");
        }

        public void AddLine(TextType txtType, String text)
        {
            string indent = "";

            switch ((int)txtType)
            {
                // HeaderOpenBlock
                case 0:
                    indent = "    ";
                    break;
                // Header
                case 1:
                    indent = "    ";
                    break;
                // Indent1
                case 2:
                    indent = "         ";
                    break;
                // CloseBlock
                case 3:
                    indent = "    )";
                    break;
                // EmptyLine
                case 4:
                    indent = "";
                    break;
            }
            
            _text = _text.AddLast(indent + text);
            _type = _type.AddLast(txtType);
        }

        public void AddEmptyLine()
        {
            AddLine(TextType.EmptyLine, "");
        }

        public void AddCloseBlock()
        {
            AddLine(TextType.CloseBlock,"");            
        }

        public void Write()
        {
            for(int i = 0; i < _text.Count; ++i)
            {
                string line = "";

                if(_type[i] == TextType.HeaderOpenBlock)
                    line = _text[i] + "(";
                else if ((i < _text.Count - 1) &&
                    (_type[i] != TextType.EmptyLine) &&
                    (_type[i] == _type[i + 1]) &&
                    (_text[i] != ""))
                {
                    line = _text[i] + ",";
                }                
                else if ( (_type[i] == TextType.CloseBlock) &&
                    (i < _text.Count - 1))
                    line = _text[i] + ",";
                else if ((_type[i] == TextType.Header) &&
                    (i < _text.Count - 1))
                    line = _text[i] + ",";
                else if (_type[i] != TextType.EmptyLine)
                    line = _text[i];

                WriteLine(line);
            }

            WriteLine(")");
        }

        private void WriteLine(object value)
        {
            if (_sw == null)
                Console.WriteLine(value);
            else
                _sw.WriteLine(value);
        }
    }
}
