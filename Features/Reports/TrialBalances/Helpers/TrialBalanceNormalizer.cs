
using System;
using System.Collections.Generic;
using System.Linq;
using AumoDekstop.Features.Reports.TrialBalances.Models;

namespace AumoDekstop.Features.Reports.TrialBalances.Helpers;

public static class TrialBalanceNormalizer
{
    public static List<TrialRow> NormalizeTrialRows(IEnumerable<RawTrialRow>? raw)
    {
        if (raw is null)
        {
            return [];
        }

        return raw.Select(r =>
        {
            var net = r.NetBalance ?? r.Amount ?? 0m;
            var debit = r.Debit ?? 0m;
            var credit = r.Credit ?? 0m;

            if (r.Debit is null && r.Credit is null)
            {
                if (r.NormalBalanceIsDebit)
                {
                    debit = net >= 0m ? net : 0m;
                    credit = net < 0m ? Math.Abs(net) : 0m;
                }
                else
                {
                    credit = net >= 0m ? net : 0m;
                    debit = net < 0m ? Math.Abs(net) : 0m;
                }
            }

            return new TrialRow
            {
                AccountId = r.AccountId,
                ReferenceNumber = r.ReferenceNumber,
                AccountName = r.AccountName,
                Type = r.Type,
                NormalBalanceIsDebit = r.NormalBalanceIsDebit,
                NetBalance = r.NetBalance,
                Debit = debit,
                Credit = credit,
                Amount = r.Amount
            };
        }).ToList();
    }
}