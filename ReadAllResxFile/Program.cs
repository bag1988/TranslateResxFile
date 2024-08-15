// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;

while (true)
{
    Console.WriteLine("Начать сбор данных?(Д/Н).");

    var answer = Console.ReadLine();

    if (answer == "Д")
    {
        List<string> CacheValue = new List<string>();
        List<string> CurrentValue = new List<string>();

        Console.WriteLine("Введите путь к файлу с текущими значениям для перевода (*.txt)");

        answer = Console.ReadLine();

        if (!string.IsNullOrEmpty(answer))
        {
            if (File.Exists(answer))
            {
                Console.WriteLine("Чтение текущих значений");

                using var currentReader = new StreamReader(answer);

                while (!currentReader.EndOfStream)
                {
                    var value = currentReader.ReadLine();
                    if (!string.IsNullOrEmpty(value) && !CurrentValue.Contains(value))
                    {
                        CurrentValue.Add(value);
                    }
                }

                Console.WriteLine("Добавлено текущих значений {0}", CurrentValue.Count);


                Console.WriteLine("Введите каталог расположения файлов *.ru.resx");

                var dir = Console.ReadLine();

                if (Directory.Exists(dir))
                {
                    Console.WriteLine("Поиск файлов *.ru.resx....");

                    var listFile = Directory.EnumerateFiles(dir, "*.ru.resx");

                    Console.WriteLine("Найдено файлов {0}", listFile.Count());

                    foreach (var file in listFile)
                    {
                        if (!file.Equals(listFile.First()))
                        {
                            Console.WriteLine("----------------------");
                        }
                        Console.WriteLine("Чтение файла {0}", Path.GetFileNameWithoutExtension(file));
                        List<string> tempValues = new List<string>();

                        XElement currentRoot = XElement.Load(file);

                        IEnumerable<XElement> nodeList = from item in currentRoot.Descendants("data") select item;
                        bool isChangeFile = false;
                        foreach (var item in nodeList)
                        {
                            var node = item.Element("value");
                            if (node != null)
                            {
                                var sTrim = node.Value.Trim();
                                tempValues.Add(sTrim);
                                if (node.Value != sTrim)
                                {
                                    node.SetValue(sTrim);
                                    isChangeFile = true;
                                }
                            }
                        }
                        if (isChangeFile)
                        {
                            currentRoot.Save(file);
                        }

                        CacheValue.AddRange(tempValues.Where(x => !string.IsNullOrEmpty(x)).Except(CacheValue));

                        Console.WriteLine("В файле {0} найдено {1} строк", Path.GetFileNameWithoutExtension(file), tempValues.Count);
                    }

                    Console.WriteLine("----------------------");
                    Console.WriteLine("Всего найдено {0} строк", CacheValue.Count);

                    CacheValue = CacheValue.Except(CurrentValue).ToList();

                    Console.WriteLine("После удаления дубликатов найдено {0} строк", CacheValue.Count);

                    if (CacheValue.Count > 0)
                    {

                        Console.WriteLine("Введите путь для сохранения файла (*.txt)");

                        answer = Console.ReadLine();

                        dir = Path.GetDirectoryName(answer);

                        if (!string.IsNullOrEmpty(answer) && !string.IsNullOrEmpty(dir))
                        {
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            Console.WriteLine("Сохраняем в файл");

                            using StreamWriter writer = new StreamWriter(answer, false);

                            foreach (var value in CacheValue)
                            {
                                writer.WriteLine(value);
                            }

                            Console.WriteLine("Файл записан!");

                            writer.Flush();
                            writer.Close();
                        }
                        else
                        {
                            Console.WriteLine("Введено пустое значение");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Директория не найдена");
                }
            }
            else
            {
                Console.WriteLine("Файл не существует");
            }
        }
        else
        {
            Console.WriteLine("Введено пустое значение");
        }


    }
    else if (answer == "Н")
    {
        return;
    }
}



