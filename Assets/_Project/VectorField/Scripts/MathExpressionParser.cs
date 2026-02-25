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

        /// <summary>
        /// Evalúa la expresión y lanza excepción si hay error. No llama Debug.LogError.
        /// </summary>
        public float Evaluate(string expr, float x, float y)
        {
            expression = expr.ToLower().Replace(" ", "");

            // 1) Proteger nombres de funciones con placeholders ANTES de sustituir variables
            //    (evita que "exp" se corrompa al sustituir "e" o "x", etc.)
            expression = expression.Replace("asin", "\x01");
            expression = expression.Replace("acos", "\x02");
            expression = expression.Replace("atan", "\x03");
            expression = expression.Replace("sqrt", "\x04");
            expression = expression.Replace("exp",  "\x05");
            expression = expression.Replace("abs",  "\x06");
            expression = expression.Replace("log",  "\x07");
            expression = expression.Replace("sin",  "\x08");
            expression = expression.Replace("cos",  "\x09");
            expression = expression.Replace("tan",  "\x0A");

            // 2) Sustituir constantes y variables
            expression = expression.Replace("pi", Math.PI.ToString(System.Globalization.CultureInfo.InvariantCulture));
            expression = expression.Replace("e",  Math.E.ToString(System.Globalization.CultureInfo.InvariantCulture));
            expression = expression.Replace("x",  x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            expression = expression.Replace("y",  y.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // 3) Restaurar nombres de funciones
            expression = expression.Replace("\x01", "asin");
            expression = expression.Replace("\x02", "acos");
            expression = expression.Replace("\x03", "atan");
            expression = expression.Replace("\x04", "sqrt");
            expression = expression.Replace("\x05", "exp");
            expression = expression.Replace("\x06", "abs");
            expression = expression.Replace("\x07", "log");
            expression = expression.Replace("\x08", "sin");
            expression = expression.Replace("\x09", "cos");
            expression = expression.Replace("\x0A", "tan");

            position = 0;
            return ParseExpression();
        }

        /// <summary>
        /// Evalúa de forma segura (devuelve 0 si hay error, sin loguear).
        /// Usar en la generación de flechas para no spam consola.
        /// </summary>
        public float EvaluateSafe(string expr, float x, float y)
        {
            try { return Evaluate(expr, x, y); }
            catch { return 0f; }
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
