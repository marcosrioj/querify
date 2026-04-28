# Proposta de Valor

## Proposta Principal

**Transforme cada pergunta em conhecimento reutilizável, resolva interações 1:1 e 1:N em qualquer canal e aprenda com cada conversa para melhorar a próxima resposta.**

## Versão Expandida

Um único sistema para capturar dúvidas de clientes e comunidades, responder com consistência e transformar padrões de interação em conhecimento aprovado.

Ele resolve conversas diretas, comentários públicos e comunidades sem perder contexto entre canais.

Cada resposta resolvida fortalece a base, reduz retrabalho e revela o que o mercado precisa saber em seguida.

## Versão Alternativa

**Da interação ao ativo:** responda onde as pessoas perguntam e transforme cada dúvida recorrente em conhecimento que a empresa pode reutilizar.

## Categoria Recomendada

**Question-to-Knowledge OS**

Uma categoria mais forte do que "plataforma de perguntas": o produto não apenas responde dúvidas, ele transforma demanda, suporte, comunidade e decisão em conhecimento reutilizável.

## Os 5 Módulos Atuais Do BaseFaq

O BaseFaq é composto por cinco módulos atuais: Tenant, QnA, Direct, Broadcast e Trust.

Esses módulos não representam departamentos da empresa. Produto, marketing, suporte, sucesso do cliente, comunidade, vendas, parceiros e liderança são áreas humanas que podem operar ou consumir o sistema, mas não são módulos do BaseFaq. Um fluxo de negócio pode envolver várias áreas humanas sem mudar a responsabilidade técnica de cada módulo.

### Regra De Não Conflito Entre Áreas

Cada comportamento persistente deve ter um único dono de módulo. Outros módulos podem consumir, referenciar, sugerir ou distribuir dados desse dono, mas não devem duplicar o estado nem assumir o fluxo principal.

| Pergunta de fronteira | Dono correto | Regra |
| --- | --- | --- |
| É identidade, plano, acesso, cobrança, entitlement, conexão ou operação de plataforma? | Tenant | Tenant habilita os módulos, mas não executa fluxos de produto. |
| É pergunta, resposta, fonte, tag, visibilidade, versão ou conhecimento aprovado? | QnA | QnA guarda conhecimento reutilizável, mas não guarda fluxo de conversa privada, comentário público ou decisão auditável. |
| É uma conversa direcionada a uma pessoa, conta, cliente, usuário ou membro específico? | Direct | Direct usa QnA para responder, mas Direct é dono de mensagens, contexto 1:1, handoff e escalação. |
| É uma interação visível por muitas pessoas ou produzida em espaço compartilhado? | Broadcast | Broadcast usa QnA para responder e gerar lacunas, mas é dono de captura pública, coordenação de resposta e sinais sociais/comunitários. |
| É validação, voto, aprovação, contestação, racional, histórico de decisão ou auditoria? | Trust | Trust valida e registra decisões, mas não substitui QnA, Direct, Broadcast ou Tenant como donos dos seus próprios fluxos. |

Se uma necessidade descreve uma área humana, como marketing ajustar campanha ou produto priorizar roadmap, ela continua sendo resultado de negócio. Ela só vira comportamento do BaseFaq quando existe estado, regra ou fluxo que um dos módulos precisa possuir.

### Áreas De Negócio E Módulos Que Elas Usam

Áreas de negócio podem aparecer em casos de uso, relatórios, métricas e decisões operacionais, mas não devem virar novos módulos nem deslocar a posse dos módulos existentes.

| Área humana | Usa principalmente | Não deve virar |
| --- | --- | --- |
| Suporte e sucesso do cliente | Direct para conversas privadas; QnA para respostas aprovadas; Trust para temas sensíveis | Um segundo dono de conversas, respostas ou validação. |
| Marketing e crescimento | Broadcast para sinais públicos; QnA para narrativas aprovadas; Direct para follow-up privado | Um módulo paralelo de campanha, social ou resposta pública. |
| Produto e roadmap | Broadcast e Direct para sinais; Trust para priorização formal; QnA para publicar status e racional | Um dono informal de decisão fora do Trust. |
| Comunidade | Broadcast para discussão; Trust para validação, voto ou moderação formal; QnA para respostas confiáveis | Uma mistura de fórum, governança e base de conhecimento no mesmo estado. |
| Vendas, parceiros e canais | Direct para atendimento privado; QnA para conhecimento por audiência; Tenant para acesso e entitlement | Regras de permissão ou negociação dentro do QnA. |
| Liderança e operações | Relatórios consolidados de QnA, Direct, Broadcast e Trust | Um módulo de inteligência que duplique sinais dos módulos de origem. |

