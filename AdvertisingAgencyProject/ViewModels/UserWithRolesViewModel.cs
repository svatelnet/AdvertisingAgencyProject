using System.Collections.Generic;

namespace AdvertisingAgencyProject.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public string CurrentRole { get; set; } = string.Empty;
    }
}