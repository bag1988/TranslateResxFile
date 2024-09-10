using System.Dynamic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Xml.Linq;

while (true)
{
    Console.WriteLine("Начать создание файлов?(Д/Н).");

    var answer = Console.ReadLine();

    if (answer == "Д")
    {
        List<string> NoFindTranslate = new();

        Console.WriteLine("Введите путь к файлу с переводом (*.resx)");

        answer = Console.ReadLine();

        if (File.Exists(answer))
        {
            Console.WriteLine("Чтение файла");

            XElement currentRoot = XElement.Load(answer);

            Dictionary<string, object> tempValue = new();

            IEnumerable<XElement> nodeList = from item in currentRoot.Descendants("data") select item;
            Regex regex = new Regex("{(\\d)}", RegexOptions.Multiline);
            foreach (var item in nodeList)
            {
                var nameNode = item.Attribute(XName.Get("name"))?.Value;
                var nodeValue = item.Element("value")?.Value;
                if (nameNode != null && nodeValue != null)
                {
                    var sTrim = nodeValue.Trim();
                    tempValue.Add(nameNode, new
                    {
                        message = regex.Replace(sTrim, x =>
                    {
                        int.TryParse(x.Groups[1].Value, out var r);
                        return $"${r + 1}";
                    })
                    });
                }
            }

            if (tempValue.Count > 0)
            {
                Console.WriteLine("Введите путь к текущему файлу для сохранения файла (*.json)");

                var saveToFile = Console.ReadLine();

                if (File.Exists(saveToFile))
                {
                    Console.WriteLine("Дублирующие значение перезаписывать новыми? (Д/Н)");


                    answer = Console.ReadLine();

                    bool isReplace = answer == "Д";

                    var oldFileText = await File.ReadAllTextAsync(saveToFile);

                    var oldValueFromFile = JsonSerializer.Deserialize<Dictionary<string, object>>(oldFileText) ?? new();

                    if (isReplace)
                    {
                        oldValueFromFile = oldValueFromFile.ExceptBy(tempValue.Keys, x => x.Key).ToDictionary();
                    }

                    foreach (var item in tempValue)
                    {
                        if (!oldValueFromFile.ContainsKey(item.Key))
                        {
                            oldValueFromFile.Add(item.Key, item.Value);
                        }
                    }

                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,//.Create(UnicodeRanges.All, UnicodeRanges.All),
                        WriteIndented = true
                    };

                    await File.WriteAllTextAsync(saveToFile, JsonSerializer.Serialize(oldValueFromFile, options), encoding: Encoding.UTF8);

                    Console.WriteLine("Файл записан!");

                }
                else
                {
                    Console.WriteLine("Файла не найден!");
                }
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