using System;
using System.Text.Json.Serialization;

namespace AumoDekstop.Features.Periods.Models;

// Merujuk pada Period / PeriodItem (akan dilengkapi dengan properti dari periods.api.ts)
public partial class PeriodItem
{
    // Properti lengkap menyusul setelah melihat periods.api.ts
}

// Merujuk pada CreatePeriodPayload
public sealed record CreatePeriodPayload(
    [property: JsonPropertyName("startDate")] string StartDate,
    [property: JsonPropertyName("endDate")] string EndDate
);
