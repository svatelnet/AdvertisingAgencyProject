using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgencyProject.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ФИО / название клиента")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Компания")]
        public string? CompanyName { get; set; }

        public ICollection<AdOrder> Orders { get; set; } = new List<AdOrder>();
    }
}