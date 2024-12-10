using KanbamApi.Models;

namespace KanbamApi.Dtos.Update
{
    public class DtoCardUpdate
    {
        public string? ListId { get; set; }
        public string? Title { get; set; }
        public int? IndexNumber { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public List<Comment>? Comments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DueDateReminder { get; set; }
    }
}
