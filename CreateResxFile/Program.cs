// See https://aka.ms/new-console-template for more information
using CreateResxFile;
using System.Xml.Linq;

while (true)
{
    Console.WriteLine("Начать создание файлов?(Д/Н).");

    var answer = Console.ReadLine();

    if (answer == "Д")
    {
        List<string> NoFindTranslate = new();

        Console.WriteLine("Введите путь к файлу с переводом (*.xlsx)");

        answer = Console.ReadLine();

        if (File.Exists(answer))
        {
            Console.WriteLine("Чтение файла с переводом");


            var translateInfo = ReadXlsx.ReadXlsxFile(answer);

            if (translateInfo.Count > 0)
            {
                int keyColumn = 0;
                int valueColumn = 0;
                string? lang = "";
                Console.WriteLine("Выберите столбец с ключом для поиска");

                var firstElem = translateInfo.First();

                foreach (var item in firstElem)
                {
                    Console.WriteLine("{0} {1}", firstElem.IndexOf(item), item);
                }

                answer = Console.ReadLine();

                if (int.TryParse(answer, out keyColumn))
                {
                    Console.WriteLine("Выберите столбец со значением");

                    foreach (var item in firstElem)
                    {
                        Console.WriteLine("{0} {1}", firstElem.IndexOf(item), item);
                    }

                    answer = Console.ReadLine();
                    if (int.TryParse(answer, out valueColumn))
                    {
                        var dicValue = translateInfo.DistinctBy(x => x.ElementAt(keyColumn)).ToList().Where(x => x.Count > valueColumn && x.Count > keyColumn).ToDictionary(x => x.ElementAt(keyColumn), x => x.ElementAt(valueColumn));

                        Console.WriteLine("Выберите параметр локализации");

                        lang = Console.ReadLine();

                        if (!string.IsNullOrEmpty(lang))
                        {
                            Console.WriteLine("Введите каталог расположения файлов *.ru.resx");

                            var dir = Console.ReadLine();

                            if (Directory.Exists(dir))
                            {
                                Console.WriteLine("Поиск файлов *.resx....");

                                var listFile = Directory.EnumerateFiles(dir, "*.ru.resx");

                                Console.WriteLine("Найдено файлов {0}", listFile.Count());


                                Console.WriteLine("Введите каталог сохранения новых файлов");

                                answer = Console.ReadLine();

                                if(!string.IsNullOrEmpty(answer))
                                {
                                    if (!Directory.Exists(answer))
                                    {
                                        Console.WriteLine("Создаем каталог сохранения новых файлов");
                                        Directory.CreateDirectory(answer);
                                    }

                                    foreach (var file in listFile)
                                    {
                                        if (!file.Equals(listFile.First()))
                                        {
                                            Console.WriteLine("----------------------");
                                        }

                                        var newFileName = Path.Combine(answer, Path.ChangeExtension(Path.GetFileNameWithoutExtension(file), $".{lang}.resx"));

                                        Console.WriteLine("Создаем новый файл на основе файла {0} с именем {1}", Path.GetFileNameWithoutExtension(file), Path.GetFileNameWithoutExtension(newFileName));

                                        File.Copy(file, newFileName, true);

                                        Console.WriteLine("Чтение файла {0}", Path.GetFileNameWithoutExtension(newFileName));
                                        List<string> tempValues = new List<string>();

                                        XElement currentRoot = XElement.Load(newFileName);

                                        IEnumerable<XElement> nodeList = from item in currentRoot.Descendants("data") select item;
                                        int countReplace = 0;
                                        bool isChangeFile = false;
                                        foreach (var item in nodeList)
                                        {
                                            var node = item.Element("value");
                                            if (node != null)
                                            {
                                                var sTrim = node.Value.Trim();

                                                if (dicValue.ContainsKey(sTrim))
                                                {
                                                    var newValue = dicValue[sTrim];
                                                    if (!string.IsNullOrEmpty(newValue) && node.Value != newValue)
                                                    {
                                                        node.SetValue(newValue);
                                                        isChangeFile = true;
                                                        countReplace++;
                                                    }
                                                    else
                                                    {
                                                        NoFindTranslate.Add(sTrim);
                                                        Console.WriteLine("Значение для ключа {0} пустое или имеет тоже значение", sTrim);
                                                    }
                                                }
                                                else
                                                {
                                                    NoFindTranslate.Add(sTrim);
                                                    Console.WriteLine("Ключ {0} в словаре не найден", sTrim);
                                                }
                                            }
                                        }
                                        if (isChangeFile)
                                        {
                                            currentRoot.Save(newFileName);
                                        }
                                        Console.WriteLine("В файле {0} заменено {1} строк, всего строк в файле {2}", Path.GetFileNameWithoutExtension(newFileName), countReplace, nodeList.Count());
                                    }

                                    var writeKey = NoFindTranslate.Distinct().ToList();

                                    if (writeKey.Count > 0)
                                    {
                                        Console.WriteLine("Сохраняем в файл (tempFile.txt) не найденые ключи, кол-во {0}", writeKey.Count);


                                        using StreamWriter writer = new StreamWriter("tempFile.txt", false);


                                        foreach (var value in writeKey)
                                        {
                                            writer.WriteLine(value);
                                        }

                                        Console.WriteLine("Файл записан!");

                                        writer.Flush();
                                        writer.Close();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Введено пустое значение");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Директория не найдена");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Введенное значение не верное!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Введенное значение не верное!");
                    }
                }
                else
                {
                    Console.WriteLine("Введенное значение не верное!");
                }
            }
            else
            {
                Console.WriteLine("Файла с переводом пустой или ошибка чтения файла");
            }
        }
        else
        {
            Console.WriteLine("Файла с переводом не найден");
        }
    }
    else if (answer == "Н")
    {
        return;
    }
}



