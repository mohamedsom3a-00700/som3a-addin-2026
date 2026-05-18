using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Som3a.Shared.Core;
using Som3a.Shared.Models;

namespace Som3a.Shared.Controllers
{
    public sealed class CompareController
    {
        private readonly ExcelCompareService _service;

        public CompareController()
        {
            _service = new ExcelCompareService();
        }

        public Task<IReadOnlyList<CompareResultItem>> RunAsync(
            Excel.Workbook wb,
            string oldSheet,
            string newSheet,
            string idCol,
            string compareCol,
            string copyCol,
            int startRow,
            bool previewMode,
            IProgress<ExcelCompareService.ProgressInfo> progress,
            CancellationToken token)
        {
            return _service.ProcessAsync(
                wb,
                oldSheet,
                newSheet,
                idCol,
                compareCol,
                copyCol,
                startRow,
                previewMode,
                progress,
                token);
        }
    }
}
