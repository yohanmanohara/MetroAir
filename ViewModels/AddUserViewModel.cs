namespace UserRoles.ViewModels
{
    public class AddUserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } // Store roles as a list of strings
    }
}
