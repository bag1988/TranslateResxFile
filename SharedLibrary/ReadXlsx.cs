﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace SharedLibrary
{
    public static class ReadXlsx
    {
        public static List<List<string>> ReadXlsxFile(string fileName)
        {
            List<List<string>> translateInfo = new();
            try
            {
                using (var spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            reader.ReadFirstChild();
                            List<string> childList = new();
                            do
                            {
                                if (reader.ElementType == typeof(Cell))
                                {
                                    var c = (Cell?)reader.LoadCurrentElement();

                                    string cellValue;

                                    if (c != null && c.DataType != null && c.CellValue != null && c.DataType == CellValues.SharedString && workbookPart.SharedStringTablePart != null)
                                    {
                                        var ssi = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(int.Parse(c.CellValue.InnerText));

                                        cellValue = ssi.Text?.Text ?? "";
                                    }
                                    else
                                    {
                                        cellValue = c?.CellValue?.InnerText ?? "";
                                    }

                                    int cellColumnIndex = GetColumnIndex((c?.CellReference?.Value ?? "")) ?? 1;

                                    if (childList.Count < cellColumnIndex - 1)
                                    {
                                        childList.AddRange(Enumerable.Repeat(string.Empty, ((cellColumnIndex - 1) - childList.Count)));
                                    }
                                    childList.Add(cellValue.Trim());
                                }

                            } while (reader.ReadNextSibling());
                            translateInfo.Add(childList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка чтения файла {0}", ex.Message);
            }
            return translateInfo;
        }


        public static bool SaveToFile(string path, Dictionary<string, List<string>> tab)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(path, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                uint rowIndex = 1;
                foreach (var d in tab)
                {
                    var row = new Row() { RowIndex = rowIndex };
                    sheetData!.Append(row);

                    int i = 1;
                    foreach (var t in d.Value)
                    {
                        var newCell = new Cell() { CellReference = (GetColumnName(i) + rowIndex) };

                        row.Append(newCell);

                        newCell.CellValue = new CellValue(t);

                        if (int.TryParse(t, out int ints))
                        {
                            newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                        else
                            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                        i++;
                    }
                    rowIndex++;
                }
                workbookPart.Workbook.Save();

                return true;
            }
        }

        public static bool AddRowToFile(string path, Dictionary<string, List<string>> tab, uint startRowIndexInsert)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(path, true))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                if (sheetData != null)
                {
                    uint rowIndex = startRowIndexInsert + 2;
                    foreach (var d in tab)
                    {
                        var row = new Row() { RowIndex = rowIndex };

                        sheetData.Append(row);

                        int i = 1;
                        foreach (var t in d.Value)
                        {
                            var newCell = new Cell() { CellReference = (GetColumnName(i) + rowIndex) };

                            row.Append(newCell);

                            newCell.CellValue = new CellValue(t);

                            if (int.TryParse(t, out int ints))
                            {
                                newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
                            }
                            else
                                newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                            i++;
                        }
                        rowIndex++;
                    }
                    workbookPart.Workbook.Save();

                    return true;
                }
            }
            return false;
        }

        static string GetColumnName(int colIndex)
        {
            int div = colIndex;
            string colLetter = string.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colLetter = (char)(65 + mod) + colLetter;
                div = (int)((div - mod) / 26);
            }
            return colLetter;
        }

        static int? GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return null;
            }
            string columnReference = Regex.Replace(cellReference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * (c - 64);

                mulitplier = mulitplier * 26;
            }
            return columnNumber + 1;
        }
    }


}
