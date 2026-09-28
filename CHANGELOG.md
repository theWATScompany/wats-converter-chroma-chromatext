# Changelog — ChromaTextConverter

## [1.0.0] — 2026-06-19

### Added
- Initial port from legacy `CromaConverters` (old repo, .NET 4.8 / MSTest).
- Upgraded to SDK-style project targeting `net8.0-windows;net48`.
- Namespace changed from `ChromaConverters` to `Virinco.WATS.Converter.Chroma`.
- Uses `Virinco.WATS.StandardConverters 7.*` (`TextConverterBase`).
- xUnit test project with `TextConverterTestBase` (4 test modes).
- Deferred: `ChromaImporter` (Access DB / OleDb prototype) — tracked separately.
