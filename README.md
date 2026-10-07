# Cognexa Backend

Backend .NET 10 independente, com arquitetura hexagonal. Domain define regras; Application define ports/casos de uso; Infrastructure implementa persistência; WebApi e Worker são composition roots.

```bash
dotnet restore Cognexa.sln
dotnet build Cognexa.sln --configuration Release
dotnet test Cognexa.sln --configuration Release
```

Requisitos: SDK .NET 10 e PostgreSQL 17 ou superior para integrações. global.json seleciona o SDK. Configure ConnectionStrings__Cognexa externamente e inicie `dotnet run --project src/Cognexa.WebApi` ou `dotnet run --project src/Cognexa.Worker`. O host Worker ainda não executa jobs de negócio.

Para integração, configure COGNEXA_TEST_CONNECTION apontando para PostgreSQL descartável; cada fixture remove somente seu schema. Integrações sem banco ficam explicitamente ignoradas.

- [Arquitetura e convenções](docs/adr/0001-arquitetura.md)
- [Persistência e migrations](docs/persistencia.md)
- [Contrato de agentes](docs/development/agent-contract.md)
- [Estado do código](docs/context/CURRENT_STATE.md)
