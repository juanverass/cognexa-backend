# Persistência

Infrastructure implementa IRepository e IUnitOfWork com EF Core/Npgsql. ConnectionStrings__Cognexa é configuração externa. /health/live verifica o host e /health/ready verifica PostgreSQL.

Migrations só surgem quando o modelo incluir entidades reais; não há migration de scaffold. Aplicação do schema é explícita, fora do startup. Testes usam COGNEXA_TEST_CONNECTION para PostgreSQL descartável, criando/removendo apenas schemas aleatórios próprios. Sem configuração, testes de integração ficam explicitamente ignorados.

```bash
dotnet tool restore
dotnet ef migrations add Nome --project src/Cognexa.Infrastructure --output-dir Persistencia/Migrations
dotnet ef database update --project src/Cognexa.Infrastructure
```
