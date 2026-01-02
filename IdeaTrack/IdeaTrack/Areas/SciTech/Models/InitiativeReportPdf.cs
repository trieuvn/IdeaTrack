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
                    .Height(80) // Giới hạn chiều cao header
                    .Padding(10)
                    .Background(lightBlue)
                    .Row(row =>
                    {
                        // Left: text
                        row.RelativeColumn(0.8f)
                            .Stack(stack =>
                            {
                                stack.Item().Text("ĐẠI HỌC KINH TẾ VÀ TÀI CHÍNH THÀNH PHỐ HỒ CHÍ MINH")
                                    .FontSize(12)
                                    .SemiBold()
                                    .FontColor(darkBlue);

                                stack.Item().Text("BÁO CÁO HỒ SƠ SÁNG KIẾN")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(darkBlue);
                                    
                            });

                        // Right: logo
                        row.ConstantColumn(60) // cố định chiều rộng
                            .AlignRight()
                            .Height(60) // cố định chiều cao
                            .Image("wwwroot/images/logo-uef.png", ImageScaling.FitArea); // FitArea vừa đủ
                    });

                // ===== NỘI DUNG BẢNG =====
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Mã
                        c.RelativeColumn(4); // Tiêu đề
                        c.RelativeColumn(3); // Người đề xuất
                        c.RelativeColumn(3); // Đơn vị
                        c.RelativeColumn(2); // Trạng thái
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(headerGray).Padding(5).Text("Mã").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Tiêu đề").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Người đề xuất").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Đơn vị").Bold();
                        header.Cell().Background(headerGray).Padding(5).Text("Trạng thái").Bold();
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
                    .Text($"In ngày: {System.DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(9)
                    .Italic()
                    .FontColor(gray);
            });
        }
    }
}
