# Estrategia De IA Para Os Modulos De Produto Do Querify

Data de referencia: 2026-05-10, America/Vancouver.

Este documento sugere o melhor caminho de produto e arquitetura para implementar IA nos quatro
modulos de produto descritos em [`value_proposition.md`](value_proposition.md): QnA, Direct,
Broadcast e Trust. Tenant fica fora do escopo funcional porque e controle de plataforma, mas
continua sendo pre-requisito para identidade, acesso, entitlement, roteamento de dados e isolamento
multi-tenant.

## Decisao Recomendada

Implementar IA como uma **capacidade transversal de orquestracao, busca, geracao, classificacao,
avaliacao e seguranca**, nao como um sexto modulo de produto.

O estado de negocio continua pertencendo aos modulos:

| Estado ou fluxo | Dono |
| --- | --- |
| Perguntas, respostas, fontes, chunks, embeddings, tags, versoes e lacunas aceitas para curadoria | QnA |
| Conversas 1:1, mensagens, contexto privado, sugestoes de resposta e handoff | Direct |
| Threads publicas, itens capturados, agrupamentos sociais, resposta publica e sinais comunitarios | Broadcast |
| Validacao, decisao, voto, contestacao, racional, politica e auditoria | Trust |
| Execucao operacional de prompts, modelo usado, custo, latencia, versao de prompt e traces tecnicos | Infraestrutura de IA, sem virar fonte de verdade de produto |

A recomendacao pratica e:

1. Construir primeiro a fundacao de conhecimento no QnA: ingestao, normalizacao, busca hibrida,
   citacoes, geracao de rascunhos e avaliacoes.
2. Depois criar copilotos humanos para Direct e Broadcast, usando QnA como fonte confiavel.
3. Em seguida implementar Trust como camada de aprovacao, validacao e auditoria para respostas,
   politicas e decisoes sensiveis.
4. Somente depois automatizar respostas de baixo risco, com limites de confianca, politicas,
   logs, rollback e revisao humana.

## Por Que Esse Caminho

O valor do Querify depende de transformar interacoes em conhecimento reutilizavel. Se a primeira
implementacao for um chatbot generico, o produto ganha uma demonstracao rapida, mas nao ganha
memoria operacional confiavel. QnA precisa ser a fundacao porque Direct, Broadcast e Trust dependem
de respostas com fonte, versao, visibilidade e status editorial.

Esse caminho tambem respeita a arquitetura atual do repositorio:

- QnA ja possui APIs, business projects, worker, dominio, persistencia, fontes e upload de fontes.
- Direct e Broadcast ja possuem fronteiras de persistencia, mas ainda precisam de APIs, business
  workflows e workers antes de automacao rica por IA.
- Trust ja possui fronteira de DbContext, mas ainda precisa de entidades de decisao, validacao e
  auditoria antes de IA de governanca.
- O desenho futuro de MCP ja aponta para agentes por modulo; isso e util como interface para
  assistentes, mas o runtime de produto deve continuar chamando services, commands e queries do
  backend diretamente.

O mercado atual tambem favorece esse desenho. As APIs modernas de IA suportam structured outputs,
tool/function calling, retrieval, agents, guardrails e evaluacoes, mas continuam probabilisticas. O
produto deve tratar modelos como motores de sugestao e automacao controlada, nao como fonte final de
verdade.

## Principios De Arquitetura

1. **IA nao e modulo de produto.** Ela nao deve possuir pergunta, conversa, thread ou decisao.
2. **QnA e a memoria confiavel.** Direct e Broadcast podem consultar e sugerir melhorias, mas nao
   duplicam resposta canonica.
3. **Tudo que a IA gerar entra como rascunho, sugestao ou evidencia.** Publicacao, envio automatico
   e decisao exigem politica explicita.
4. **Busca antes de geracao.** Respostas devem partir de fontes recuperadas, com citacao e versao.
5. **Structured output por padrao.** Saidas de IA devem obedecer JSON Schema ou DTO equivalente
   antes de virarem comando.
6. **Ferramentas com permissao minima.** Um agente so pode chamar ferramentas do modulo e do caso de
   uso atual.