### Critério De Responsabilidade

A fronteira entre módulos é definida por três perguntas, nessa ordem:

1. A demanda é controle de plataforma? Se sim, o dono é Tenant.
2. A interação é privada, compartilhada ou uma decisão formal?
3. O resultado precisa virar conhecimento reutilizável, validação auditável ou apenas sinal operacional?

A fronteira entre Direct e Broadcast não é o nome do canal. É a visibilidade da comunicação.

**Direct é sempre responsável quando a comunicação é direta para uma pessoa, conta, cliente, usuário ou membro específico.** Isso inclui chat, DM, WhatsApp, portal logado, email ou qualquer atendimento privado, mesmo quando o canal tem alto volume.

**Broadcast é sempre responsável quando a comunicação é para muitas pessoas ou quando a resposta pode ser vista por muitas pessoas.** Isso inclui comentários públicos, posts, vídeos, lives, comunidades, fóruns, grupos, canais compartilhados e discussões abertas ou semiabertas.

Quando um mesmo conector permite os dois modos, como WhatsApp, Instagram, Slack ou Teams, o módulo é escolhido pelo modo da interação: conversa privada fica com Direct; resposta compartilhada ou visível por muitos fica com Broadcast. Se a conversa pública precisa continuar de forma privada, Broadcast encerra ou referencia a interação pública e Direct assume o trecho privado. Se a conversa privada gera uma resposta reutilizável, Direct registra a lacuna ou evidência e QnA assume a curadoria do conhecimento.

### 1. Tenant

O módulo de controle da plataforma.

Ele organiza tenants, usuários, membros, permissões, cobrança, entitlement, chaves públicas, conexões de banco e operações assíncronas de controle.

**Valor principal:** governança operacional. Cada workspace tem identidade, plano, acesso, roteamento de dados e processamento de plataforma controlados antes dos módulos de produto executarem seus fluxos.

**Responsabilidades**

- Provisionamento e administração de workspaces.
- Associação de usuários, papéis e acesso por tenant.
- Mapeamento de conexão dos bancos dos módulos.
- Billing, assinaturas, faturas, pagamentos, entitlements e webhooks.
- Chaves públicas e identificação de tenants para superfícies públicas.
- Outbox de email e processamento assíncrono de plataforma.
- Suporte a cenários back-office, portal, público e worker.

**Não possui**

- Perguntas, respostas, fontes, tags ou fluxos de conhecimento.
- Conversas 1:1, mensagens, handoff ou resolução de tickets.
- Comentários públicos, menções, threads comunitárias ou sinais sociais.
- Validação, voto, decisão auditável ou racional de governança de produto.

### 2. QnA

O centro de conhecimento aprovado da empresa.

Ele transforma perguntas recorrentes, respostas validadas e aprendizados de suporte em uma base reutilizável para todos os canais.

**Valor principal:** consistência. A empresa para de responder a mesma dúvida várias vezes e passa a usar uma fonte confiável para sites, apps, portais, documentação, suporte e comunidades.

**Responsabilidades**

- Espaços de conhecimento, perguntas canônicas, respostas aprovadas, fontes, tags e visibilidade.
- Curadoria, publicação, versionamento e status de perguntas e respostas.
- Proveniência de conhecimento, como canal de entrada, fonte de origem, evidência e referência.
- Distribuição de respostas aprovadas para sites, apps, portais, documentação, Direct e Broadcast.
- Lacunas de conhecimento quando uma interação ainda não tem resposta reutilizável.
- Sinais públicos ou feedback sobre utilidade da resposta quando esses sinais pertencem ao ativo QnA.

**Superfícies onde QnA aparece**

