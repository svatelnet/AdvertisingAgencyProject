using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgencyProject.Models
{
    public class AdCategory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Категория рекламы")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Базовая цена")]
        public decimal BasePrice { get; set; }

        public ICollection<AdOrder> Orders { get; set; } = new List<AdOrder>();
    }
}