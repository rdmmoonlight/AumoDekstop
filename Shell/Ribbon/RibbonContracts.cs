namespace AumoDekstop.Shell.Ribbon;

/// <summary>Id perintah yang dikirim tombol ribbon (diisi lewat atribut CommandId di XAML).</summary>
public static class RibbonCommand
{
    public const string New = "new";
    public const string Save = "save";
    public const string Sync = "sync";
    public const string CheckUpdate = "checkupdate";
    public const string RefreshPeriods = "periods.refresh";
    public const string AddCoa = "coa.add";
    public const string EditCoa = "coa.edit";
    public const string DeleteCoa = "coa.delete";
    public const string RefreshCoa = "coa.refresh";
    public const string ImportCoa = "coa.import";
    public const string Logout = "logout";
    public const string ComingSoon = "comingsoon";
}

/// <summary>Data tab ribbon yang baru dipilih.</summary>
public sealed record RibbonTabChangedEventArgs(string? Tag, string? Text);