- Sites institucionais, landing pages e páginas comerciais.
- Help centers, bases de conhecimento e portais de autoatendimento.
- E-commerce, páginas de produto, checkout, status de pedido e pós-venda, quando a necessidade é uma resposta aprovada reutilizável.
- Portais de clientes, parceiros, revendas e fornecedores, quando a necessidade é consulta a conhecimento permitido para aquela audiência.
- Produtos SaaS em onboarding, cobrança, configurações, permissões, segurança e integrações, quando a necessidade é ajuda contextual aprovada.
- Documentação técnica, portais de APIs, SDKs e guias para desenvolvedores.
- Apps web, PWAs e apps mobile com ajuda contextual.
- Setores regulados, como educação, saúde, finanças, seguros, governo e B2B enterprise.

**Não possui**

- Conversa privada, sequência de mensagens, handoff, negociação, exceção ou atendimento individual. Isso pertence ao Direct.
- Comentário público, menção, thread comunitária, grupo, live, post ou sinal social/comunitário. Isso pertence ao Broadcast.
- Voto formal, decisão coletiva, contestação, aprovação auditável ou trilha de governança. Isso pertence ao Trust.
- Plano, billing, tenant connection ou entitlement. Isso pertence ao Tenant.

### 3. Direct

A camada de resolução para conversas diretas.

Ele ajuda clientes, usuários, agentes e times internos a encontrar a resposta certa, concluir tarefas e escalar para uma pessoa quando o caso exige julgamento humano. Sempre que a resposta é direcionada a uma pessoa específica, a responsabilidade é do Direct.

**Valor principal:** velocidade de resolução. Cada conversa individual deixa de ser apenas atendimento e passa a alimentar conhecimento para futuras interações.

**Responsabilidades**

- Conversas 1:1, mensagens, status de conversa, ator da mensagem e contexto de resolução.
- Atendimento privado em chat, in-app, email, portal logado, DM, WhatsApp Business e canais equivalentes.
- Assistência a agentes, clientes, usuários, parceiros ou times internos em jornadas privadas.
- Handoff para humano ou ticket quando o caso exige decisão, exceção ou negociação.
- Continuação privada de uma interação pública quando dados pessoais, negociação ou exceção não devem ficar em espaço visível por muitos.
- Registro de lacunas, evidências ou respostas finais que podem alimentar QnA depois de revisão.

**Cenários de negócio**

- Chat de suporte em sites, web apps e áreas logadas.
- Assistente de conversão em e-commerce para entrega, troca, garantia, pagamento e compatibilidade.
- Fluxos de onboarding e ativação em SaaS, edtech, fintech, healthtech e marketplaces.
- Portais de cliente para suporte procedural, segunda via, planos, assinaturas e solicitações.
- Apps mobile com ajuda dentro da jornada do usuário.
- Suporte interno para vendas, sucesso do cliente, implantação, suporte técnico e parceiros.

**Não possui**

- Resposta canônica reutilizável, fonte oficial, tag, visibilidade pública ou ciclo editorial do conhecimento. Isso pertence ao QnA.
- Resposta pública, comentário social, thread comunitária, live, fórum ou grupo compartilhado. Isso pertence ao Broadcast.
- Aprovação auditável, votação, decisão coletiva ou racional formal. Isso pertence ao Trust.
- Entitlement, cobrança, papéis de tenant ou conexão de banco. Isso pertence ao Tenant.

### 4. Broadcast

A camada de captura e resposta em canais públicos, comunitários e um-para-muitos.

Ele organiza comentários, menções, dúvidas sociais e discussões de comunidade, identifica padrões recorrentes e transforma interações dispersas em sinais acionáveis. Sempre que a resposta pode ser vista por muitas pessoas, a responsabilidade é do Broadcast.

**Valor principal:** presença multicanal com aprendizado. A empresa responde onde as pessoas perguntam e usa essas interações para melhorar produto, comunicação e suporte.

**Responsabilidades**

- Threads, itens, atores, status, canais compartilhados e sinais de interações visíveis por muitas pessoas.
- Comentários, menções, respostas públicas, grupos, comunidades, fóruns, canais compartilhados, lives e discussões abertas ou semiabertas.
- Coordenação de resposta pública usando conhecimento aprovado do QnA.
- Agrupamento de perguntas, objeções, feedback e padrões recorrentes vindos de espaços compartilhados.
- Encaminhamento de lacunas para QnA e de decisões formais para Trust quando o sinal exige curadoria ou governança.
- Encaminhamento para Direct quando uma interação pública precisa continuar em atendimento privado.

**Cenários de negócio**

