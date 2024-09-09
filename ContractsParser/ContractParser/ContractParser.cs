using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContractsParser.ContractParser
{
    public class ContractParser
    {
        /* public List<string> ExtractFunctions(string contractText)
         {
             var functions = new List<string>();

             // Регулярное выражение для поиска функций
             string pattern = @"function\s+(\w+)\s*\(([^)]*)\)\s*(public|private|internal|external)?\s*(\w+)?\s*{(\s*(?:[^{}]*|{(?:[^{}]*|{[^{}]*})*})*)}"; ;

             // Используем Regex для поиска совпадений
             MatchCollection matches = Regex.Matches(contractText, pattern);

             Console.WriteLine($"found {matches.Count} functions");

             foreach (Match match in matches)
             {

                 functions.Add(match.Value);
             }

             return functions;
         }*/

        public List<string> ExtractFunctions(string contractText)
        {
            var functions = new List<string>();
            ReadOnlySpan<char> text = contractText.AsSpan();

            bool insideInterface = false;

            for (int i = 0; i < text.Length; i++)
            {
                // Поиск ключевого слова "interface"
                if (IsKeywordAtPosition(text, i, "interface"))
                {
                    insideInterface = true;
                }

                // Поиск ключевого слова "function"
                if (IsKeywordAtPosition(text, i, "function"))
                {
                    if (insideInterface)
                    {
                        // Игнорируем функции в интерфейсах, они без тела (заканчиваются на ;)
                        int semicolonIndex = text.Slice(i).IndexOf(';');
                        if (semicolonIndex != -1)
                        {
                            i += semicolonIndex; // Продвигаем индекс после точки с запятой
                        }
                    }
                    else
                    {
                        // Ищем начало и конец тела функции (обычная функция)
                        int startIndex = FindFunctionStart(text.Slice(i));
                        if (startIndex != -1)
                        {
                            int endIndex = FindMatchingBracket(text.Slice(i), startIndex);

                            if (endIndex != -1)
                            {
                                // Извлекаем тело функции и проверяем его содержимое
                                var functionBody = text.Slice(i + startIndex + 1, endIndex - startIndex - 1).ToString().Trim();

                                // Если тело функции содержит только return (или пустые строки/пробелы), игнорируем такую функцию
                                if (!IsTrivialReturnFunction(functionBody))
                                {
                                    functions.Add(text.Slice(i, endIndex + 1).ToString());
                                }

                                i += endIndex; // Продвигаем индекс после закрывающей скобки
                            }
                        }
                    }
                }

                // Если нашли конец интерфейса
                if (insideInterface && text[i] == '}')
                {
                    insideInterface = false;
                }
            }

            return functions;
        }

        // Метод для поиска ключевого слова в определенной позиции
        private bool IsKeywordAtPosition(ReadOnlySpan<char> text, int position, string keyword)
        {
            if (position + keyword.Length > text.Length)
            {
                return false; // Если недостаточно символов для сравнения, возвращаем false
            }

            return text.Slice(position, keyword.Length).ToString().Equals(keyword, StringComparison.Ordinal);
        }

        // Метод для поиска начала тела функции (открывающей скобки)
        private int FindFunctionStart(ReadOnlySpan<char> text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    return i;
                }
                else if (text[i] == ';')
                {
                    return -1; // Если найдена точка с запятой, это объявление функции без тела
                }
            }
            return -1;
        }

        // Метод для поиска закрывающей скобки с учетом вложенных скобок и пропуском строк и комментариев
        private int FindMatchingBracket(ReadOnlySpan<char> text, int startIndex)
        {
            int depth = 0;
            bool inString = false;
            bool inChar = false;
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;

            for (int i = startIndex; i < text.Length; i++)
            {
                char c = text[i];
                char nextChar = i + 1 < text.Length ? text[i + 1] : '\0';

                if (inSingleLineComment)
                {
                    if (c == '\n')
                    {
                        inSingleLineComment = false;
                    }
                }
                else if (inMultiLineComment)
                {
                    if (c == '*' && nextChar == '/')
                    {
                        inMultiLineComment = false;
                        i++; // Пропустить "/"
                    }
                }
                else if (inString)
                {
                    if (c == '"' && text[i - 1] != '\\')
                    {
                        inString = false;
                    }
                }
                else if (inChar)
                {
                    if (c == '\'' && text[i - 1] != '\\')
                    {
                        inChar = false;
                    }
                }
                else
                {
                    if (c == '/' && nextChar == '/')
                    {
                        inSingleLineComment = true;
                        i++; // Пропустить "/"
                    }
                    else if (c == '/' && nextChar == '*')
                    {
                        inMultiLineComment = true;
                        i++; // Пропустить "*"
                    }
                    else if (c == '"')
                    {
                        inString = true;
                    }
                    else if (c == '\'')
                    {
                        inChar = true;
                    }
                    else if (c == '{')
                    {
                        depth++;
                    }
                    else if (c == '}')
                    {
                        depth--;

                        if (depth == 0)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1; // Если не найдено соответствие
        }

        // Метод для проверки, является ли тело функции тривиальным (содержит только return или пустое)
        private bool IsTrivialReturnFunction(string functionBody)
        {
            // Удаление пробелов и символов новой строки для упрощения анализа
            var trimmedBody = functionBody.Trim();

            // Удаление однострочных и многострочных комментариев
            trimmedBody = Regex.Replace(trimmedBody, @"//.*?$|/\*.*?\*/", "", RegexOptions.Singleline);

            // Проверка начинается ли тело функции с 'return' и заканчивается ли ';'
            // Также проверяем, что после return нет других операторов, кроме возможного выражения
            if (trimmedBody.StartsWith("return") && trimmedBody.EndsWith(";"))
            {
                // Удаляем 'return' и ';' и проверяем, что внутри нет ничего сложного
                var expression = trimmedBody.Substring(6, trimmedBody.Length - 7).Trim();

                // Если после return только выражение, функция считается тривиальной
                // Игнорируем, если это просто возвращение переменной, вызов метода и т.д.
                return !expression.Contains("{") && !expression.Contains("}") && !expression.Contains(";");
            }

            return false; // Функция не тривиальная, если содержит другие операторы
        }


    }
}
