using System.ComponentModel.DataAnnotations;

namespace DCAS.Models
{
    public class Services
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Medical service name is required")]
        [Display(Name = "Medical Service Name")]
        public string MedicalNameService { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }
    }
}