- Instagram e Facebook para comentários, menções, dúvidas em posts, anúncios e respostas públicas.
- YouTube para comentários em vídeos, tutoriais, lançamentos, reviews e lives.
- WhatsApp Channels, grupos, comunidades e outros espaços compartilhados onde a resposta pode ser vista por muitas pessoas.
- Comunidades próprias em Discord, GitHub Discussions, Discourse, Slack, Teams e fóruns privados ou públicos.
- Lançamentos, campanhas e eventos que geram objeções, dúvidas e comentários em massa.
- Marcas DTC, e-commerce e creators com alto volume de perguntas repetidas.
- SaaS e produtos B2B que precisam entender objeções, pedidos de melhoria e dúvidas de adoção.
- Voz do cliente para produto, marketing, suporte e liderança.

**Não possui**

- Conversa privada, handoff individual, exceção de cliente ou atendimento operacional 1:1. Isso pertence ao Direct.
- Resposta aprovada como fonte canônica reutilizável. Isso pertence ao QnA.
- Votação formal, priorização auditável, decisão coletiva ou histórico de aprovação. Isso pertence ao Trust.
- Plano, entitlement, usuário de tenant ou billing. Isso pertence ao Tenant.

### 5. Trust

A camada de confiança para decisões, validações e participação.

Ele cria processos claros para revisar respostas, validar contribuições, priorizar demandas e registrar decisões quando transparência e auditoria importam.

**Valor principal:** confiança. Comunidades, clientes e times internos conseguem entender de onde veio uma resposta, por que uma decisão foi tomada e qual versão deve ser usada.

**Responsabilidades**

- Validação, aprovação, contestação, voto, regras de decisão, racional e histórico auditável.
- Processos de revisão para respostas, contribuições comunitárias, políticas, propostas, roadmap e decisões sensíveis.
- Registro de quem participou, qual regra foi aplicada, qual resultado foi aprovado e qual versão deve ser usada.
- Evidência e trilha de auditoria quando uma resposta, decisão ou política precisa ser explicável.
- Publicação de status, racional ou decisão para QnA quando esse resultado deve virar conhecimento reutilizável.

**Cenários de negócio**

- Portais de roadmap, priorização de produto e conselhos de clientes.
- Comunidades de membros, associações, cooperativas, creator communities e ecossistemas digitais.
- Programas beta, comunidades de parceiros e grupos consultivos.
- Programas de grants, editais, hackathons, incubadoras e aceleradoras.
- Decisões de moderação, políticas de comunidade e eleição de representantes.
- Comunidades open source para propostas, discussões, prioridades e decisões públicas.
- DAOs e ecossistemas web3 quando verificabilidade pública for um requisito real.
- Ambientes regulados ou sensíveis que precisam de histórico, evidência e auditoria.

**Não possui**

- Captura de comentários, menções, threads ou interações comunitárias comuns. Isso pertence ao Broadcast.
- Conversas privadas, handoff, mensagens e resolução individual. Isso pertence ao Direct.
- Armazenamento editorial de perguntas, respostas, fontes e tags. Isso pertence ao QnA.
- Controle de tenant, plano, billing e entitlement. Isso pertence ao Tenant.

## Fluxo Central Do Negócio

1. Uma pergunta, objeção, contribuição, solicitação ou proposta aparece em uma superfície própria, conversa direta, rede social, comunidade ou processo formal.
2. O sistema classifica o dono da interação: QnA para conhecimento próprio, Direct para conversa privada, Broadcast para espaço compartilhado e Trust para decisão ou validação auditável.
3. O módulo de entrada consulta QnA quando precisa de resposta aprovada.
4. Se existir resposta aprovada, o módulo de entrada entrega no canal certo sem assumir a posse editorial do conhecimento.
5. Se não existir resposta aprovada, o módulo de entrada registra lacuna, evidência ou sinal no seu próprio contexto e encaminha a curadoria para QnA.
6. Se a lacuna exige aprovação formal, voto, contestação ou histórico auditável, Trust valida a decisão antes ou depois da publicação no QnA.
7. A resposta, decisão ou política aprovada volta para Direct, Broadcast e superfícies QnA como conhecimento reutilizável.
8. Cada interação gera sinais sobre demanda, fricção, objeções, prioridades e confiança sem deslocar a posse do fluxo original.

