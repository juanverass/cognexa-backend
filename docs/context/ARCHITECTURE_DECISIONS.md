# Decisões arquiteturais

Decisões locais do Cognexa. Não importar decisões específicas do produto Dante. Nova decisão ganha ID; decisão substituída é marcada, não apagada.

## AD-01 — Arquitetura hexagonal

Status: vigente como requisito da epic #1.

Domain independente; Application define casos de uso e ports; Infrastructure implementa adapters; WebApi/Worker compõem dependências. Testes protegem a direção das referências.

## AD-02 — Identidade e idioma

Status: vigente como requisito de #1 e #9.

Entidades usam Guid Id gerado no domínio via EntidadeBase, setter protegido e FKs IdEntidade. Não há TId. Código novo de Domain/Application em PT-BR, mantendo sufixos técnicos definidos pelas issues.

## AD-03 — Persistência e composição

Status: vigente como requisito de #1 e #11.

EF Core/PostgreSQL somente na Infrastructure. Migrations acompanham entidades reais; não migrar no startup. Hosts não contêm regras de negócio. Composition roots usam AddApplication e AddInfrastructure.

## AD-04 — Workflow por issue executável

Status: vigente — solicitação humana e #15.

Uma issue executável, uma branch e uma PR. Epics agrupam tarefas. Claims e finalização são comentários na issue; review vive na PR. Dependências não mescladas podem formar PRs empilhadas quando autorizadas; a pilha deve ser declarada. Merge é humano.


## AD-05 — Conteúdo citado, captura transitória e fronteira pública

Status: vigente — requisitos #37–#42.

Trechos citados permanecem separados de comentários e carregam proveniência. OCR é um port da Application com provider na Infrastructure; imagens não são persistidas e somente uma seleção confirmada chega ao domínio. Capturas brutas expiram na memória do processo.

Controles de volume usam histórico sem texto, transação e advisory lock PostgreSQL por livro; exclusão de uma anotação não reinicia a quota. Publicação é explícita, tem validação própria e depende de gate jurídico fechado por padrão. Denúncia oculta conteúdo público; retirada impede reexposição. A auditoria preserva atribuição e estados, sem cópia do trecho. Esses controles de produto não definem limites jurídicos universais.
