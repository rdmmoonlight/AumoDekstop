# Fase 2 — Komponen Bersama (`Features/Shared`)

Tujuan: satu sumber untuk kontrol, dialog, format, dan gaya yang kini tersebar di banyak fitur.
Prasyarat: Fase 1 selesai (`Features/Shared/Http` sudah ada).
Rujukan: `docs/PETA-APLIKASI-UTUH.md` (T10, K4, K5).

## Keadaan Sekarang

Folder `Shared` tidak ada di repo (Git tidak mencatat folder kosong). Komponen yang sama ada di beberapa tempat:

| Jenis | Salinan yang ada | Baris XAML |
| ----- | ---------------- | ---------- |
| Kartu "belum ada periode" | `Reports/FinancialStatements/Controls/NoPeriodCardControl` | 91 |
| | `Reports/TrialBalances/Controls/NoPeriodCardControl` | 72 |
| | `Reports/Journals/States/NoPeriodState` | 46 |
| | `Reports/GeneralLedgers/States/GeneralLedgersNoPeriodState` | 68 |
| Status memuat | `Reports/FinancialStatements/Controls/LoadingStateControl` | 46 |
| Pita pesan | `Reports/FinancialStatements/Controls/ErrorAlertControl` | 84 |
| | `Reports/TrialBalances/Controls/BalanceAlertControl` | 40 |
| | `Periods/Controls/PeriodsAlertsControl` | 48 |
| | `JournalEntry/Controls/JournalAlertsControl` | 41 |
| Dialog hapus | `ChartOfAccounts/Dialogs/DeleteAccountDialog.cs`, `Reports/Journals/Dialogs/DeleteEntryDialog` | — |
| Format angka dan tanggal | `FinancialStatementFormatter`, `TrialBalanceFormatter`, `PeriodFormatter`, `JournalEntriesHelpers` | — |
| Warna lokal | Resource `Gray50HalfBrush`, `Gray100Brush`, `Gray300Brush`, dan sejenisnya didefinisikan di dalam kontrol, padahal ada `Styles/AppColors.xaml` | — |

Kontrol `NoPeriodCardControl` di FinancialStatements dan TrialBalances memiliki isi yang hampir sama. Perbedaannya hanya `x:Class` dan blok `UserControl.Resources` palet warna.

## Struktur Target

```text
Features/Shared/
├── Http/              # sudah dibuat di Fase 1
├── Formatting/
│   ├── CurrencyFormatter.cs     # Rupiah
│   ├── NumberFormatter.cs       # angka akuntansi, negatif dalam kurung
│   └── DateFormatter.cs         # tanggal dan nama periode
├── Controls/
│   ├── StateCard.xaml(.cs)      # kartu keadaan: tanpa periode, kosong, galat
│   ├── LoadingState.xaml(.cs)   # indikator memuat
│   ├── AlertBanner.xaml(.cs)    # pita pesan: galat, peringatan, info, sukses
│   └── PageHeader.xaml(.cs)     # judul halaman + tombol aksi
├── Dialogs/
│   └── ConfirmDialog.cs         # konfirmasi umum (hapus, tutup periode)
├── Models/
│   └── LoadState.cs             # Idle, Loading, Loaded, Empty, NoPeriod, Error
└── Styles/
    └── SharedStyles.xaml        # gaya kartu, tabel, tombol
```

Aturan:

- Namespace `AumoDekstop.Features.Shared.<Subfolder>`, ditulis paling atas.
- `Features/Shared` tidak boleh merujuk `Features/<fitur>` mana pun (K5).
- Model berupa `class` dengan `{ get; set; }`.
- Warna memakai `ThemeResource` dari `Styles/AppColors.xaml`. Tidak ada palet lokal di dalam kontrol.
- Kontrol bersama menerima data lewat `DependencyProperty`, bukan lewat ViewModel fitur.

## Langkah

Urutan dari risiko terendah. Satu langkah = satu commit. Setiap langkah dua tahap: buat komponen bersama, pindahkan pemakai satu fitur per commit, lalu hapus salinan lama setelah pemakai terakhir pindah.

### 2.1 Pemformat (Formatting)

- [ ] Buat `CurrencyFormatter.Format(decimal?)` dan `FormatPercent(decimal?)`. Ambil perilaku dari `FinancialStatementFormatter.FormatCurrency` dan `FormatPercent`.
- [ ] Buat `NumberFormatter.Format(decimal?)`. Ambil perilaku dari `TrialBalanceFormatter.FormatNumber`. Satu aturan untuk nol, negatif, dan pemisah ribuan.
- [ ] Buat `DateFormatter.FormatDate(string?)` dan `FormatPeriod(string?)`. Ambil dari `JournalEntriesHelpers.FormatDate` dan `PeriodFormatter.FormatPeriodDate`.
- [ ] Sebelum menghapus yang lama, tulis uji sederhana: nilai `null`, `0`, negatif, dan angka besar menghasilkan keluaran identik dengan formatter lama. Bila ada beda, putuskan satu format dan catat di PR.
- [ ] Ganti pemanggil per fitur (FinancialStatements → TrialBalances → Periods → JournalEntry).
- [ ] Hapus `FinancialStatementFormatter`, `TrialBalanceFormatter`, `PeriodFormatter`, dan fungsi format di `JournalEntriesHelpers`. Pertahankan `CalculateTotals` di fitur JournalEntry.

### 2.2 Gaya dan warna

