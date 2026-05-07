import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm, type UseFormReturn } from "react-hook-form";
import { Braces, FileUp, Link2, X } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  SourceKind,
  VisibilityScope,
  backendEnumSelectOptions,
  sourceKindLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  useCreateSource,
  useCompleteSourceUpload,
  useCreateSourceUploadIntent,
  useSource,
  useUpdateSource,
} from "@/domains/sources/hooks";
import { uploadSourceFile } from "@/domains/sources/upload-flow";
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
  Input,
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

const sourceKindOptions = backendEnumSelectOptions(sourceKindLabels);
const visibilityOptions = backendEnumSelectOptions(visibilityScopeLabels);

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
  const createUploadIntent = useCreateSourceUploadIntent();
  const completeSourceUpload = useCompleteSourceUpload();
  const updateSource = useUpdateSource(id ?? "");
  const initialLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;
  const [createMode, setCreateMode] = useState<"external" | "upload">("external");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadError, setUploadError] = useState<string | null>(null);

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
      visibility: VisibilityScope.Internal,
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
  const isUploadMode = mode === "create" && createMode === "upload";
  const isSubmitting =
    createSource.isPending ||
    updateSource.isPending ||
    createUploadIntent.isPending ||
    completeSourceUpload.isPending ||
    (isUploadMode && uploadProgress > 0 && uploadProgress < 100);
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
      label: isUploadMode ? "File" : "Locator",
      description: isUploadMode
        ? "Choose the file that should become this source."
        : "Add the canonical URL, path, ticket, or document locator.",
      complete: isUploadMode
        ? Boolean(selectedFile)
        : hasSetupText(setupValues.locator, 3),
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
  const uploadVisibilityOptions = visibilityOptions.filter(
    (option) => option.value !== String(VisibilityScope.Public),
  );

  useEffect(() => {
    if (!isUploadMode || form.getValues("visibility") !== VisibilityScope.Public) {
      return;
    }

    form.setValue("visibility", VisibilityScope.Internal, {
      shouldDirty: true,
      shouldValidate: true,
    });
  }, [form, isUploadMode]);

  useEffect(() => {
    if (!isUploadMode) {
      return;
    }

    if (!form.getValues("locator")) {
      form.setValue("locator", "upload", {
        shouldDirty: true,
        shouldValidate: true,
      });
    }
  }, [form, isUploadMode]);

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
                  {
                    label: "Visibility",
                    value: "Internal, authenticated, or public",
                  },
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
                    setUploadError(null);

                    if (isUploadMode) {
                      if (!selectedFile) {
                        setUploadError("Choose a file before starting upload.");
                        return;
                      }

                      setUploadProgress(0);
                      const visibility =
                        Number(values.visibility) === VisibilityScope.Public
                          ? VisibilityScope.Internal
                          : (Number(values.visibility) as VisibilityScope);
                      const intent = await createUploadIntent.mutateAsync({
                        fileName: selectedFile.name,
                        contentType:
                          selectedFile.type || "application/octet-stream",
                        sizeBytes: selectedFile.size,
                        kind: Number(values.kind) as SourceKind,
                        language: values.language,
                        visibility,
                        label: values.label || undefined,
                        contextNote: values.contextNote || undefined,
                      });

                      await uploadSourceFile({
                        file: selectedFile,
                        intentResponse: intent,
                        onProgress: setUploadProgress,
                      });
                      await completeSourceUpload.mutateAsync({
                        id: intent.sourceId,
                        body: {},
                      });
                      navigate(`/app/sources/${intent.sourceId}`);
                      return;
                    }

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
                  {mode === "create" ? (
                    <div className="inline-flex max-w-full rounded-md border border-border bg-muted/20 p-1">
                      <Button
                        type="button"
                        variant={createMode === "external" ? "secondary" : "ghost"}
                        size="sm"
                        onClick={() => setCreateMode("external")}
                      >
                        <Link2 className="size-4" />
                        {translateText("External URL")}
                      </Button>
                      <Button
                        type="button"
                        variant={createMode === "upload" ? "secondary" : "ghost"}
                        size="sm"
                        onClick={() => setCreateMode("upload")}
                      >
                        <FileUp className="size-4" />
                        {translateText("File upload")}
                      </Button>
                    </div>
                  ) : null}
                  <FormSectionHeading
                    title="Evidence identity"
                    description="Capture the canonical locator first, then add the operator-facing label and evidence type."
                  />
                  <div className="grid gap-4 lg:grid-cols-6">
                    {isUploadMode ? (
                      <div className="lg:col-span-6">
                        <div className="space-y-2">
                          <div className="flex items-center gap-1.5">
                            <FormLabel>{translateText("File")}</FormLabel>
                            <ContextHint
                              content={translateText(
                                "Upload uses a single presigned PUT URL and keeps the source private until worker verification completes.",
                              )}
                              label={translateText("File upload details")}
                            />
                          </div>
                          <Input
                            type="file"
                            accept=".pdf,.png,.jpg,.jpeg,.mp4,.txt,.md,.markdown,application/pdf,image/png,image/jpeg,video/mp4,text/plain,text/markdown"
                            onChange={(event) => {
                              const file = event.currentTarget.files?.[0] ?? null;
                              setSelectedFile(file);
                              setUploadProgress(0);
                              setUploadError(null);

                              if (!file) {
                                return;
                              }

                              form.setValue("locator", file.name, {
                                shouldDirty: true,
                                shouldValidate: true,
                              });
                              form.setValue("mediaType", file.type, {
                                shouldDirty: true,
                                shouldValidate: true,
                              });

                              if (!form.getValues("label")) {
                                form.setValue("label", file.name, {
                                  shouldDirty: true,
                                  shouldValidate: true,
                                });
                              }
                            }}
                          />
                          {selectedFile ? (
                            <p className="break-words text-sm text-muted-foreground">
                              {translateText("{name} · {size} MB · {type}", {
                                name: selectedFile.name,
                                size: (selectedFile.size / 1024 / 1024).toFixed(2),
                                type: selectedFile.type || "unknown",
                              })}
                            </p>
                          ) : null}
                          {uploadProgress > 0 ? (
                            <div className="h-2 overflow-hidden rounded bg-muted">
                              <div
                                className="h-full bg-primary transition-all"
                                style={{ width: `${uploadProgress}%` }}
                              />
                            </div>
                          ) : null}
                          {uploadError ? (
                            <p className="text-sm text-destructive">
                              {translateText(uploadError)}
                            </p>
                          ) : null}
                        </div>
                      </div>
                    ) : (
                      <div className="lg:col-span-6">
                        <TextField
                          control={form.control}
                          name="locator"
                          label="Locator"
                          description="Use the canonical URL, path, repository URI, ticket reference, or document locator."
                        />
                      </div>
                    )}
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
                        options={sourceKindOptions}
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
                      options={isUploadMode ? uploadVisibilityOptions : visibilityOptions}
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
