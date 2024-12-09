namespace ApiFirma.Models
{
    using iText.Kernel.Pdf;
    using iText.Layout;
    using iText.Layout.Element;
    using iText.IO.Image;
    using iText.Kernel.Geom;
    using System.IO;
    using Microsoft.AspNetCore.Mvc;
    using iText.Kernel.Pdf.Canvas;
    using iText.Kernel.Pdf.Extgstate;

    public class Solicitud : ControllerBase
    {
        [HttpPost]
        [Route("AppMarca/agua")]
        public IActionResult AddWatermarkWithImage([FromForm] Class request)
        {
            if (request.PdfFile == null || string.IsNullOrEmpty(request.MarcaDeAguaBase64))
            {
                return BadRequest("Debe proporcionar un archivo PDF y una imagen (Base64) para la marca de agua.");
            }

            try
            {
                // Leer el archivo PDF
                using var pdfStream = request.PdfFile.OpenReadStream();
                using var reader = new PdfReader(pdfStream);
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                var pdfDoc = new PdfDocument(reader, writer);

                // Convertir la cadena Base64 en bytes
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(request.MarcaDeAguaBase64);
                }
                catch (FormatException)
                {
                    return BadRequest("La cadena Base64 de la imagen proporcionada no es válida.");
                }

                // Crear ImageData con el byte[]
                var imageData = ImageDataFactory.Create(imageBytes);

                int totalPages = pdfDoc.GetNumberOfPages();

                for (int i = 1; i <= totalPages; i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var pageSize = page.GetPageSize();

                    float x, y;

                    // Si no se especifica posición, centramos la imagen en la página
                    if (!request.PosX.HasValue && !request.PosY.HasValue)
                    {
                        x = (pageSize.GetWidth() - imageData.GetWidth()) / 2;
                        y = (pageSize.GetHeight() - imageData.GetHeight()) / 2;
                    }
                    else
                    {
                        // Si se han proporcionado valores, los usamos directamente.
                        // Si alguno no se proporciona, asumimos que queda centrado en ese eje.
                        x = request.PosX.HasValue ? request.PosX.Value : (pageSize.GetWidth() - imageData.GetWidth()) / 2;
                        y = request.PosY.HasValue ? request.PosY.Value : (pageSize.GetHeight() - imageData.GetHeight()) / 2;
                    }

                    var pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                    pdfCanvas.SaveState();
                    pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.6f)); // Ajusta la opacidad de la marca de agua
                    pdfCanvas.AddImageAt(imageData, x, y, true);
                    pdfCanvas.RestoreState();
                }

                pdfDoc.Close();

                return File(memoryStream.ToArray(), "application/pdf", "WatermarkedImage.pdf");
            }
            catch (iText.Kernel.PdfException pdfEx)
            {
                return StatusCode(500, $"Error de PDF: {pdfEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

    }

}
