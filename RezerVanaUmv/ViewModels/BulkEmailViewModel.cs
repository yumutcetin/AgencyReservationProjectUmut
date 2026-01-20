using System.ComponentModel.DataAnnotations;

namespace RezerVanaUmv.ViewModels
{
    /// <summary>
    /// Toplu e-posta gönderim formu için ViewModel.
    /// - Alıcıları tenant geneli, seçili acenteler veya ekstra e-postalarla belirler.
    /// - E-posta içeriği HTML desteklidir.
    /// </summary>
    public class BulkEmailViewModel
    {
        [Required(ErrorMessage = "Konu zorunludur.")]
        [MaxLength(200)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "E-posta gövdesi zorunludur.")]
        public string? BodyHtml { get; set; }

        // Tek seçenek: Tenant’taki tüm kullanıcılar
        public bool SendToAllInTenant { get; set; }

        // İstersen ajansa göre gönderme ve ekstra mail yine kalsın:
        public List<string> SelectedAgencyIds { get; set; } = new();
        public string? AdditionalEmails { get; set; }

        public List<AgencyWithCountVM> Agencies { get; set; } = new();
        public int PreviewRecipientCount { get; set; }
    }


    /// <summary>Listelemede kullanılacak acente verisi.</summary>
    public class AgencyWithCountVM
    {
        /// <summary>Acente kimliği.</summary>
        [Required]
        public string Id { get; set; } = "";

        /// <summary>Acente adı (görünümde gösterilir).</summary>
        [Required]
        public string Name { get; set; } = "";

        /// <summary>Bu acenteye bağlı kullanıcı sayısı.</summary>
        public int UserCount { get; set; }

        /// <summary>Görünümde ön-seçim durumu.</summary>
        public bool IsSelected { get; set; }
    }
}
