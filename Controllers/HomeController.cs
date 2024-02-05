using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ShadmanLegal.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Collections.Generic;

namespace ShadmanLegal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=kid;AccountKey=N5NfXOynClwidKft+B6B/owt3S/xAHwjd+xviI5Ty1szp3tgFWpHMPYoE1tz5WTFzrbQP8EHOlJv+AStaEmzGA==;EndpointSuffix=core.windows.net";
        const string ContainerName = "shadman";
        const string DBStorageConnectionString = "Server=tcp:shadmanlegal.database.windows.net,1433;Initial Catalog=ShadmanLegal;Persist Security Info=False;User ID=adminshadmanlegal;Password=A.z_lover11;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private readonly DAL dAL = new DAL(DBStorageConnectionString);

        static HomeController()
        {           
            // Set the EPPlus license context to comply with licensing requirements
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set to NonCommercial if you don't have a commercial license
        }
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            StampDutyIndexViewModel stampDutyIndexViewModel = new StampDutyIndexViewModel();
            List<StampDutyModel> stampDutyModels = new List<StampDutyModel>();

            // Retrieve all blobs using hierarchical listing
            await foreach (BlobHierarchyItem item in containerClient.GetBlobsByHierarchyAsync())
            {
                if (item.IsBlob)
                {
                    BlobClient blobClient = containerClient.GetBlobClient(item.Blob.Name);
                    string blobName = blobClient.Name;
                    // Process the blob information

                    var splitted = blobName.Split("_");

                    if (splitted.Length >= 2)
                    {
                        StampDutyModel existingModel = stampDutyModels.FirstOrDefault(model => model.StateName == splitted[0]);

                        if (existingModel == null)
                        {
                            // If not, create a new StampDutyModel
                            StampDutyModel newModel = new StampDutyModel
                            {
                                StateName = splitted[0],
                                StampDutyDocs = new List<string> { string.Join("_",splitted.Skip(1))}
                            };

                            // Add the new model to the list
                            stampDutyModels.Add(newModel);
                        }
                        else
                        {
                            // If exists, add the stamp duty document to the existing model
                            existingModel.StampDutyDocs.Add(string.Join("_", splitted.Skip(1)));
                        }
                    }
                }
            }
            StampDutyDataModel x = dAL.GetAllDataAsync();
            stampDutyIndexViewModel.SelectedStateStampDutyList = stampDutyModels;
            stampDutyIndexViewModel.StampDutyData = x;
            return View(stampDutyIndexViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult StampDuty()
        {

            //connect to db layer
            StampDutyDataModel x = dAL.GetAllDataAsync();
            return View(x);
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


        [HttpPost]
        public async Task<ActionResult> UploadDocAsync(string stateName, IFormFile file)
        {


            // Create BlobServiceClient using DefaultAzureCredential
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);

            try
            {
                // Create a unique blob name
                //string folderName = $"{stateName}/";
                string blobName = $"{stateName}_" + file.FileName;  //+ Path.GetExtension(file.FileName);

                // Get a reference to the container
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

                // Create a blob client for the new blob
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload the file to the blob
                await blobClient.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders { ContentType = file.ContentType });

                // Return a success message with the blob URL
                //return Ok($"File uploaded successfully: {blobClient.Uri}");
                //RedirectToAction("Index");
            }
            catch (RequestFailedException ex)
            {
                // Handle blob storage errors
                return StatusCode(500, "Error uploading file: " + ex.Message);
            }

            return Json(null);
        }




        [HttpGet]
        [Route("Home/DownloadDoc/{fileName}")]
        public async Task<IActionResult> DownloadDoc(string fileName)
        {
            // Create BlobServiceClient using DefaultAzureCredential
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            try
            {

                    
                    var memoryStream = new MemoryStream();
                    await blobClient.DownloadToAsync(memoryStream);
                //responseMessage.result = "Success";

                //var contentType = "application/octet-stream";

                //using (var stream = new MemoryStream())
                //{
                //    return File(memoryStream.GetBuffer(), contentType, fileName);
                //}
                using (var stream = new MemoryStream())
                {
                    var contentType = "application/octet-stream";
                        return File(memoryStream.GetBuffer(), contentType, fileName);
                }
            }
            catch (RequestFailedException ex)
            {
                // Handle blob storage errors
                return StatusCode(500, "Error downloading file: " + ex.Message);
            }

           
        }






    }
}
