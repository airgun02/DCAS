using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCAS.Models
{
    public class PaymentService
    {
        [Key]
        public int Id { get; set; }

        public int PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        public int ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public Services Service { get; set; }

        // Snapshot of the price at time of payment
        public decimal UnitPrice { get; set; }
    }
}