namespace AumoDekstop.Features.ChartOfAccounts.Models;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Assets,
    Liabilities,
    Equity,
    OperatingIncome,
    OperatingExpenses,
    OtherIncome,
    OtherExpenses
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountRole
{
    Default,
    CashAndEquivalents,
    RetainedEarnings
}

public record AccountTypeOption(
    AccountType Value,
    string Label,
    int MinRange,
    int MaxRange
)
{
    public string RangeDisplay => $"{MinRange} - {MaxRange}";
}

public record AccountRoleOption(
    AccountRole Value,
    string Label
);

public static class ChartOfAccountsConstants
{
    public static readonly IReadOnlyList<AccountTypeOption> AccountTypeOptions = new List<AccountTypeOption>
    {
        new(AccountType.Assets, "Aset (Assets)", 100, 199),
        new(AccountType.Liabilities, "Liabilitas (Liabilities)", 200, 299),
        new(AccountType.Equity, "Ekuitas (Equity)", 300, 399),
        new(AccountType.OperatingIncome, "Pendapatan Usaha (Operating Income)", 400, 499),
        new(AccountType.OperatingExpenses, "Beban Usaha (Operating Expenses)", 500, 599),
        new(AccountType.OtherIncome, "Pendapatan Lain (Other Income)", 600, 799),
        new(AccountType.OtherExpenses, "Beban Lain (Other Expenses)", 800, 999),
    };

    public static readonly IReadOnlyList<AccountRoleOption> AccountRoleOptions = new List<AccountRoleOption>
    {
        new(AccountRole.Default, "Umum (Default)"),
        new(AccountRole.CashAndEquivalents, "Kas dan Setara Kas"),
        new(AccountRole.RetainedEarnings, "Laba Ditahan"),
    };

    public static string GetAccountTypeLabel(string type)
    {
        if (Enum.TryParse<AccountType>(type, ignoreCase: true, out var parsed))
        {
            return AccountTypeOptions.FirstOrDefault(o => o.Value == parsed)?.Label ?? type;
        }
        return type;
    }

    public static string GetAccountRoleLabel(string role)
    {
        if (Enum.TryParse<AccountRole>(role, ignoreCase: true, out var parsed))
        {
            return AccountRoleOptions.FirstOrDefault(o => o.Value == parsed)?.Label ?? role;
        }
        return role;
    }
}

/// <summary>
/// Satu baris akun, sesuai AccountItemDto.
/// </summary>
public class Account
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    // Properti bantuan untuk mempermudah x:Bind di XAML WinUI 3
    [JsonIgnore]
    public string TypeLabel => ChartOfAccountsConstants.GetAccountTypeLabel(Type);

    [JsonIgnore]
    public string RoleLabel => ChartOfAccountsConstants.GetAccountRoleLabel(Role);
}

/// <summary>
/// Bentuk respons GET /api/v1/chart-of-accounts, sesuai ChartOfAccountsListResponseDto.
/// </summary>
public class ChartOfAccountsList
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("accounts")]
    public List<Account> Accounts { get; set; } = new();
}

/// <summary>
/// Nilai yang dikirim oleh form dialog (Create / Update).
/// </summary>
public class AccountFormValues
{
    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public AccountType Type { get; set; } = AccountType.Assets;

    [JsonPropertyName("role")]
    public AccountRole Role { get; set; } = AccountRole.Default;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}