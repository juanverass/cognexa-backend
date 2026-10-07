# Providers de inteligência

Application define `IInteligenciaProvider` e `IBuscaSemanticaProvider`. Infrastructure implementa dois adapters HTTP: APIs compatíveis com Chat Completions/embeddings e um protocolo de gateway customizado. A troca ocorre por configuração, preservando os contratos da Application e mantendo o domínio independente de modelos/SDKs.

A inferência real exige um serviço configurado. Sem BaseUrl/modelos, a API retorna 503. Os testes usam respostas controladas, sem chamadas pagas, e verificam isolamento, vetores, falhas, recusas, truncamento e rastreabilidade.

## API compatível

Configure um endpoint que suporte `POST chat/completions` com modo JSON e `POST embeddings`:

```bash
export Inteligencia__Protocolo='compativel'
export Inteligencia__BaseUrl='https://api.openai.com/v1/'
export Inteligencia__Token='CHAVE_DO_PROVIDER'
export Inteligencia__Modelo='MODELO_COM_SUPORTE_A_JSON'
export Inteligencia__ModeloDeVetores='MODELO_DE_EMBEDDINGS'
```

BaseUrl e modelos são configuráveis para usar outros serviços compatíveis. O adapter envia instruções para JSON no system message, `response_format={"type":"json_object"}` e `store=false`. Exige geração completa, recusa respostas truncadas/recusadas e valida o conteúdo e as fontes na Application. Os contratos seguem a [documentação oficial OpenAI de Chat Completions](https://developers.openai.com/api/reference/resources/chat/subresources/completions/methods/create) e [embeddings](https://developers.openai.com/api/reference/resources/embeddings/methods/create).

Textos longos são divididos em segmentos de até 2 mil caracteres e lotes de até 32 entradas. A agregação dos embeddings normalizados pondera o tamanho dos segmentos. O adapter valida e ordena os índices da resposta antes de associar vetores aos documentos, sem truncar o texto armazenado.

## Gateway customizado

Para um serviço com contrato próprio, configure:

```bash
export Inteligencia__Protocolo='gateway'
export Inteligencia__BaseUrl='https://SEU_GATEWAY/cognexa/'
export Inteligencia__Token='TOKEN_DO_GATEWAY'
```

A URL exige HTTPS; o token fica apenas na Infrastructure. Os dois adapters usam timeout de 60 segundos e repassam CancellationToken. O gateway implementa o protocolo abaixo e pode escolher seus próprios SDKs/modelos.

### Geração no gateway

O adapter faz `POST geracoes` relativo à BaseUrl, com JSON:

```json
{
  "operacao": "SugerirAprendizado",
  "fontes": [
    {"id":"UUID","tipo":"Anotacao","conteudo":"Trecho original: ...\nComentário do usuário: ...","idLivro":"UUID"}
  ],
  "instrucao": "Use apenas as fontes fornecidas; cite seus Ids. ..."
}
```

Resposta exigida:

```json
{"conteudo":"Sugestão gerada","idFontes":["UUID"]}
```

As fontes citadas precisam pertencer ao conjunto enviado. Conteúdo vazio, acima de 20 mil caracteres ou sem fontes válidas produz 503. O adapter não possui ferramentas para alterar o banco. As fontes são selecionadas com o usuário autenticado antes da chamada remota.

Operações da API de sugestões (enum numérico no JSON):

| Valor | Operação | Fontes |
| --- | --- | --- |
| 0 | SugerirAprendizado | Anotações selecionadas |
| 1 | SintetizarAnotacoes | Múltiplas anotações selecionadas |
| 2 | SugerirAplicacao | Aprendizados selecionados |
| 3 | GerarPerguntas | Aprendizados selecionados |
| 4 | DetectarIdeiasSemelhantes | Anotações/aprendizados selecionados |
| 5 | SugerirRelacoes | Conceitos/aprendizados selecionados |
| 6 | ResumirLivro | Anotações armazenadas do livro informado |
| 7 | ResumirCapitulo | Anotações armazenadas do capítulo informado |

```json
{
  "operacao":0,
  "idAnotacoes":["UUID"],
  "idAprendizados":[],
  "idConceitos":[],
  "idLivro":null,
  "idCapitulo":null
}
```

Nos resumos, as três seleções devem estar vazias e o livro/capítulo é obrigatório. O serviço busca exclusivamente as anotações desse recorte. Acima de 100 fontes, rejeita o pedido e pede um recorte menor; não omite fontes silenciosamente. Não usa fontes externas para completar o resumo.

A resposta da API inclui fontes, data de geração, `geradoPorIA=true` e `requerConfirmacao=true`. A confirmação é uma criação explícita em `/conhecimento/aprendizados`, `/conhecimento/aplicacoes` ou `/revisoes/perguntas` com `origem=1`. A sugestão não altera o texto original nem é aplicada automaticamente.

## Busca semântica

O adapter faz `POST vetores` com `{"textos":["consulta","documento 1","documento 2"]}`. O gateway deve retornar `{"vetores":[[0.1,0.2],[0.3,0.4],[0.5,0.6]]}` com um vetor por texto na mesma ordem.

Os vetores precisam ter a mesma dimensão (1–4096), valores finitos e norma não nula. O gateway deve usar o mesmo modelo/espaço vetorial para todos os lotes. A similaridade é o cosseno normalizado, de -1 a 1, e resultados são ordenados de forma decrescente.

A implementação inicial lê anotações e aprendizados **do usuário autenticado** em lotes de 100 do PostgreSQL, gera embeddings derivados e conserva os melhores resultados. Vetores não são entidades de domínio nem substituem os dados canônicos. Eles são reconstruídos por busca; isso prioriza consistência e simplicidade, com custo/latência proporcional ao tamanho da biblioteca. Cache ou índice vetorial poderão substituir este provider mantendo o contrato e o isolamento.
