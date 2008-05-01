//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Internals;

namespace NModel.Terms
{
    /// <summary>
    /// Reads a string like "f(1, "abc", File(1))" and returns the corresponding term. 
    /// </summary>
    internal static class TermReader
    {
        const char EOF = '\u0000';
        const char QuoteChar = '"';
        const char Escape = '\\';
        const char OpenParenthesis = '(';
        const char ClosedParenthesis = ')';
        const char OpenBracket = '<';
        const char ClosedBracket = '>';
        const char Comma = ',';
        const char Minus = '-';
        const char Underscore = '_';
        const char AtSign = '@';

        /// <summary>
        /// Reads a string like "f(1, "abc", File(1))" and returns the corresponding abstract syntax tree. 
        /// </summary>
        /// <param name="str">The text representation of the term</param>
        /// <returns>The abstract syntax tree representation of the term</returns>
        public static Term/*?*/ Read(string str)
        {
            try
            {
                LinkedList<Token> tokens = Tokenize(str);
                Parser p = new Parser(str, tokens);
                return p.Parse();
            }
            // Catch signaling exception for lexical and syntax errors and propagate as an argument exception
            catch (ReadException e)
            {
                throw new ArgumentException(e.msg);
            }
        }

        /// <summary>
        /// Prints a string "f(1, "abc", File(1))" given the abstract syntax representation of a term.
        /// </summary>
        /// <param name="term">The abstract syntax tree representation of the term</param>
        /// <returns>The text representation of the term</returns>
        public static string Write(Term/*?*/ term)
        {
            return term == null ? "null" : term.ToString();
        }

        // just for test
        //public static string[] Tokens(string str)
        //{
        //    LinkedList<Token> tokens = Tokenize(str);
        //    string[] result = new string[tokens.Count];
        //    int i = 0;
        //    foreach (Token t in tokens)
        //    {
        //        result[i++] = t.value;
        //    }
        //    return result;
        //}

        #region ReadException class
        /// <summary>
        /// Internal class for signalling syntax errors
        /// </summary>
        class ReadException : Exception
        {
            public string msg;

            public ReadException(string msg)
            {
                this.msg = msg;
            }
        }
        #endregion

        #region Buffer class
        /// <summary>
        /// Represents a string stream
        /// </summary>
        class Buffer
        {
            string str;
            int pos;
            int bufLen;

            public int Position { get { return pos; } }

            public Buffer(string str)
            {
                this.str = str;
                this.pos = 0;
                this.bufLen = str.Length;
            }

            public char GetCharacter()
            {
                return (pos < bufLen) ? str[pos++] : EOF;
            }

            public char Peek()
            {
                return (pos < bufLen) ? str[pos] : EOF;
            }

            public void UngetCharacter()
            {
                if (pos > 0) pos = pos - 1;
            }

            public void EatWhitespace()
            {
                char ch = GetCharacter();
                while (ch != EOF && Char.IsWhiteSpace(ch)) { ch = GetCharacter(); }
                if (ch != EOF) UngetCharacter();
            }

            public bool EndOfFile()
            {
                return pos >= bufLen;
            }

            /// <summary>
            /// Is next char EOF, whitespace or a separator char?
            /// </summary>
            /// <returns></returns>
            public bool IsDelimited()
            {
                if (pos < bufLen)
                {
                    char c = str[pos];
                    return (Char.IsWhiteSpace(c) || c == OpenParenthesis || c == ClosedParenthesis || c == Comma
                            || c == OpenBracket || c == ClosedBracket);
                }
                else
                    return true;
            }
        }
        #endregion

        #region Token class
        /// <summary>
        /// Token produced by the scanner. Consists of (kind, stringValue, source position)
        /// </summary>
        class Token
        {
            public enum Kind { Open, Close, Comma, LBracket, RBracket, String, Integer, Symbol, Boolean, Null, Wildcard, EOF }
            public readonly Kind kind;
            public readonly string value;
            public readonly int position;

