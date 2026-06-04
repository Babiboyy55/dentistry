using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class TreatmentPlanSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TreatmentPlanId { get; set; }
        [ForeignKey("TreatmentPlanId")]
        public TreatmentPlan? TreatmentPlan { get; set; }

        [Required]
        public int SessionNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Postponed

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int? AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
