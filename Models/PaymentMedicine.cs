using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCAS.Models
{
    public class PaymentMedicine
    {
        [Key]
        public int PaymentMedicineId { get; set; }

        // Link to MedicineInventory (optional FK)
        public int MedicineId { get; set; }

        public string MedicineName { get; set; }

        // Foreign key for Payment
        public int PaymentId { get; set; }

        // Unit price at the time of the transaction
        public decimal UnitPrice { get; set; }

        // Quantity of tablets sold for this medicine
        public int Quantity { get; set; }

        // Total price for this line (UnitPrice * Quantity) - helpful for persistence
        public decimal Price { get; set; }

        public virtual Payment Payment { get; set; }
    }
}