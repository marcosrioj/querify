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

### Critério De Responsabilidade

A fronteira entre Direct e Broadcast não é o nome do canal. É a visibilidade da comunicação.

**Direct é sempre responsável quando a comunicação é direta para uma pessoa, conta, cliente, usuário ou membro específico.** Isso inclui chat, DM, WhatsApp, portal logado, email ou qualquer atendimento privado, mesmo quando o canal tem alto volume.

**Broadcast é sempre responsável quando a comunicação é para muitas pessoas ou quando a resposta pode ser vista por muitas pessoas.** Isso inclui comentários públicos, posts, vídeos, lives, comunidades, fóruns, grupos, canais compartilhados e discussões abertas ou semiabertas.

Quando um mesmo conector permite os dois modos, como WhatsApp, Instagram, Slack ou Teams, o módulo é escolhido pelo modo da interação: conversa privada fica com Direct; resposta compartilhada ou visível por muitos fica com Broadcast.

### 1. Tenant

O módulo de controle da plataforma.

Ele organiza tenants, usuários, membros, permissões, cobrança, entitlement, chaves públicas, conexões de banco e operações assíncronas de controle.

**Valor principal:** governança operacional. Cada workspace tem identidade, plano, acesso, roteamento de dados e processamento de plataforma controlados antes dos módulos de produto executarem seus fluxos.

**Atuacoes**

- Provisionamento e administração de workspaces.
- Associação de usuários, papéis e acesso por tenant.
- Mapeamento de conexão dos bancos dos módulos.
- Billing, assinaturas, faturas, pagamentos, entitlements e webhooks.
- Chaves públicas e identificação de tenants para superfícies públicas.
- Outbox de email e processamento assíncrono de plataforma.
- Suporte a cenários back-office, portal, público e worker.

### 2. QnA

O centro de conhecimento aprovado da empresa.

Ele transforma perguntas recorrentes, respostas validadas e aprendizados de suporte em uma base reutilizável para todos os canais.

**Valor principal:** consistência. A empresa para de responder a mesma dúvida várias vezes e passa a usar uma fonte confiável para sites, apps, portais, documentação, suporte e comunidades.

**Atuacoes**

- Sites institucionais, landing pages e páginas comerciais.
- Help centers, bases de conhecimento e portais de autoatendimento.
- E-commerce, páginas de produto, checkout, status de pedido e pós-venda.
- Portais de clientes, parceiros, revendas e fornecedores.
- Produtos SaaS em onboarding, cobrança, configurações, permissões, segurança e integrações.
- Documentação técnica, portais de APIs, SDKs e guias para desenvolvedores.
- Apps web, PWAs e apps mobile com ajuda contextual.
- Setores regulados, como educação, saúde, finanças, seguros, governo e B2B enterprise.

### 3. Direct

A camada de resolução para conversas diretas.

Ele ajuda clientes, usuários, agentes e times internos a encontrar a resposta certa, concluir tarefas e escalar para uma pessoa quando o caso exige julgamento humano. Sempre que a resposta é direcionada a uma pessoa específica, a responsabilidade é do Direct.

**Valor principal:** velocidade de resolução. Cada conversa individual deixa de ser apenas atendimento e passa a alimentar conhecimento para futuras interações.

**Atuacoes**

- Chat de suporte em sites, web apps e áreas logadas.
- Assistente de conversão em e-commerce para entrega, troca, garantia, pagamento e compatibilidade.
- Fluxos de onboarding e ativação em SaaS, edtech, fintech, healthtech e marketplaces.
- Portais de cliente para suporte procedural, segunda via, planos, assinaturas e solicitações.
- Apps mobile com ajuda dentro da jornada do usuário.
- WhatsApp Business, DMs e outros canais de mensageria usados para atendimento operacional individual.
- Continuação privada de uma interação pública quando a empresa precisa tratar dados pessoais, exceções ou negociação com uma pessoa específica.
- Suporte interno para vendas, sucesso do cliente, implantação, suporte técnico e parceiros.
- Handoff para humano ou ticket quando o caso exige decisão, exceção ou negociação.

### 4. Broadcast

A camada de captura e resposta em canais públicos, comunitários e um-para-muitos.

Ele organiza comentários, menções, dúvidas sociais e discussões de comunidade, identifica padrões recorrentes e transforma interações dispersas em sinais acionáveis. Sempre que a resposta pode ser vista por muitas pessoas, a responsabilidade é do Broadcast.

