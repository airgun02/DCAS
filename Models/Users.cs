using System.ComponentModel.DataAnnotations;

namespace DCAS.Models
{
    public class Users
    {
        [Key]
        public int UsersId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string JobTitle { get; set; }
        public string Specialization { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Position { get; set; }
        public string WorkStatus { get; set; }
        public string Age { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime StartDate { get; set; }

        public byte[]? Profile { get; set; }

        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
