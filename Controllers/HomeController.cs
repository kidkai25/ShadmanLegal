using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ShadmanLegal.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShadmanLegal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        static HomeController()
        {
            // Set the EPPlus license context to comply with licensing requirements
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set to NonCommercial if you don't have a commercial license
        }
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                using (var stream = file.OpenReadStream())
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    // Assuming the first row contains headers
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var rowDict = new Dictionary<string, object>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            var key = worksheet.Cells[1, col].Text;
                            var value = worksheet.Cells[row, col].Text;
                            rowDict[key] = value;
                        }
                        data.Add(rowDict);
                    }
                }

                return Json(data);
            }

            return Json(null);
        }
    }
}
