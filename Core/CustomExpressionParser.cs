using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScaleWindows.Core
{
    /// <summary>
    /// Dynamic mathematical expression parser for complex dynamics: z_{n+1} = f(z, c).
    /// Parses arbitrary mathematical expressions into an abstract syntax tree (AST) with zero hardcoded patterns.
    /// Supports:
    /// - Variables: z, c, a/x (Re(z)), b/y (Im(z)), x0/cx (Re(c)), y0/cy (Im(c)), i (imaginary unit), pi, e
    /// - Operators: +, -, *, /, ^ (power), |...| (absolute value / modulus)
    /// - Functions: sin(), cos(), exp(), sqrt(), abs(), conj(), bar()
    /// - Implicit multiplication: 2z, 2(a+b), (a+b)(a-b), 2ab, 4ac, i|y|, 2xy
    /// </summary>
    public class CustomExpressionParser
    {
        public string ExpressionText { get; private set; } = "z^2 + c";
        public bool HasParseError { get; private set; }
        private AstNode _rootNode;

        public CustomExpressionParser(string expression)
        {
            _rootNode = new AddNode(new PowNode(new VarZNode(), new ConstNode(new ComplexNumber(2, 0))), new VarCNode());
            SetExpression(expression);
        }

        public void SetExpression(string expr)
        {
            ExpressionText = string.IsNullOrWhiteSpace(expr) ? "z^2 + c" : expr.Trim();
            HasParseError = false;
            try
            {
                var tokens = Tokenize(ExpressionText);
                var parser = new Parser(tokens);
                _rootNode = parser.ParseExpression();
            }
            catch
            {
                HasParseError = true;
                // Fallback default: z^2 + c
                _rootNode = new AddNode(new PowNode(new VarZNode(), new ConstNode(new ComplexNumber(2, 0))), new VarCNode());
            }
        }

        public ComplexNumber EvaluateStep(ComplexNumber z, ComplexNumber c, double x0, double y0)
        {
            return _rootNode.Eval(z, c);
        }

        // ================= AST NODES =================

        public abstract class AstNode
        {
            public abstract ComplexNumber Eval(ComplexNumber z, ComplexNumber c);
        }

        private sealed class ConstNode : AstNode
        {
            private readonly ComplexNumber _val;
            public ConstNode(ComplexNumber val) => _val = val;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _val;
        }

        private sealed class VarZNode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => z;
        }

        private sealed class VarCNode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => c;
        }

        private sealed class VarANode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => new(z.Real, 0);
        }

        private sealed class VarBNode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => new(z.Imaginary, 0);
        }

        private sealed class VarXNode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => new(c.Real, 0);
        }

        private sealed class VarYNode : AstNode
        {
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => new(c.Imaginary, 0);
        }

        private sealed class AddNode : AstNode
        {
            private readonly AstNode _left, _right;
            public AddNode(AstNode left, AstNode right) { _left = left; _right = right; }
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _left.Eval(z, c) + _right.Eval(z, c);
        }

        private sealed class SubNode : AstNode
        {
            private readonly AstNode _left, _right;
            public SubNode(AstNode left, AstNode right) { _left = left; _right = right; }
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _left.Eval(z, c) - _right.Eval(z, c);
        }

        private sealed class MulNode : AstNode
        {
            private readonly AstNode _left, _right;
            public MulNode(AstNode left, AstNode right) { _left = left; _right = right; }
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _left.Eval(z, c) * _right.Eval(z, c);
        }

        private sealed class DivNode : AstNode
        {
            private readonly AstNode _left, _right;
            public DivNode(AstNode left, AstNode right) { _left = left; _right = right; }
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _left.Eval(z, c) / _right.Eval(z, c);
        }

        private sealed class PowNode : AstNode
        {
            private readonly AstNode _left, _right;
            public PowNode(AstNode left, AstNode right) { _left = left; _right = right; }
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c)
            {
                ComplexNumber b = _left.Eval(z, c);
                ComplexNumber exp = _right.Eval(z, c);
                if (Math.Abs(exp.Imaginary) < 1e-9)
                {
                    return b.Pow(exp.Real);
                }
                double r = b.Magnitude;
                if (r == 0) return new ComplexNumber(0, 0);
                double theta = Math.Atan2(b.Imaginary, b.Real);
                ComplexNumber lnB = new(Math.Log(r), theta);
                return (exp * lnB).Exp();
            }
        }

        private sealed class NegNode : AstNode
        {
            private readonly AstNode _child;
            public NegNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => -_child.Eval(z, c);
        }

        private sealed class SinNode : AstNode
        {
            private readonly AstNode _child;
            public SinNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Sin();
        }

        private sealed class CosNode : AstNode
        {
            private readonly AstNode _child;
            public CosNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Cos();
        }

        private sealed class ExpNode : AstNode
        {
            private readonly AstNode _child;
            public ExpNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Exp();
        }

        private sealed class SinhNode : AstNode
        {
            private readonly AstNode _child;
            public SinhNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Sinh();
        }

        private sealed class CoshNode : AstNode
        {
            private readonly AstNode _child;
            public CoshNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Cosh();
        }

        private sealed class TanNode : AstNode
        {
            private readonly AstNode _child;
            public TanNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Tan();
        }

        private sealed class LogNode : AstNode
        {
            private readonly AstNode _child;
            public LogNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Log();
        }

        private sealed class SqrtNode : AstNode
        {
            private readonly AstNode _child;
            public SqrtNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Sqrt();
        }

        private sealed class AbsNode : AstNode
        {
            private readonly AstNode _child;
            public AbsNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => new(_child.Eval(z, c).Magnitude, 0);
        }

        private sealed class ConjNode : AstNode
        {
            private readonly AstNode _child;
            public ConjNode(AstNode child) => _child = child;
            public override ComplexNumber Eval(ComplexNumber z, ComplexNumber c) => _child.Eval(z, c).Conjugate();
        }

        // ================= TOKENIZER =================

        private enum TokenType
        {
            Number,
            Identifier,
            Plus,
            Minus,
            Multiply,
            Divide,
            Power,
            LParen,
            RParen,
            PipeOpen,
            PipeClose,
            EndOfFile
        }

        private readonly struct Token
        {
            public readonly TokenType Type;
            public readonly string Text;
            public readonly double NumberValue;

            public Token(TokenType type, string text = "", double numberValue = 0)
            {
                Type = type;
                Text = text;
                NumberValue = numberValue;
            }
        }

        private static List<Token> Tokenize(string text)
        {
            var rawTokens = new List<Token>();
            int i = 0;
            int len = text.Length;
            int pipeDepth = 0;

            while (i < len)
            {
                char ch = text[i];
                if (char.IsWhiteSpace(ch))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(ch) || (ch == '.' && i + 1 < len && char.IsDigit(text[i + 1])))
                {
                    int start = i;
                    while (i < len && (char.IsDigit(text[i]) || text[i] == '.')) i++;
                    string numStr = text[start..i];
                    double val = double.Parse(numStr, CultureInfo.InvariantCulture);
                    rawTokens.Add(new Token(TokenType.Number, numStr, val));
                    continue;
                }

                if (char.IsLetter(ch))
                {
                    int start = i;
                    while (i < len && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                    string ident = text[start..i].ToLowerInvariant();
                    rawTokens.Add(new Token(TokenType.Identifier, ident));
                    continue;
                }

                if (ch == '*' && i + 1 < len && text[i + 1] == '*')
                {
                    rawTokens.Add(new Token(TokenType.Power, "**"));
                    i += 2;
                    continue;
                }

                switch (ch)
                {
                    case '+': rawTokens.Add(new Token(TokenType.Plus, "+")); break;
                    case '-': rawTokens.Add(new Token(TokenType.Minus, "-")); break;
                    case '*': rawTokens.Add(new Token(TokenType.Multiply, "*")); break;
                    case '/': rawTokens.Add(new Token(TokenType.Divide, "/")); break;
                    case '^': rawTokens.Add(new Token(TokenType.Power, "^")); break;
                    case '(': rawTokens.Add(new Token(TokenType.LParen, "(")); break;
                    case ')': rawTokens.Add(new Token(TokenType.RParen, ")")); break;
                    case '|':
                        if (pipeDepth > 0 && rawTokens.Count > 0 && 
                            (rawTokens[^1].Type is TokenType.Number or TokenType.Identifier or TokenType.RParen or TokenType.PipeClose))
                        {
                            rawTokens.Add(new Token(TokenType.PipeClose, "|"));
                            pipeDepth--;
                        }
                        else
                        {
                            rawTokens.Add(new Token(TokenType.PipeOpen, "|"));
                            pipeDepth++;
                        }
                        break;
                }
                i++;
            }

            // Insert implicit multiplication tokens (e.g. 2z -> 2*z, i|y| -> i*|y|, (a+b)(a-b) -> (a+b)*(a-b))
            var tokens = new List<Token>();
            for (int k = 0; k < rawTokens.Count; k++)
            {
                tokens.Add(rawTokens[k]);
                if (k + 1 < rawTokens.Count)
                {
                    Token cur = rawTokens[k];
                    Token next = rawTokens[k + 1];

                    bool curCanEndTerm = cur.Type is TokenType.Number or TokenType.Identifier or TokenType.RParen or TokenType.PipeClose;
                    bool nextCanStartTerm = next.Type is TokenType.Number or TokenType.Identifier or TokenType.LParen or TokenType.PipeOpen;

                    bool isCurFunction = cur.Type == TokenType.Identifier &&
                        cur.Text is "sin" or "cos" or "tan" or "sinh" or "cosh" or "exp" or "sqrt" or "abs" or "conj" or "bar" or "log" or "ln";

                    if (isCurFunction && next.Type == TokenType.LParen)
                    {
                        // Function calls like sin(...) must NOT have '*' inserted!
                    }
                    else if (curCanEndTerm && nextCanStartTerm)
                    {
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                    }
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile));
            return tokens;
        }

        // ================= RECURSIVE DESCENT PARSER =================

        private sealed class Parser
        {
            private readonly List<Token> _tokens;
            private int _pos;

            public Parser(List<Token> tokens)
            {
                _tokens = tokens;
                _pos = 0;
            }

            private Token Current => _pos < _tokens.Count ? _tokens[_pos] : new Token(TokenType.EndOfFile);

            private Token Advance()
            {
                Token token = Current;
                if (_pos < _tokens.Count) _pos++;
                return token;
            }

            private bool Match(TokenType type)
            {
                if (Current.Type == type)
                {
                    Advance();
                    return true;
                }
                return false;
            }

            public AstNode ParseExpression()
            {
                return ParseAdditive();
            }

            private AstNode ParseAdditive()
            {
                AstNode node = ParseMultiplicative();

                while (Current.Type is TokenType.Plus or TokenType.Minus)
                {
                    TokenType op = Advance().Type;
                    AstNode right = ParseMultiplicative();
                    node = op == TokenType.Plus ? new AddNode(node, right) : new SubNode(node, right);
                }

                return node;
            }

            private AstNode ParseMultiplicative()
            {
                AstNode node = ParsePower();

                while (Current.Type is TokenType.Multiply or TokenType.Divide)
                {
                    TokenType op = Advance().Type;
                    AstNode right = ParsePower();
                    node = op == TokenType.Multiply ? new MulNode(node, right) : new DivNode(node, right);
                }

                return node;
            }

            private AstNode ParsePower()
            {
                AstNode node = ParseUnary();

                if (Current.Type == TokenType.Power)
                {
                    Advance();
                    AstNode right = ParsePower(); // Right-associative: a^b^c = a^(b^c)
                    node = new PowNode(node, right);
                }

                return node;
            }

            private AstNode ParseUnary()
            {
                if (Match(TokenType.Plus))
                {
                    return ParseUnary();
                }
                if (Match(TokenType.Minus))
                {
                    return new NegNode(ParseUnary());
                }

                return ParsePrimary();
            }

            private AstNode ParsePrimary()
            {
                Token token = Advance();

                if (token.Type == TokenType.Number)
                {
                    return new ConstNode(new ComplexNumber(token.NumberValue, 0));
                }

                if (token.Type == TokenType.LParen)
                {
                    AstNode expr = ParseExpression();
                    Match(TokenType.RParen);
                    return expr;
                }

                // Absolute value bars: |expr|
                if (token.Type == TokenType.PipeOpen)
                {
                    AstNode expr = ParseExpression();
                    Match(TokenType.PipeClose);
                    return new AbsNode(expr);
                }

                if (token.Type == TokenType.Identifier)
                {
                    string name = token.Text;
                    bool isFunction = name is "sin" or "cos" or "tan" or "sinh" or "cosh" or "exp" or "sqrt" or "abs" or "conj" or "bar" or "log" or "ln";

                    // Function calls: func(arg)
                    if (isFunction && Current.Type == TokenType.LParen)
                    {
                        Advance(); // Consume '('
                        AstNode arg = ParseExpression();
                        Match(TokenType.RParen);

                        return name switch
                        {
                            "sin" => new SinNode(arg),
                            "cos" => new CosNode(arg),
                            "tan" => new TanNode(arg),
                            "sinh" => new SinhNode(arg),
                            "cosh" => new CoshNode(arg),
                            "exp" => new ExpNode(arg),
                            "sqrt" => new SqrtNode(arg),
                            "abs" => new AbsNode(arg),
                            "conj" or "bar" => new ConjNode(arg),
                            "log" or "ln" => new LogNode(arg),
                            _ => arg
                        };
                    }

                    // Named Variables & Constants:
                    // Both a/x represent Re(z)
                    // Both b/y represent Im(z)
                    // c represents the complex constant parameter
                    return name switch
                    {
                        "z" => new VarZNode(),
                        "c" => new VarCNode(),
                        "a" or "x" or "re" or "rez" => new VarANode(),        // Re(z)
                        "b" or "y" or "im" or "imz" => new VarBNode(),        // Im(z)
                        "x0" or "cx" or "rec" or "cr" => new VarXNode(),      // Re(c)
                        "y0" or "cy" or "imc" or "ci" => new VarYNode(),      // Im(c)
                        "i" => new ConstNode(new ComplexNumber(0, 1)),
                        "pi" => new ConstNode(new ComplexNumber(Math.PI, 0)),
                        "e" => new ConstNode(new ComplexNumber(Math.E, 0)),
                        _ => new VarZNode()
                    };
                }

                return new ConstNode(new ComplexNumber(0, 0));
            }
        }
    }
}
