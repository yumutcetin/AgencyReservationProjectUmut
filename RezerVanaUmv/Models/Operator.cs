namespace RezerVanaUmv.Models
{
    public class Operator
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property (opsiyonel)
        public Tenant? Tenant { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
