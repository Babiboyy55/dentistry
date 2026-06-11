using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class ExaminationSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; } // StaffProfileId
        [ForeignKey("DoctorId")]
        public StaffProfile? Doctor { get; set; }

        public int? AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ClinicalNotes { get; set; }

        [MaxLength(2000)]
        public string? TreatmentPlanSummary { get; set; }

        [MaxLength(2000)]
        public string? HomeCareInstructions { get; set; }

        [Required]
        [Column(TypeName = "decimal(3,2)")]
        public decimal PatientCoefficient { get; set; } = 0.00m; // Range: 0.0 - 0.5

        // ── Luồng duyệt ca phức tạp ─────────────────────────────────
        // null = chưa báo | "Pending" = chờ duyệt | "Approved" = đã duyệt | "Rejected" = từ chối
        [MaxLength(20)]
        public string? ComplexStatus { get; set; } = null;

        // Lý do bác sĩ mô tả tại sao ca này phức tạp
        [MaxLength(1000)]
        public string? ComplexReason { get; set; } = null;

        // Hệ số bác sĩ đề xuất (0.1 – 0.5)
        [Column(TypeName = "decimal(3,2)")]
        public decimal? RequestedCoefficient { get; set; } = null;

        // Ghi chú của admin khi duyệt hoặc từ chối
        [MaxLength(500)]
        public string? AdminNote { get; set; } = null;

        // Thời điểm admin xử lý yêu cầu
        public DateTime? ReviewedAt { get; set; } = null;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public bool IsCompleted { get; set; } = false;

        [ConcurrencyCheck]
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}
