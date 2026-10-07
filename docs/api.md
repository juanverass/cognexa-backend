# API

Todos os endpoints de dados exigem `Authorization: Bearer TOKEN`. Os IDs recebidos nos corpos são referências a recursos do usuário autenticado. Não há parâmetro livre de usuário para ler dados de outra conta. Recursos alheios retornam 404. Erros seguem ProblemDetails com `traceId`; detalhes internos de SQL/exceções não são retornados em produção.

Enums são representados por números nos corpos JSON (também aceitam nomes nos filtros de query). As listas possuem limite padrão 100, máximo 500. O histórico usa páginas de 100 registros em ordem cronológica. A busca semântica aceita no máximo 50 resultados.

| Rota | Métodos | Uso |
| --- | --- | --- |
| `/usuarios/me` | POST, GET, PUT, DELETE | Cadastro, consulta, nome e desativação |
| `/usuarios/me/preferencias` | GET, PUT | Meta diária em minutos e idioma |
| `/biblioteca/livros` | POST, GET | Cadastro e filtro `titulo`, `limite` |
| `/biblioteca/livros/{id}` | GET, PUT, DELETE | Livro e autores |
| `/biblioteca/leituras` | GET | Filtro por `status` |
| `/biblioteca/livros/{id}/leitura` | GET, PUT | Estado e página atual |
| `/biblioteca/livros/{id}/capitulos` | GET, POST | Capítulos ordenados |
| `/anotacoes` | POST, GET | Filtros `idLivro`, `idCapitulo`, `tipo`, `pagina`, `localizacao`, `limite` |
| `/anotacoes/{id}` | GET, PUT, DELETE | Anotação |
| `/conhecimento/aprendizados` | POST, GET | Filtros `idLivro`, `idConceito`, `idAnotacao`, `limite` |
| `/conhecimento/aprendizados/{id}` | GET, PUT, DELETE | Conteúdo e fontes |
| `/conhecimento/conceitos` | POST, GET | Conceitos |
| `/conhecimento/conceitos/{id}` | GET, PUT, DELETE | Conceito |
| `/conhecimento/aplicacoes` | POST, GET | Aplicações práticas; filtro `idAprendizado` |
| `/conhecimento/aplicacoes/{id}` | PUT, DELETE | Alterar conteúdo ou remover aplicação |
| `/conhecimento/relacoes` | POST, GET | Relações; filtro `idConceito` |
| `/conhecimento/relacoes/{id}` | DELETE | Remover relação |
| `/revisoes/perguntas` | POST | Criar pergunta e revisão devida |
| `/revisoes/perguntas/{id}` | GET | Pergunta e resposta esperada |
| `/revisoes/devidas` | GET | Filtros `idLivro`, `idConceito`, `limite` |
| `/revisoes/{id}` | GET | Próxima revisão e nível de domínio |
| `/revisoes/{id}/respostas` | POST | Resposta e autoavaliação |
| `/revisoes/{id}/historico` | GET | Histórico; filtro `pagina` |
| `/inteligencia/sugestoes` | POST | Sugestão com fontes, sem escrita |
| `/inteligencia/busca` | GET | `consulta`, `limite`; similaridade por embeddings |

## Fluxo mínimo

Use o token da identidade provisionada para cadastrar a conta; o `IdUsuario` retornado é gerado pelo domínio. IDs abaixo são substituídos pelos retornados em cada resposta.

```http
POST /usuarios/me
{"nome":"João"}

POST /biblioteca/livros
{"titulo":"Livro","autores":["Autor"],"totalDePaginas":100}

PUT /biblioteca/livros/ID_LIVRO/leitura
{"status":1,"paginaAtual":10}

POST /anotacoes
{"idLivro":"ID_LIVRO","idCapitulo":null,"tipo":1,"trechoOriginal":"Texto original","comentario":"Minha reflexão","pagina":10}

POST /conhecimento/conceitos
{"nome":"Tema"}

POST /conhecimento/aprendizados
{"conteudo":"Síntese pessoal","idAnotacoes":["ID_ANOTACAO"],"idConceitos":["ID_CONCEITO"],"origem":0}

POST /revisoes/perguntas
{"idAprendizado":"ID_APRENDIZADO","pergunta":"O que aprendi?","respostaEsperada":"Resposta","origem":0}

POST /revisoes/ID_REVISAO/respostas
{"resultado":2,"resposta":"Minha resposta"}
```

Status de leitura: 0 QueroLer, 1 Lendo, 2 Pausado, 3 Concluido, 4 Abandonado. Um livro cadastrado começa como QueroLer sem data de início. A conclusão com total conhecido exige a página final. Ao reabrir a leitura, a data de conclusão é removida, preservando a data inicial.

Tipos de anotação: 0 Destaque, 1 Insight, 2 Duvida, 3 Citacao, 4 Aplicacao. Trecho original e comentário são campos distintos. Anotações aceitam comentário sem trecho. O livro de uma fonte existente não é alterável.

Tipos de relação: 0 Complementa, 1 Contradiz, 2 Exemplifica, 3 DependeDe. Conceitos vinculados a aprendizados permitem conectar conhecimento de livros diferentes.

Resultado da revisão: 0 Errou, 1 Dificil, 2 Bom, 3 Facil. A revisão só aceita resposta quando devida. Cada resposta gera um histórico novo na mesma transação do reagendamento. Controle de concorrência evita duas respostas sobrescreverem o mesmo agendamento. Níveis: 0 Inicial, 1 EmAprendizado, 2 Consolidado, 3 Dominado.

Origem do conteúdo de aprendizado, aplicação ou pergunta: 0 Manual, 1 InteligenciaArtificial. Para aplicar uma sugestão, o usuário faz uma criação explícita com a origem 1 e referências válidas. A API de inteligência nunca aplica sugestões automaticamente.

As fontes de um aprendizado são preservadas. Remover uma anotação referenciada, um conceito em uso ou um aprendizado com aplicações/perguntas retorna 409. Para remover, desvincule/remova explicitamente os recursos dependentes que permitem exclusão. Histórico de revisão não tem endpoint de exclusão.


## Conteúdo protegido, OCR e publicação

Os endpoints e DTOs de captura, proveniência, elegibilidade, publicação, denúncia e retirada estão documentados na [política de conteúdo](conteudo-protegido.md). As novas ações seguem autenticação de conta ativa e os erros ProblemDetails existentes; consulta pública respeita o gate jurídico.
