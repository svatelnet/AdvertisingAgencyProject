using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgencyProject.ViewModels
{
    public class ClientProfileViewModel
    {
        [Required]
        [Display(Name = "ФИО / название клиента")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Компания")]
        public string? CompanyName { get; set; }
    }
}