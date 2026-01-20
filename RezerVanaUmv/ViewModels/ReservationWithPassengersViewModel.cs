using RezerVanaUmv.Models;

namespace RezerVanaUmv.ViewModels
{
    public class ReservationWithPassengersViewModel
    {
        public Reservation Reservation { get; set; }

        public List<PassengerInputModel> Passengers { get; set; } = new List<PassengerInputModel>();
        public int TotalAmount { get; set; }
    }

    public class PassengerInputModel
    {
        public int Id { get; set; }
        public string? Gender { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }

    }
}
