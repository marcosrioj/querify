import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm, type UseFormReturn } from "react-hook-form";
import { Braces, X } from "lucide-react";
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
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormSetupProgressCard,
  FormCardSkeleton,
  FormSectionHeading,
  hasSetupText,
  hasSetupValue,
  SidebarSummarySkeleton,
  Textarea,
} from "@/shared/ui";
import { ErrorState } from "@/shared/ui/placeholder-state";
import {
  SearchSelectField,
  SelectField,
  SwitchField,
  TextField,
  TextareaField,
} from "@/shared/ui/form-fields";
import { translateText } from "@/shared/lib/i18n-core";
import {
  DEFAULT_PORTAL_LANGUAGE,
  getStoredPortalLanguage,
  portalLanguageOptions,
} from "@/shared/lib/language";

function MetadataJsonEditor({
  form,
}: {
  form: UseFormReturn<SourceFormValues>;
}) {
  const metadataDescription =
    "Optional structured metadata. The form only saves valid JSON.";

  return (
    <FormField
      control={form.control}
      name="metadataJson"
      render={({ field }) => (
        <FormItem>
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-1.5">
              <FormLabel>{translateText("Metadata JSON")}</FormLabel>
              <ContextHint
                content={translateText(metadataDescription)}
                label={translateText("Metadata JSON details")}
              />
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => {
                const value = String(field.value ?? "").trim();

                if (!value) {
                  return;
                }

                try {
                  const formatted = JSON.stringify(JSON.parse(value), null, 2);
                  form.setValue("metadataJson", formatted, {
                    shouldDirty: true,
                    shouldValidate: true,
                  });
                } catch {
                  form.setError("metadataJson", {
                    message: translateText("Enter valid JSON."),
                  });
                }
              }}
            >
              <Braces className="size-4" />
              {translateText("Format")}
            </Button>
          </div>
          <FormControl>
            <Textarea
              {...field}
              className="min-h-48 font-mono text-sm"
              placeholder='{"sourceSystem": "docs", "owner": "support"}'
              spellCheck={false}
            />
          </FormControl>
          <FormDescription className="sr-only">
            {translateText(metadataDescription)}
          </FormDescription>
          <FormMessage />
        </FormItem>
      )}
    />
  );
}

