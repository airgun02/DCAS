using System.ComponentModel.DataAnnotations;

namespace DCAS.Models
{
    public class MedicineInventory
    {
        [Key]
        public int Id { get; set; }

        public string MedicineName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string Miligram { get; set; }

        public string Description { get; set; }

        public DateTime ExpiryDate { get; set; }

        // Add this property to store the image as a byte array
        public byte[]? Image { get; set; }

    }
}
