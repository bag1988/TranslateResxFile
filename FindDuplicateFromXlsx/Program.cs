// See https://aka.ms/new-console-template for more information


using DocumentFormat.OpenXml.Bibliography;
using SharedLibrary;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

while (true)
{
    Console.WriteLine("Начать создание файлов?(Д/Н).");

    var answer = Console.ReadLine();

    if (answer == "Д")
    {
        List<string> NoFindTranslate = new();

        Console.WriteLine("Введите путь к текущему файлу с переводом (*.xlsx)");

        var urlFile = Console.ReadLine();

        if (File.Exists(urlFile))
        {
            Console.WriteLine("Чтение файла с переводом");

            var translateInfo = ReadXlsx.ReadXlsxFile(urlFile);

            if (translateInfo.Count > 0)
            {
                int keyColumn = 0;
                Console.WriteLine("Выберите столбец для поиска дубликатов");

                var firstElem = translateInfo.First();

                foreach (var item in firstElem)
                {
                    Console.WriteLine("{0} {1}", firstElem.IndexOf(item), item);
                }

                answer = Console.ReadLine();

                if (int.TryParse(answer, out keyColumn))
                {
                    var currentValue = translateInfo.Select(x => x.ElementAt(keyColumn)).Distinct().ToList();

                    Console.WriteLine("Добавлено текущих значений {0}", currentValue.Count);


                    Console.WriteLine("Выберите 1 - используя каталог с файлами *.ru.resx, 2 - используя файл *.json");

                    answer = Console.ReadLine();

                    if (int.TryParse(answer, out var algoritm))
                    {
                        Dictionary<string, List<string>> tab = new();
                        List<string> cacheValue = new List<string>();
                        if (algoritm == 1)
                        {
                            Console.WriteLine("Введите каталог расположения файлов *.ru.resx");

                            var dir = Console.ReadLine();

                            if (Directory.Exists(dir))
                            {

                                Console.WriteLine("Поиск файлов *.ru.resx....");

                                var listFile = Directory.EnumerateFiles(dir, "*.ru.resx", SearchOption.AllDirectories);

                                Console.WriteLine("Найдено файлов {0}", listFile.Count());

                                foreach (var file in listFile)
                                {
                                    if (!file.Equals(listFile.First()))
                                    {
                                        Console.WriteLine("----------------------");
                                    }
                                    Console.WriteLine("Чтение файла {0}", file);
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

                                    cacheValue.AddRange(tempValues.Where(x => !string.IsNullOrEmpty(x)).Except(cacheValue));

                                    Console.WriteLine("В файле {0} найдено {1} строк", Path.GetFileNameWithoutExtension(file), tempValues.Count);
                                }

                                Console.WriteLine("----------------------");
                                Console.WriteLine("Всего найдено {0} строк", cacheValue.Count);
                            }
                            else
                            {
                                Console.WriteLine("Директория не найдена");
                            }
                        }
                        else if (algoritm == 2)
                        {
                            Console.WriteLine("Введите путь к файлу (*.json)");

                            var jsonFile = Console.ReadLine();

                            if (File.Exists(jsonFile))
                            {
                                var json = await File.ReadAllTextAsync(jsonFile);

                                var oldValueFromFile = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();

                                if (oldValueFromFile?.Count > 0)
                                {
                                    cacheValue = oldValueFromFile.Select(x => x.Value.GetProperty("message").ToString()).ToList();

                                    Regex regex = new Regex(@"\$(\d)", RegexOptions.Multiline);
                                    int countReplace = 0;

                                    for (int i = 0; i < cacheValue.Count; i++)
                                    {
                                        if (regex.IsMatch(cacheValue[i]))
                                        {
                                            countReplace++;
                                            cacheValue[i] = regex.Replace(cacheValue[i], x =>
                                            {
                                                int.TryParse(x.Groups[1].Value, out var r);
                                                return "{" + $"{r - 1}" + "}";
                                            });

                                        }
                                    }
                                    Console.WriteLine("Перезаписаны параметры {0}", countReplace);
                                }
                                else
                                {
                                    Console.WriteLine("В файле нет подходящих данных!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Файл не найден!");
                            }
                        }

                        cacheValue = cacheValue.Except(currentValue).ToList();

                        Console.WriteLine("После удаления дубликатов найдено {0} строк", cacheValue.Count);

                        if (cacheValue.Count > 0)
                        {
                            foreach (var row in cacheValue)
                            {
                                string[] addRow = new string[keyColumn + 1];
                                addRow[keyColumn] = row;
                                tab.Add((tab.Count + 1).ToString(), addRow.ToList());
                            }

                            if (tab.Count > 0)
                            {
                                var b = ReadXlsx.AddRowToFile(urlFile, tab, (uint)translateInfo.Count);

                                if (b)
                                {
                                    Console.WriteLine("Файл успешно изменен, добавлено {0} строк", tab.Count);
                                }
                                else
                                {
                                    Console.WriteLine("Ошибка добавления строк");
                                }
                            }
                        }
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
