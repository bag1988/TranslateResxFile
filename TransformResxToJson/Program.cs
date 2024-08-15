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
                Console.WriteLine("Введите путь для сохранения файла (*.txt)");

                answer = Console.ReadLine();

                var dir = Path.GetDirectoryName(answer);

                if (!string.IsNullOrEmpty(answer) && !string.IsNullOrEmpty(dir))
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    Console.WriteLine("Сохраняем в файл");

                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,//.Create(UnicodeRanges.All, UnicodeRanges.All),
                        WriteIndented = true
                    };

                    await File.WriteAllTextAsync(answer, JsonSerializer.Serialize(tempValue, options), encoding: Encoding.UTF8);

                    Console.WriteLine("Файл записан!");
                }
                else
                {
                    Console.WriteLine("Введено пустое значение");
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