using AdvertisingAgencyProject.Models;

namespace AdvertisingAgencyProject.ViewModels
{
    public class ClientDashboardViewModel
    {
        public Client Client { get; set; } = null!;
        public List<AdOrder> RecentOrders { get; set; } = new();
    }
}