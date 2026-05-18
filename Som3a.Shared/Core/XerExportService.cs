using Som3a.Shared.Core;
using System.IO;

public class XerExportService
{
    public void Export(string path, XerParser parser)
    {
        using var sw = new StreamWriter(path.Replace(".xer", "_edited.xer"));

        foreach (var table in parser.Tables)
        {
            sw.WriteLine($"%T\t{table.Name}");

            sw.WriteLine("%F\t" + string.Join("\t", table.Headers));

            foreach (var row in table.Rows)
            {
                sw.WriteLine("%R\t" + string.Join("\t", row));
            }
        }
    }
}