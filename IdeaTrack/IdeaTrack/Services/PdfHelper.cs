using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;


namespace IdeaTrack.Services
{
    public static class PdfHelper
    {
        public static string ExtractText(string path)
        {
            var sb = new StringBuilder();

            using var reader = new PdfReader(path);
            using var pdf = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                sb.AppendLine(
                    PdfTextExtractor.GetTextFromPage(pdf.GetPage(i))
                );
            }

            return sb.ToString();
        }
    }
}
