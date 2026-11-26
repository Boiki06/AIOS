// Represents payment-related invoice data returned to transaction views.
namespace AIOS.Models
{
    // Simple data-transfer model used by payment and invoice displays.
    public class Payment
    {
        // Invoice associated with this payment view.
        public int InvoiceID { get; set; }

        // Request associated with the invoiced work.
        public int RequestID { get; set; }

        // Human-readable invoice reference.
        public string InvoiceNumber { get; set; } = "";

        // Amount represented by the payment or invoice record.
        public decimal Amount { get; set; }

        // Date associated with the invoice.
        public DateTime InvoiceDate { get; set; }

        // Current payment status.
        public string Status { get; set; } = "Outstanding";
    }
}