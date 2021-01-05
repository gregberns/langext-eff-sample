# LanguageExt Eff/Aff Example

## Purpose

POC/Demo of using the Eff/Aff system in [LanguageExt](https://github.com/louthy/language-ext). The goal is to build an end to end API + business layer + data layer, which connects to an actual database, to demonstrate how "to be opinionated about dealing with the world's side-effects." [Source](https://github.com/louthy/language-ext/issues/844#issuecomment-754621842)

## Run

```bash
# start the database
docker-compose up -d db

# wait a minute so the database starts up

# Run the tests
dotnet test
```
