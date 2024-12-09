using iText.Kernel.Pdf.Function;

namespace ApiFirma.Models
{
    public class Class
    {
        public required IFormFile PdfFile { get; set; }
        public required string MarcaDeAguaBase64 { get; set; }
        public required float? PosX {get; set;}
        public required float? PosY {get; set;}
    }
}