7. **Avaliacoes antes de escala.** Mudanca de modelo, prompt, chunking ou reranking precisa passar
   por dataset de avaliacao.
8. **Human-in-the-loop primeiro.** Automacao direta so depois de metricas estaveis em baixo risco.
9. **Conteudo externo e sempre nao confiavel.** Comentarios, documentos, HTML, PDFs e mensagens
   podem conter prompt injection.
10. **Observabilidade e requisito de produto.** Cada resposta assistida por IA precisa ter modelo,
    prompt version, fontes, custo, latencia, confianca e decisao tomada.

## Capacidades Comuns De IA

Estas capacidades devem ficar em infraestrutura compartilhada e serem consumidas pelos modulos:

| Capacidade | Uso | Observacao arquitetural |
| --- | --- | --- |
| Provider abstraction | Trocar OpenAI, Anthropic, Azure OpenAI ou outro provedor sem reescrever features | Criar contratos como `IAiTextClient`, `IAiEmbeddingClient`, `IAiSafetyClient` e `IAiToolRunner`. |
| Prompt registry | Versionar prompts por caso de uso, idioma, modulo e risco | Prompt version deve aparecer em telemetry e metadata. |
| Structured output validator | Validar JSON Schema/DTO antes de chamar command | Falha de validacao vira retry controlado ou handoff. |
| Retrieval service | Busca hibrida, filtros por tenant, visibilidade, fonte e status | O indice de conhecimento pertence ao QnA. |
| Reranking | Ordenar evidencias por relevancia e confiabilidade | Pode ser modelo pequeno, cross-encoder externo ou scoring deterministico inicial. |
| Safety classifier | Classificar risco, PII, abuso, politica sensivel e automacao permitida | Resultado roteia para Direct, Broadcast ou Trust. |
| Evaluation harness | Testar prompts, retrieval, respostas e agents antes de deploy | Deve rodar em CI ou pipeline manual antes de promover automacao. |
| AI run telemetry | Registrar modelo, tokens, custo, latencia, tool calls, erros e trace id | Operacional; nao substitui historico do modulo dono. |

## Estrategia De Modelo E Provedor

Recomendacao: comecar com uma arquitetura provider-agnostic e usar um provedor principal moderno
para acelerar o produto. OpenAI e uma boa opcao padrao hoje porque a plataforma atual combina
Responses API, structured outputs, tool/function calling, file search/retrieval, Agents SDK,
guardrails e Evals em uma mesma superficie. Anthropic, Azure OpenAI ou outros provedores podem ser
mantidos como adapters quando houver requisito de custo, residencia, cliente enterprise ou qualidade
por idioma.

Padrao recomendado por tipo de tarefa:

| Tarefa | Modelo recomendado |
| --- | --- |
| Classificacao, roteamento, idioma, sentimento, deduplicacao simples | Modelo pequeno/rapido e barato |
| Extracao estruturada de fontes e mensagens | Modelo medio com structured output |
| Resposta ao usuario com contexto recuperado | Modelo medio ou frontier, dependendo do risco |
| Decisao de ferramenta, multi-step reasoning e analise de evidencias | Modelo frontier com tool calling controlado |
| Embeddings | Modelo de embedding dedicado, separado do modelo gerativo |
| Re-ranking | Comecar com scoring hibrido; evoluir para reranker/modelo quando o corpus justificar |

Nao recomendo fine-tuning no inicio. Para Querify, o ganho inicial vem de RAG bem feito, fontes
confiaveis, prompts versionados, structured outputs, guardrails e avaliacoes. Fine-tuning so deve
entrar depois que existirem dados reais revisados e um gargalo claro que prompt/RAG nao resolve,
como estilo de marca em escala ou classificacoes muito repetitivas.

## Busca E RAG

O QnA precisa de busca hibrida como fundacao. Apenas busca vetorial nao e suficiente para produto de
conhecimento porque perguntas reais misturam termos exatos, codigos, nomes de planos, mensagens de
erro, sinonimos e linguagem natural.

Recomendacao de retrieval:

1. Ingestao de fonte no QnA.
2. Extracao de texto por tipo de midia.
3. Normalizacao, idioma, metadados e checksum.
4. Chunking com preservacao de estrutura: titulo, secao, pagina, URL, timestamp ou trecho de
   conversa.
