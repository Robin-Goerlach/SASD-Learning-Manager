# Desktop Deployment Plan

## Plattform

Windows 11 x64 / .NET 8 / Single User.

## Pilot

`dotnet publish -c Release -r win-x64`.

Framework-dependent vs. self-contained anhand Paketgröße und Nutzerkomfort entscheiden.

## Daten

Binaries im Installationspfad, Userdaten unter `%LOCALAPPDATA%\SASD\LearningManager`.

## Installer

Vor 1.0 MSIX vs. WiX evaluieren. Kriterien: Upgrade, Uninstall ohne Userdatenverlust, CI, Signierung.

## Upgrade

Neue App → Schema Check → Safety Backup → Migration → Betrieb.

Downgrade nicht generell garantiert.
