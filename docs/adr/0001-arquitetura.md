# ADR 0001 — Arquitetura hexagonal

Status: aceita.

Domain representa regras e identidade sem dependências externas. Application organiza casos de uso e define ports. Infrastructure implementa persistência e providers. WebApi e Worker são composition roots.

Referências: Application → Domain; Infrastructure → Application + Domain; WebApi/Worker → Application + Infrastructure. Testes de arquitetura verificam o grafo e impedem EF/Npgsql na Application e qualquer pacote no Domain.

Entidades herdam EntidadeBase com Guid Id gerado no domínio e setter protegido. FKs seguem IdEntidade; não há TId. Domain/Application usam PT-BR; Repository, AppService, Dto, SearchDto, DbContext, WebApi e Worker são sufixos técnicos permitidos.

Features agrupam entidades, contratos, DTOs, mappings e AppServices em suas respectivas camadas. IRepository<TEntity> é a porta mínima; repositories específicos acrescentam consultas próprias. AppServices controlam transações via IUnitOfWork e usam CancellationToken. CrudBasicoAppService exige filtros por identidade, inclusive nas operações por Id.

Mapster converte modelos em DTOs; criação e alteração passam por métodos do domínio. Mappings ficam na feature, registrados pelo scan de AddApplication. Nunca mapear um DTO de entrada sobre uma entidade existente.

Os hosts registram dependências e rotas; regras de negócio ficam no Domain/Application. Dados de configuração e credenciais são externos.

Migrations pertencem à Infrastructure e só surgem a partir de entidades persistentes reais. Exclusões de fontes são restringidas quando há conhecimento derivado. Migração do banco é uma ação explícita de operação, nunca automática no startup.

Testes: Domain para invariantes; Application para autorização, casos de uso e ports; Infrastructure contra PostgreSQL descartável; WebApi para contratos HTTP; Architecture para dependências e nomenclatura.

Para criar uma feature: definir entidades e invariantes; definir port/DTO e AppService com isolamento de usuário; criar mapping e configuração EF; registrar via composition root; criar endpoints finos; adicionar testes e migration quando necessária.