5. Embeddings por chunk.
6. Indice lexical mais indice vetorial.
7. Filtros obrigatorios: tenant, visibilidade, status, space, idioma, fonte, audiencia e validade.
8. Re-ranking dos candidatos.
9. Resposta com citacoes e lista de evidencias usadas.
10. Feedback de utilidade e correcao voltando para QnA Activity.

Implementacao sugerida:

- MVP: PostgreSQL com `pgvector` quando a operacao quiser simplicidade, junto de busca textual do
  proprio Postgres.
- Escala enterprise: Azure AI Search, OpenSearch, Pinecone, Qdrant ou servico equivalente quando
  houver grande volume, filtros complexos, multi-regiao ou necessidade operacional dedicada.
- Produto: manter uma interface `IQnARetrievalService`; a escolha do backend de indice nao deve
  vazar para Direct, Broadcast ou Trust.

## Requisitos Por Modulo

### QnA

Objetivo: transformar fonte e interacao recorrente em conhecimento confiavel.

Capacidades de IA:

- Gerar perguntas e respostas em rascunho a partir de fontes, uploads, URLs, documentos e
  transcricoes.
- Detectar duplicidade sem persistir "duplicidade" como estado canonico: o processo de criacao
  sugere merge, link ou nova pergunta.
- Sugerir tags, espaco, idioma, visibilidade e audiencia.
- Criar resumo, resposta curta, resposta longa, variantes por canal e referencias.
- Identificar lacunas de cobertura vindas de Direct e Broadcast.
- Validar se uma resposta esta suportada pelas fontes citadas.
- Sugerir atualizacao quando uma fonte muda ou fica obsoleta.
- Traduzir/adaptar resposta por mercado sem perder origem e versao.

Requisitos de estado:

- `SourceChunk` ou equivalente para chunks indexaveis derivados de `Source`.
- Embeddings por chunk, com modelo, dimensao, checksum e data de geracao.
- Metadata de geracao em perguntas/respostas: modelo, prompt version, confidence score, evidencias
  e run id.
- Status editorial sempre iniciando como `Draft` e `Internal` para conteudo gerado por IA.
- Activity para feedback, utilidade, erro reportado e resultado de revisao.

MVP recomendado:

1. `Querify.QnA.Portal.Business.Search` para busca hibrida em perguntas, respostas e fontes.
2. Pipeline Source -> chunks -> embeddings -> retrieval.
3. `GenerateQnAFromSourceCommand` criando perguntas/respostas como draft.
4. Tela de revisao humana com fonte, trechos usados e score.
5. Dataset de avaliacao com perguntas reais e respostas esperadas.

### Direct

Objetivo: resolver conversas privadas com velocidade, usando QnA sem perder contexto individual.

Capacidades de IA:

- Classificar intencao, urgencia, risco, idioma e necessidade de humano.
- Recuperar respostas QnA permitidas para a audiencia da pessoa.
- Sugerir resposta ao agente ou ao usuario com citacoes internas.
- Resumir conversa para handoff humano.
- Extrair dados estruturados de solicitacoes privadas quando permitido.
- Criar evidencia de lacuna para QnA quando nao houver resposta confiavel.
- Sugerir follow-up, proxima melhor acao e encerramento.
- Detectar PII, negociacao, excecao contratual ou tema sensivel que exige humano/Trust.

Requisitos de estado:

- Conversa e mensagens continuam em Direct.
- Sugestoes de IA devem ser registros associados a conversa, com status: suggested, accepted,
  edited, rejected, sent.
- Handoff deve guardar resumo, motivo, risco e fontes consultadas.
- Lacuna originada no Direct deve manter link para conversa ou trecho permitido, sem copiar dados
  pessoais para QnA.

MVP recomendado:

1. API/business modules de Direct antes de automacao.
2. Copiloto interno: sugestao de resposta, resumo e motivo de handoff.
3. Criacao de gap candidate para QnA quando confidence baixa.
4. Automacao de envio apenas para perguntas conhecidas, baixo risco e resposta com fonte QnA ativa.

### Broadcast

Objetivo: transformar interacoes publicas e comunitarias em resposta coordenada, sinal de mercado e
lacuna reutilizavel.

