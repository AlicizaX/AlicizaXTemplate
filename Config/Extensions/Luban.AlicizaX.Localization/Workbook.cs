using System.IO.Compression;
using System.Xml.Linq;

namespace AlicizaX.LubanLocalization;

internal static class Workbook
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    internal static List<Dictionary<int, string?>> ReadTexts(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        XDocument workbook = Load(archive, "xl/workbook.xml");
        XDocument relationships = Load(archive, "xl/_rels/workbook.xml.rels");
        var targets = relationships.Descendants().Where(element => element.Name.LocalName == "Relationship")
            .ToDictionary(element => (string)element.Attribute("Id")!, element => (string)element.Attribute("Target")!);
        XElement sheet = workbook.Descendants(Main + "sheet")
            .FirstOrDefault(element => (string?)element.Attribute("name") == "Texts")
            ?? throw new InvalidOperationException($"Missing Texts sheet: {path}");
        string target = targets[(string)sheet.Attribute(Relationship + "id")!].Replace('\\', '/').TrimStart('/');
        if (!target.StartsWith("xl/", StringComparison.Ordinal)) target = "xl/" + target;

        string[] strings = Array.Empty<string>();
        if (archive.GetEntry("xl/sharedStrings.xml") is { } shared)
        {
            using Stream stream = shared.Open();
            strings = XDocument.Load(stream).Descendants(Main + "si")
                .Select(item => string.Concat(item.Descendants(Main + "t").Select(text => text.Value))).ToArray();
        }

        var rows = new List<Dictionary<int, string?>>();
        foreach (XElement row in Load(archive, target).Descendants(Main + "sheetData").Elements(Main + "row"))
        {
            int number = (int)row.Attribute("r")!;
            while (rows.Count < number) rows.Add(new Dictionary<int, string?>());
            var cells = rows[number - 1];
            foreach (XElement cell in row.Elements(Main + "c"))
            {
                string address = (string)cell.Attribute("r")!;
                int column = 0;
                foreach (char character in address)
                {
                    if (!char.IsLetter(character)) break;
                    column = column * 26 + char.ToUpperInvariant(character) - 'A' + 1;
                }
                string? value = (string?)cell.Element(Main + "v");
                string? type = (string?)cell.Attribute("t");
                if (type == "s" && value != null) value = strings[int.Parse(value)];
                else if (type == "inlineStr") value = string.Concat(cell.Descendants(Main + "t").Select(text => text.Value));
                cells[column - 1] = value;
            }
        }
        return rows;
    }

    private static XDocument Load(ZipArchive archive, string path)
    {
        using Stream stream = archive.GetEntry(path)?.Open()
            ?? throw new InvalidOperationException($"Missing workbook entry {path}.");
        return XDocument.Load(stream);
    }
}
