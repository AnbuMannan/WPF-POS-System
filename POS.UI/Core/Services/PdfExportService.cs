using PdfSharp.Drawing;
using PdfSharp.Pdf;
using POS.Shared.Models;
using POS.UI.Components;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace POS.UI.Core.Services
{
    public class PdfExportService
    {
        private const string StoreCode = "STORE001"; // Replace with actual store code from configuration

        public PdfExportService()
        {
            // Constructor
        }

        public void ExportEODReportToPdf(EODReportDto report, DateTime reportDate, decimal openingCash, decimal actualCash, decimal expectedCash, decimal cashDifference)
        {
            if (report == null)
            {
                DialogService.Error("PDF Export Error", "No EOD report data available to export.");
                return;
            }

            try
            {
                // Create reports directory in application execution directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string reportsDir = Path.Combine(baseDir, "Reports", "EOD");
                if (!Directory.Exists(reportsDir))
                {
                    Directory.CreateDirectory(reportsDir);
                }
                string filePath = Path.Combine(reportsDir, $"EOD_Report_{reportDate:yyyy-MM-dd_HHmmss}.pdf");

                PdfDocument document = new PdfDocument();
                document.Info.Title = $"EOD Report - {reportDate:yyyy-MM-dd}";
                document.Info.Author = "POS Pro System";

                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Font definitions
                XFont fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
                XFont fontSubtitle = new XFont("Arial", 10, XFontStyleEx.Regular);
                XFont fontHeader = new XFont("Arial", 11, XFontStyleEx.Bold);
                XFont fontNormal = new XFont("Arial", 9, XFontStyleEx.Regular);
                XFont fontSmall = new XFont("Arial", 8, XFontStyleEx.Regular);
                XFont fontBold = new XFont("Arial", 9, XFontStyleEx.Bold);

                // Colors
                XColor headerBgColor = XColor.FromArgb(240, 240, 240);
                XColor borderColor = XColor.FromArgb(200, 200, 200);
                XColor accentColor = XColor.FromArgb(0, 51, 102);

                XUnit margin = XUnit.FromPoint(40);
                XUnit yPos = margin;
                XUnit contentWidth = page.Width - (margin * 2);

                // Header Section
                DrawHeader(gfx, reportDate, fontTitle, fontSubtitle, accentColor, margin, ref yPos, contentWidth);
                yPos += XUnit.FromPoint(20);

                // Summary Table (3 columns)
                DrawSummaryTable(gfx, report, fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                yPos += XUnit.FromPoint(15);

                // Payment Breakdown Table
                DrawPaymentBreakdownTable(gfx, report, fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                yPos += XUnit.FromPoint(15);

                // Tax Collected Table
                DrawTaxTable(gfx, report, fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                yPos += XUnit.FromPoint(15);

                // Cash Reconciliation Table
                DrawCashReconciliationTable(gfx, report, openingCash, actualCash, expectedCash, cashDifference, 
                    fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                yPos += XUnit.FromPoint(15);

                // Check if we need new page for Top Sales
                if (yPos > page.Height - XUnit.FromPoint(100))
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = margin;
                }

                // Top Sales Table
                if (report.TopSales.Any())
                {
                    DrawTopSalesTable(gfx, report, fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                    yPos += XUnit.FromPoint(15);
                }

                // Check if we need new page for Top Selling Products
                if (yPos > page.Height - XUnit.FromPoint(100))
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = margin;
                }

                // Top Selling Products Table
                if (report.TopSellingProducts.Any())
                {
                    DrawTopProductsTable(gfx, report, fontHeader, fontNormal, fontBold, headerBgColor, borderColor, margin, ref yPos, contentWidth);
                }

                // Footer
                DrawFooter(gfx, fontSmall, borderColor, page, margin, contentWidth);

                // Save the PDF
                document.Save(filePath);

                DialogService.Info("PDF Export", $"EOD Report exported to:\n{filePath}");

                // Open the PDF
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                DialogService.Error("PDF Export Error", $"Failed to export EOD report to PDF: {ex.Message}");
            }
        }

        private void DrawHeader(XGraphics gfx, DateTime reportDate, XFont fontTitle, XFont fontSubtitle, 
            XColor accentColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            // Title
            gfx.DrawString("END OF DAY REPORT", fontTitle, new XSolidBrush(accentColor), 
                margin + (contentWidth.Point / 2), yPos, XStringFormats.TopCenter);
            yPos += XUnit.FromPoint(25);

            // Store Code and Date
            gfx.DrawString($"Store: {StoreCode} | Report Date: {reportDate:dd-MMM-yyyy}", fontSubtitle, XBrushes.Black,
                margin + (contentWidth.Point / 2), yPos, XStringFormats.TopCenter);
            yPos += XUnit.FromPoint(15);

            // Separator line
            gfx.DrawLine(new XPen(accentColor, 1), margin, yPos, margin + contentWidth, yPos);
            yPos += XUnit.FromPoint(10);
        }

        private void DrawSummaryTable(XGraphics gfx, EODReportDto report, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit colWidth = contentWidth / 3;
            XUnit rowHeight = XUnit.FromPoint(20);

            // Table header
            DrawTableRow(gfx, new[] { "SUMMARY", "COUNT", "AMOUNT (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            string[][] summaryData = {
                new[] { "Total Sales", report.SaleCount.ToString(), $"{report.TotalSales:N2}" },
                new[] { "Total Discounts", "-", $"{report.DiscountSum:N2}" },
                new[] { "Total Refunds", report.TotalReturnsCount.ToString(), $"{report.TotalRefunds:N2}" },
                new[] { "Total Expenses", "-", $"{report.TotalExpenses:N2}" }
            };

            foreach (var row in summaryData)
            {
                DrawTableRow(gfx, row, fontNormal, XBrushes.White, borderColor, margin, yPos, colWidth, rowHeight, false);
                yPos += rowHeight;
            }

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawPaymentBreakdownTable(XGraphics gfx, EODReportDto report, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit colWidth = contentWidth / 2;
            XUnit rowHeight = XUnit.FromPoint(18);

            // Table header
            DrawTableRow(gfx, new[] { "PAYMENT METHOD", "AMOUNT (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            foreach (var item in report.PaymentBreakdown)
            {
                DrawTableRow(gfx, new[] { item.Key, $"{item.Value:N2}" }, 
                    fontNormal, XBrushes.White, borderColor, margin, yPos, colWidth, rowHeight, false);
                yPos += rowHeight;
            }

            // Total row
            decimal totalPayments = report.PaymentBreakdown.Values.Sum();
            DrawTableRow(gfx, new[] { "TOTAL", $"{totalPayments:N2}" }, 
                fontBold, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawTaxTable(XGraphics gfx, EODReportDto report, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit colWidth = contentWidth / 2;
            XUnit rowHeight = XUnit.FromPoint(18);

            // Table header
            DrawTableRow(gfx, new[] { "TAX TYPE", "AMOUNT (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            string[][] taxData = {
                new[] { "CGST", $"{report.TotalCGST:N2}" },
                new[] { "SGST", $"{report.TotalSGST:N2}" },
                new[] { "IGST", $"{report.TotalIGST:N2}" }
            };

            foreach (var row in taxData)
            {
                DrawTableRow(gfx, row, fontNormal, XBrushes.White, borderColor, margin, yPos, colWidth, rowHeight, false);
                yPos += rowHeight;
            }

            // Total tax
            decimal totalTax = report.TotalCGST + report.TotalSGST + report.TotalIGST;
            DrawTableRow(gfx, new[] { "TOTAL TAX", $"{totalTax:N2}" }, 
                fontBold, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawCashReconciliationTable(XGraphics gfx, EODReportDto report, decimal openingCash, 
            decimal actualCash, decimal expectedCash, decimal cashDifference, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit colWidth = contentWidth / 2;
            XUnit rowHeight = XUnit.FromPoint(18);

            // Table header
            DrawTableRow(gfx, new[] { "CASH RECONCILIATION", "AMOUNT (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidth, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            string[][] cashData = {
                new[] { "Opening Cash", $"{openingCash:N2}" },
                new[] { "+ Cash Sales", $"{report.CashSalesAmount:N2}" },
                new[] { "- Cash Refunds", $"{report.CashRefundAmount:N2}" },
                new[] { "- Petty Cash/Expenses", $"{report.TotalExpenses:N2}" },
                new[] { "Expected Closing Cash", $"{expectedCash:N2}" },
                new[] { "Actual Cash", $"{actualCash:N2}" }
            };

            foreach (var row in cashData)
            {
                DrawTableRow(gfx, row, fontNormal, XBrushes.White, borderColor, margin, yPos, colWidth, rowHeight, false);
                yPos += rowHeight;
            }

            // Difference row with color coding
            XBrush differenceColor = cashDifference < 0 ? XBrushes.Red : XBrushes.Green;
            DrawTableRow(gfx, new[] { "DIFFERENCE", $"{cashDifference:N2}" }, 
                fontBold, differenceColor, borderColor, margin, yPos, colWidth, rowHeight, false);
            yPos += rowHeight;

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawTopSalesTable(XGraphics gfx, EODReportDto report, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit[] colWidths = { contentWidth * 0.15, contentWidth * 0.45, contentWidth * 0.20, contentWidth * 0.20 };
            XUnit rowHeight = XUnit.FromPoint(18);

            // Table header
            DrawTableRow(gfx, new[] { "S.No", "BILL NUMBER", "DATE", "TOTAL (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidths, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            int serialNo = 1;
            foreach (var sale in report.TopSales)
            {
                DrawTableRow(gfx, new[] { 
                    serialNo++.ToString(), 
                    sale.BillNumber, 
                    sale.CreatedAt.ToString("dd-MMM-yy"), 
                    $"{sale.TotalAmount:N2}" 
                }, fontNormal, XBrushes.White, borderColor, margin, yPos, colWidths, rowHeight, false);
                yPos += rowHeight;
            }

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawTopProductsTable(XGraphics gfx, EODReportDto report, XFont fontHeader, XFont fontNormal, 
            XFont fontBold, XColor headerBgColor, XColor borderColor, XUnit margin, ref XUnit yPos, XUnit contentWidth)
        {
            XUnit[] colWidths = { contentWidth * 0.15, contentWidth * 0.45, contentWidth * 0.20, contentWidth * 0.20 };
            XUnit rowHeight = XUnit.FromPoint(18);

            // Table header
            DrawTableRow(gfx, new[] { "S.No", "PRODUCT NAME", "QTY SOLD", "REVENUE (₹)" }, 
                fontHeader, new XSolidBrush(headerBgColor), borderColor, margin, yPos, colWidths, rowHeight, true);
            yPos += rowHeight;

            // Table rows
            int serialNo = 1;
            foreach (var product in report.TopSellingProducts)
            {
                DrawTableRow(gfx, new[] { 
                    serialNo++.ToString(), 
                    product.Name, 
                    product.QuantitySold.ToString(), 
                    $"{product.Revenue:N2}" 
                }, fontNormal, XBrushes.White, borderColor, margin, yPos, colWidths, rowHeight, false);
                yPos += rowHeight;
            }

            // Bottom border
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, yPos, margin + contentWidth, yPos);
        }

        private void DrawFooter(XGraphics gfx, XFont fontSmall, XColor borderColor, PdfPage page, XUnit margin, XUnit contentWidth)
        {
            // Footer separator
            XUnit footerY = page.Height - margin - XUnit.FromPoint(20);
            gfx.DrawLine(new XPen(borderColor, 0.5), margin, footerY, margin + contentWidth, footerY);
            footerY += XUnit.FromPoint(5);

            // Footer text
            gfx.DrawString($"Generated by POS Pro System on {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 
                fontSmall, XBrushes.Gray, margin + (contentWidth.Point / 2), footerY, XStringFormats.TopCenter);
        }

        private void DrawTableRow(XGraphics gfx, string[] cells, XFont font, XBrush bgColor, XColor borderColor, 
            XUnit margin, XUnit yPos, XUnit colWidth, XUnit rowHeight, bool isHeader)
        {
            XUnit xPos = margin;
            
            for (int i = 0; i < cells.Length; i++)
            {
                // Draw cell background
                gfx.DrawRectangle(new XPen(borderColor, 0.5), bgColor, 
                    xPos, yPos, colWidth, rowHeight);
                
                // Draw cell content with proper alignment
                XStringFormat format = new XStringFormat();
                if (i == cells.Length - 1 && cells[i].Contains("₹")) // Right-align currency values
                    format.Alignment = XStringAlignment.Far;
                else
                    format.Alignment = XStringAlignment.Near;
                
                format.LineAlignment = XLineAlignment.Center;
                
                gfx.DrawString(cells[i], font, isHeader ? XBrushes.Black : XBrushes.Black, 
                    new XRect(xPos + 5, yPos, colWidth - 10, rowHeight), format);
                
                xPos += colWidth;
            }
        }

        private void DrawTableRow(XGraphics gfx, string[] cells, XFont font, XBrush bgColor, XColor borderColor, 
            XUnit margin, XUnit yPos, XUnit[] colWidths, XUnit rowHeight, bool isHeader)
        {
            XUnit xPos = margin;
            
            for (int i = 0; i < cells.Length; i++)
            {
                // Draw cell background
                gfx.DrawRectangle(new XPen(borderColor, 0.5), bgColor, 
                    xPos, yPos, colWidths[i], rowHeight);
                
                // Draw cell content with proper alignment
                XStringFormat format = new XStringFormat();
                if (i == cells.Length - 1 && cells[i].Contains("₹")) // Right-align currency values in last column
                    format.Alignment = XStringAlignment.Far;
                else
                    format.Alignment = XStringAlignment.Near;
                
                format.LineAlignment = XLineAlignment.Center;
                
                gfx.DrawString(cells[i], font, isHeader ? XBrushes.Black : XBrushes.Black, 
                    new XRect(xPos + 5, yPos, colWidths[i] - 10, rowHeight), format);
                
                xPos += colWidths[i];
            }
        }
    }
}