            public Token(Kind kind, string value, int position)
            {
                this.kind = kind;
                this.value = value;
                this.position = position;
            }
        }
        #endregion

        #region Scanner
        /// <summary>
        /// Lexical analysis on the input string
        /// </summary>
        /// <param name="str">The text representation of the term</param>
        /// <returns>List of tokens contained</returns>
        /// <exception cref="ReadException">Thrown if lexical error found</exception>
        static LinkedList<Token> Tokenize(string str)
        {
            LinkedList<Token> result = new LinkedList<Token>();
            Buffer b = new Buffer(str);
            b.EatWhitespace();
            while (!b.EndOfFile())
            {
                bool error;
                Token/*?*/ token = Scan(b, out error);
                if (error)
                {
                    string msg = "At position " + (b.Position - 1).ToString() + ", "
                                 + (b.EndOfFile() ? "end reached while scanning string: " :
                                                    "lexical error in string: ");
                    throw new ReadException(msg + str);
                }
                // assert token != null;
                result.AddLast(token);
                b.EatWhitespace();
            }
            result.AddLast(new Token(Token.Kind.EOF, "eof", b.Position));
            return result;
        }

        /// <summary>
        /// Produces the next token from the stream
        /// </summary>
        /// <param name="b">The string stream</param>
        /// <param name="error">Set to true if a lexical error occurs</param>
        /// <returns>The next token in the stream</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static Token Scan(Buffer b, out bool error)
        {
            int startPosition = b.Position;
            char ch = b.GetCharacter();

            error = false;

            // CASE 1: Next token is a separator character
            if (ch == OpenParenthesis)
                return new Token(Token.Kind.Open, "(", startPosition);
            else if (ch == ClosedParenthesis)
                return new Token(Token.Kind.Close, ")", startPosition);
            if (ch == OpenBracket)
                return new Token(Token.Kind.LBracket, "(", startPosition);
            else if (ch == ClosedBracket)
                return new Token(Token.Kind.RBracket, ")", startPosition);
            else if (ch == Comma)
                return new Token(Token.Kind.Comma, ",", startPosition);
            // CASE 2: Next token is a quoted string
            else if (ch == QuoteChar)
            {
                StringBuilder stringToken = new StringBuilder();
                bool sawEscape = false;
                bool endOfString = false;
                while (!endOfString)
                {
                    ch = b.GetCharacter();
                    if (ch == EOF)
                    {
                        error = true;
                        return null;
                    }
                    else if (ch == Escape && !sawEscape)
                    {
                        sawEscape = true;
                    }
                    else if (sawEscape)  // to do: add && IsEscapedChar(ch)
                    {
                        sawEscape = false;
                        stringToken.Append(ch);
                    }
                    else if (ch == QuoteChar)
                    {
                        endOfString = true;
                    }
                    else
                    {
                        stringToken.Append(ch);
                    }
                }
                return new Token(Token.Kind.String, stringToken.ToString(), startPosition);
            }

            // Case 3: Integer
            else if (Char.IsDigit(ch) || (ch == Minus && !b.IsDelimited() && Char.IsDigit(b.Peek())))
            {
                StringBuilder intToken = new StringBuilder();
                intToken.Append(ch);

                while (!b.IsDelimited())
                {
                    ch = b.GetCharacter();
                    if (Char.IsDigit(ch))
                        intToken.Append(ch);
                    else
                    {
                        error = true;
                        return null;
                    }
                }
                return new Token(Token.Kind.Integer, intToken.ToString(), startPosition);
            }

            // Case 4: Function symbol
            else if (Char.IsLetterOrDigit(ch) || ch == AtSign)
            {
                StringBuilder symbolToken = new StringBuilder();
                symbolToken.Append(ch);

                while (!b.IsDelimited())
                {
                    ch = b.GetCharacter();
                    if (Char.IsLetterOrDigit(ch) || ch == Underscore)
                        symbolToken.Append(ch);
                    else
                    {
                        error = true;
                        return null;
                    }
                }

                string str = symbolToken.ToString();
                if (str == "true" || str == "false" || str == "True" || str == "False")
                    return new Token(Token.Kind.Boolean, str, startPosition);
                else if (str == "null")
                    return new Token(Token.Kind.Null, str, startPosition);
                else
                    return new Token(Token.Kind.Symbol, str, startPosition);
            }

            // Case 5: wildcard
            else if (ch == Underscore && b.IsDelimited())
            {
                return new Token(Token.Kind.Wildcard, new String(ch, 1), startPosition);
            }
            // Case 6: error
            else
            {
                error = true;
                return null;
            }

        }
        #endregion

