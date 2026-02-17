using System.ComponentModel.DataAnnotations;

namespace DCAS.Models
{
    public class TodaySchedule
    {
        [Key]
        public int Id { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string PersonName { get; set; }
        public string Status { get; set; } = "Waiting";
    }
}
