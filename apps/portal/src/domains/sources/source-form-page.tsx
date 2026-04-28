import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { X } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  SourceKind,
  VisibilityScope,
  sourceKindLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  useCreateSource,
  useSource,
  useUpdateSource,
} from "@/domains/sources/hooks";
import {
  sourceFormSchema,
  type SourceFormValues,
} from "@/domains/sources/schemas";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
} from "@/shared/layout/page-layouts";
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
  Form,
  FormCardSkeleton,
  FormSectionHeading,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { ErrorState } from "@/shared/ui/placeholder-state";
import {
  SelectField,
  SwitchField,
  TextField,
  TextareaField,
} from "@/shared/ui/form-fields";
import { translateText } from "@/shared/lib/i18n-core";

export function SourceFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const sourceQuery = useSource(mode === "edit" ? id : undefined);
  const createSource = useCreateSource();
  const updateSource = useUpdateSource(id ?? "");

  const form = useForm<SourceFormValues>({
    resolver: zodResolver(sourceFormSchema),
    defaultValues: {
      kind: SourceKind.Article,
      locator: "",
      label: "",
      contextNote: "",
      externalId: "",
      language: "",
      mediaType: "",
      checksum: "",
      metadataJson: "",
      visibility: VisibilityScope.Internal,
      allowsPublicCitation: false,
      allowsPublicExcerpt: false,
      isAuthoritative: false,
      capturedAtUtc: "",
      markVerified: false,
    },
  });

  useEffect(() => {
    if (!sourceQuery.data) {
      return;
    }

    form.reset({
      kind: sourceQuery.data.kind,
      locator: sourceQuery.data.locator,
      label: sourceQuery.data.label ?? "",
      contextNote: sourceQuery.data.contextNote ?? "",
      externalId: sourceQuery.data.externalId ?? "",
      language: sourceQuery.data.language,
      mediaType: sourceQuery.data.mediaType ?? "",
      checksum: sourceQuery.data.checksum,
      metadataJson: sourceQuery.data.metadataJson ?? "",
      visibility: sourceQuery.data.visibility,
      allowsPublicCitation: sourceQuery.data.allowsPublicCitation,
      allowsPublicExcerpt: sourceQuery.data.allowsPublicExcerpt,
      isAuthoritative: sourceQuery.data.isAuthoritative,
      capturedAtUtc: sourceQuery.data.capturedAtUtc ?? "",
      markVerified: false,
    });
  }, [form, sourceQuery.data]);

  const isSubmitting = createSource.isPending || updateSource.isPending;
  const backTo = mode === "edit" && id ? `/app/sources/${id}` : "/app/sources";

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === "create" ? "New source" : "Edit source"}
          description="Capture locator, trust metadata, and public-citation rules for reusable evidence."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && sourceQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Good sources are durable, clearly classified, and explicit about what can be shown publicly.",
                    )}
                    label={translateText("Quick notes details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Kinds",
                    value:
                      "Article, web page, ticket, repository, chat, and more",
                  },
                  { label: "Visibility", value: "Internal to public indexed" },
                  {
                    label: "Trust",
                    value: "Public citation and excerpt flags matter",
                  },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {sourceQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          error={sourceQuery.error}
          retry={() => void sourceQuery.refetch()}
        />
      ) : mode === "edit" && sourceQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Source details")}</span>
                <ContextHint
                  content={translateText(
                    "Start with the locator and classification, then capture trust, public-citation, and verification metadata.",
                  )}
                  label={translateText("Form details")}
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form
                className="space-y-5"
                onSubmit={form.handleSubmit(async (values) => {
                  const body = {
                    ...values,
                    label: values.label || undefined,
                    contextNote: values.contextNote || undefined,
                    externalId: values.externalId || undefined,
                    mediaType: values.mediaType || undefined,
                    metadataJson: values.metadataJson || undefined,
                    capturedAtUtc: values.capturedAtUtc || undefined,
                    kind: Number(values.kind) as SourceKind,
                    visibility: Number(values.visibility) as VisibilityScope,
                  };

                  if (mode === "create") {
                    const createdId = await createSource.mutateAsync(body);
                    navigate(`/app/sources/${createdId}`);
                    return;
                  }

                  await updateSource.mutateAsync(body);
                  navigate(`/app/sources/${id}`);
                })}
              >
                <FormSectionHeading
                  title="Core identity"
                  description="Define the locator and source kind first so the record can be reused across threads."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <SelectField
                    control={form.control}
                    name="kind"
                    label="Source kind"
                    description="The type of evidence or reusable reference this source represents."
                    options={Object.entries(sourceKindLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                  <SelectField
                    control={form.control}
                    name="visibility"
                    label="Visibility"
                    description="Controls which audiences can see or reuse this source."
                    options={Object.entries(visibilityScopeLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                </div>
                <TextField
                  control={form.control}
                  name="locator"
                  label="Locator"
                  description="Use the canonical URL, path, repository URI, ticket reference, or document locator."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="label"
                    label="Label"
                    description="Human-readable name shown when operators choose this source."
                  />
                  <TextField
                    control={form.control}
                    name="contextNote"
                    label="Context note"
                    description="Internal note describing why this source matters or where it applies."
                  />
                  <TextField
                    control={form.control}
                    name="externalId"
                    label="External ID"
                    description="Identifier from the upstream connector, repository, or source system."
                  />
                  <TextField
                    control={form.control}
                    name="language"
                    label="Language"
                    description="Locale code for the source content when known."
                  />
                  <TextField
                    control={form.control}
                    name="mediaType"
                    label="Media type"
                    description="MIME type or source format such as text/html, application/pdf, or text/markdown."
                  />
                  <TextField
                    control={form.control}
                    name="checksum"
                    label="Checksum"
                    description="Digest used to detect source content changes."
                  />
                  <TextField
                    control={form.control}
                    name="capturedAtUtc"
                    label="Captured at (ISO)"
                    description="Optional ISO timestamp such as 2026-04-16T12:00:00Z."
                  />
                </div>
                <TextareaField
                  control={form.control}
                  name="metadataJson"
                  label="Metadata JSON"
                  rows={5}
                  description="Optional structured metadata for connectors, ingestion, or future audit needs."
                />
                <FormSectionHeading
                  title="Trust and public use"
                  description="Control whether the source can be cited publicly and whether it should be treated as authoritative."
                />
                <div className="grid gap-4 md:grid-cols-2">
                  <SwitchField
                    control={form.control}
                    name="allowsPublicCitation"
                    label="Allows public citation"
                    description="Allow public answers to cite this source as a reference."
                  />
                  <SwitchField
                    control={form.control}
                    name="allowsPublicExcerpt"
                    label="Allows public excerpt"
                    description="Allow public answers to include short excerpts from this source."
                  />
                  <SwitchField
                    control={form.control}
                    name="isAuthoritative"
                    label="Authoritative"
                    description="Treat this source as a trusted canonical reference."
                  />
                  <SwitchField
                    control={form.control}
                    name="markVerified"
                    label="Mark verified now"
                    description="Set the verification timestamp when saving this source."
                  />
                </div>
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {translateText(
                      mode === "create" ? "Create source" : "Save changes",
                    )}
                  </Button>
                  <Button asChild variant="outline">
                    <Link to={backTo}>
                      <X className="size-4" />
                      {translateText("Cancel")}
                    </Link>
                  </Button>
                </div>
              </form>
            </Form>
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
