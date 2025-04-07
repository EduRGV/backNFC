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

    }

}
