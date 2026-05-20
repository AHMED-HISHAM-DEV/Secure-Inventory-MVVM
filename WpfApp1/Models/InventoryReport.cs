using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using WpfApp1.Models;
public class InventoryReport : IDocument
{
    public List<Product> Products { get; }

    public InventoryReport(List<Product> products)
    {
        Products = products;
    }
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.PageColor("#0D0D0D"); // خلفية سوداء تماشياً مع الـ Cyber Theme

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("CYBERSTOCK_OS: INVENTORY_REPORT").FontSize(20).Bold().FontColor("#00FFFF");
                    col.Item().Text($"Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm}").FontColor("#666");
                });
            });

            page.Content().PaddingVertical(20).Table(table =>
            {
                // تعريف الأعمدة (Columns)
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); // Barcode
                    columns.RelativeColumn();    // Name
                    columns.ConstantColumn(80);  // Stock
                    columns.ConstantColumn(100); // Total Value
                });

                // رأس الجدول (Header)
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("BARCODE");
                    header.Cell().Element(CellStyle).Text("PRODUCT_NAME");
                    header.Cell().Element(CellStyle).Text("STOCK");
                    header.Cell().Element(CellStyle).Text("TOTAL_VAL");

                    static IContainer CellStyle(IContainer container) =>
                        container.Background("#111").BorderBottom(1).BorderColor("#00FFFF").Padding(5);
                });

                // تعبئة البيانات (Rows)
                foreach (var item in Products)
                {
                    table.Cell().Element(RowStyle).Text(item.Barcode).FontColor("#FFF");
                    table.Cell().Element(RowStyle).Text(item.Name).FontColor("#FFF");
                    table.Cell().Element(RowStyle).Text(item.StockLevel.ToString()).FontColor("#00FFFF");
                    table.Cell().Element(RowStyle).Text($"${item.StockLevel * item.CostPrice:N2}").FontColor("#FFF");

                    static IContainer RowStyle(IContainer container) =>
                        container.BorderBottom(1).BorderColor("#222").Padding(5);
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" / Secure Audit Document");
            });
        });
    }
}