Capacidades de IA:

- Capturar e classificar comentarios, mencoes, posts, lives, foruns e comunidades.
- Agrupar perguntas, objecoes, feedbacks, bugs percebidos e temas recorrentes.
- Separar ruido, spam, abuso, crise de marca e pergunta legitima.
- Recuperar resposta QnA adequada para resposta publica.
- Sugerir resposta curta, segura e contextual ao canal.
- Criar lacuna para QnA quando um padrao recorrente nao tem resposta.
- Encaminhar para Direct quando a conversa exige dado pessoal ou excecao.
- Encaminhar para Trust quando o tema exige politica, moderacao formal ou decisao auditavel.
- Produzir relatorios de objecoes de campanha, voz do cliente e temas emergentes.

Requisitos de estado:

- Thread e item continuam em Broadcast.
- Clusters e sinais recorrentes pertencem ao Broadcast ate virarem lacuna aceita no QnA ou decisao
  no Trust.
- Resposta publica deve guardar fonte QnA, politica usada, risco, status e ator que aprovou/enviou.
- Auto-resposta publica deve ser opt-in por canal, tenant, tema e nivel de risco.

MVP recomendado:

1. APIs/workers de Broadcast para ingestao de canais.
2. Classificacao e agrupamento de itens.
3. Resposta sugerida com QnA retrieval.
4. Painel de padroes recorrentes e criacao de gap para QnA.
5. Sem auto-responder publicamente no primeiro ciclo, exceto ambiente controlado.

### Trust

Objetivo: garantir que respostas, politicas, validacoes e decisoes sensiveis tenham racional,
participacao e auditoria.

Capacidades de IA:

- Classificar quando uma resposta ou fluxo exige validacao formal.
- Montar pacote de evidencias: fontes, alternativas, riscos, historico e impactos.
- Resumir argumentos, votos, contestacoes e racional.
- Verificar se a decisao proposta segue a regra definida.
- Identificar contradicoes entre politica vigente, resposta QnA e nova proposta.
- Publicar resultado validado de volta ao QnA como resposta, status, politica ou racional
  reutilizavel.

Limite importante: IA nao deve votar, aceitar, rejeitar ou decidir sozinha. Ela prepara evidencia,
aponta risco e verifica consistencia; a decisao pertence ao processo Trust.

Requisitos de estado:

- Entidades de proposta, revisao, participante, regra de decisao, voto/aceite, contestacao,
  racional e audit trail.
- Link de decisao Trust para pergunta/resposta/fonte QnA, thread Broadcast ou conversa Direct.
- Snapshot das evidencias usadas no momento da decisao.
- Politica de publicacao do racional para QnA quando o resultado for reutilizavel.

MVP recomendado:

1. Modelo de dominio Trust minimo para validacao de respostas QnA sensiveis.
2. Workflow "QnA precisa de aprovacao" antes de publicar resposta de alto risco.
3. Assistente de evidencias e resumo.
4. Registro de decisao e publicacao do resultado no QnA.

## Arquitetura Alvo

Fluxo conceitual:

```text
Entrada do canal ou fonte
  -> modulo dono classifica e persiste o evento bruto
  -> AI Orchestrator executa classificacao, retrieval, geracao ou avaliacao
  -> tools chamam apenas commands/queries permitidos do modulo dono
  -> resultado volta como draft, sugestao, lacuna, resposta enviada ou evidencia
  -> Trust participa quando politica, risco ou auditoria exigirem
```

Componentes recomendados:

| Componente | Responsabilidade | Dono tecnico |
| --- | --- | --- |
| `Querify.Common.Infrastructure.AI` | Clients de modelo, embeddings, safety, structured output, prompt registry e telemetry | Infra comum |
| `Querify.QnA.Portal.Business.Search` | Busca QnA para Portal, Direct, Broadcast e Trust | QnA |
| `Querify.QnA.Worker.Business.Ingestion` | Extracao, chunking, embeddings e reindexacao de fontes | QnA |
| `Querify.QnA.Portal.Business.SourceIngestion` | Geracao de QnA draft a partir de fontes | QnA |
| `Querify.Direct.*.Business.Conversation` | Conversas, sugestoes, handoff e gap evidence | Direct |
| `Querify.Broadcast.*.Business.Thread` | Captura, clustering, resposta publica e sinais | Broadcast |
| `Querify.Trust.*.Business.Decision` | Validacao, voto, racional, politica e auditoria | Trust |
| `Querify.AI.Evaluation` ou ferramenta equivalente | Datasets, graders, regressao de prompts/modelos | Infra de engenharia |
| `Querify.MCP.Server` | Interface de ferramentas para agentes externos/admins | Integracao, nao runtime obrigatorio do produto |

