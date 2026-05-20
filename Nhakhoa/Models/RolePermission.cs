using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ModuleKey { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ModuleName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ModuleIcon { get; set; } = "bi-grid";

        public int SortOrder { get; set; } = 0;

        public bool IsAllowed { get; set; } = false;
    }
}
