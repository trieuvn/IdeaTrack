using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace IdeaTrack.Areas.SciTech.Models
{
    public class InitiativeReportPdf : IDocument
    {
        private readonly List<InitiativePdfVM> _data;

        public InitiativeReportPdf(List<InitiativePdfVM> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        [Obsolete]
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                var lightBlue = Color.FromRGB(200, 230, 255);
                var darkBlue = Color.FromRGB(0, 51, 102);
                var green = Color.FromRGB(0, 153, 0);
                var orange = Color.FromRGB(255, 153, 0);
                var red = Color.FromRGB(255, 51, 51);
                var gray = Color.FromRGB(100, 100, 100);
                var lightGray = Color.FromRGB(245, 250, 255);
                var headerGray = Color.FromRGB(220, 235, 250);

                // ===== HEADER =====
                page.Header()
                    .Height(80) // Gioi han chieu cao header
                    .Padding(10)
                    .Background(lightBlue)
                    .Row(row =>
                    {
                        // Left: text
                        row.RelativeColumn(0.8f)
                            .Stack(stack =>
                            {
                                stack.Item().Text("DAI HOC KINH TE VA TAI CHINH THANH PHO HO CHI MINH")
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor(darkBlue);

                                stack.Item().Text("BAO CAO HO SO SANG KIEN")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(darkBlue);
                                    
                            });

                        // Right: logo
                        row.ConstantColumn(60) // co dinh chieu rong
                            .AlignRight()
                            .Height(60) // co dinh chieu cao
                            .Image("wwwroot/images/logo-uef.png", ImageScaling.FitArea); // FitArea vua du
                    });

                // ===== NOI DUNG BANG =====
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Ma
                        c.RelativeColumn(4); // Tieu de
                        c.RelativeColumn(3); // Nguoi de xuat
                        c.RelativeColumn(3); // Don vi
                        c.RelativeColumn(2); // Status
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(headerGray).Padding(5).Text("Ma").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Tieu de").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Nguoi de xuat").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Don vi").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Status").Bold();
                    });

                    // Rows
                    bool isAlternate = false;
                    foreach (var i in _data)
                    {
                        var rowColor = isAlternate ? lightGray : Colors.White;

                        table.Cell().Background(rowColor).Padding(5).Text(i.InitiativeCode).WrapAnywhere();
                        table.Cell().Background(rowColor).Padding(5).Text(i.Title).WrapAnywhere();
                        table.Cell().Background(rowColor).Padding(5).Text(i.Proposer).WrapAnywhere();
                        table.Cell().Background(rowColor).Padding(5).Text(i.Department).WrapAnywhere();

                        var statusColor = i.Status switch
                        {
                            "Approved" => green,
                            "Pending" => orange,
                            "Rejected" => red,
                            _ => Colors.Black
                        };

                        table.Cell()
                            .Background(rowColor)
                            .Padding(5)
                            .Text(i.Status)
                            .FontColor(statusColor)
                            .SemiBold()
                            .WrapAnywhere();

                        isAlternate = !isAlternate;
                    }
                });

                // ===== FOOTER =====
                page.Footer()
                    .Padding(5)
                    .AlignRight()
                    .Text($"In ngay: {System.DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(9)
                    .Italic()
                    .FontColor(gray);
            });
        }
    }
}
