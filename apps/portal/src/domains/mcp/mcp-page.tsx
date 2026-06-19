import { CheckCircle2, Clipboard, PlugZap, ShieldCheck } from "lucide-react";
import { useMemo, useState } from "react";
import { PageHeader, PageSurface } from "@/shared/layout/page-layouts";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import {
  ActionButton,
  ActionPanel,
  Badge,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui";
import type { McpAvailability, McpReadinessItem, McpToolGroup } from "@/domains/mcp/types";

const qnaTools = [
  "qna_list_spaces",
  "qna_get_space",
  "qna_list_questions",
  "qna_get_question",
  "qna_search",
  "qna_list_sources",
  "qna_get_source",
  "qna_create_question",
  "qna_create_answer",
  "qna_activate_answer",
  "qna_create_source",
  "qna_link_question_source",
  "qna_link_answer_source",
];

const tenantTools = [
  "tenant_list_workspaces",
  "tenant_get_client_key",
  "tenant_list_members",
  "tenant_get_profile",
  "tenant_get_billing_summary",
  "tenant_get_subscription",
];

const toolGroups: McpToolGroup[] = [
  {
    module: "QnA",
    tools: qnaTools,
    boundary: "Query and command",
    availability: "available",
  },
  {
    module: "Tenant",
    tools: tenantTools,
    boundary: "Query only",
    availability: "available",
  },
];

const readinessItems: McpReadinessItem[] = [
  {
    module: "Source Generation",
    state: "future",
    note: "Source Generation belongs to QnA SourceGeneration.",
  },
  {
    module: "Direct",
    state: "future",
    note: "Waiting for module CQRS surface.",
  },
  {
    module: "Broadcast",
    state: "future",
    note: "Waiting for module CQRS surface.",
  },
  {
    module: "Trust",
    state: "future",
    note: "Waiting for module CQRS surface.",
  },
];

const prompts = [
  {
    name: "qna_assistant",
    description:
      "Checks existing QnA first and creates Draft/Internal content only.",
  },
  {
    name: "tenant_assistant",
    description: "Keeps Tenant workspace assistance read-only.",
  },
];

function AvailabilityBadge({ availability }: { availability: McpAvailability }) {
  const variant = availability === "available" ? "success" : "warning";
  const label = availability === "available" ? "Available" : "Future";

  return (
    <Badge variant={variant} appearance="outline">
      {label}
    </Badge>
  );
}

export function McpPage() {
  const { t } = usePortalI18n();
  const [copied, setCopied] = useState(false);
  const clientConfig = useMemo(
    () =>
      JSON.stringify(
        {
          mcpServers: {
            querify: {
              command: "dotnet",
              args: [
                "run",
                "--project",
                "dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj",
              ],
              env: {
                "ConnectionStrings__TenantDb": "<tenant-db-connection-string>",
                "McpServer__ServiceUserId": "<service-user-guid>",
                "McpServer__DefaultTenantId": "<tenant-guid>",
                "McpServer__EnableWriteTools": "false",
              },
            },
          },
        },
        null,
        2,
      ),
    [],
  );

  const handleCopyConfig = () => {
    if (!navigator.clipboard?.writeText) {
      return;
    }

    void navigator.clipboard
      .writeText(clientConfig)
      .then(() => {
        setCopied(true);
        window.setTimeout(() => setCopied(false), 1600);
      })
      .catch(() => undefined);
  };

  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="MCP"
        description="Model Context Protocol access for this workspace."
        actions={
          <ActionButton type="button" onClick={handleCopyConfig}>
            {copied ? (
              <CheckCircle2 className="size-4" />
            ) : (
              <Clipboard className="size-4" />
            )}
            {t(copied ? "Copied" : "Copy client config")}
          </ActionButton>
        }
      />

      <ActionPanel layout="bar" title="Server status">
        <p className="text-sm text-muted-foreground">
          <span className="font-medium text-foreground">
            {t("Local stdio MVP")}
          </span>
        </p>
        <Badge variant="warning" appearance="outline">
          {t("Write tools disabled by default")}
        </Badge>
        <Badge variant="info" appearance="outline">
          {t("Tenant context required")}
        </Badge>
        <Badge variant="secondary" appearance="outline">
          {t("Service user required")}
        </Badge>
      </ActionPanel>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.35fr)_minmax(320px,0.65fr)]">
        <Card>
          <CardHeader>
            <CardHeading className="flex min-w-0 flex-row items-center gap-2">
              <PlugZap className="size-4 text-primary" />
              <CardTitle>{t("Available tools")}</CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>{t("Module")}</TableHead>
                    <TableHead>{t("Tools")}</TableHead>
                    <TableHead>{t("Boundary")}</TableHead>
                    <TableHead>{t("Availability")}</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {toolGroups.map((group) => (
                    <TableRow key={group.module}>
                      <TableCell className="font-medium">{group.module}</TableCell>
                      <TableCell>
                        <div className="flex max-w-xl flex-wrap gap-1.5">
                          {group.tools.map((tool) => (
                            <code
                              key={tool}
                              className="max-w-full break-all rounded bg-muted px-1.5 py-1 text-[0.72rem] text-muted-foreground"
                            >
                              {tool}
                            </code>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell>{t(group.boundary)}</TableCell>
                      <TableCell>
                        <AvailabilityBadge availability={group.availability} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>

        <div className="min-w-0 space-y-5">
          <Card>
            <CardHeader>
              <CardHeading className="flex min-w-0 flex-row items-center gap-2">
                <ShieldCheck className="size-4 text-primary" />
                <CardTitle>{t("Agent prompts")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {prompts.map((prompt) => (
                <div
                  key={prompt.name}
                  className="rounded-lg border border-border/70 p-3"
                >
                  <code className="break-all text-xs font-semibold">
                    {prompt.name}
                  </code>
                  <p className="mt-2 text-sm leading-6 text-muted-foreground">
                    {t(prompt.description)}
                  </p>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{t("Future readiness")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-2">
              {readinessItems.map((item) => (
                <div
                  key={item.module}
                  className="flex min-w-0 items-start justify-between gap-3 rounded-lg border border-border/70 p-3"
                >
                  <div className="min-w-0">
                    <p className="text-sm font-medium">{item.module}</p>
                    <p className="mt-1 text-xs leading-5 text-muted-foreground">
                      {t(item.note)}
                    </p>
                  </div>
                  <AvailabilityBadge availability={item.state} />
                </div>
              ))}
            </CardContent>
          </Card>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>{t("Client connection")}</CardTitle>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-sm leading-6 text-muted-foreground">
            {t("Use this local stdio snippet in MCP clients.")}
          </p>
          <pre className="max-h-[420px] overflow-auto rounded-lg border border-border/70 bg-muted/60 p-4 text-xs leading-5 text-muted-foreground">
            <code>{clientConfig}</code>
          </pre>
        </CardContent>
      </Card>
    </PageSurface>
  );
}
