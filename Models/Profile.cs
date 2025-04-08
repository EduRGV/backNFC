namespace NFC.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Position { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ProfileUrl { get; set; }
        public string? WebsiteUrl { get; set; }      
        public string? LinkedInUrl { get; set; }    
        public string? FacebookUrl { get; set; }     
        public string? PhoneNumber { get; set; }    
        public string? Email { get; set; }
    }

}