Decisao sobre workers:

- QnA deve continuar usando `Querify.QnA.Worker.Api` para ingestao, embeddings e reindexacao.
- Direct e Broadcast devem ganhar workers proprios quando houver ingestao continua, filas de
  mensagens, conectores ou analise assincorna.
- Trust pode comecar sincronamente em Portal/API e ganhar worker quando houver alto volume de
  revisoes, expiracoes, notificacoes ou auditorias assicronas.
- O Tenant Worker nao deve processar IA de produto.

## Runtime De Agents E Tools

Agentes devem ser aplicados apenas onde ha workflow multi-etapa. Pipelines deterministicas devem
continuar sendo services e commands.

| Caso | Melhor padrao |
| --- | --- |
| Extrair QnA de uma fonte | Pipeline deterministica com chamada de modelo e structured output |
| Buscar resposta para uma pergunta | Retrieval service com reranking e geracao controlada |
| Resolver conversa Direct | Agent/tool workflow com limites, porque precisa ler conversa, buscar QnA, decidir handoff e sugerir resposta |
| Coordenar Broadcast | Agent/tool workflow com limites, porque precisa agrupar, checar risco, buscar QnA e escolher roteamento |
| Montar evidencia Trust | Agent/tool workflow com limites, porque precisa consultar historico, fontes, regras e conflitos |

MCP deve ser usado como interface externa padronizada para agentes e ferramentas, especialmente para
admin, desenvolvimento, integracoes e assistentes especializados. Para o produto em producao, o
caminho principal deve ser backend -> service -> command/query -> provider/tool adapter, preservando
SLO, autenticacao, filas, retry, idempotencia e observabilidade.

## Guardrails E Seguranca

Riscos principais:

- Prompt injection em documentos, comentarios publicos, HTML e mensagens privadas.
- Vazamento entre tenants ou entre audiencias dentro do mesmo tenant.
- Resposta sem fonte ou com fonte errada.
- Ferramenta chamada fora de escopo.
- Auto-resposta publica com tom, conteudo ou promessa incorreta.
- Decisao automatizada em tema sensivel.
- PII enviada para provedor sem politica contratual.
- Regressao silenciosa ao trocar modelo, prompt, chunking ou embedding.

Controles obrigatorios:

1. Separar instrucoes do sistema, dados recuperados e conteudo do usuario.
2. Tratar fontes externas como dados, nunca como instrucoes.
3. Usar allowlist de tools por modulo, tenant, papel, risco e caso de uso.
4. Validar structured output antes de persistir ou executar command.
5. Exigir citacao de fontes QnA para respostas factuais.
6. Aplicar filtros de tenant, visibilidade e audiencia dentro do retrieval.
7. Bloquear envio automatico quando houver baixa confianca, tema sensivel, PII, negociacao,
   excecao ou ausencia de fonte.
8. Registrar run id, trace id, prompt version, modelo, fontes, ferramentas e decisao final.
9. Rodar avaliacoes offline antes de promover prompt/modelo.
10. Manter revisao humana e rollback para respostas publicadas.

## Avaliacoes

Antes de liberar qualquer automacao, criar cinco conjuntos de avaliacao:

| Dataset | O que mede |
| --- | --- |
| QnA retrieval | Se a pergunta recupera a resposta/fonte correta e nao vaza conteudo privado |
| QnA generation | Se rascunhos gerados estao fiéis a fonte, completos e publicaveis apos revisao |
| Direct resolver | Se a sugestao resolve, escala corretamente e nao inventa informacao |
| Broadcast responder | Se a resposta publica e segura, curta, adequada ao canal e roteia excecoes |
| Trust evidence | Se o pacote de evidencias e completo, neutro, rastreavel e coerente com a regra |

