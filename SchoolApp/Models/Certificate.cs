using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApp.Models
{
    [Table("Certificates")]
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(36)]
        public string CertificateCode { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Ngày cấp")]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
