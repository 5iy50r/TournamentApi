# TournamentApi

API GraphQL (HotChocolate) dla prostego turnieju pucharowego (.NET 8).

## Start
- Otwórz `TournamentApi.sln` w Visual Studio 2022
- Uruchom (F5)
- Endpoint GraphQL: `/graphql`

## Baza danych
EF Core + SQLite, migracje zawarte w repo. Plik bazy (`tournament.db`) nie jest wersjonowany.
Po sklonowaniu projektu utwórz bazę przez `Update-Database` (Package Manager Console).