Esse é o ciclo que cria valor acumulado: **interação -> dono correto -> resolução -> lacuna ou validação -> conhecimento ou decisão -> distribuição -> aprendizado -> melhor próxima resposta.**

## Como Os Módulos Se Conectam

Os módulos do BaseFaq funcionam como partes de um mesmo sistema. Tenant controla identidade, acesso, cobrança e roteamento de dados. QnA, Direct, Broadcast e Trust executam os fluxos de produto e alimentam o ciclo de conhecimento.

A tabela abaixo mostra responsabilidade de módulo, não responsabilidade de times humanos. Produto, marketing, suporte, comunidade, vendas, parceiros e liderança podem participar da decisão operacional, mas continuam sendo áreas de negócio.

| Situação | Dono da interação | Dono do conhecimento reutilizável | Dono de validação ou decisão | Limite de não conflito |
| --- | --- | --- | --- | --- |
| Pergunta em site, app, documentação ou portal de autoatendimento | QnA | QnA | Trust, quando aprovação auditável for necessária | Se virar conversa personalizada, o trecho privado passa para Direct. |
| Conversa 1:1 de suporte, sucesso, vendas, parceiro ou cliente | Direct | QnA, apenas quando a resposta final vira conhecimento aprovado | Trust, quando houver decisão sensível ou validação formal | Direct não armazena resposta canônica; QnA não armazena mensagens e handoff. |
| Comentário, menção, post, live ou resposta pública | Broadcast | QnA, apenas quando o padrão vira pergunta ou resposta reutilizável | Trust, quando houver política, moderação formal ou aprovação auditável | Broadcast não vira base editorial; QnA não vira caixa de comentários. |
| Pergunta ou contribuição em comunidade, fórum, grupo ou canal compartilhado | Broadcast | QnA, quando a contribuição validada vira resposta confiável | Trust, quando houver voto, moderação, aceite ou decisão formal | Discussão comum é Broadcast; decisão verificável é Trust. |
| Pedido de melhoria, objeção recorrente ou sinal de mercado em espaço compartilhado | Broadcast | QnA, quando a empresa publica explicação, status ou resposta reutilizável | Trust, quando houver priorização, roadmap ou decisão auditável | Produto e marketing podem agir como áreas humanas sem virar módulos. |
| Decisão coletiva, proposta, votação, política ou racional formal | Trust | QnA, somente para publicar o resultado ou explicação reutilizável | Trust | Broadcast pode divulgar a decisão e Direct pode explicar em privado, mas nenhum deles é dono da decisão. |
| Plano, assinatura, permissão, tenant, chave pública ou conexão de banco | Tenant | Não se aplica, salvo documentação operacional publicada no QnA | Trust, apenas se houver auditoria formal sobre decisão de acesso ou política | Tenant habilita o uso dos módulos, mas não absorve os fluxos de produto. |

### Regras De Handoff

- Direct para QnA: uma conversa privada revela uma lacuna ou resposta final que deve virar conhecimento aprovado.
- Broadcast para QnA: uma interação pública ou comunitária revela pergunta, objeção ou explicação recorrente.
- Broadcast para Direct: uma interação visível por muitos precisa continuar com dados pessoais, negociação, exceção ou atendimento individual.
- Direct para Broadcast: uma dúvida privada revela que a empresa precisa publicar uma resposta em espaço visível por muitos.
- QnA para Trust: uma resposta, fonte, política ou status precisa de aprovação formal, contestação, voto, racional ou auditoria.
- Trust para QnA: uma decisão ou validação deve ser publicada como resposta, status, política ou racional reutilizável.
- Tenant para qualquer módulo: um workspace, plano, entitlement, conexão ou permissão habilita ou bloqueia o fluxo, mas não muda o dono do comportamento.

## Fluxos De Negócio Possíveis

Os fluxos abaixo mostram como o sistema cria valor em diferentes momentos da jornada: resolução, aprendizado, conversão, comunidade, governança e escala operacional.

### Resolução E Conhecimento