**Valor principal:** presença multicanal com aprendizado. A empresa responde onde as pessoas perguntam e usa essas interações para melhorar produto, comunicação e suporte.

**Atuacoes**

- Instagram e Facebook para comentários, menções, dúvidas em posts, anúncios e respostas públicas.
- YouTube para comentários em vídeos, tutoriais, lançamentos, reviews e lives.
- WhatsApp Channels, grupos, comunidades e outros espaços compartilhados onde a resposta pode ser vista por muitas pessoas.
- Comunidades próprias em Discord, GitHub Discussions, Discourse, Slack, Teams e fóruns privados ou públicos.
- Lançamentos, campanhas e eventos que geram objeções, dúvidas e comentários em massa.
- Marcas DTC, e-commerce e creators com alto volume de perguntas repetidas.
- SaaS e produtos B2B que precisam entender objeções, pedidos de melhoria e dúvidas de adoção.
- Voz do cliente para produto, marketing, suporte e liderança.

### 5. Trust

A camada de confiança para decisões, validações e participação.

Ele cria processos claros para revisar respostas, validar contribuições, priorizar demandas e registrar decisões quando transparência e auditoria importam.

**Valor principal:** confiança. Comunidades, clientes e times internos conseguem entender de onde veio uma resposta, por que uma decisão foi tomada e qual versão deve ser usada.

**Atuacoes**

- Portais de roadmap, priorização de produto e conselhos de clientes.
- Comunidades de membros, associações, cooperativas, creator communities e ecossistemas digitais.
- Programas beta, comunidades de parceiros e grupos consultivos.
- Programas de grants, editais, hackathons, incubadoras e aceleradoras.
- Decisões de moderação, políticas de comunidade e eleição de representantes.
- Comunidades open source para propostas, discussões, prioridades e decisões públicas.
- DAOs e ecossistemas web3 quando verificabilidade pública for um requisito real.
- Ambientes regulados ou sensíveis que precisam de histórico, evidência e auditoria.

## Fluxo Central Do Negócio

1. Uma pergunta aparece em um canal próprio, conversa direta, rede social ou comunidade.
2. O sistema identifica se já existe uma resposta aprovada.
3. Se existir, a resposta certa é entregue no canal certo.
4. Se não existir, a interação vira uma lacuna de conhecimento.
5. A lacuna é revisada, validada e transformada em resposta reutilizável.
6. A nova resposta passa a resolver futuras conversas, comentários, buscas e decisões.
7. Cada interação gera sinais sobre demanda, fricção, objeções, prioridades e confiança.

Esse é o ciclo que cria valor acumulado: **pergunta -> resolução -> conhecimento -> distribuição -> aprendizado -> melhor próxima resposta.**

## Como Os Módulos Se Conectam

Os módulos do BaseFaq funcionam como partes de um mesmo sistema. Tenant controla identidade, acesso, cobrança e roteamento de dados. QnA, Direct, Broadcast e Trust executam os fluxos de produto e alimentam o ciclo de conhecimento.

A tabela abaixo mostra responsabilidade de módulo, não responsabilidade de times humanos. Produto, marketing, suporte, comunidade e liderança podem participar da decisão operacional, mas não são módulos do BaseFaq.

| Origem da interação | Módulo de entrada e entrega no canal | Conhecimento reutilizável | Validação e governança | Resultado de negócio |
| --- | --- | --- | --- | --- |
| Pergunta em site, app ou portal | QnA | QnA | Trust, quando necessário | Autoatendimento consistente e menos demanda repetida |
| Conversa 1:1 de suporte | Direct | QnA | Trust, quando necessário | Resolução mais rápida e melhoria contínua da base |
| Comentário em rede social | Broadcast | QnA | Trust, quando necessário | Resposta pública consistente e captura de sinais de mercado |
| Pergunta em comunidade | Broadcast | QnA | Trust | Conhecimento comunitário convertido em resposta confiável |
| Pedido de melhoria ou objeção recorrente em espaço compartilhado | Broadcast | QnA | Trust | Priorização melhor e comunicação mais clara |
| Decisão coletiva ou proposta | Trust | QnA | Trust | Decisão auditável, explicável e reutilizável |

## Fluxos De Negócio Possíveis

Os fluxos abaixo mostram como o sistema cria valor em diferentes momentos da jornada: resolução, aprendizado, conversão, comunidade, governança e escala operacional.

### Resolução E Conhecimento

