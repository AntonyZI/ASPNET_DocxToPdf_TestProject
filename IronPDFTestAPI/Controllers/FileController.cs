using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using IronPdf;

namespace IronPDFTestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        private DocxToPdfRenderer renderer;

        private static string docxFilesRelPath = "docxFiles/";
        private static string pdfFilesRelPath = "pdfFiles/";

        public FileController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            IronPdf.License.LicenseKey = "<insertLicenseKey>";
            renderer = new DocxToPdfRenderer();
        }

        [HttpGet("convertExistingDocToSavePdf")]
        public IActionResult ConvertExistingDocToSavePdf()
        {

            Console.WriteLine("Hola mundo");
            // Instantiate Renderer
            DocxToPdfRenderer renderer = new DocxToPdfRenderer();
            // Render from RTF file
            PdfDocument pdf = renderer.RenderDocxAsPdf(docxFilesRelPath + "DocumentoPrueba.docx");
            // Save the PDF
            pdf.SaveAs(pdfFilesRelPath + "pdfFromRtfFile.pdf");

            return Ok("File converted and saved on system");
        }

        [HttpPost("convertDocToPdf")]
        public async Task<IActionResult> ConvertDocToPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            // Filename without extension
            string lowerFileNameNE = "";
            string lowerFileName = file.FileName.ToLower();
            if (lowerFileName.EndsWith(".docx"))
            {
                lowerFileNameNE = lowerFileName.Substring(0, lowerFileName.Length - 5);
            }
            else if (lowerFileName.EndsWith(".doc"))
            {
                lowerFileNameNE = lowerFileName.Substring(0, lowerFileName.Length - 4);
            }
            else
            {
                return BadRequest(".DOC and .DOCX filetypes only");
            }

            // Read the DOCX file into a byte array
            byte[] docxBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                docxBytes = memoryStream.ToArray();
            }

            // Render the PDF directly from the byte array
            PdfDocument pdf = renderer.RenderDocxAsPdf(docxBytes);

            // Return the PDF as a file
            var pdfStream = new MemoryStream(pdf.BinaryData);
            return File(pdfStream, "application/pdf", lowerFileNameNE + "_" + DateTime.Now.Ticks.ToString() + ".pdf");
        }

    }
}
