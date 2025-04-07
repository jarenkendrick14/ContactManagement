namespace ContactApi.Models
{
    public class Contact
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = string.Empty; // Use defaults for non-nullable strings
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; } // Nullable string
        public string? Phone { get; set; }
    }
}