| Processo | Situação | Fluxo | Valor |
| --- | --- | --- | --- |
| 1. Pergunta Conhecida -> Resposta Imediata | Um cliente pergunta algo que já foi aprovado antes. | QnA -> canal próprio, Direct ou Broadcast -> resposta entregue -> dados de uso retornam para aprendizado. | reduz tickets, aumenta consistência e evita que times diferentes respondam a mesma dúvida de formas diferentes. |
| 2. Pergunta Nova -> Ativo Reutilizável | Uma pergunta aparece sem resposta aprovada. | Direct captura a lacuna quando a pergunta é privada; Broadcast captura quando a pergunta ou resposta é visível por muitas pessoas -> time revisa -> QnA publica a resposta -> todos os canais passam a usar a mesma resposta. | cada falha de cobertura vira conhecimento permanente. |
| 3. Atendimento 1:1 -> Conhecimento Para Todos | Um agente ou o Direct resolve uma dúvida individual. | Direct resolve ou escala -> resposta final é revisada -> QnA armazena -> futuros usuários recebem a mesma resposta sem abrir novo contato. | o custo de uma conversa individual gera retorno em escala. |

### Social, Comunidade E Conversão

| Processo | Situação | Fluxo | Valor |
| --- | --- | --- | --- |
| 4. Comentário Público -> Resposta Segura | Alguém faz uma pergunta ou objeção em comentário público. | Broadcast captura -> QnA sugere resposta aprovada -> Broadcast responde no canal -> variações da pergunta alimentam a base. | aumenta velocidade de resposta sem sacrificar controle de marca. |
| 5. Rede Social -> Insight De Mercado | Vários comentários repetem a mesma dúvida, medo ou objeção. | Broadcast agrupa sinais -> QnA cria resposta ou narrativa -> Direct usa em conversas -> marketing ajusta campanha ou mensagem. | transforma ruído social em aprendizado comercial. |
| 6. Comunidade -> Resposta Verificada | Usuários respondem perguntas uns dos outros. | Broadcast captura a melhor resposta -> Trust valida por moderação, voto ou revisão -> QnA publica como resposta confiável. | aproveita conhecimento da comunidade sem abrir mão de confiança. |
| 7. Página De Produto -> Conversão | Um comprador tem uma dúvida antes de comprar. | QnA responde na página -> se a dúvida for nova em conversa privada, Direct captura; se aparecer em comentário, comunidade ou canal compartilhado, Broadcast captura -> QnA publica para futuros compradores. | remove objeções no momento de decisão e reduz abandono. |
| 8. Onboarding -> Ativação | Um usuário não sabe como concluir uma configuração ou primeiro uso. | Direct guia a pessoa -> respostas aprovadas explicam passos -> perguntas repetidas viram melhorias no QnA e nas telas do produto. | acelera ativação e reduz fricção inicial. |

### Suporte Operacional

| Processo | Situação | Fluxo | Valor |
| --- | --- | --- | --- |
| 9. Baixa Confiança -> Escalação Humana | O sistema não tem segurança para responder. | Direct escala para humano -> humano resolve -> resposta é revisada -> QnA passa a cobrir o caso. | cria um caminho controlado para melhorar cobertura sem respostas improvisadas. |
| 10. Ticket Recorrente -> Redução De Volume | O mesmo motivo de contato aparece muitas vezes. | Direct e Broadcast detectam repetição -> QnA cria resposta padrão -> canais próprios e sociais passam a responder antes do ticket. | ataca a causa do volume, não apenas a fila. |

### Mercado, Produto E Decisão

| Processo | Situação | Fluxo | Valor |
| --- | --- | --- | --- |
| 11. Campanha -> Mapa De Objeções | Uma campanha gera comentários, dúvidas e resistência. | Broadcast captura reações -> QnA estrutura respostas -> Direct usa em conversas privadas -> marketing ajusta criativos, landing pages e argumentos. | melhora a performance de campanhas a partir das perguntas reais do mercado. |
| 12. Feedback De Produto -> Decisão De Roadmap | Usuários pedem melhorias, reportam confusão ou questionam limites do produto. | Broadcast identifica temas -> Trust prioriza com votos, conselhos ou regras de decisão -> QnA publica status, racional e próximos passos. | transforma demanda dispersa em decisão transparente. |
| 13. Proposta Comunitária -> Decisão Auditável | Uma comunidade, conselho ou grupo de clientes precisa decidir algo. | Trust registra proposta -> participantes validam ou votam -> resultado é publicado no QnA -> Direct responde dúvidas privadas e Broadcast responde perguntas visíveis pela comunidade. | aumenta confiança porque a decisão não fica escondida em conversas soltas. |
| 14. Mudança De Política -> Atualização Multicanal | Uma regra de preço, entrega, segurança, privacidade ou uso muda. | QnA atualiza a resposta aprovada -> todos os canais recebem a nova versão -> Trust guarda histórico de aprovação quando necessário. | reduz risco de respostas antigas continuarem circulando. |

