//////////////////////////////////////////////////////////////////////////////
// This source code and all associated files and resources are copyrighted by
// the author(s). This source code and all associated files and resources may
// be used as long as they are used according to the terms and conditions set
// forth in The Code Project Open License (CPOL), which may be viewed at
// http://www.blackbeltcoder.com/Legal/Licenses/CPOL.
//
// Copyright (c) 2010 Jonathan Wood
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibCS.EvaluateEquation
{
    /// <summary>
    /// Custom exception for evaluation errors
    /// </summary>
    public class EvalException : Exception
    {
        /// <summary>
        /// Zero-base position in expression where exception occurred
        /// </summary>
        public int Column { get; set; }

        public string ErrorIn = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message that describes this exception</param>
        /// <param name="position">Position within expression where exception occurred</param>
        //public EvalException(string message, int position)
        //    : base(message)
        //{
        //    this.Column = position;
        //}

        public EvalException(string message, string ErrorIn, int position) : base(message)
        {
            this.ErrorIn = ErrorIn;
            this.Column = position;

        }

        /// <summary>
        /// Gets the message associated with this exception
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("{0} (column {1})", base.Message, Column + 1);
            }
        }
    }

    public enum SymbolStatus
    {
        OK,
        UndefinedSymbol,
        UndefinedVariable,
    }

    // ProcessSymbol arguments
    public class SymbolEventArgs : EventArgs
    {
        public string Name { get; set; }
        public object Result { get; set; }
        public SymbolStatus Status { get; set; }
    }

    /// <summary>
    /// Expression evaluator class
    /// </summary>
    public class EvaluateEquation
    {
        // Event handers
        public delegate void ProcessSymbolHandler(object sender, SymbolEventArgs e);
        public event ProcessSymbolHandler ProcessSymbol;

        // Token state enums
        protected enum State
        {
            None = 0,
            Operand = 1,
            Operator = 2,
            UnaryOperator = 3
        }

        // Operator state enum 
        protected enum Operator
        {
            None,
            Plus,
            Minus,
            Multiply,
            Division,
            Power,

            /// <summary>
            /// To distinguish it from a minus operator, we'll use a character unlikely to appear in expressions to signify a unary negative.
            /// </summary>
            UnaryMinus,
        }

        // Error messages
        protected static class ErrorMessages
        {
            public static string ErrInvalidOperand = "Invalid operand";
            public static string ErrOperandExpected = "Operand expected";
            public static string ErrOperatorExpected = "Operator expected";
            public static string ErrUnmatchedClosingParen = "Closing parenthesis without matching open parenthesis";
            public static string ErrMultipleDecimalPoints = "Operand contains multiple decimal points";
            public static string ErrUnexpectedCharacter = "Unexpected character encountered \"{0}\"";
            public static string ErrUndefinedSymbol = "Undefined symbol \"{0}\"";
            public static string ErrUndefinedFunction = "Undefined function \"{0}\"";
            public static string ErrClosingParenExpected = "Closing parenthesis expected";
            public static string ErrWrongParamCount = "Wrong number of function parameters";
            public static string ErrWrongArguments = "Wrong arguments of function parameters";
            public static string ErrDataVariable = "Data variable not existent \"{0}\"";
        }


        // Constructor
        public EvaluateEquation()
        {
        }

        /// <summary>
        /// Evaluates the given expression and returns the result
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns></returns>
        public object Execute(string expression)
        {
            if (expression.Contains('{'))
                expression = expression.Replace('{', '(');
            if (expression.Contains('['))
                expression = expression.Replace('[', '(');
            if (expression.Contains('}'))
                expression = expression.Replace('}', ')');
            if (expression.Contains(']'))
                expression = expression.Replace(']', ')');
            
            return ExecuteTokens(TokenizeExpression(expression));
        }

        /// <summary>
        /// Converts a standard infix expression to list of tokens in
        /// postfix order.
        /// </summary>
        /// <param name="expression">Expression to evaluate</param>
        /// <returns></returns>
        protected List<object> TokenizeExpression(string expression)
        {
            List<object> tokens = new List<object>();
            Stack<object> stack = new Stack<object>();
            State state = State.None;
            int parenCount = 0;
            string temp;

            TextParser parser = new TextParser(expression);

            while (!parser.EndOfText)
            {
                if (Char.IsWhiteSpace(parser.Peek()))
                {
                    // Ignore spaces, tabs, etc.
                }
                else if (parser.Peek() == '(')
                {
                    // Cannot follow operand
                    if (state == State.Operand)
                        throw new EvalException(ErrorMessages.ErrOperatorExpected, parser.Peek().ToString(), parser.Position);

                    // Allow additional unary operators after "("
                    if (state == State.UnaryOperator)
                        state = State.Operator;

                    // Push opening parenthesis onto stack
                    stack.Push(parser.Peek().ToString());

                    // Track number of parentheses
                    parenCount++;
                }
                else if (parser.Peek() == ')')
                {
                    // Must follow operand
                    if (state != State.Operand)
                        throw new EvalException(ErrorMessages.ErrOperandExpected, parser.Peek().ToString(), parser.Position);

                    // Must have matching open parenthesis
                    if (parenCount == 0)
                        throw new EvalException(ErrorMessages.ErrUnmatchedClosingParen, parser.Peek().ToString(), parser.Position);

                    // Pop all operators until matching "(" found
                    var tmp = stack.Pop();
                    while (true)
                    {
                        if (tmp is String && (string)tmp == "(")
                        {
                            break;
                        }
                        else
                        {
                            tokens.Add(tmp);
                            tmp = stack.Pop();
                        }
                    }
                    // Track number of parentheses
                    parenCount--;
                }
                else if ("+-*/^".Contains(parser.Peek()))
                {
                    // Need a bit of extra code to support unary operators
                    if (state == State.Operand)
                    {
                        // Pop operators with precedence >= current operator
                        char opration = parser.Peek();
                        Operator op = Operator.None;
                        if (opration == '+')
                            op = Operator.Plus;
                        else if (opration == '-')
                            op = Operator.Minus;
                        else if (opration == '*')
                            op = Operator.Multiply;
                        else if (opration == '/')
                            op = Operator.Division;
                        else if (opration == '^')
                            op = Operator.Power;

                        int currPrecedence = GetPrecedence(op);

                        while (stack.Count > 0 &&
                               stack.Peek() is Operator &&
                               (GetPrecedence((Operator)stack.Peek()) >= currPrecedence))
                            tokens.Add(stack.Pop());
                        stack.Push(op);
                        state = State.Operator;
                    }
                    else if (state == State.UnaryOperator)
                    {
                        // Don't allow two unary operators together
                        throw new EvalException(ErrorMessages.ErrOperandExpected,parser.Peek().ToString(), parser.Position);
                    }
                    else
                    {
                        // Test for unary operator
                        if (parser.Peek() == '-')
                        {
                            // Push unary minus
                            stack.Push(Operator.UnaryMinus);
                            state = State.UnaryOperator;
                        }
                        else if (parser.Peek() == '+')
                        {
                            // Just ignore unary plus
                            state = State.UnaryOperator;
                        }
                        else
                        {
                            throw new EvalException(ErrorMessages.ErrOperandExpected, parser.Peek().ToString(), parser.Position);
                        }
                    }
                }
                else if (Char.IsDigit(parser.Peek()) || parser.Peek() == '.')
                {
                    if (state == State.Operand)
                    {
                        // Cannot follow other operand
                        throw new EvalException(ErrorMessages.ErrOperatorExpected, parser.Peek().ToString(), parser.Position);
                    }
                    // Parse number
                    double v = ParseNumberToken(parser);
                    tokens.Add(v);
                    state = State.Operand;
                    continue;
                }
                else
                {
                    object result;

                    // Parse symbols and functions
                    if (state == State.Operand)
                    {
                        // Symbol or function cannot follow other operand
                        throw new EvalException(ErrorMessages.ErrOperatorExpected, parser.Peek().ToString(), parser.Position);
                    }
                    if (!(Char.IsLetter(parser.Peek()) || parser.Peek() == '_'))
                    {
                        // Invalid character
                        throw new EvalException(String.Format(ErrorMessages.ErrUnexpectedCharacter, parser.Peek()), parser.Peek().ToString(), parser.Position);
                    }
                    // Save start of symbol for error reporting
                    int symbolPos = parser.Position;
                    // Parse this symbol
                    temp = ParseSymbolToken(parser);
                    // Skip whitespace
                    parser.MovePastWhitespace();

                    // Check for parameter list
                    if (parser.Peek() == '(')
                    {
                        // Found parameter list, evaluate function
                        result = EvaluateFunction(parser, temp, symbolPos);
                    }
                    else
                    {
                        // No parameter list, evaluate symbol (variable)
                        result = EvaluateSymbol(temp, symbolPos);
                    }

                    tokens.Add(result);
                    state = State.Operand;
                    continue;
                }
                parser.MoveAhead();
            }

            // Expression cannot end with operator
            if (state == State.Operator || state == State.UnaryOperator)
                throw new EvalException(ErrorMessages.ErrOperandExpected, parser.Peek().ToString(), parser.Position);
            // Check for balanced parentheses
            if (parenCount > 0)
                throw new EvalException(ErrorMessages.ErrClosingParenExpected, parser.Peek().ToString(), parser.Position);
            // Retrieve remaining operators from stack
            while (stack.Count > 0)
                tokens.Add(stack.Pop());
            return tokens;
        }

        /// <summary>
        /// Parses and extracts a numeric value at the current position
        /// </summary>
        /// <param name="parser">TextParser object</param>
        /// <returns></returns>
        protected double ParseNumberToken(TextParser parser)
        {
            System.Globalization.NumberFormatInfo NumberFormatEN = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
            bool hasDecimal = false;
            int start = parser.Position;
            while (Char.IsDigit(parser.Peek()) || parser.Peek() == '.')
            {
                if (parser.Peek() == '.')
                {
                    if (hasDecimal)
                        throw new EvalException(ErrorMessages.ErrMultipleDecimalPoints, parser.Peek().ToString(), parser.Position);
                    hasDecimal = true;
                }
                parser.MoveAhead();
            }
            // Extract token
            string token = parser.Extract(start, parser.Position);
            if (token == ".")
                throw new EvalException(ErrorMessages.ErrInvalidOperand, parser.Peek().ToString(), parser.Position - 1);
      
            return double.Parse(token, NumberFormatEN);
        }

        /// <summary>
        /// Parses and extracts a symbol at the current position
        /// </summary>
        /// <param name="parser">TextParser object</param>
        /// <returns></returns>
        protected string ParseSymbolToken(TextParser parser)
        {
            int start = parser.Position;
            while (Char.IsLetterOrDigit(parser.Peek()) || parser.Peek() == '_')
                parser.MoveAhead();
            return parser.Extract(start, parser.Position);
        }

        /// <summary>
        /// Evaluates a function and returns its value. It is assumed the current
        /// position is at the opening parenthesis of the argument list.
        /// </summary>
        /// <param name="parser">TextParser object</param>
        /// <param name="name">Name of function</param>
        /// <param name="pos">Position at start of function</param>
        /// <returns></returns>
        protected object EvaluateFunction(TextParser parser, string name, int pos)
        {
            FunctionEvent f = new FunctionEvent()
            {
                Name = name,
                // Parse function parameters
                Parameters = ParseParameters(parser),
                Status = FunctionStatus.OK,
            };

            ProcessFunction(ref f);
            if (f.Status == FunctionStatus.UndefinedFunction)
                throw new EvalException(String.Format(ErrorMessages.ErrUndefinedFunction, name), name, pos);
            if (f.Status == FunctionStatus.WrongParameterCount)
                throw new EvalException(ErrorMessages.ErrWrongParamCount, name, pos);
            if (f.Status == FunctionStatus.WrongArgument)
                throw new EvalException(ErrorMessages.ErrWrongArguments, name, pos);
            return f.Result;
        }

        /// <summary>
        /// Evaluates each parameter of a function's parameter list and returns
        /// a list of those values. An empty list is returned if no parameters
        /// were found. It is assumed the current position is at the opening
        /// parenthesis of the argument list.
        /// </summary>
        /// <param name="parser">TextParser object</param>
        /// <returns></returns>
        protected List<object> ParseParameters(TextParser parser)
        {
            // Move past open parenthesis
            parser.MoveAhead();

            // Look for function parameters
            List<object> parameters = new List<object>();
            parser.MovePastWhitespace();
            if (parser.Peek() != ')')
            {
                // Parse function parameter list
                int paramStart = parser.Position;
                int parenCount = 1;

                while (!parser.EndOfText)
                {
                    if (parser.Peek() == ',')
                    {
                        // Note: Ignore commas inside parentheses. They could be
                        // from a parameter list for a function inside the parameters
                        if (parenCount == 1)
                        {
                            parameters.Add(EvaluateParameter(parser, paramStart));
                            paramStart = parser.Position + 1;
                        }
                    }
                    if (parser.Peek() == ')')
                    {
                        parenCount--;
                        if (parenCount == 0)
                        {
                            parameters.Add(EvaluateParameter(parser, paramStart));
                            break;
                        }
                    }
                    else if (parser.Peek() == '(')
                    {
                        parenCount++;
                    }
                    parser.MoveAhead();
                }
            }
            // Make sure we found a closing parenthesis
            if (parser.Peek() != ')')
                throw new EvalException(ErrorMessages.ErrClosingParenExpected, parser.Peek().ToString(), parser.Position);
            // Move past closing parenthesis
            parser.MoveAhead();
            // Return parameter list
            return parameters;
        }

        /// <summary>
        /// Extracts and evaluates a function parameter and returns its value. If an
        /// exception occurs, it is caught and the column is adjusted to reflect the
        /// position in original string, and the exception is rethrown.
        /// </summary>
        /// <param name="parser">TextParser object</param>
        /// <param name="paramStart">Column where this parameter started</param>
        /// <returns></returns>
        protected object EvaluateParameter(TextParser parser, int paramStart)
        {
            try
            {
                // Extract expression and evaluate it
                string expression = parser.Extract(paramStart, parser.Position);
                return Execute(expression);
            }
            catch (EvalException ex)
            {
                // Adjust column and rethrow exception
                ex.Column += paramStart;
                throw ex;
            }
        }

        /// <summary>
        /// This method evaluates a symbol name and returns its value.
        /// </summary>
        /// <param name="name">Name of symbol</param>
        /// <param name="pos">Position at start of symbol</param>
        /// <returns></returns>
        protected object EvaluateSymbol(string name, int pos)
        {
            object result = default(double);
            // We found a symbol reference
            SymbolStatus status = SymbolStatus.UndefinedSymbol;
            if (ProcessSymbol != null)
            {
                SymbolEventArgs args = new SymbolEventArgs();
                args.Name = name;
                args.Result = result;
                args.Status = SymbolStatus.OK;
                ProcessSymbol(this, args);
                result = args.Result;
                status = args.Status;
            }
            if (status == SymbolStatus.UndefinedSymbol)
                throw new EvalException(String.Format(ErrorMessages.ErrUndefinedSymbol, name), name , pos);
            if( status == SymbolStatus.UndefinedVariable)
                throw new EvalException(String.Format(ErrorMessages.ErrDataVariable, name), name, pos);

            return result;
        }

        /// <summary>
        /// Evaluates the given list of tokens and returns the result.
        /// Tokens must appear in postfix order.
        /// </summary>
        /// <param name="tokens">List of tokens to evaluate.</param>
        /// <returns></returns>
        protected object ExecuteTokens(List<object> tokens)
        {
            Stack<object> stack = new Stack<object>();
            foreach (object token in tokens)
            {
                if (token is Double)
                {
                    stack.Push((double)token);
                }
                else if (token is DoubleArray)
                {
                    stack.Push((DoubleArray)token);
                }
                else if (token is Operator)
                {
                    Operator op = (Operator)token;
                    if (op == Operator.Plus)
                    {
                        var tmp1 = stack.Pop();
                        var tmp2 = stack.Pop();

                        if (tmp1 is double && tmp2 is double)
                            stack.Push((double)tmp1 + (double)tmp2);

                        else if (tmp1 is DoubleArray && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp1 + (DoubleArray)tmp2);

                        else if (tmp1 is double && tmp2 is DoubleArray)
                            stack.Push((double)tmp1 + (DoubleArray)tmp2);

                        else if (tmp1 is DoubleArray && tmp2 is double)
                            stack.Push((DoubleArray)tmp1 + (double)tmp2);
                    }
                    else if (op == Operator.Minus)
                    {
                        var tmp1 = stack.Pop();
                        var tmp2 = stack.Pop();

                        if (tmp1 is double && tmp2 is double)
                            stack.Push((double)tmp2 - (double)tmp1);

                        else if (tmp1 is DoubleArray && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp2 - (DoubleArray)tmp1);

                        else if (tmp1 is double && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp2 - (double)tmp1);

                        else if (tmp1 is DoubleArray && tmp2 is double)
                            stack.Push((double)tmp2 - (DoubleArray)tmp1);
                    }
                    else if (op == Operator.Multiply)
                    {
                        var tmp1 = stack.Pop();
                        var tmp2 = stack.Pop();

                        if (tmp1 is double && tmp2 is double)
                            stack.Push((double)tmp1 * (double)tmp2);

                        else if (tmp1 is DoubleArray && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp1 * (DoubleArray)tmp2);

                        else if (tmp1 is double && tmp2 is DoubleArray)
                            stack.Push((double)tmp1 * (DoubleArray)tmp2);

                        else if (tmp1 is DoubleArray && tmp2 is double)
                            stack.Push((DoubleArray)tmp1 * (Double)tmp2);
                    }
                    else if (op == Operator.Division)
                    {
                        var tmp1 = stack.Pop();
                        var tmp2 = stack.Pop();

                        if (tmp1 is Double && tmp2 is Double)
                            stack.Push((double)tmp2 / (double)tmp1);

                        else if (tmp1 is DoubleArray && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp2 / (DoubleArray)tmp1);

                        else if (tmp1 is Double && tmp2 is DoubleArray)
                            stack.Push((DoubleArray)tmp2 / (double)tmp1);

                        else if (tmp1 is DoubleArray && tmp2 is Double)
                            stack.Push((double)tmp2 / (DoubleArray)tmp1);
                    }
                    else if (op == Operator.Power)
                    {
                        var tmp1 = stack.Pop();
                        var tmp2 = stack.Pop();

                        if (tmp1 is Double && tmp2 is Double)
                            stack.Push(Math.Pow((double)tmp2, (double)tmp1));

                        else if (tmp1 is DoubleArray && tmp2 is DoubleArray)
                            stack.Push(DoubleArray.Pow((DoubleArray)tmp2, (DoubleArray)tmp1));

                        else if (tmp1 is Double && tmp2 is DoubleArray)
                            stack.Push(DoubleArray.Pow((DoubleArray)tmp2, (double)tmp1));

                        else if (tmp1 is DoubleArray && tmp2 is Double)
                            stack.Push(DoubleArray.Pow((double)tmp2, (DoubleArray)tmp1));
                    }
                    else if (op == Operator.UnaryMinus)
                    {
                        var tmp = stack.Pop();

                        if (tmp is Double)
                            stack.Push(-(double)tmp);

                        else if (tmp is DoubleArray)
                            stack.Push(-1.0 * (DoubleArray)tmp);
                    }
                }
            }
            // Remaining item on stack contains result
            object r = null;
            if (stack.Peek() is Double)
                r = (stack.Count > 0) ? (double)stack.Pop() : 0.0;
            else if (stack.Peek() is DoubleArray)
                r = (stack.Count > 0) ? (DoubleArray)stack.Pop() : new DoubleArray();

            return r;
        }

        /// <summary>
        /// Returns a value that indicates the relative precedence of
        /// the specified operator
        /// </summary>
        /// <param name="s">Operator to be tested</param>
        /// <returns></returns>
        protected int GetPrecedence(Operator op)
        {
            switch (op)
            {
                case Operator.Plus:
                case Operator.Minus:
                    return 1;
                case Operator.Multiply:
                case Operator.Division:
                    return 2;
                case Operator.Power:
                    return 3;
                case Operator.UnaryMinus:
                    return 10;
            }
            return 0;
        }

        // Implement expression functions

        protected enum FunctionStatus
        {
            OK,
            UndefinedFunction,
            WrongParameterCount,
            WrongArgument,
        }

        // ProcessFunction arguments
        protected class FunctionEvent
        {
            public string Name { get; set; }
            public List<object> Parameters { get; set; }
            public object Result { get; set; }
            public FunctionStatus Status { get; set; }
        }

        public enum Functions
        {
            none, 
            abs,
            arcos,
            arsin,
            arctan,
            ceiling,
            cos,
            cosh,
            exp,
            floor,
            log,
            log10,
            pow,
            round,
            sin,
            sinh,
            sqrt,
            tan,
            tanh,
            truncate,
            toDeg,
            toRad,
            mean,
        }

        protected void ProcessFunction(ref FunctionEvent e)
        {
            if (String.Compare(e.Name, Functions.abs.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Abs((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Abs((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.arcos.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Acos((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Acos((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.arsin.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Asin((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Asin((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.arctan.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Atan((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Atan((DoubleArray)e.Parameters[0]);
                }
                else if (e.Parameters.Count == 2)
                {
                    if (e.Parameters[0] is Double && e.Parameters[1] is Double)
                        e.Result = Math.Atan2((double)e.Parameters[0], (double)e.Parameters[1]);
                    else if (e.Parameters[0] is DoubleArray && e.Parameters[1] is Double)
                        e.Result = DoubleArray.Atan2((DoubleArray)e.Parameters[0], (double)e.Parameters[1]);
                    else if (e.Parameters[0] is Double && e.Parameters[1] is DoubleArray)
                        e.Result = DoubleArray.Atan2((Double)e.Parameters[0], (DoubleArray)e.Parameters[1]);
                    else
                        e.Status = FunctionStatus.WrongArgument;
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.ceiling.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Ceiling((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Ceiling((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.cos.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Cos((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Cos((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.cosh.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Cosh((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Cosh((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.exp.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Exp((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Exp((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.floor.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Floor((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Floor((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.log.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Log((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Log((DoubleArray)e.Parameters[0]);
                }
                else if (e.Parameters.Count == 2)
                {
                    if (e.Parameters[0] is Double && e.Parameters[1] is Double)
                        e.Result = Math.Log((double)e.Parameters[0], (int)e.Parameters[1]);
                    else if (e.Parameters[0] is DoubleArray && e.Parameters[1] is Double)
                        e.Result = DoubleArray.Log((DoubleArray)e.Parameters[0], (int)e.Parameters[1]);
                    else
                        e.Status = FunctionStatus.WrongArgument;
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.log10.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Log10((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Log10((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.pow.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 2)
                {
                    if (e.Parameters[0] is Double && e.Parameters[1] is Double)
                        e.Result = Math.Pow((double)e.Parameters[0], (double)e.Parameters[1]);
                    else if (e.Parameters[0] is DoubleArray && e.Parameters[1] is Double)
                        e.Result = DoubleArray.Pow((DoubleArray)e.Parameters[0], (double)e.Parameters[1]);
                    else
                        e.Status = FunctionStatus.WrongArgument;
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.round.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Round((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Round((DoubleArray)e.Parameters[0]);
                }
                else if (e.Parameters.Count == 2)
                {
                    if (e.Parameters[0] is Double && e.Parameters[1] is Double)
                        e.Result = Math.Round((double)e.Parameters[0], (int)((double)e.Parameters[1]));
                    else if (e.Parameters[0] is DoubleArray && e.Parameters[1] is Double)
                        e.Result = DoubleArray.Round((DoubleArray)e.Parameters[0], (int)((double)e.Parameters[1]));
                    else
                        e.Status = FunctionStatus.WrongArgument;
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.sin.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Sin((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Sin((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.sinh.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Sinh((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Sinh((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.sqrt.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Sqrt((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Sqrt((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.tan.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Tan((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Tan((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.tanh.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Tanh((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Tanh((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.truncate.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = Math.Truncate((double)e.Parameters[0]);
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.Truncate((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.mean.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.MeanValue((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            // Conversion Degree <-> Radian
            else if (String.Compare(e.Name, Functions.toDeg.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = (double)e.Parameters[0] / Math.PI * 180.0;
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.RadianToDegree((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, Functions.toRad.ToString(), true) == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] is Double)
                        e.Result = (double)e.Parameters[0] * Math.PI / 180.0;
                    else if (e.Parameters[0] is DoubleArray)
                        e.Result = DoubleArray.DegreeToRadian((DoubleArray)e.Parameters[0]);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            // Unknown function name
            else e.Status = FunctionStatus.UndefinedFunction;
        }
    }
}
