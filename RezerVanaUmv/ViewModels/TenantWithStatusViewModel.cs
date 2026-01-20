namespace RezerVanaUmv.ViewModels
{
    public class TenantWithStatusViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; } // 🔥 Bu bilgi DavetKodu tablosundan gelir
    }
}