### Escala Organizacional

| Processo | Situação | Fluxo | Valor |
| --- | --- | --- | --- |
| 15. Time Interno -> Resposta Externa Consistente | Vendas, suporte, sucesso do cliente e parceiros precisam responder a mesma pergunta. | QnA centraliza a resposta -> Direct ajuda o time interno -> Broadcast garante consistência em comentários e comunidades. | alinha a empresa inteira em torno da mesma verdade operacional. |
| 16. Parceiros E Revendas -> Conhecimento Controlado | Parceiros precisam de respostas aprovadas, mas nem sempre devem ver tudo. | QnA entrega respostas por audiência -> Direct atende parceiros em portal ou canal privado -> lacunas retornam para revisão. | escala suporte de canal sem perder controle sobre permissão e mensagem. |
| 17. Documentação Técnica -> Resolução De Desenvolvedores | Desenvolvedores fazem perguntas sobre APIs, SDKs, erros ou limites. | Perguntas entram por docs, comunidade ou suporte -> QnA publica resposta versionada -> Direct orienta casos individuais -> Broadcast captura discussões técnicas recorrentes. | reduz fricção para adoção técnica e transforma diagnóstico técnico em conhecimento. |
| 18. Expansão Internacional -> Aprendizado Local | Um novo mercado faz perguntas diferentes ou usa outras palavras. | Broadcast e Direct capturam variações locais -> QnA adapta respostas por idioma, país e contexto -> canais locais passam a responder com consistência. | evita copiar respostas de um mercado para outro sem aprender com a demanda real. |
| 19. Risco, Compliance E Auditoria -> Prova De Decisão | Um tema exige controle, revisão ou evidências. | QnA guarda resposta aprovada -> Trust registra validação, regra e histórico -> Direct e Broadcast usam apenas a versão permitida. | reduz risco operacional em temas sensíveis. |
| 20. Liderança -> Inteligência Operacional | Diretoria quer entender o que clientes, usuários e comunidades estão perguntando. | Broadcast, Direct e QnA consolidam sinais -> temas recorrentes viram relatórios -> decisões geram novas respostas, campanhas ou propostas. | transforma interações do dia a dia em insumo para estratégia. |

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
- [Slack App Home](https://api.slack.com/start/designing/tabs) e [Slack Shortcuts](https://api.slack.com/start/designing/shortcuts) para levar conhecimento e ações para o fluxo de trabalho.
- [Microsoft Teams tabs](https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/conversational-tabs) e [message extensions](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/in-depth-guides/message-extensions/overview) para superfícies internas de trabalho.
- [GitHub Discussions](https://docs.github.com/en/free-pro-team@latest/discussions/collaborating-with-your-community-using-discussions/about-discussions) para perguntas, respostas, polls e decisões abertas.
- [Discourse Solved](https://meta.discourse.org/t/discourse-solved/30155) e [Discourse Topic Voting](https://meta.discourse.org/t/discourse-topic-voting/40121) para suporte colaborativo e priorização por votos.
- [Discord bots](https://discord.com/developers/docs/bots), [application commands](https://docs.discord.com/developers/interactions/application-commands) e [webhooks](https://docs.discord.com/developers/platform/webhooks) para comunidades próprias e automações.
- [YouTube CommentThreads API](https://developers.google.com/youtube/v3/docs/commentThreads) para ingestão e resposta em comentários de vídeo.
- [TikTok Research API](https://developers.tiktok.com/products/research-api/) e [About Research Tools](https://developers.tiktok.com/doc/about-research-api/) como evidência de que TikTok ainda deve ser tratado como conector restrito.
- [WhatsApp Cloud API overview](https://developers.facebook.com/docs/whatsapp/cloud-api/overview) para mensageria operacional em escala.
- [Snapshot docs](https://docs.snapshot.box/) para voto off-chain, mensagens assinadas e modelos flexíveis de governança.
- [W3C WebAuthn Level 3](https://www.w3.org/TR/webauthn-3/) e [NIST SP 800-63-4](https://www.nist.gov/publications/nist-sp-800-63-4-digital-identity-guidelines) para autenticação forte e arquitetura de confiança.
