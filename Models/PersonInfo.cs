using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCAS.Models
{
    public class PersonInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DetailOnee { get; set; }
        public string? HomeAddress { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? Age { get; set; }

        [Display(Name = "Home Number")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Home number must be exactly 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Home number must contain only digits (11 digits).")]
        public string? HomeNumber { get; set; }

        public string? Occupation { get; set; }

        [Display(Name = "Mobile Number")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Mobile number must be exactly 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Mobile number must contain only digits (11 digits).")]
        public string? MobileNumber { get; set; }

        public string? OfficeAddress { get; set; }
        public string? EmailAddress { get; set; }
        public string? Status { get; set; }   // keep marital/status as-is
        public string? NameOfSpouse { get; set; }
        public string? PersonalResponsibleforAccount { get; set; }
        public string? Relationship { get; set; }
        public string? DetailTwoo { get; set; }
        public string? PhysicianCare { get; set; }
        public string? PhysicianName { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Contact number must be exactly 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Contact number must contain only digits (11 digits).")]
        public string? ContactNumber { get; set; }

        public string? MedicalServices { get; set; }  
        public decimal Price { get; set; }
        public string? DetailOne { get; set; }
        public string? DetailTwo { get; set; }
        public string? DetailThree { get; set; }
        public string? DetailFour { get; set; }
        public string? DetailFive { get; set; }
        public string? DetailSix { get; set; }
        public string? DetailSeven { get; set; }
        public string? DetailEigth { get; set; }
        public string? AvailableDay { get; set; }
        public string? AvailableTime { get; set; }

        // New: walk-in registration state (Pre-register / Registered)
        public string? WalkInStatus { get; set; }
    }
}
