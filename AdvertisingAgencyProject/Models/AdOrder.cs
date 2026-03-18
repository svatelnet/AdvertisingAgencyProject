using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvertisingAgencyProject.Models
{
    public class AdOrder
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Клиент")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        [Display(Name = "Категория рекламы")]
        public int AdCategoryId { get; set; }
        public AdCategory? AdCategory { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Стоимость")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Статус")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Активна")]
        public bool IsActive { get; set; }
    }
}