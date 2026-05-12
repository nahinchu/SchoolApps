using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    public enum PaymentStatus
    {
        PENDING,
        PAID,
        CANCELLED
    }

    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public long OrderCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
