using System;
using System.ComponentModel.DataAnnotations;

namespace DCAS.Models
{
    public class LatestUpdate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        public string? Body { get; set; }

        // Image bytes (optional)
        public byte[]? Image { get; set; }

        // Keep content-type for proper File(...) response
        public string? ImageContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}