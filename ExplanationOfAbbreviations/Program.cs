using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ExplanationOfAbbreviations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Excel Processor (Interactive) ===");

            Console.Write("Введите путь к файлу .xlsx: ");
            var filePath = Console.ReadLine()?.Trim('"');

            if (!File.Exists(filePath)) { Console.WriteLine("Файл не найден."); return; }

            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, true))
                {
                    var wbPart = doc.WorkbookPart;
                    var sheets = wbPart!.Workbook!.Descendants<Sheet>().ToList();

                    // --- ВЫБОР ЛИСТОВ ---
                    Console.WriteLine("\nДоступные листы:");
                    for (int i = 0; i < sheets.Count; i++) Console.WriteLine($"{i + 1}. {sheets[i].Name}");

                    var sourceSheet = sheets[ChooseIndex(sheets.Count, "Выберите лист с данными")];
                    var lookupSheet = sheets[ChooseIndex(sheets.Count, "Выберите лист со справочником")];

                    // --- ВЫБОР СТОЛБЦОВ ---
                    var sourceCols = GetHeaders(wbPart, sourceSheet);
                    Console.WriteLine("\nСтолбцы на листе данных:");
                    foreach (var col in sourceCols) Console.WriteLine($"{col.Key}. {col.Value}");

                    string sourceCol = ChooseColumn(sourceCols, "Выберите столбец с предложениями");
                    string targetCol = ChooseColumn(sourceCols, "Выберите столбец для записи результата");

                    var lookupCols = GetHeaders(wbPart, lookupSheet);
                    Console.WriteLine("\nСтолбцы в справочнике:");
                    foreach (var col in lookupCols) Console.WriteLine($"{col.Key}. {col.Value}");

                    string keyCol = ChooseColumn(lookupCols, "Выберите столбец с сокращениями");
                    string valCol = ChooseColumn(lookupCols, "Выберите столбец с описаниями");

                    // --- ОБРАБОТКА ---
                    Console.WriteLine("\nЗагрузка справочника...");
                    var dict = BuildDictionary(wbPart, lookupSheet, keyCol, valCol);

                    Console.WriteLine("Обработка строк...");
                    ProcessSheet(wbPart, sourceSheet, sourceCol, targetCol, dict);

                    wbPart.Workbook.Save();
                    Console.WriteLine("\nГотово! Файл сохранен.");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
            Console.ReadKey();
        }

        // Вспомогательные методы выбора
        static int ChooseIndex(int max, string prompt)
        {
            int choice;
            do { Console.Write($"{prompt} (1-{max}): "); }
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > max);
            return choice - 1;
        }

        static string ChooseColumn(Dictionary<string, string> cols, string prompt)
        {
            string choice;
            do { Console.Write($"{prompt} (буква, например {cols.Keys.First()}): "); choice = Console.ReadLine()!.ToUpper(); }
            while (!cols.ContainsKey(choice));
            return choice;
        }

        // Получение имен столбцов (из первой строки)
        static Dictionary<string, string> GetHeaders(WorkbookPart wbPart, Sheet sheet)
        {
            var headers = new Dictionary<string, string>();
            var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);
            var firstRow = wsPart.Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>().FirstOrDefault();

            if (firstRow != null)
            {
                foreach (var cell in firstRow.Elements<Cell>())
                {
                    string colLetter = new string(cell.CellReference!.Value!.Where(char.IsLetter).ToArray());
                    headers[colLetter] = GetCellValue(wbPart, cell) ?? "Пусто";
                }
            }
            return headers;
        }

        static void ProcessSheet(WorkbookPart wbPart, Sheet sheet, string sCol, string tCol, Dictionary<string, string> dict)
        {
            var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);
            var rows = wsPart.Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>().Skip(1); // Пропускаем заголовок

            foreach (var row in rows)
            {
                var text = GetCellValue(wbPart, row, sCol);
                if (string.IsNullOrEmpty(text)) continue;

                var words = text.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                var found = words.Where(w => dict.ContainsKey(w)).Select(w => dict[w]).Distinct();

                if (found.Any()) UpdateCell(wsPart, row, tCol, string.Join(", ", found));
            }
        }

        // Базовые методы чтения/записи
        static string? GetCellValue(WorkbookPart wbPart, Row row, string colName) =>
            GetCellValue(wbPart, row.Elements<Cell>().FirstOrDefault(c => c.CellReference == (colName + row.RowIndex))!);

        static string? GetCellValue(WorkbookPart wbPart, Cell cell)
        {
            if (cell == null || cell.CellValue == null) return null;
            string val = cell.CellValue.InnerText;
            return (cell.DataType != null && cell.DataType == CellValues.SharedString)
                ? wbPart.SharedStringTablePart!.SharedStringTable!.ElementAt(int.Parse(val)).InnerText : val;
        }

        static void UpdateCell(WorksheetPart wsPart, Row row, string colName, string text)
        {
            string cellRef = colName + row.RowIndex;
            var cell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference == cellRef) ?? new Cell { CellReference = cellRef };
            if (cell.Parent == null) row.Append(cell);
            cell.CellValue = new CellValue(text);
            cell.DataType = CellValues.String;
        }

        static Dictionary<string, string?> BuildDictionary(WorkbookPart wbPart, Sheet sheet, string k, string v)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var rows = ((WorksheetPart)wbPart.GetPartById(sheet.Id!)).Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>();
            foreach (var r in rows)
            {
                var key = GetCellValue(wbPart, r, k);
                if (!string.IsNullOrEmpty(key)) dict[key] = GetCellValue(wbPart, r, v);
            }
            return dict;
        }
    }
}
