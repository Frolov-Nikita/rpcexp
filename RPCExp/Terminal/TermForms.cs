using RPCExp.Common;
using RPCExp.Modbus;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Terminal
{
    // TODO Удалить и использовать elw00d
    public static class TermTable
    {
        public class Column
        {
            public string Header { get; set; }
            public int Width { get; set; }
        }

        public static void Draw(string[,] values, string[] headers = null)
        {
            var h = Console.WindowHeight - Console.CursorTop;
            var w = Console.WindowWidth;

            //определим длину отображаемых колонок
            int rowsCount = h - (headers == null ? 0 : 1);
            rowsCount = rowsCount > values.GetLength(0) ? values.GetLength(0): rowsCount;
            var colsCount = values.GetLength(1);

            var headersCount = headers?.Length??0;
            headersCount = colsCount < headersCount ? colsCount : headersCount;
            int[] colsLength = new int[values.GetLength(1)];

            for (var c = 0; c < headersCount; c++)
                if (colsLength[c] < headers[c].Length)
                    colsLength[c] = headers[c].Length;

            for (var r = 0; r < rowsCount; r++)
                for (var c = 0; c < colsCount; c++)
                    if (colsLength[c] < values[r, c].Length)
                        colsLength[c] = values[r, c].Length;

            int totLength = colsCount - 1;
            for (var c = 0; c < colsCount; c++)
                totLength += colsLength[c];

            // подгоняем длину колонок
            var overlength = totLength - w;
            for (var c = colsCount - 1; (c > 0) && (overlength > 0); c--)
            {
                if(colsLength[c] > overlength)
                {
                    colsLength[c] = colsLength[c] - overlength;
                    overlength = 0;
                }
                else
                {
                    overlength = overlength - colsLength[c];
                    colsLength[c] = 0;
                    colsCount--;
                }
            }

            // рассчет ширины сепараторов
            string[] separators = new string[colsCount];
            Array.Fill(separators, " ");

            if(overlength < 0)
            {
                overlength = -overlength;
                int i = 0;
                while ((overlength-- > 0) && (separators[i % colsCount].Length < 3))
                    separators[i++ % colsCount] += " ";
            }
                

            // Выводим заголовок
            if (headers != null)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.Black;
                for (var i = 0; i < headersCount; i++)
                    Console.Write(Fit(headers[i], colsLength[i], Align.Center) + (i == headersCount-1?"": separators[i]));
                Console.WriteLine();
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            // Выводим данные
            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < colsCount; c++)
                    Console.Write(Fit(values[r,c], colsLength[c], Align.Center) + (c == colsCount - 1 ? "" : separators[c]));
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        private enum Align
        {
            Left,
            Center,
            Right
        }

        private static string Fit(string val, int length, Align align)
        {
            var d = val.Length - length;

            if (d == 0) return val;
            
            // надо подрезать
            if (d > 0) return val.Substring(0, length);
            
            // надо дополнить
            d = -d;

            if (align == Align.Left)
                return val.PadRight(val.Length + d);

            if (align == Align.Right)
                return val.PadLeft(val.Length + d);

            //if (align == Align.Center)
            //{ 
                var l = d / 2;
                var r = d - l;
                return val.PadLeft(val.Length + r).PadRight(val.Length + r + l );
            //}                
        }
    }

    public static class TermForms
    {

        public static void DisplayModbusDevice(ModbusDevice device)
        {
            // ─│┌┐└┘├┤┬┴┼
            var origColor = Console.ForegroundColor;

            var w = Console.WindowWidth;
            var h = Console.WindowHeight;

            //Console.Clear();
            var cursorL = Console.CursorLeft;
            var cursorT = Console.CursorTop;

            //Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine($"Connection: {device.Connection.ConnectionCfg}, state: {device.State}");

            Console.Write("─");
            for (var i = 0; i < w - 2; i++)
                Console.Write("─");
            Console.WriteLine("─");

            var headers = new string[] { "Name", "Region", "Addr", "Value", "Quality", "Success", "TimestampLast", "Period", "Alive"};

            var rowsCountAviable = h - 2;
            var rowsCount = 0;
            List<MTag> tags = new List<MTag>();

            foreach (var t in device.Tags)
            {
                tags.Add((MTag)t.Value);
                if (++rowsCount > rowsCountAviable)
                    break;
            }
            
            string[,] vals = new string[rowsCount, headers.Length];
            for(var r = 0; r < rowsCount; r++)
            {
                var t = tags[r];
                vals[r, 0] = t.Name;
                vals[r, 1] = t.Region.ToString();
                vals[r, 2] = t.Begin.ToString();
                vals[r, 3] = t.GetValue()?.ToString()??"null";
                vals[r, 4] = t.Quality.ToString();
                vals[r, 5] = DateTime.FromBinary( t.LastGood).ToString();
                vals[r, 6] = DateTime.FromBinary(t.Last).ToString();
                vals[r, 7] = TimeSpan.FromTicks(t.Period).ToString();
                vals[r, 8] = t.IsActive.ToString();
            }
                
            //{
            //    Console.WriteLine($"{t.Name}\t{t.Region}\t{t.Begin}\t{t.GetValue()}\t{t.Quality}" +
            //        $"\t{DateTime.FromFileTime(t.TimestampSuccess)}\t{DateTime.FromFileTime(t.TimestampLast)}");
            //}

            TermTable.Draw(vals, headers);
            Console.SetCursorPosition(cursorL, cursorT);
        }
    }
}
