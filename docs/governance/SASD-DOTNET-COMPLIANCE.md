# SASD .NET Compliance

## Baseline

C# / .NET 8, Nullable, `dotnet restore/build/test`, xUnit, Microsoft.Extensions Host/DI/Logging, Microsoft.Data.Sqlite.

## Planned Checks

- [ ] Nullable enabled
- [ ] Analyzer
- [ ] Release 0 Errors / Ziel 0 Warnings
- [ ] XML Docs für öffentliche fachliche APIs
- [ ] kein `async void` außer UI Events
- [ ] HttpClient Factory
- [ ] Cancellation für lange I/O
- [ ] keine Secrets in Config
- [ ] Dependency Review
- [ ] CI reproduzierbar
- [ ] Domain ohne WinForms/SQLite