Metricas minimas:

- groundedness: percentual de respostas suportadas por fontes recuperadas.
- retrieval recall: percentual de perguntas que recuperam a fonte correta no top K.
- answer acceptance rate: percentual de sugestoes aceitas sem edicao relevante.
- escalation precision: percentual de escalacoes realmente necessarias.
- unsafe automation rate: eventos em que uma resposta automatica deveria ter sido bloqueada.
- tenant leakage: deve ser zero.
- cost per resolved interaction.
- latency p50/p95 por fluxo.

## Roadmap Recomendado

### Fase 0: Requisitos, Risco E Dataset

Entregaveis:

- Lista de canais priorizados.
- Idiomas e mercados prioritarios.
- Taxonomia de risco: baixo, medio, alto, proibido.
- Politica de automacao por modulo.
- Datasets iniciais de avaliacao.
- Decisao de provedor e politica de dados.

Razao: sem dataset e politica, cada prompt vira opiniao. O produto precisa medir qualidade antes de
automatizar.

### Fase 1: Fundacao QnA

Entregaveis:

- Busca hibrida QnA.
- Source chunking e embeddings.
- Geracao de QnA drafts a partir de fontes.
- Revisao humana com citacoes.
- Evals de retrieval e generation.

Razao: QnA e a memoria operacional. Sem isso, Direct e Broadcast tendem a responder com contexto
fraco.

### Fase 2: Direct Copilot

Entregaveis:

- APIs/business workflows de conversa.
- Sugestao de resposta com fonte QnA.
- Resumo para handoff.
- Gap evidence para QnA.
- Escalacao por risco.

Razao: Direct captura dor real e reduz tempo de resolucao sem expor risco publico inicialmente.

### Fase 3: Broadcast Intelligence

Entregaveis:

- Ingestao de threads e itens por conectores priorizados.
- Classificacao, clustering e sinais recorrentes.
- Resposta publica sugerida com QnA.
- Criacao de gaps QnA.
- Roteamento para Direct ou Trust.

Razao: Broadcast transforma ruido publico em demanda estruturada, mas auto-resposta publica deve
esperar maturidade.

### Fase 4: Trust Governance

Entregaveis:

- Entidades e workflow de validacao.
- Aprovacao de respostas QnA sensiveis.
- Evidencia, regra, participante, racional e auditoria.
- Publicacao de decisao validada no QnA.

Razao: Trust permite escalar IA para temas sensiveis sem perder explicabilidade.

### Fase 5: Automacao Controlada

Entregaveis:

- Auto-resposta Direct para baixo risco.
- Auto-resposta Broadcast apenas em canais/temas permitidos.
- Politica de rollback e auditoria.
- Monitoramento de drift, custo e qualidade.
- Revisao periodica dos evals.

Razao: automacao so deve vir depois que o sistema prova qualidade, seguranca e valor.

## Backlog De Requisitos

Requisitos funcionais:

- RF-01: O sistema deve recuperar respostas QnA por pergunta natural, termo exato, tag, space,
  visibilidade e audiencia.
- RF-02: O sistema deve gerar QnA drafts a partir de fontes verificadas.
- RF-03: O sistema deve mostrar fontes e trechos usados em qualquer resposta gerada.
- RF-04: O sistema deve sugerir resposta Direct com opcoes de aceitar, editar, rejeitar e escalar.
- RF-05: O sistema deve resumir conversa Direct para handoff.
- RF-06: O sistema deve criar lacuna QnA a partir de Direct e Broadcast.
- RF-07: O sistema deve agrupar itens Broadcast recorrentes por tema.
- RF-08: O sistema deve sugerir resposta publica Broadcast com politica de risco.
- RF-09: O sistema deve encaminhar interacao Broadcast para Direct quando houver dado privado.
- RF-10: O sistema deve encaminhar resposta ou politica para Trust quando houver validacao formal.
- RF-11: O sistema deve registrar decisao Trust com evidencia, regra, participantes e racional.
- RF-12: O sistema deve publicar resultado Trust como conteudo reutilizavel no QnA quando aplicavel.

