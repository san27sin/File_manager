using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace File_manager
{
    //Проблемы кода:
    /*1)Не затирается текст в консоле после ввода 
     *
     */
   
    static class Program
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

        private static string currentDir = Directory.GetCurrentDirectory();

        //константные значения
        const int width = 120;
        const int height = 40;
        

        static void Main(string[] args)
        {
            Console.Title = "File Manager";

            ConsoleInterface();


            //дерево с файлами и каталогами
            DrawWindow(2, 20);

            //информация
            DrawWindow(22, 34);

            //командная строка
            UpdateConsole();

            Console.ReadKey(true);

        }


        /// <summary>
        /// Обработка процесса данных с консоли
        /// </summary>
        /// <param name="width">Длина строки ввода</param>
        static void ProcessEnterCommand(int width)
        {
            (int left, int top) = GetCursorPosition();
            StringBuilder command = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            char key;

            do
            {
                keyInfo = Console.ReadKey();//ввод только одного символа вместо целой команды=================================
                key = keyInfo.KeyChar;
                if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.UpArrow)
                    command.Append(key);

                (int currentLeft, int currentRight) = GetCursorPosition();

                //стирать последние буквы чтобы не заходило за рамки

                if(currentLeft == width-4)
                {
                    Console.SetCursorPosition(currentLeft - 3, top);
                    Console.Write(" ");
                    Console.SetCursorPosition(currentLeft - 3, top);
                }

                if(keyInfo.Key == ConsoleKey.Backspace)
                {
                    if(command.Length > 0)
                    {
                        command.Remove(command.Length - 1, 1);
                    }

                    if(currentLeft >= left)
                    {
                        Console.SetCursorPosition(currentLeft, top);
                        Console.Write(" ");
                        Console.SetCursorPosition(currentLeft, top);
                    }
                    else
                    {
                        command.Clear();//doesn't work
                        Console.SetCursorPosition(left,top);
                    }
                }

            } while (keyInfo.Key != ConsoleKey.Enter);
            ParseCommandString(command.ToString());
        }

        static void ParseCommandString(string command)
        {
            //Проверить метод

            string[] commandParams = command.ToLower().Split(' ');
            if(commandParams.Length > 0)
            {
                switch(commandParams[0])
                {
                    case "cd":
                        if (commandParams.Length > 1)
                        {
                            if(Directory.Exists(commandParams[1]))
                            {
                                currentDir = commandParams[1];
                            }                            
                        }
                        break;

                    case "ls":
                        if(commandParams.Length > 1 && Directory.Exists(commandParams[1]))
                        {
                            if(commandParams.Length > 3 && commandParams[2]=="-p" && int.TryParse(commandParams[3], out int n))
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]),n);//указываем номер страниццы n
                            }
                            else
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]), 1);//указываем номер страниццы 1
                            }
                        }
                        break;
                }                
            }
            UpdateConsole();
        }


        /// <summary>
        /// Отрисовать дерево каталога
        /// </summary>
        static void DrawTree(DirectoryInfo dir, int page)
        {
            StringBuilder tree = new StringBuilder();
            GetTree(tree,dir,"",true);
            //Надо определиться с областью видимостью и исходя из этого определиться с пэджинком
            DrawWindow(2,20);//каждый раз перезатираем окно
            (int currentLeft, int currentTop) = GetCursorPosition();
            int pageLines = 16; //сколько кол-во строк может вывести наш первый блок
            string[] lines = tree.ToString().Split('\n');
            int pageTotal = (lines.Length + pageLines - 1) / pageLines; //берем с запасом
            if (page > pageTotal)
                page = pageTotal;

            for(int i = (page-1)*pageLines, counter = 0; i < page*pageLines;i++,counter++)
            {
                if(lines.Length - 1 > i)
                { 
                    Console.SetCursorPosition(currentLeft + 1, currentTop + 1 + counter);
                    Console.WriteLine(lines[i]);
                }
            }
            string footer = $"╡{page} of {pageTotal}╞";
            Console.SetCursorPosition(width/2-footer.Length/2,19);
            Console.Write(footer);
        }

        static void GetTree(StringBuilder tree, DirectoryInfo dir, string indent, bool lastDirectory)
        {
            tree.Append(indent);
            if(lastDirectory)
            {
                tree.Append("└─");
                indent += " ";
            }
            else
            {
                tree.Append("├─");
                indent += "|";
            }

            tree.Append($"{dir.Name}\n");


            FileInfo[] subFiles = dir.GetFiles();

            for(int i = 0; i < subFiles.Length; i++)
            {
                if(i == subFiles.Length-1)
                {
                    tree.Append($"{indent}└─{subFiles[i].Name}\n");
                }
                else
                {
                    tree.Append($"{indent}├─{subFiles[i].Name}\n");
                }
            }

            DirectoryInfo[] subDirects = dir.GetDirectories();
            for (int i = 0; i < subDirects.Length; i++)
                GetTree(tree,subDirects[i],indent,i==subDirects.Length - 1);

        }

        static void PrintDir(DirectoryInfo dir, string indent, bool lastDirectory, ref string text)
        {
            Console.Write(indent);
            text += indent;
            Console.Write(lastDirectory ? "└─" : "├─");
            text += lastDirectory ? "└─" : "├─";
            indent += lastDirectory ? " " : "|";
            text += lastDirectory ? " " : "|";
            DirectoryInfo[] subDirects = dir.GetDirectories();


            FileInfo[] subFiles = dir.GetFiles();


            if (subFiles.Length > 0)
            {
                int count = 1;
                Console.Write(dir.Name + " : ");
                foreach (var file in subFiles)
                {
                    if (count == subFiles.Length)
                    {
                        Console.Write(file.Name);
                        text += file.Name;
                    }
                    else
                    {
                        Console.Write(file.Name + " ; ");
                        text += file.Name + " ; ";
                    }
                    count++;
                }
                Console.WriteLine();
                text += "\n";
            }
            else
            {
                Console.WriteLine(dir.Name);
                text += dir.Name + "\n";
            }



            for (int i = 0; i < subDirects.Length; i++)
            {
                PrintDir(subDirects[i], indent, i == subDirects.Length - 1, ref text);
            }
        }


        /// <summary>
        /// Вспомогательный метод получить позицию курсора
        /// </summary>
        /// <returns></returns>
        static (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft, Console.CursorTop);
        }

        /// <summary>
        /// Обновление ввода с консоли
        /// </summary>
        static void UpdateConsole()
        {
            DrawConsole(GetShortPath(currentDir), 4, 37, 36, 40);
            ProcessEnterCommand(width);
        }

        /// <summary>
        /// Отрисовка консоли
        /// </summary>
        /// <param name="di"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        static void DrawConsole(string dir, int left, int top, int startPoint, int finishPoint)
        {
            DrawWindow(startPoint, finishPoint);
            Console.SetCursorPosition(left, top);
            Console.Write($"<{dir}>");
            int firstPosition = Console.CursorLeft;
            int count = Console.CursorLeft;
            
            
            //дописан дополнительный блок для стирания символов после ввода
            do
            {
                Console.Write(" ");
            } while (count++ < width - 4);
            Console.SetCursorPosition(firstPosition, top);            
        }


        static string GetShortPath(string path)
        {
            StringBuilder stringBuilder = new StringBuilder((int)API.MAX_PATH);
            API.GetShortPathName(path, stringBuilder, (int)API.MAX_PATH);
            return stringBuilder.ToString();
        }

        static void PrintDir(DirectoryInfo dir, string indent, bool lastDirectory)
        {
            Console.Write(indent);
         
            Console.Write(lastDirectory ? "└─" : "├─");
            
            indent += lastDirectory ? " " : "|";
            
            DirectoryInfo[] subDirects = dir.GetDirectories();


            FileInfo[] subFiles = dir.GetFiles();


            if (subFiles.Length > 0)
            {
                int count = 1;
                Console.Write(dir.Name + " : ");
                foreach (var file in subFiles)
                {
                    if (count == subFiles.Length)
                    {
                        Console.Write(file.Name);
                        
                    }
                    else
                    {
                        Console.Write(file.Name + " ; ");
                        
                    }
                    count++;
                }
                Console.WriteLine();
                
            }
            else
            {
                Console.WriteLine(dir.Name);
                
            }



            for (int i = 0; i < subDirects.Length; i++)
            {
                PrintDir(subDirects[i], indent, i == subDirects.Length - 1);
            }
        }


        static public void ConsoleInterface()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
                        
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            
            Console.WindowHeight = height;
            Console.WindowWidth = width;                   

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);
        }


        /// <summary>
        /// Рисуем рамки
        /// </summary>
        /// <param name="startPoint">начальная точка</param>
        /// <param name="finishPoint">конечная точка</param>
        static public void DrawWindow(int startPoint, int finishPoint)
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
