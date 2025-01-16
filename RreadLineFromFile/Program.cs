// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Text.RegularExpressions;

string[] ignoreDir = ["bin", "obj", "V2"];

while (true)
{
    Console.WriteLine("Введите директорию для поиска");



    var dir = Console.ReadLine();

    if (!string.IsNullOrEmpty(dir))
    {
        if (Directory.Exists(dir))
        {
            Console.WriteLine("Введите расширение файлов для анализа в формате: cs,razor,proto");

            var extensionsFile = Console.ReadLine();
            var extensions = extensionsFile?.Split(',') ?? Array.Empty<string>();
            if (extensions.Length > 0)
            {
                Dictionary<string, int> filesInfo = new();
                Dictionary<string, (long, long)> filesDensity = new();
                foreach (var extension in extensions)
                {

                    var files = Directory.EnumerateFiles(dir, $"*.{extension}");

                    if (files?.Any() ?? false)
                    {
                        foreach (var file in files)
                        {
                            var read = await File.ReadAllLinesAsync(file);
                            Console.WriteLine($"{file} строк: {read?.Length}");
                            filesInfo.Add(Path.GetFileName(file), read?.Length ?? 0);
                        }
                    }

                    var dirList = Directory.EnumerateDirectories(dir);

                    if (dirList?.Any() ?? false)
                    {
                        foreach (var currentDir in dirList)
                        {
                            var rootDir = Path.GetRelativePath(dir, currentDir);

                            if (ignoreDir.Contains(rootDir))
                            {
                                Console.Error.WriteLine("Пропуск игнорируемой директории {0}", rootDir);
                            }
                            else
                            {
                                Console.WriteLine("Запуск поиска файлов в {0}", currentDir);

                                files = Directory.EnumerateFiles(currentDir, $"*.{extension}", new EnumerationOptions() { RecurseSubdirectories = true });

                                if (files?.Any() ?? false)
                                {
                                    bool IsFindIgnoreDir = false;
                                    foreach (var file in files)
                                    {
                                        var dirFile = Path.GetDirectoryName(file)?.Split("\\");

                                        if (dirFile?.Length > 0)
                                        {
                                            foreach (var igDir in ignoreDir)
                                            {
                                                if (dirFile.Contains(igDir))
                                                {
                                                    Console.Error.WriteLine("Пропуск игнорируемой директории {0}", igDir);
                                                    IsFindIgnoreDir = true;
                                                    continue;
                                                }
                                            }
                                        }

                                        if (IsFindIgnoreDir)
                                        {
                                            break;
                                        }

                                        var read = await File.ReadAllLinesAsync(file);
                                        Console.WriteLine($"{file} строк: {read?.Length}");
                                        filesInfo.Add(file, read?.Length ?? 0);
                                        var text = await File.ReadAllTextAsync(file);

                                        var noSpaceLength = Regex.Replace(text ?? string.Empty, "\\s\\S", "")?.Length ?? 0;
                                        var textLength = text?.Length ?? 0;
                                        var density = textLength > 0 ? noSpaceLength / (double)textLength : 0;
                                        Console.WriteLine($"{file} символов: {textLength}, без пробелов {noSpaceLength}, плотность {density:0%}");
                                        filesDensity.Add(file, (textLength, noSpaceLength));
                                    }
                                }
                            }

                        }
                    }
                }

                var totalText = filesDensity.Sum(x => x.Value.Item1);
                var totalNoSpaceText = filesDensity.Sum(x => x.Value.Item2);
                var densityTotal = totalText > 0 ? totalNoSpaceText / (double)totalText : 0;
                Console.WriteLine($"Итого файлов {filesInfo.Count} строк: {filesInfo.Sum(x => x.Value)}, символов: {totalText}, без пробелов {totalNoSpaceText}, плотность {densityTotal:0%}");
            }
            else
            {
                Console.Error.WriteLine("Расширения файлов не заданы");
            }
        }
        else
        {
            Console.Error.WriteLine("Директория не существует");
        }
    }
    else
    {
        Console.Error.WriteLine("Директория не введена");
    }
}


