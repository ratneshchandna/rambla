using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace otr_project.Models
{
    public class MessageModel
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string From { get; set; }
        
        [Required]
        public string To { get; set; }

        [Required]
        public string Subject { get; set; }

        public virtual ICollection<ThreadModel> Messages { get; set; }

        public bool isRead { get; set; }
    }

    public class ThreadModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string MessageModelId { get; set; }

        [Required]
        public virtual UserModel Author { get; set; }

        public virtual System.DateTime Date { get; set; }
    }
}