        /// <summary>
        /// Parser for terms
        /// </summary>
        class Parser
        {
            string str;                           // the input string, used for error msgs only
            IEnumerator<Token> remainingTokens;   // tokens left to process

            public Parser(string str, LinkedList<Token> tokens)
            {
                // this.tokens = tokens;
                this.str = str;
                this.remainingTokens = tokens.GetEnumerator();
                this.remainingTokens.MoveNext();
            }

            /// <summary>
            /// Indicates a syntax error
            /// </summary>
            class SyntaxErrorException : Exception
            {
                public string msg;

                public SyntaxErrorException(string msg)
                {
                    this.msg = msg;
                }
            }

            public Token nextToken
            {
                get { return remainingTokens.Current; }  // lookahead
            }

            void Next()
            {
                remainingTokens.MoveNext();
            }

            void Expect(Token.Kind kind)
            {
                if (nextToken.kind == kind)
                    Next();
                else
                    throw new SyntaxErrorException("saw " + nextToken.kind.ToString() + " where " +
                                                   kind.ToString() + " was expected");
            }

            public Term Parse()
            {
                try
                {
                    Term result = Term();
                    Expect(Token.Kind.EOF);
                    return result;
                }
                catch (SyntaxErrorException e)
                {
                    string msg = "At position " + nextToken.position.ToString() + ", " + e.msg + " in source: " + str;
                    throw new ReadException(msg);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            Term Term()
            {
                Term result;
                switch (nextToken.kind)
                {
                    case Token.Kind.Integer:
                        result = new Literal(int.Parse(nextToken.value));
                        Next();
                        return result;

                    case Token.Kind.String:
                        result = new Literal(nextToken.value);
                        Next();
                        return result;

                    case Token.Kind.Boolean:
                        result = new Literal(bool.Parse(nextToken.value));
                        Next();
                        return result;

                    case Token.Kind.Null:
                        result = new Literal((string)null);
                        Next();
                        return result;

                    case Token.Kind.Wildcard:
                        Next();
                        return Any.Value;

                    case Token.Kind.Symbol:
                        {
                            Symbol symbol = ParseSymbol();

                            // take care of literals
                            if (AbstractValue.GetLiteralTypes().ContainsValue(symbol.Name)
                                && symbol.Name != "string"
                                && symbol.Name != "int"
                                && symbol.Name != "bool")
                            {
                                Expect(Token.Kind.Open);

                                if (nextToken.kind != Token.Kind.String)
                                    throw new SyntaxErrorException("saw " + nextToken.kind.ToString() + " where " +
                                                   Token.Kind.String.ToString() + " was expected");

                                string literalString = nextToken.value;
                                Next();

                                IComparable value;
                                bool flag = false;
                                switch (symbol.Name)
                                {
                                    case "byte":
                                        {
                                            byte v;
                                            flag = byte.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "char":
                                        {
                                            char v;
                                            flag = char.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "double":
                                        {
                                            double v;
                                            flag = double.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "float":
                                        {
                                            float v;
                                            flag = float.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "long":
                                        {
                                            long v;
                                            flag = long.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "sbyte":
                                        {
                                            sbyte v;
                                            flag = sbyte.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "short":
                                        {
                                            short v;
                                            flag = short.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "uint":
                                        {
                                            uint v;
                                            flag = uint.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "ulong":
                                        {
                                            ulong v;
                                            flag = ulong.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    case "ushort":
                                        {
                                            ushort v;
                                            flag = ushort.TryParse(literalString, out v);
                                            value = v;
                                            break;
                                        }
                                    default:
                                        throw new SyntaxErrorException("saw unexpected token " + symbol.Name + " where " +
                                                   "name of literal type was expected (internal error)");

                                }
                                if (!flag)
                                    throw new SyntaxErrorException("could not parse " + literalString + " as .NET type " + symbol.Name);

                                Expect(Token.Kind.Close);
                                return new Literal(value);
                            }
                            else if (nextToken.kind != Token.Kind.Open)
                            {
                                if (null != symbol.DomainParameters &&
                                       symbol.DomainParameters != Sequence<Symbol>.EmptySequence)
                                    throw new SyntaxErrorException("domain parameters not allowed in variable " + symbol.FullName);

                                string variableName = symbol.Name;
                                return new Variable(variableName);
                            }
                            else
                            {
                                Expect(Token.Kind.Open);
                                bool expectComma = false;

                                LinkedList<Term> arguments = new LinkedList<Term>();
                                while (nextToken.kind != Token.Kind.Close)
                                {
                                    if (expectComma) Expect(Token.Kind.Comma);
                                    Term arg = Term();
                                    arguments.AddLast(arg);
                                    expectComma = true;
                                }

                                Expect(Token.Kind.Close);
                                Term[] argsArray = new Term[arguments.Count];
                                arguments.CopyTo(argsArray, 0);
                                return new CompoundTerm(symbol, argsArray);
                            }
                        }
                    default:
                        throw new SyntaxErrorException("saw unexpected " + nextToken.kind.ToString());
                }
            }

            Symbol ParseSymbol()
            {
                if (nextToken.kind != Token.Kind.Symbol)
                    throw new SyntaxErrorException("saw " + nextToken.kind.ToString() + " where " +
                                                   Token.Kind.Symbol.ToString() + " was expected");
                string symbolName = nextToken.value;
                Next();

                if (nextToken.kind == Token.Kind.LBracket)
                {
                    Next();
                    bool expectComma = false;

                    Sequence<Symbol> domainParameters = Sequence<Symbol>.EmptySequence;
                    while (nextToken.kind != Token.Kind.RBracket)
                    {
                        if (expectComma) Expect(Token.Kind.Comma);
                        Symbol domainParameter = ParseSymbol();
                        domainParameters = domainParameters.AddLast(domainParameter);
                        expectComma = true;
                    }

                    //if (domainParameters.Count == 0) 
                    //  throw new SyntaxErrorException("found empty list of domain parameters ");

                    Expect(Token.Kind.RBracket);
                    Symbol[] symArray = new Symbol[domainParameters.Count];
                    domainParameters.CopyTo(symArray, 0);
                    return new Symbol(symbolName, symArray);
                }
                else
                {
                    return new Symbol(symbolName);
                }
            }


            ///// <summary>
            ///// Symbol names are encoded as name[_Start | _Finish].
            ///// </summary>
            ///// <param name="fullName"></param>
            ///// <param name="name"></param>
            ///// <param name="kind"></param>
            //void DecodeSymbolName(string fullName, out string name, out ActionKind kind)
            //{
            //    if (fullName.EndsWith("_Start"))
            //    {
            //        name = fullName.Substring(0, fullName.Length - 6);
            //        kind = ActionKind.Start;
            //    }
            //    else if (fullName.EndsWith("_Finish"))
            //    {
            //        name = fullName.Substring(0, fullName.Length - 7);
            //        kind = ActionKind.Finish;
            //    }
            //    else
            //    {
            //        name = fullName;
            //        kind = ActionKind.Atomic;
            //    }
            //}
        }
    }
}
