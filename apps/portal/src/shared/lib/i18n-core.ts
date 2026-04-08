import {
  DEFAULT_PORTAL_LANGUAGE,
  normalizePortalLanguage,
} from "@/shared/lib/language";

type TranslationValues = Record<
  string,
  string | number | boolean | null | undefined
>;

type TranslationDictionary = Record<string, string>;

let currentPortalLanguage = DEFAULT_PORTAL_LANGUAGE;

const ptBrMessages: TranslationDictionary = {
  "Activate": "Ativar",
  "Activate this record?": "Ativar este registro?",
  "Active": "Ativo",
  "Add or request access to a workspace to enable Portal features.":
    "Adicione ou solicite acesso a um workspace para habilitar os recursos do Portal.",
  "Applied theme right now": "Tema aplicado agora",
  "Appearance": "Aparência",
  "Audience": "Audience",
  "Available workspaces": "Workspaces disponíveis",
  "A new public client key was generated.":
    "Uma nova chave pública do client foi gerada.",
  "AI": "IA",
  "AI provider credentials stored for the current workspace.":
    "As credenciais do provedor de IA foram salvas para o workspace atual.",
  "Actions": "Ações",
  "Auth runtime": "Runtime de autenticação",
  "BaseFAQ Portal": "BaseFAQ Portal",
  "Back to sign in": "Voltar para entrar",
  "Billing": "Cobrança",
  "Cancel": "Cancelar",
  "Callback": "Callback",
  "Cannot reach the {serviceLabel}.":
    "Não foi possível acessar {serviceLabel}.",
  "Cannot reach the FAQ API.":
    "Não foi possível acessar a API de FAQ.",
  "Cannot reach the Tenant API.":
    "Não foi possível acessar a API de Tenant.",
  "Clear selection": "Limpar seleção",
  "Confirm": "Confirmar",
  "Continue with Auth0": "Continuar com Auth0",
  "Current selection": "Seleção atual",
  "Current theme": "Tema atual",
  "Dark mode": "Modo escuro",
  "Dashboard": "Painel",
  "Deactivate": "Desativar",
  "Deactivate this record?": "Desativar este registro?",
  "Email claim unavailable": "Claim de e-mail indisponível",
  "Everything is in place": "Tudo está pronto",
  "Field details": "Detalhes do campo",
  "FAQ API": "API de FAQ",
  "FAQ created.": "FAQ criada.",
  "FAQ deleted.": "FAQ removida.",
  "FAQ updated.": "FAQ atualizada.",
  "First page": "Primeira página",
  "Forgot password": "Esqueci minha senha",
  "Generation requested. Correlation ID: {correlationId}":
    "Geração solicitada. Correlation ID: {correlationId}",
  "Go to dashboard": "Ir para o painel",
  "Go to dashboard settings": "Abrir configurações do painel",
  "Initializing session": "Inicializando sessão",
  "item": "item",
  "items": "itens",
  "Language": "Idioma",
  "Last page": "Última página",
  "Matches": "Resultados",
  "Manage your BaseFAQ workspace":
    "Gerencie seu workspace do BaseFAQ",
  "Member added to the workspace.":
    "Membro adicionado ao workspace.",
  "Member removed from the workspace.":
    "Membro removido do workspace.",
  "Members": "Membros",
  "Metric details": "Detalhes da métrica",
  "More information": "Mais informações",
  "Navigation": "Navegação",
  "Next page": "Próxima página",
  "Next: {label}": "Próximo: {label}",
  "No email in token": "Nenhum e-mail no token",
  "No results found.": "Nenhum resultado encontrado.",
  "No time zones found.": "Nenhum fuso horário encontrado.",
  "No workspaces available": "Nenhum workspace disponível",
  "No workspaces found.": "Nenhum workspace encontrado.",
  "Notifications": "Notificações",
  "Open Auth0 reset flow": "Abrir fluxo de redefinição do Auth0",
  "Open billing": "Abrir cobrança",
  "Open FAQ workspace": "Abrir área de FAQs",
  "Options": "Opções",
  "Page {page}": "Página {page}",
  "Page details": "Detalhes da página",
  "Password reset": "Redefinição de senha",
  "Portal": "Portal",
  "Portal command search": "Busca de comandos do portal",
  "Portal login": "Login do portal",
  "Portal delegates password recovery to Auth0. Keep this route as a UI placeholder unless the identity provider requires a custom callback screen here.":
    "O Portal delega a recuperação de senha ao Auth0. Mantenha esta rota como placeholder de UI, a menos que o provedor de identidade exija uma tela de callback personalizada aqui.",
  "Portal user": "Usuário do portal",
  "Preference": "Preferência",
  "Previous page": "Página anterior",
  "Profile": "Perfil",
  "Profile settings": "Configurações do perfil",
  "Profile settings saved.": "Configurações do perfil salvas.",
  "Refine view": "Refinar visualização",
  "Request failed.": "A solicitação falhou.",
  "Reset password": "Redefinir senha",
  "Rows per page": "Linhas por página",
  "Route not found": "Rota não encontrada",
  "Q&A item created.": "Item de Q&A criado.",
  "Q&A item deleted.": "Item de Q&A removido.",
  "Q&A item updated.": "Item de Q&A atualizado.",
  "Search...": "Buscar...",
  "Search portal": "Buscar no portal",
  "Search is route-launch only in this foundation build.":
    "A busca neste build base serve apenas para abrir rotas.",
  "Search time zones": "Buscar fusos horários",
  "Search workspaces...": "Buscar workspaces...",
  "Searching results...": "Buscando resultados...",
  "Security": "Segurança",
  "Select a workspace to activate Portal features.":
    "Selecione um workspace para ativar os recursos do Portal.",
  "Select a workspace to continue.":
    "Selecione um workspace para continuar.",
  "Select an option": "Selecione uma opção",
  "Selected": "Selecionado",
  "Select workspace": "Selecionar workspace",
  "Session expired. Sign in again.":
    "Sua sessão expirou. Entre novamente.",
  "Settings": "Configurações",
  "Sign in": "Entrar",
  "Sign in again to continue.":
    "Entre novamente para continuar.",
  "Sign in to manage FAQs, Q&A items, sources, billing, and AI settings.":
    "Entre para gerenciar FAQs, itens de Q&A, fontes, cobrança e configurações de IA.",
  "Sign in to your tenant workspace":
    "Entre no workspace do seu tenant",
  "Sign out": "Sair",
  "Source created.": "Fonte criada.",
  "Source deleted.": "Fonte removida.",
  "Source updated.": "Fonte atualizada.",
  "Source": "Fonte",
  "Start here": "Comece aqui",
  "steps complete": "etapas concluídas",
  "Stored locally for the portal app":
    "Armazenado localmente para o app do portal",
  "Switch workspace": "Trocar workspace",
  "Table details": "Detalhes da tabela",
  "The latest request failed. Try again.":
    "A última solicitação falhou. Tente novamente.",
  "The Portal app authenticates against Auth0 and then calls only the Portal-side Tenant and FAQ APIs already present in this repository.":
    "O app Portal autentica com Auth0 e depois chama apenas as APIs Tenant e FAQ do lado do Portal já presentes neste repositório.",
  "There is no Portal-owned password reset endpoint in the repo. This flow should stay with the external identity provider.":
    "Não existe endpoint de redefinição de senha mantido pelo Portal neste repositório. Esse fluxo deve permanecer no provedor externo de identidade.",
  "The command surface is intentionally lean for now. Use it as a quick launcher while full global search stays a placeholder.":
    "A superfície de comandos ainda é enxuta de propósito. Use-a como lançador rápido enquanto a busca global completa continua como placeholder.",
  "The configured `VITE_AUTH0_CLIENT_ID` matches the Swagger UI Auth0 client from the .NET APIs. That application is documented with Swagger callback URLs only, so Portal login will fail unless Auth0 also allows {callbackUrl}.":
    "O `VITE_AUTH0_CLIENT_ID` configurado corresponde ao cliente Auth0 do Swagger UI das APIs .NET. Essa aplicação foi documentada apenas com URLs de callback do Swagger, então o login do Portal falhará a menos que o Auth0 também permita {callbackUrl}.",
  "The request is invalid.": "A solicitação é inválida.",
  "The requested record was not found.":
    "O registro solicitado não foi encontrado.",
  "The submitted data is invalid.":
    "Os dados enviados são inválidos.",
  "This change conflicts with the current data.":
    "Essa alteração entra em conflito com os dados atuais.",
  "This route is outside the Portal surface or has not been mapped yet.":
    "Esta rota está fora da superfície do Portal ou ainda não foi mapeada.",
  "Time zone": "Fuso horário",
  "Tenant API": "API de Tenant",
  "Toggle dark mode": "Alternar modo escuro",
  "Try again": "Tentar novamente",
  "Use {timeZone}": "Usar {timeZone}",
  "User profile": "Perfil do usuário",
  "Update your name and contact info.":
    "Atualize seu nome e informações de contato.",
  "Update the details your team sees across the portal.":
    "Atualize os detalhes que sua equipe vê no portal.",
  "Workspace settings saved.": "Configurações do workspace salvas.",
  "0 of 0 items": "0 de 0 itens",
  "{count} available": "{count} disponíveis",
  "{serviceLabel} is throttling requests right now.":
    "{serviceLabel} está limitando requisições neste momento.",
  "{serviceLabel} is unavailable right now.":
    "{serviceLabel} está indisponível no momento.",
  "{serviceLabel} request failed.":
    "A solicitação para {serviceLabel} falhou.",
  "{start}-{end} of {totalCount} {summaryLabel}":
    "{start}-{end} de {totalCount} {summaryLabel}",
  "{title} details": "Detalhes de {title}",
  "Workspace": "Workspace",
  "Workspace overview and usage signals":
    "Visão geral do workspace e sinais de uso",
  "Your session expired. Sign in again.":
    "Sua sessão expirou. Entre novamente.",
  "You are offline.": "Você está offline.",
  "You do not have access to this workspace.":
    "Você não tem acesso a este workspace.",
  "available": "disponíveis",
};

const messagesByLanguage: Record<string, TranslationDictionary> = {
  "pt-BR": ptBrMessages,
};

function formatMessage(message: string, values?: TranslationValues) {
  if (!values) {
    return message;
  }

  return message.replace(/\{(\w+)\}/g, (_match, key) => {
    const value = values[key];
    return value === undefined || value === null ? "" : String(value);
  });
}

export function setCurrentPortalLanguage(language?: string | null) {
  currentPortalLanguage = normalizePortalLanguage(language);
}

export function getCurrentPortalLanguage() {
  return currentPortalLanguage;
}

export function translateText(
  input: string,
  values?: TranslationValues,
  language = currentPortalLanguage,
) {
  const normalizedLanguage = normalizePortalLanguage(language);
  const dictionary = messagesByLanguage[normalizedLanguage];
  const message = dictionary?.[input] ?? input;
  return formatMessage(message, values);
}
