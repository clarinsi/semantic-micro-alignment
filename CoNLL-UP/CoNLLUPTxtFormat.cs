using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoNLLUP
{
    public class SingleLine
    {
        public bool IsControl { get; private set; }
        public bool IsEmpty { get; }
        public string Text { get; }

        protected SingleLine(string text, bool isControl, bool isEmpty)
        {
            Text = text;
            IsControl = isControl;
            IsEmpty = isEmpty;
        }

        public static SingleLine ParseLine(string text)
        {
            if (text == null)
            {
                return new SingleLine("", false, true);
            }

            var trimmedText = text.Trim();
            if (trimmedText == "")
            {
                return new SingleLine("", false, true);
            }

            if (trimmedText.StartsWith("#"))
            {
                //Control line

                trimmedText = trimmedText.Trim('#').Trim();
                var tokens = trimmedText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 0)
                {
                    ControlType ct = ControlType.None;
                    if (tokens[0].StartsWith("new"))
                    {
                        switch (tokens[0])
                        {
                            case "newdoc":
                                ct = ControlType.Document;
                                break;
                            case "newpar":
                                ct = ControlType.Paragraph;
                                break;
                            case "sent_id":
                                ct = ControlType.Sentence;
                                break;
                        }

                        if (tokens.Any(t => t == "id"))
                        {
                            return new ControlLine(trimmedText, true, ct, "id", tokens.Last());
                        }
                        return new ControlLine(trimmedText, true, ct, null, null);
                    }
                    else
                    {
                        if (tokens.Any(t => t == "="))
                        {
                            ct = ControlType.Parameter;
                            int idx = Array.IndexOf(tokens, "=");

                            return new ControlLine(trimmedText, true, ct, tokens[idx - 1], tokens[idx + 1]);

                        }
                    }

                    return new SingleLine("", false, true);
                }
            }
            else
            {
                //Token line
                var tokens = trimmedText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 10)
                {
                    return new TokenLine(trimmedText, tokens[0], tokens[1], tokens[tokens.Length - 2],
                        tokens[tokens.Length - 1]);
                }
            }

            return new SingleLine("", false, true);
        }
    }

    public enum ControlType
    {
        None,
        Parameter,
        Document,
        Paragraph,
        Sentence
    }

    public class ControlLine : SingleLine
    {
        public bool IsNewDirective { get; }
        public ControlType ControlType { get; }
        public string ProvidedParameterName { get; }
        public string ProvidedParameterValue { get; }

        public ControlLine(string text, bool isNewDirective, ControlType controlType, string providedParameterName, string providedParameterValue) :
            base(text, true, false)
        {
            IsNewDirective = isNewDirective;
            ControlType = controlType;
            ProvidedParameterName = providedParameterName;
            ProvidedParameterValue = providedParameterValue;
        }
    }

    public class TokenLine : SingleLine
    {
        public string EuroVoc { get; }
        public string Iate { get; }
        public string TokenNormative { get; }
        public string Token { get; }

        public TokenLine(string text, string normativeToken, string token, string iate, string euroVoc) :
            base(text, false, false)
        {
            EuroVoc = euroVoc;
            Iate = iate;
            Token = token;
            TokenNormative = normativeToken;
        }
    }

    public class CoNLLUPTxtFormat
    {
    }
}