Requisitos nao funcionais:

- RNF-01: Todo acesso a conhecimento deve respeitar tenant, visibilidade e audiencia.
- RNF-02: Toda chamada de IA deve registrar modelo, prompt version, custo, latencia e trace id.
- RNF-03: Saida de IA que cria ou altera estado deve usar schema validado.
- RNF-04: Automacao deve ter limites por risco, canal, tenant e confidence threshold.
- RNF-05: O sistema deve suportar troca de provedor/modelo por configuracao e adapter.
- RNF-06: O sistema deve rodar avaliacoes antes de promover prompt/modelo para producao.
- RNF-07: O sistema deve impedir que conteudo externo instrua o agente a ignorar regras.
- RNF-08: Dados sensiveis devem passar por redacao, mascaramento ou bloqueio conforme politica.
- RNF-09: Mudancas em respostas ativas devem preservar versao e historico.
- RNF-10: Falhas de IA devem degradar para busca, draft, handoff ou fila de revisao, nao para erro
  silencioso.

## Decisoes Que Ainda Precisam Ser Tomadas

- Quais canais entram primeiro em Direct e Broadcast.
- Quais setores o Querify vai priorizar no MVP, porque isso muda risco e compliance.
- Idiomas iniciais e exigencia de localizacao.
- Provedor principal de IA e politica de dados com clientes enterprise.
- Backend de busca vetorial no MVP: Postgres/pgvector ou servico dedicado.
- Nivel de automacao permitido por modulo no primeiro lancamento.
- Quem aprova respostas sensiveis e quais SLAs de revisao.
- Quais eventos de IA entram em analytics de produto versus logs operacionais.

## Nao Fazer Agora

- Criar um "modulo de IA" que possua estado de produto.
- Colocar fluxos de IA no Tenant Worker.
- Comecar por chatbot generico sem QnA confiavel.
- Publicar resposta gerada por IA diretamente no QnA como ativa.
- Auto-responder publicamente sem revisao, politica e metricas.
- Usar apenas busca vetorial sem filtros, citacoes e busca lexical.
- Expor todas as ferramentas a um agente unico.
- Trocar modelo em producao sem evals.
- Fazer fine-tuning antes de existir base revisada e problema especifico.

## Fontes Atuais Consultadas

- OpenAI API docs: Responses API, function calling, structured outputs, file search/retrieval,
  embeddings, Evals, Agents SDK, guardrails e safety best practices.
- Model Context Protocol: arquitetura de hosts, clients, servers, tools e resources.
- Microsoft Azure AI Search: visao de busca hibrida.
- NIST AI RMF 1.0 e NIST AI 600-1 Generative AI Profile.
- OWASP Top 10 for LLM Applications 2025.
- OpenTelemetry Semantic Conventions for Generative AI.
- European Commission AI Act implementation timeline e GPAI obligations.

Links:

- https://developers.openai.com/api/docs/guides/function-calling
- https://developers.openai.com/api/docs/guides/migrate-to-responses
- https://developers.openai.com/api/docs/guides/structured-outputs
- https://developers.openai.com/api/docs/guides/tools-file-search
- https://developers.openai.com/api/docs/guides/retrieval
- https://developers.openai.com/api/docs/guides/embeddings
- https://developers.openai.com/api/docs/guides/evals
- https://developers.openai.com/api/docs/guides/agents/guardrails-approvals
- https://developers.openai.com/api/docs/guides/agent-evals
- https://developers.openai.com/api/docs/guides/safety-best-practices
- https://modelcontextprotocol.io/docs/learn/architecture
- https://learn.microsoft.com/en-us/azure/search/hybrid-search-overview
- https://www.nist.gov/itl/ai-risk-management-framework
- https://www.nist.gov/publications/artificial-intelligence-risk-management-framework-generative-artificial-intelligence
- https://genai.owasp.org/resource/owasp-top-10-for-llm-applications-2025/
- https://opentelemetry.io/docs/specs/semconv/gen-ai/
- https://ai-act-service-desk.ec.europa.eu/en/ai-act/eu-ai-act-implementation-timeline
- https://digital-strategy.ec.europa.eu/en/factpages/general-purpose-ai-obligations-under-ai-act