export function SourceFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const sourceQuery = useSource(mode === "edit" ? id : undefined);
  const createSource = useCreateSource();
  const updateSource = useUpdateSource(id ?? "");
  const initialLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;

  const form = useForm<SourceFormValues>({
    resolver: zodResolver(sourceFormSchema),
    defaultValues: {
      kind: SourceKind.Article,
      locator: "",
      label: "",
      contextNote: "",
      externalId: "",
      language: initialLanguage,
      mediaType: "",
      checksum: "",
      metadataJson: "",
      visibility: VisibilityScope.Authenticated,
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
      markVerified: false,
    });
  }, [form, sourceQuery.data]);

  const languageOptions = portalLanguageOptions.map((option) => ({
    value: option.code,
    label: option.label,
    description: `${option.code} • ${option.direction.toUpperCase()}`,
    keywords: [option.code, option.label, option.direction],
  }));
  const selectedLanguageValue = form.watch("language");
  const selectedLanguageOption =
    languageOptions.find((option) => option.value === selectedLanguageValue) ??
    null;
  const isSubmitting = createSource.isPending || updateSource.isPending;
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "source-type",
      label: "Source type",
      description: "Choose what kind of evidence this source represents.",
      complete: hasSetupValue(setupValues.kind),
    },
    {
      id: "locator",
      label: "Locator",
      description: "Add the canonical URL, path, ticket, or document locator.",
      complete: hasSetupText(setupValues.locator, 3),
    },
    {
      id: "language",
      label: "Language",
      description: "Set the locale code for this source content.",
      complete: hasSetupText(setupValues.language, 2),
    },
    {
      id: "visibility",
      label: "Visibility",
      description: "Choose who can see or reuse this source.",
      complete: hasSetupValue(setupValues.visibility),
    },
  ];
  const backTo = mode === "edit" && id ? `/app/sources/${id}` : "/app/sources";
  const sourceTitle = sourceQuery.data?.label || sourceQuery.data?.locator;
  const pageTitle =
    mode === "create"
      ? "New source"
      : sourceTitle
        ? `${translateText("Edit")} ${sourceTitle}`
        : "Edit source";

  return (
    <DetailLayout
      header={
        <PageHeader
          title={pageTitle}
          description="Capture locator, visibility, and verification metadata for reusable evidence."
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
                      "Good sources are durable, clearly classified, and explicit about who can see them.",
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
                  { label: "Visibility", value: "Authenticated or public" },
                  { label: "Metadata", value: "Optional valid JSON object" },
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
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Source details")}</span>
                  <ContextHint
                    content={translateText(
                      "Start with the locator and classification, then capture visibility and verification metadata.",
                    )}
                    label={translateText("Form details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <Form {...form}>
                <form
                  className="space-y-6"
                  onSubmit={form.handleSubmit(async (values) => {
                    const body = {
                      kind: Number(values.kind) as SourceKind,
                      locator: values.locator,
                      label: values.label || undefined,
                      contextNote: values.contextNote || undefined,
                      externalId: values.externalId || undefined,
                      language: values.language,
                      mediaType: values.mediaType || undefined,
                      metadataJson: values.metadataJson?.trim() || undefined,
                      visibility: Number(values.visibility) as VisibilityScope,
                      markVerified: values.markVerified,
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
                    title="Evidence identity"
                    description="Capture the canonical locator first, then add the operator-facing label and evidence type."
                  />
                  <div className="grid gap-4 lg:grid-cols-6">
                    <div className="lg:col-span-6">
                      <TextField
                        control={form.control}
                        name="locator"
                        label="Locator"
                        description="Use the canonical URL, path, repository URI, ticket reference, or document locator."
                      />
                    </div>
                    <div className="lg:col-span-3">
                      <TextField
                        control={form.control}
                        name="label"
                        label="Label"
                        description="Human-readable name shown when operators choose this source."
                      />
                    </div>
                    <div className="lg:col-span-3">
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
                    </div>
                    <div className="lg:col-span-6">
                      <TextareaField
                        control={form.control}
                        name="contextNote"
                        label="Context note"
                        rows={4}
                        description="Internal note describing why this source matters or where it applies."
                      />
                    </div>
                  </div>
                  <FormSectionHeading
                    title="Classification"
                    description="Set who can reuse the source and how external systems should recognize it."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
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
                    <SearchSelectField
                      control={form.control}
                      name="language"
                      label="Language"
                      description="Use the main locale for this source content."
                      options={languageOptions}
                      selectedOption={selectedLanguageOption}
                      searchPlaceholder="Search languages"
                      emptyMessage="No languages found."
                      resultCountHint={translateText(
                        "{count} languages available",
                        {
                          count: portalLanguageOptions.length,
                        },
                      )}
                    />
                    <TextField
                      control={form.control}
                      name="externalId"
                      label="External ID"
                      description="Identifier from the upstream connector, repository, or source system."
                    />
                    <TextField
                      control={form.control}
                      name="mediaType"
                      label="Media type"
                      description="MIME type or source format such as text/html, application/pdf, or text/markdown."
                    />
                  </div>
                  <FormSectionHeading
                    title="Verification and metadata"
                    description="Refresh verification state and keep optional structured metadata close to system fields."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SwitchField
                      control={form.control}
                      name="markVerified"
                      label="Mark verified now"
                      description="Set the verification timestamp when saving this source."
                    />
                    {mode === "edit" ? (
                      <TextField
                        control={form.control}
                        name="checksum"
                        label="Checksum"
                        description="Read-only value generated by the backend from the locator."
                        readOnly
                      />
                    ) : null}
                  </div>
                  <MetadataJsonEditor form={form} />
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
          <FormSetupProgressCard
            title={mode === "create" ? "Source setup" : "Source edit setup"}
            description="Complete the required evidence fields before saving this source."
            steps={setupSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
