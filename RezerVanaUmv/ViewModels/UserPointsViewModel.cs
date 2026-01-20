namespace RezerVanaUmv.ViewModels
{
    public class UserPointsViewModel
    {
        public dynamic User { get; set; }
        public decimal PendingPoints { get; set; }
        public decimal GainedPoints { get; set; }
        public decimal BonusPoints { get; set; }
        public decimal TotalPoints => GainedPoints + BonusPoints;
    }
}
