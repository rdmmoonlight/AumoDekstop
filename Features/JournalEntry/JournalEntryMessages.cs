
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AumoDekstop.Features.JournalEntry.Messages;
/// <summary>
/// Dikirim setelah operasi Create, Edit, atau Delete Journal Entry berhasil.
/// ViewModel untuk Journal, Reports, dan Dashboard dapat mendengarkan pesan ini untuk refresh data.
/// </summary>
public sealed class JournalEntryChangedMessage : ValueChangedMessage<int?>
{
    public JournalEntryChangedMessage(int? journalEntryId = null) : base(journalEntryId)
    {
    }
}