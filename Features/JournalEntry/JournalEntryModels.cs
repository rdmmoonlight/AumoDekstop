
using System;
using System.Collections.Generic;

namespace AumoDekstop.Features.JournalEntry.Models;

public record JournalEntryLineDto(
    int AccountId,
    string? Description,
    decimal Debit,
    decimal Credit
);

public record JournalEntryDetailDto(
    int Id,
    string TransactionNumber,
    string JournalType,
    DateTime EntryDate,
    string? Description,
    List<JournalEntryLineDto> Lines
);

public record CreateJournalEntryRequest(
    string TransactionNumber,
    string JournalType,
    DateTime EntryDate,
    string? Description,
    List<JournalEntryLineDto> Lines
);

public record UpdateJournalEntryRequest(
    string TransactionNumber,
    string JournalType,
    DateTime EntryDate,
    string? Description,
    List<JournalEntryLineDto> Lines
);

public record SearchDescriptionsParams(string? Q = null);

public record NextTransactionNumberParams(
    string? JournalType = null,
    string? EntryDate = null
);