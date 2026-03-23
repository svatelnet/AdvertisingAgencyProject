using AdvertisingAgencyProject.Models;

namespace AdvertisingAgencyProject.Services
{
    public static class AdOrderWorkflowService
    {
        public static decimal CalculateTotalPrice(DateTime startDate, DateTime endDate, decimal basePrice)
        {
            var days = (endDate.Date - startDate.Date).Days + 1;
            return basePrice * days;
        }

        public static void ApplyManagerChanges(AdOrder order, decimal basePrice, DateTime? today = null)
        {
            order.TotalPrice = CalculateTotalPrice(order.StartDate, order.EndDate, basePrice);

            if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
            {
                order.Status = OrderStatus.Approved;
            }

            RefreshStatus(order, today);
        }

        public static void ApplyClientRequest(AdOrder order, decimal basePrice)
        {
            order.TotalPrice = CalculateTotalPrice(order.StartDate, order.EndDate, basePrice);
            order.Status = OrderStatus.Submitted;
            order.IsActive = false;
        }

        public static bool RefreshStatus(AdOrder order, DateTime? today = null)
        {
            var currentDate = (today ?? DateTime.Today).Date;
            var oldStatus = order.Status;
            var oldIsActive = order.IsActive;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                order.IsActive = false;
            }
            else if (order.Status == OrderStatus.Submitted)
            {
                order.IsActive = false;
            }
            else if (order.EndDate.Date < currentDate)
            {
                order.Status = OrderStatus.Expired;
                order.IsActive = false;
            }
            else if (order.StartDate.Date <= currentDate && order.EndDate.Date >= currentDate)
            {
                order.Status = OrderStatus.Active;
                order.IsActive = true;
            }
            else
            {
                order.Status = OrderStatus.Approved;
                order.IsActive = false;
            }

            return oldStatus != order.Status || oldIsActive != order.IsActive;
        }

        public static string ToDisplayText(OrderStatus status) => status switch
        {
            OrderStatus.Draft => "Черновик",
            OrderStatus.Submitted => "На рассмотрении",
            OrderStatus.Approved => "Подтвержден",
            OrderStatus.Active => "Активен",
            OrderStatus.Completed => "Завершен",
            OrderStatus.Cancelled => "Отменен",
            OrderStatus.Expired => "Истек",
            _ => status.ToString()
        };

        public static string ToBadgeClass(OrderStatus status) => status switch
        {
            OrderStatus.Submitted => "bg-info text-dark",
            OrderStatus.Approved => "bg-primary",
            OrderStatus.Active => "bg-success",
            OrderStatus.Completed => "bg-secondary",
            OrderStatus.Cancelled => "bg-danger",
            OrderStatus.Expired => "bg-dark",
            _ => "bg-light text-dark"
        };
    }
}