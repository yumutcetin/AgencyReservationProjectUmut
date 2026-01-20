using RezerVanaUmv.Models;

namespace RezerVanaUmv.ViewModels
{
    public class AgencyReservationsViewModel
    {
        public Agency agency { get; set; }          // Ajans bilgileri
        public int PendingPoints { get; set; }   // Reservations.Status == Confirmed
        public int GainedPoints { get; set; }   // Reservations.Status == Acquired
        public int SpentPoints { get; set; }   // Reservations.Status == Acquired
        public int SpentPendingPoints { get; set; }   // Reservations.Status == Acquired
        public int BonusPoints { get; set; }   // BalancePoints by UserId (mapped to agency)

        public int TotalPoints => GainedPoints + BonusPoints - SpentPoints; 
        public IEnumerable<ReservationWithUserViewModel> Reservations { get; set; } // Alt tarafta listeleyeceğiz
    }


}
