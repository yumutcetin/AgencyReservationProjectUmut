using RezerVanaUmv.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Passenger
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Gender { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? ReservationId { get; set; } // Yeni eklenen alan

    public string? BookingReference { get; set; }

    [ForeignKey("ReservationId")]
    public virtual Reservation? Reservation { get; set; } // Navigation property

    public virtual Tenant? Tenant { get; set; }
}