| Processo | Situação | Módulos sem conflito | Valor |
| --- | --- | --- | --- |
| 1. Pergunta Conhecida -> Resposta Imediata | Um cliente pergunta algo que já foi aprovado antes. | QnA fornece a resposta aprovada; QnA entrega em superfície própria, Direct entrega em conversa privada ou Broadcast entrega em espaço visível por muitos. O módulo de entrada mantém a posse da interação. | Reduz tickets, aumenta consistência e evita que times diferentes respondam a mesma dúvida de formas diferentes. |
| 2. Pergunta Nova -> Ativo Reutilizável | Uma pergunta aparece sem resposta aprovada. | Direct registra lacuna se a origem é privada; Broadcast registra lacuna se a origem é pública ou compartilhada; QnA assume a curadoria da resposta; Trust valida quando houver risco ou decisão formal. | Cada falha de cobertura vira conhecimento permanente sem mover fluxo de canal para QnA. |
| 3. Atendimento 1:1 -> Conhecimento Para Todos | Um agente ou o Direct resolve uma dúvida individual. | Direct mantém conversa, mensagens, handoff e resolução; QnA recebe apenas a resposta reutilizável depois de revisão; Trust participa se a resposta exigir aprovação auditável. | O custo de uma conversa individual gera retorno em escala. |

### Social, Comunidade E Conversão

| Processo | Situação | Módulos sem conflito | Valor |
| --- | --- | --- | --- |
| 4. Comentário Público -> Resposta Segura | Alguém faz uma pergunta ou objeção em comentário público. | Broadcast captura e responde no canal; QnA sugere ou recebe resposta aprovada; Trust participa somente quando a resposta exige validação formal. | Aumenta velocidade de resposta sem sacrificar controle de marca. |
| 5. Rede Social -> Insight De Mercado | Vários comentários repetem a mesma dúvida, medo ou objeção. | Broadcast agrupa sinais; QnA guarda explicações reutilizáveis; Direct usa essas explicações em conversas privadas; marketing ajusta campanha como área humana. | Transforma ruído social em aprendizado comercial sem transformar marketing em módulo. |
| 6. Comunidade -> Resposta Verificada | Usuários respondem perguntas uns dos outros. | Broadcast captura a discussão; Trust valida por moderação, voto ou revisão quando houver regra formal; QnA publica a resposta confiável. | Aproveita conhecimento da comunidade sem abrir mão de confiança. |
| 7. Página De Produto -> Conversão | Um comprador tem uma dúvida antes de comprar. | QnA responde dúvidas aprovadas na página; Direct assume assistência privada de compra; Broadcast assume dúvidas em comentários ou comunidades; QnA publica novas respostas recorrentes. | Remove objeções no momento de decisão e reduz abandono. |
| 8. Onboarding -> Ativação | Um usuário não sabe como concluir uma configuração ou primeiro uso. | Direct guia a pessoa em jornada privada; QnA fornece passos aprovados e ajuda contextual; mudanças nas telas do produto são resultado de negócio fora do módulo, salvo quando viram conteúdo QnA. | Acelera ativação e reduz fricção inicial. |

### Suporte Operacional

| Processo | Situação | Módulos sem conflito | Valor |
| --- | --- | --- | --- |
| 9. Baixa Confiança -> Escalação Humana | O sistema não tem segurança para responder. | Direct escala casos privados; Broadcast segura ou encaminha respostas públicas; QnA recebe a lacuna; Trust valida quando o tema é sensível. | Cria um caminho controlado para melhorar cobertura sem respostas improvisadas. |
| 10. Ticket Recorrente -> Redução De Volume | O mesmo motivo de contato aparece muitas vezes. | Direct detecta repetição em conversas privadas; Broadcast detecta repetição em espaços compartilhados; QnA publica resposta aprovada; canais passam a responder antes do ticket. | Ataca a causa do volume, não apenas a fila. |

### Mercado, Produto E Decisão

