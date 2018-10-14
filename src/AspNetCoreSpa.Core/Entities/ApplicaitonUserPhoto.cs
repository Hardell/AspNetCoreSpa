namespace AspNetCoreSpa.Core.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUserPhoto : PhotoBase
    {
        public int ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