- [ ] Buat `Styles/SharedStyles.xaml`. Daftarkan di `App.xaml` (`MergedDictionaries`) setelah `AppColors.xaml`.
- [ ] Pindahkan brush lokal yang berulang (`Gray50HalfBrush`, `Gray100Brush`, `Gray300Brush`, dan sejenisnya) ke `AppColors.xaml` dengan nama dan kamus tema yang jelas (terang dan gelap).
- [ ] Hapus blok `UserControl.Resources` yang hanya menyalin palet.
- [ ] Tampilan harus sama pada tema Dark dan Light. Aplikasi memakai Dark sebagai bawaan.

### 2.3 `LoadState` dan `LoadingState`

- [ ] Buat enum `LoadState` (`Idle`, `Loading`, `Loaded`, `Empty`, `NoPeriod`, `Error`).
- [ ] Buat `LoadingState` dengan properti `Message`. Ambil tampilan dari `LoadingStateControl`.
- [ ] Ganti pemakai di empat halaman laporan keuangan. Hapus `LoadingStateControl`.

### 2.4 `AlertBanner`

- [ ] Properti: `Severity` (`Error`, `Warning`, `Info`, `Success`), `Title`, `Message`, `IsOpen`, `ActionText`, `ActionCommand`.
- [ ] Ganti `ErrorAlertControl`, `BalanceAlertControl`, `PeriodsAlertsControl`, `JournalAlertsControl`.
- [ ] Khusus `BalanceAlertControl` (neraca saldo seimbang atau tidak): isi `Severity` dari ViewModel. Logika selisih tetap di ViewModel neraca saldo, bukan di kontrol bersama.

### 2.5 `StateCard` (menggantikan empat kartu kembar)

- [ ] Properti: `Glyph`, `Title`, `Message`, `ActionText`, `ActionCommand`.
- [ ] Tiga pemakaian: tanpa periode ("Belum ada periode aktif", aksi "Buka Periods"), data kosong, galat dengan tombol "Coba lagi".
- [ ] Ganti `NoPeriodCardControl` (2), `NoPeriodState`, `GeneralLedgersNoPeriodState`, dan `JournalEmptyState`.
- [ ] Aksi "Buka Periods" memakai pesan antar-komponen (lihat Fase 3, butir 3.5), bukan referensi ke `MainWindow`.

### 2.6 `ConfirmDialog`

- [ ] Satu dialog: `Title`, `Content`, `PrimaryText`, `CloseText`, dan `IsDestructive` (gaya tombol bahaya).
- [ ] Ganti `DeleteAccountDialog` dan `DeleteEntryDialog`. Pemakai baru berikutnya (tutup periode) langsung memakai ini.
- [ ] `XamlRoot` diisi oleh pemanggil.

### 2.7 `PageHeader`

- [ ] Properti: `Title`, `Subtitle`, dan satu `ContentPresenter` untuk tombol aksi.
- [ ] Ganti `GeneralLedgersPageHeader`. Pakai di halaman baru Fase 3.

### 2.8 Pembersihan akhir

- [ ] `grep -rn "NoPeriodCardControl\|NoPeriodState\|LoadingStateControl\|ErrorAlertControl\|BalanceAlertControl\|PeriodsAlertsControl\|JournalAlertsControl\|DeleteAccountDialog\|DeleteEntryDialog\|FinancialStatementFormatter\|TrialBalanceFormatter\|PeriodFormatter" --include=*.cs --include=*.xaml .` tidak menemukan apa pun.
- [ ] `grep -rn "using AumoDekstop.Features\." Features/Shared` tidak menemukan referensi ke fitur lain (K5).
- [ ] Setiap `DataTemplate` berpemakai `{x:Bind}` memiliki `x:DataType`.

## Pemetaan Lama → Baru

| Lama | Baru |
| ---- | ---- |
| `FinancialStatementFormatter.FormatCurrency/FormatPercent` | `CurrencyFormatter` |
| `TrialBalanceFormatter.FormatNumber` | `NumberFormatter` |
| `PeriodFormatter.FormatPeriodDate`, `JournalEntriesHelpers.FormatDate` | `DateFormatter` |
| `JournalEntriesHelpers.FormatCurrency` | `CurrencyFormatter` |
| `LoadingStateControl` | `LoadingState` |
| `ErrorAlertControl`, `BalanceAlertControl`, `PeriodsAlertsControl`, `JournalAlertsControl` | `AlertBanner` |
| `NoPeriodCardControl` ×2, `NoPeriodState`, `GeneralLedgersNoPeriodState`, `JournalEmptyState` | `StateCard` |
| `DeleteAccountDialog`, `DeleteEntryDialog` | `ConfirmDialog` |
| `GeneralLedgersPageHeader` | `PageHeader` |

## Pola Halaman Setelah Fase 2

Setiap halaman laporan memakai satu `LoadState` pada ViewModel dan menampilkan satu keadaan:

| `LoadState` | Tampilan |
| ----------- | -------- |
| `Loading` | `LoadingState` |
| `NoPeriod` | `StateCard` (tanpa periode) |
| `Empty` | `StateCard` (data kosong) |
| `Error` | `AlertBanner` + tombol "Coba lagi" |
| `Loaded` | Tabel fitur |

## Selesai Fase 2 Bila

- [ ] Semua butir 2.1–2.8 tercentang.
- [ ] Tabel pemetaan: tidak ada kelas "Lama" yang tersisa.
- [ ] Tampilan halaman lama tidak berubah selain yang sengaja diseragamkan (catat di PR).
- [ ] Build hijau. Semua halaman lama dibuka satu per satu pada tema Dark dan Light.
