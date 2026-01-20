using RezerVanaUmv.Models;

namespace RezerVanaUmv.ViewModels
{
    public class AgencyWithStatusViewModel
    {
        public Agency Agency { get; set; } = default!;
        public bool IsActive { get; set; }
        public string Email { get; set; } = "";

        public int PendingPoints { get; set; }   // Reservations.Status == Confirmed
        public int GainedPoints { get; set; }   // Reservations.Status == Acquired
        public int SpentPoints { get; set; }   // Reservations.Status == Acquired
        public int SpentPendingPoints { get; set; }   // Reservations.Status == Acquired
        public int BonusPoints { get; set; }   // BalancePoints by UserId (mapped to agency)

        public int TotalPoints => GainedPoints + BonusPoints - SpentPoints;
    }


}
