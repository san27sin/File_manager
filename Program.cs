using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace File_manager
{
    class Program
    {
        //C++ функции чтобы сделать консольное окно фиксированным
        const int MF_BYCOMMAND = 0x00000000;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();


        //константные значения
        const int width = 120;
        const int height = 40;

        static void Main(string[] args)
        {
            ConsoleInterface();
            //дерево с файлами и каталогами
            DrawConsole(2, 20);

            //информация
            DrawConsole(22, 34);

            //командная строка
            DrawConsole(36,40);
            
            Console.SetCursorPosition(4, 37);
            Console.Write($"<{Directory.GetCurrentDirectory()}> ");
            string path = Console.ReadLine();
           
            Console.ReadKey(true);
        }

        static public void ConsoleInterface()
        {
            Console.WindowHeight = height;
            Console.WindowWidth = width;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            Console.WindowHeight = 40;
            Console.WindowWidth = 120;

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);
        }


        /// <summary>
        /// Рисуем рамки
        /// </summary>
        /// <param name="startPoint">начальная точка</param>
        /// <param name="finishPoint">конечная точка</param>
        static public void DrawConsole(int startPoint, int finishPoint)
        {
            for (int a = 2; a < width - 2; a++)
            {
                Console.SetCursorPosition(a, startPoint-1);
                Console.Write(a==width-3? "╗" : "═");
            }

            for (int b = startPoint; b < finishPoint; b++)
            {
                Console.SetCursorPosition(width-3, b);
                Console.Write(b==finishPoint-1? "╝":"║");
            }

            for (int a = width -4; a > 1; a--)
            {
                Console.SetCursorPosition(a, finishPoint-1);
                Console.Write(a==2? "╚" : "═");
            }

            for (int b = finishPoint-2; b >= startPoint-1; b--)
            {
                Console.SetCursorPosition(2, b);
                Console.Write(b==startPoint-1? "╔" : "║");
            }
        }
    }
}
