namespace RezerVanaUmv.ViewModels;

public class EditUserViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string NewPassword { get; set; }

    public string AgencyName { get; set; }
    public string AgencyCountry { get; set; }

    public bool IsActive { get; set; }

    public int? DavetKoduId { get; set; }
    public string DavetKodu { get; set; }

    public int? ToplamKazanilanPuan { get; set; }

}
