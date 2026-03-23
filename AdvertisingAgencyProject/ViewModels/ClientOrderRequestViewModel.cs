using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgencyProject.ViewModels
{
    public class ClientOrderRequestViewModel
    {
        [Required]
        [Display(Name = "Категория рекламы")]
        public int AdCategoryId { get; set; }

        [Required]
        [Display(Name = "Название рекламы")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }
    }
}