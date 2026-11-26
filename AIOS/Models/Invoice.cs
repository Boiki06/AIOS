// Represents a bill issued for work completed or requested through the platform.
namespace AIOS.Models
{
    // Simple data-transfer model for invoice records.
    public class Invoice
    {
        // Database identifier for the invoice.
        public int InvoiceID { get; set; }

        // Request that produced the invoice.
        public int RequestID { get; set; }

        // Human-readable invoice reference displayed to users.
        public string InvoiceNumber { get; set; } = "";

        // Monetary amount owed for the request.
        public decimal Amount { get; set; }

        // Date on which the invoice was issued.
        public DateTime InvoiceDate { get; set; }

        // Payment lifecycle value, such as Outstanding or Paid.
        public string Status { get; set; } = "Outstanding";
    }
}