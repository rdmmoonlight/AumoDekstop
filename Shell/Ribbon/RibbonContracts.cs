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

    // Laporan: nilainya harus sama dengan case di MainWindow.Commands.cs
    public const string UnadjustedTrialBalance = "UnadjustedTrialBalance";
    public const string AdjustedTrialBalance = "AdjustedTrialBalance";
    public const string PostClosingTrialBalance = "PostClosingTrialBalance";
    public const string Worksheet = "Worksheet";
    public const string IncomeStatement = "IncomeStatement";
    public const string RetainedEarnings = "RetainedEarnings";
    public const string CashFlow = "CashFlow";
    public const string FinancialPosition = "FinancialPosition";

    public const string Logout = "logout";
    public const string ComingSoon = "comingsoon";
}

/// <summary>Data tab ribbon yang baru dipilih.</summary>
public sealed record RibbonTabChangedEventArgs(string? Tag, string? Text);