| Processo | Situação | Módulos sem conflito | Valor |
| --- | --- | --- | --- |
| 11. Campanha -> Mapa De Objeções | Uma campanha gera comentários, dúvidas e resistência. | Broadcast captura reações; QnA estrutura respostas aprovadas; Direct usa as respostas em conversas privadas; marketing ajusta criativos, landing pages e argumentos como área humana. | Melhora a performance de campanhas a partir das perguntas reais do mercado. |
| 12. Feedback De Produto -> Decisão De Roadmap | Usuários pedem melhorias, reportam confusão ou questionam limites do produto. | Broadcast identifica temas públicos e comunitários; Direct pode registrar feedback privado; Trust prioriza quando existe regra de decisão; QnA publica status, racional ou próximos passos aprovados. | Transforma demanda dispersa em decisão transparente. |
| 13. Proposta Comunitária -> Decisão Auditável | Uma comunidade, conselho ou grupo de clientes precisa decidir algo. | Trust registra proposta, participação, regra e resultado; QnA publica o racional reutilizável; Broadcast responde perguntas visíveis; Direct responde dúvidas privadas. | Aumenta confiança porque a decisão não fica escondida em conversas soltas. |
| 14. Mudança De Política -> Atualização Multicanal | Uma regra de preço, entrega, segurança, privacidade ou uso muda. | QnA atualiza a resposta aprovada; Trust guarda aprovação e histórico quando necessário; Direct e Broadcast distribuem apenas a versão permitida. | Reduz risco de respostas antigas continuarem circulando. |

### Escala Organizacional

| Processo | Situação | Módulos sem conflito | Valor |
| --- | --- | --- | --- |
| 15. Time Interno -> Resposta Externa Consistente | Vendas, suporte, sucesso do cliente e parceiros precisam responder a mesma pergunta. | QnA centraliza a resposta; Direct ajuda em interações privadas; Broadcast garante consistência em comentários e comunidades; áreas humanas continuam responsáveis pela execução comercial. | Alinha a empresa inteira em torno da mesma verdade operacional. |
| 16. Parceiros E Revendas -> Conhecimento Controlado | Parceiros precisam de respostas aprovadas, mas nem sempre devem ver tudo. | QnA entrega respostas por audiência; Direct atende parceiros em portal ou canal privado; Tenant controla acesso e entitlement; lacunas retornam para QnA. | Escala suporte de canal sem perder controle sobre permissão e mensagem. |
| 17. Documentação Técnica -> Resolução De Desenvolvedores | Desenvolvedores fazem perguntas sobre APIs, SDKs, erros ou limites. | QnA publica resposta versionada; Direct orienta casos individuais; Broadcast captura discussões técnicas recorrentes; Trust valida mudanças sensíveis ou decisões públicas. | Reduz fricção para adoção técnica e transforma diagnóstico técnico em conhecimento. |
| 18. Expansão Internacional -> Aprendizado Local | Um novo mercado faz perguntas diferentes ou usa outras palavras. | Broadcast captura variações públicas; Direct captura variações privadas; QnA adapta respostas por idioma, país e contexto; Trust valida termos sensíveis quando necessário. | Evita copiar respostas de um mercado para outro sem aprender com a demanda real. |
| 19. Risco, Compliance E Auditoria -> Prova De Decisão | Um tema exige controle, revisão ou evidências. | QnA guarda resposta aprovada; Trust registra validação, regra, participantes e histórico; Direct e Broadcast usam apenas a versão permitida. | Reduz risco operacional em temas sensíveis. |
| 20. Liderança -> Inteligência Operacional | Diretoria quer entender o que clientes, usuários e comunidades estão perguntando. | Broadcast, Direct e QnA consolidam sinais de suas próprias fronteiras; Trust registra decisões quando houver regra formal; relatórios orientam novas respostas, campanhas ou propostas. | Transforma interações do dia a dia em insumo para estratégia. |

## Padrões De Mercado Validados Pela Pesquisa

- Plataformas de suporte estão convergindo para conhecimento conectado que alimenta autosserviço, agentes humanos, automação e análise a partir de uma mesma fonte confiável.
- Canais sociais e de vídeo já oferecem estruturas de comentários e conversas que podem ser capturadas, respondidas e transformadas em inventário de perguntas.
- Comunidades maduras usam respostas aceitas, votos, moderação e verificação para transformar discussão em conhecimento confiável.
- Governança confiável depende menos de promessas tecnológicas e mais de identidade, regras claras, trilha de auditoria, contestabilidade e publicação do racional.
- O uso de IA deve ser tratado como uma camada auxiliar, não como a promessa central. O valor principal é conhecimento confiável, resolução consistente e aprendizado contínuo.

## Promessa Comercial Em Uma Linha

**Responda melhor hoje, aprenda com cada interação e transforme perguntas repetidas em conhecimento que escala.**

## Referências Consultadas

