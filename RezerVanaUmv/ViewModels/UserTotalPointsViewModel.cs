using RezerVanaUmv.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RezerVanaUmv.ViewModels
{
    public class UserTotalPointsViewModel
    {
        // 🔢 Puan bilgileri
        public List<TenantPointsSummary> TenantPoints { get; set; } = new();
        public List<RewardCatalogRecord> RewardCatalogs { get; set; }
        public List<RedemptionRecord> RedemptionRecords { get; set; }
    }

    public class EditPointsViewModel
    {
        // 🔢 Puan bilgileri
        public TenantPointsSummary TenantPoints { get; set; } = new();
        public Agency agency { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }
    }

    public class TenantPointsSummary
    {
        public int TenantId { get; set; }
        public string? HotelName { get; set; }
        public decimal PointsEarned { get; set; }
        public decimal BonusPoints { get; set; }
        public decimal PointsPending { get; set; }
        public decimal PointsSpent { get; set; }
        public decimal PointsSpentPending { get; set; }
        public decimal PointsSpendable => PointsEarned + BonusPoints - PointsSpent - PointsSpentPending;
    }


    // 🎁 Kazanılabilecek ödül bilgileri
    public class RewardCatalogRecord
    {
        public string HotelName { get; set; }
        public string RoomType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RequiredPoints { get; set; }
    }

    // 💸 Harcanmış ödül talepleri
    public class RedemptionRecord
    {
        public string HotelName { get; set; }
        public string RoomType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RequiredPoints { get; set; }
    }
}
