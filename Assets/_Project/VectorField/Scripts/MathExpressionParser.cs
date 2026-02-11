using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorFieldTools
{
    /// <summary>
    /// Parser y evaluador de expresiones matemáticas
    /// Soporta: +, -, *, /, ^, sin, cos, tan, sqrt, abs, exp, log, pi, e
    /// Variables: x, y
    /// </summary>
    public class MathExpressionParser
    {
        private string expression;
        private int position;

        public float Evaluate(string expr, float x, float y)
        {
            try
            {
                expression = expr.ToLower().Replace(" ", "");
                expression = expression.Replace("x", x.ToString(System.Globalization.CultureInfo.InvariantCulture));
                expression = expression.Replace("y", y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                expression = expression.Replace("pi", Math.PI.ToString(System.Globalization.CultureInfo.InvariantCulture));
                expression = expression.Replace("e", Math.E.ToString(System.Globalization.CultureInfo.InvariantCulture));
                
                position = 0;
                return ParseExpression();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error evaluating expression '{expr}': {e.Message}");
                return 0f;
            }
        }

        public bool IsValidExpression(string expr)
        {
            try
            {
                Evaluate(expr, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private float ParseExpression()
        {
            float result = ParseTerm();

            while (position < expression.Length)
            {
                char op = expression[position];
                if (op == '+' || op == '-')
                {
                    position++;
                    float term = ParseTerm();
                    result = op == '+' ? result + term : result - term;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private float ParseTerm()
        {
            float result = ParseFactor();

            while (position < expression.Length)
            {
                char op = expression[position];
                if (op == '*' || op == '/')
                {
                    position++;
                    float factor = ParseFactor();
                    result = op == '*' ? result * factor : result / factor;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private float ParseFactor()
        {
            float result = ParsePower();
            return result;
        }

        private float ParsePower()
        {
            float result = ParseUnary();

            if (position < expression.Length && expression[position] == '^')
            {
                position++;
                float exponent = ParsePower();
                result = (float)Math.Pow(result, exponent);
            }

            return result;
        }

        private float ParseUnary()
        {
            if (position < expression.Length)
            {
                char c = expression[position];
                if (c == '-')
                {
                    position++;
                    return -ParseUnary();
                }
                else if (c == '+')
                {
                    position++;
                    return ParseUnary();
                }
            }

            return ParsePrimary();
        }

        private float ParsePrimary()
        {
            if (position >= expression.Length)
                throw new Exception("Unexpected end of expression");

            // Parse number
            if (char.IsDigit(expression[position]) || expression[position] == '.')
            {
                return ParseNumber();
            }

            // Parse parentheses
            if (expression[position] == '(')
            {
                position++;
                float result = ParseExpression();
                if (position >= expression.Length || expression[position] != ')')
                    throw new Exception("Missing closing parenthesis");
                position++;
                return result;
            }

            // Parse functions
            if (char.IsLetter(expression[position]))
            {
                return ParseFunction();
            }

            throw new Exception($"Unexpected character: {expression[position]}");
        }

        private float ParseNumber()
        {
            int start = position;
            while (position < expression.Length && 
                   (char.IsDigit(expression[position]) || expression[position] == '.'))
            {
                position++;
            }

            string numberStr = expression.Substring(start, position - start);
            return float.Parse(numberStr, System.Globalization.CultureInfo.InvariantCulture);
        }

        private float ParseFunction()
        {
            int start = position;
            while (position < expression.Length && char.IsLetter(expression[position]))
            {
                position++;
            }

            string functionName = expression.Substring(start, position - start);

            if (position >= expression.Length || expression[position] != '(')
                throw new Exception($"Expected '(' after function '{functionName}'");

            position++;
            float arg = ParseExpression();

            if (position >= expression.Length || expression[position] != ')')
                throw new Exception($"Missing closing parenthesis for function '{functionName}'");

            position++;

            switch (functionName)
            {
                case "sin": return (float)Math.Sin(arg);
                case "cos": return (float)Math.Cos(arg);
                case "tan": return (float)Math.Tan(arg);
                case "sqrt": return (float)Math.Sqrt(arg);
                case "abs": return Math.Abs(arg);
                case "exp": return (float)Math.Exp(arg);
                case "log": return (float)Math.Log(arg);
                case "asin": return (float)Math.Asin(arg);
                case "acos": return (float)Math.Acos(arg);
                case "atan": return (float)Math.Atan(arg);
                default:
                    throw new Exception($"Unknown function: {functionName}");
            }
        }
    }
}