- [Zendesk Knowledge](https://www.zendesk.com/service/knowledge/) reforça a tese de conhecimento conectado para resoluções em autosserviço, agentes e canais.
- [YouTube CommentThreads API](https://developers.google.com/youtube/v3/docs/commentThreads) mostra comentários e respostas como estrutura capturável de interações públicas.
- [WhatsApp Cloud API overview](https://developers.facebook.com/docs/whatsapp/cloud-api/overview) mostra mensageria programática e manual em escala, conectada a sistemas de negócio.
- [GitHub Discussions](https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/participating-in-a-discussion) mostra enquetes, votos e marcação de respostas em comunidades.
- [Stack Overflow Internal Questions](https://stackoverflowteams.help/en/articles/8882140-questions) reforça a ideia de base de conhecimento construída a partir de perguntas bem respondidas.
- [FIDO Alliance Passkeys](https://fidoalliance.org/passkeys/) referencia autenticação forte e resistente a phishing para fluxos de confiança.
- [OpenZeppelin Governor](https://docs.openzeppelin.com/contracts/5.x/governance) referencia proposta, voto e ciclo de decisão para processos auditáveis.

### Referências de viabilidade atual (abril de 2026)

- [WordPress Plugin Developer Handbook](https://developer.wordpress.org/plugins/intro/) para distribuição via plugins em sites WordPress.
- [Shopify theme app extensions](https://shopify.dev/apps/build/online-store/theme-app-extensions/configuration) para app embeds e blocos em storefront.
- [BigCommerce Widgets API](https://developer.bigcommerce.com/docs/storefront/widgets) para posicionar widgets em páginas específicas.
- [Webflow custom code embed](https://help.webflow.com/hc/en-us/articles/33961332238611-Custom-code-embed) para embeds em builders visuais.
- [Squarespace code injection](https://support.squarespace.com/hc/en-us/articles/205815908-Using-code-injection) para scripts e widgets em sites Squarespace.
- [Wix custom mobile widgets](https://support.wix.com/en/article/wix-mobile-apps-getting-started-with-custom-widgets) para superfícies mobile customizadas.
- [Intercom web installation](https://developers.intercom.com/installing-intercom/web/installation) e [Intercom mobile installation](https://developers.intercom.com/installing-intercom) como evidência de padrão de mercado para web/mobile embeds.
- [HubSpot Conversations guide](https://developers.hubspot.com/docs/api-reference/latest/conversations/guide) e [Chat Widget SDK](https://developers.hubspot.com/docs/api-reference/latest/conversations/chat-configuration/chat-widget-sdk) para chat web/mobile e canais customizados.
- [Slack App Home](https://docs.slack.dev/surfaces/app-home/) e [Slack Shortcuts](https://docs.slack.dev/interactivity/implementing-shortcuts/) para levar conhecimento e ações para o fluxo de trabalho.
- [Microsoft Teams tabs](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/what-are-tabs) e [message extensions](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-sdk/in-depth-guides/message-extensions/overview) para superfícies internas de trabalho.
- [GitHub Discussions](https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/about-discussions) para perguntas, respostas, polls e decisões abertas.
- [Discourse Solved](https://meta.discourse.org/t/discourse-solved/30155) e [Discourse Topic Voting](https://meta.discourse.org/t/discourse-topic-voting/40121) para suporte colaborativo e priorização por votos.
- [Discord bots](https://discord.com/developers/docs/bots), [application commands](https://docs.discord.com/developers/interactions/application-commands) e [webhooks](https://docs.discord.com/developers/platform/webhooks) para comunidades próprias e automações.
- [YouTube CommentThreads API](https://developers.google.com/youtube/v3/docs/commentThreads) para ingestão e resposta em comentários de vídeo.
- [TikTok Research API](https://developers.tiktok.com/products/research-api/) e [About Research Tools](https://developers.tiktok.com/doc/about-research-api/) como evidência de que TikTok ainda deve ser tratado como conector restrito.
- [WhatsApp Cloud API overview](https://developers.facebook.com/docs/whatsapp/cloud-api/overview) para mensageria operacional em escala.
- [Snapshot docs](https://docs.snapshot.box/) para voto off-chain, mensagens assinadas e modelos flexíveis de governança.
- [W3C WebAuthn Level 3](https://www.w3.org/TR/webauthn-3/) e [NIST SP 800-63-4](https://www.nist.gov/publications/nist-sp-800-63-4-digital-identity-guidelines) para autenticação forte e arquitetura de confiança.
