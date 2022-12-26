﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Hndy
{
    public class CsWriter
    {
        private readonly StringBuilder _sb;
        private int _indent;
        private bool _lineHomed = true;

        public override string ToString() => _sb.ToString();

        public CsWriter(ParseOptions parseOptions, string? generatorInfo)
        {
            _sb = new StringBuilder();
            if (!string.IsNullOrEmpty(generatorInfo))
            {
                _sb.Append($"// <auto-generated> Generated by ")
                    .Append(generatorInfo)
                    .AppendLine(" </auto-generated>")
                    .AppendLine();
            }
        }

        public CsWriter Write(string? text)
        {
            if (_lineHomed)
            {
                _sb.Append('\t', _indent);
            }
            _sb.Append(text);
            _lineHomed = false;
            return this;
        }

        public CsWriter Write(string? separator, IEnumerable<string> items)
        {
            bool first = true;
            foreach (var item in items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Write(separator);
                }
                Write(item);
            }
            return this;
        }

        public CsWriter WriteLine(string? text = null)
        {
            if (_lineHomed)
            {
                _sb.Append('\t', _indent);
            }
            _sb.AppendLine(text);
            _lineHomed = true;
            return this;
        }

        public CsWriter WriteLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                WriteLine(line);
            }
            return this;
        }

        public CsWriter Indent(Action action)
        {
            var indent = _indent;
            _indent = indent + 1;
            action.Invoke();
            _indent = indent;
            return this;
        }

        public CsWriter IndentChain<T>(IEnumerable<T> chain, Action<T>? preAction, Action? action, Action<T>? postAction)
        {
            var indent = _indent;
            var stack = new Stack<T>();
            foreach (var item in chain)
            {
                stack.Push(item);
                preAction?.Invoke(item);
                _indent = ++indent;
            }
            action?.Invoke();
            foreach (var item in stack)
            {
                _indent = --indent;
                postAction?.Invoke(item);
            }
            return this;
        }

        public CsWriter BraceIndent(Action? action, string postfix = "", bool endLastLine = true)
        {
            WriteLine("{");
            if (action is not null)
            {
                Indent(action);
            }
            Write("}" + postfix);
            if (endLastLine)
            {
                WriteLine();
            }
            return this;
        }

        public CsWriter BraceIndent(string preLine, Action? action, string postfix = "", bool endLastLine = true)
        {
            WriteLine(preLine);
            return BraceIndent(action, postfix, endLastLine);
        }

        public CsWriter BraceIndentChain<T>(IEnumerable<T> chain, Func<T, string>? preLine, Action? action)
        {
            return IndentChain(chain,
                t =>
                {
                    if(preLine is not null)
                    {
                        WriteLine(preLine.Invoke(t));
                    }
                    WriteLine("{");
                },
                action,
                t =>
                {
                    WriteLine("}");
                });
        }

        public CsWriter WriteParentheses(string? preText, IEnumerable<string> items)
        {
            Write(preText);
            Write("(");
            Write(", ", items);
            Write(")");
            return this;
        }

        public CsWriter WriteParenthesesWrapped<T>(string? preText, IEnumerable<T> items, Action<T> writeItem)
        {
            Write(preText);
            Write("(");
            bool first = true;
            var indent = _indent;
            foreach (var item in items)
            {
                if (first)
                {
                    first = false;
                    WriteLine();
                    _indent = indent + 1;
                }
                else
                {
                    WriteLine(",");
                }
                writeItem(item);
            }
            Write(")");
            _indent = indent;
            return this;
        }

        public CsWriter WriteGeneric(string? preText, IEnumerable<string> args)
        {
            Write(preText);
            if (args.Any())
            {
                Write("<");
                Write(", ", args);
                Write(">");
            }
            return this;
        }
    }
}
