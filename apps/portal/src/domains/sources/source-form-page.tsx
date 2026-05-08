import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm, type UseFormReturn } from "react-hook-form";
import { Braces, FileUp, Link2, LoaderCircle, X } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  VisibilityScope,
  backendEnumSelectOptions,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  useCreateSource,
  useCompleteSourceUpload,
  useCreateSourceUploadIntent,
  useInspectSourceExternalUrl,
  useSource,
  useUpdateSource,
} from "@/domains/sources/hooks";
import type { SourceExternalUrlInspectionDto } from "@/domains/sources/types";
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

const visibilityOptions = backendEnumSelectOptions(visibilityScopeLabels);
const GENERATED_SOURCE_METADATA_KEY = "sourceInspection";
const GENERATED_SOURCE_METADATA_FIELDS = new Set([
  "contentLengthBytes",
  "contentType",
  "fileName",
  "finalUrl",
  "host",
  "lastModified",
  "lastModifiedAtUtc",
  "origin",
  "path",
  "sizeBytes",
  "stagedAtUtc",
  "status",
  "statusText",
  "url",
  "validatedAtUtc",
]);

type JsonObject = Record<string, unknown>;

type ExternalUrlValidationState = {
  status: "idle" | "scheduled" | "validating" | "valid" | "invalid";
  locator?: string;
  message?: string;
};

type ExternalUrlInspection = {
  contentLengthBytes?: number;
  contentType?: string;
  finalUrl: string;
  lastModified?: string;
  mediaType?: string;
  status: number;
  statusText: string;
};

type InspectSourceExternalUrl = (
  locator: string,
  signal: AbortSignal,
) => Promise<SourceExternalUrlInspectionDto>;

const EXTERNAL_URL_BROWSER_VALIDATION_MESSAGE =
  "Browser blocked or could not complete the link check.";
const EXTERNAL_URL_HTTP_STATUS_REASONS: Record<number, string> = {
  300: "multiple choices.",
  301: "moved permanently.",
  302: "found at another address.",
  303: "see another location.",
  304: "not modified.",
  305: "proxy required.",
  307: "temporary redirect.",
  308: "permanent redirect.",
  400: "bad request.",
  401: "login required.",
  402: "payment required.",
  403: "access blocked.",
  404: "not found.",
  405: "method not allowed.",
  406: "not acceptable.",
  407: "proxy login required.",
  408: "request timed out.",
  409: "conflict.",
  410: "link no longer exists.",
  411: "length required.",
  412: "precondition failed.",
  413: "response too large.",
  414: "link is too long.",
  415: "unsupported media type.",
  416: "range not available.",
  417: "expectation failed.",
  418: "unexpected server response.",
  421: "misdirected request.",
  422: "invalid request.",
  423: "locked.",
  424: "failed dependency.",
  425: "too early to retry.",
  426: "upgrade required.",
  428: "precondition required.",
  429: "too many checks.",
  431: "headers too large.",
  451: "blocked by legal rules.",
  500: "site error.",
  501: "check method not supported.",
  502: "bad gateway.",
  503: "site unavailable.",
  504: "request timed out.",
  505: "HTTP version not supported.",
  506: "server configuration error.",
  507: "insufficient storage.",
  508: "loop detected.",
  510: "extension required.",
  511: "network login required.",
};

function isJsonObject(value: unknown): value is JsonObject {
  return Boolean(value) && typeof value === "object" && !Array.isArray(value);
}

function parseMetadataJson(value: string | null | undefined) {
  const trimmed = value?.trim();
  if (!trimmed) {
    return {};
  }

  const parsed = JSON.parse(trimmed);
  if (!isJsonObject(parsed)) {
    throw new Error("Metadata JSON must be an object.");
  }

  return parsed;
}

function pruneEmptyMetadataFields(metadata: JsonObject) {
  return Object.fromEntries(
    Object.entries(metadata).filter(([, value]) => value !== undefined),
  );
}

function preserveUserSourceInspectionMetadata(metadata: unknown) {
  if (!isJsonObject(metadata)) {
    return {};
  }

  return Object.fromEntries(
    Object.entries(metadata).filter(
      ([key]) => !GENERATED_SOURCE_METADATA_FIELDS.has(key),
    ),
  );
}

function setSourceInspectionMetadata(
  form: UseFormReturn<SourceFormValues>,
  metadata: JsonObject,
) {
  let currentMetadata: JsonObject;

  try {
    currentMetadata = parseMetadataJson(form.getValues("metadataJson"));
  } catch {
    return;
  }

  const sourceInspectionMetadata = {
    ...preserveUserSourceInspectionMetadata(
      currentMetadata[GENERATED_SOURCE_METADATA_KEY],
    ),
    ...pruneEmptyMetadataFields(metadata),
  };
  const nextMetadata = {
    ...currentMetadata,
  };

  if (Object.keys(sourceInspectionMetadata).length > 0) {
    nextMetadata[GENERATED_SOURCE_METADATA_KEY] = sourceInspectionMetadata;
  } else {
    delete nextMetadata[GENERATED_SOURCE_METADATA_KEY];
  }

  form.setValue("metadataJson", JSON.stringify(nextMetadata, null, 2), {
    shouldDirty: true,
    shouldValidate: true,
  });
}

function inferMediaTypeFromUrl(locator: string) {
  const path = new URL(locator).pathname.toLowerCase();

  if (path.endsWith(".pdf")) return "application/pdf";
  if (path.endsWith(".png")) return "image/png";
  if (path.endsWith(".jpg") || path.endsWith(".jpeg")) return "image/jpeg";
  if (path.endsWith(".mp4")) return "video/mp4";
  if (path.endsWith(".md") || path.endsWith(".markdown"))
    return "text/markdown";
  if (path.endsWith(".txt")) return "text/plain";
  if (path.endsWith(".html") || path.endsWith(".htm")) return "text/html";

  return "application/octet-stream";
}

function buildLabelFromExternalUrl(locator: string) {
  const url = new URL(locator);
  const lastPathSegment = url.pathname.split("/").filter(Boolean).at(-1);

  if (!lastPathSegment) {
    return url.hostname;
  }

  return decodeURIComponent(lastPathSegment).replace(/\.[a-z0-9]+$/i, "");
}

function buildExternalUrlMetadata(inspection: ExternalUrlInspection) {
  return {
    contentLengthBytes: inspection.contentLengthBytes,
    contentType: inspection.contentType,
  };
}

function buildFileUploadMetadata(file: File) {
  return {
    contentType: file.type || undefined,
    contentLengthBytes: file.size,
  };
}

function normalizeHeaderContentType(contentType: string | null) {
  return contentType?.split(";", 1)[0].trim().toLowerCase() || undefined;
}

function parseContentLength(contentLength: string | null) {
  if (!contentLength) {
    return undefined;
  }

  const parsed = Number(contentLength);
  return Number.isFinite(parsed) && parsed >= 0 ? parsed : undefined;
}

async function fetchExternalUrl(
  locator: string,
  method: "HEAD" | "GET",
  signal: AbortSignal,
) {
  return fetch(locator, {
    cache: "no-store",
    method,
    redirect: "follow",
    signal,
  });
}

function getExternalUrlHttpErrorMessage(status: number) {
  return `HTTP ${status}: ${
    EXTERNAL_URL_HTTP_STATUS_REASONS[status] ?? "unrecognized status."
  }`;
}

function buildExternalUrlInspectionFromPortal(
  locator: string,
  result: SourceExternalUrlInspectionDto,
) {
  const status = result.status ?? 0;

  if (!result.isReachable || status < 200 || status >= 300) {
    return {
      message:
        status > 0
          ? getExternalUrlHttpErrorMessage(status)
          : EXTERNAL_URL_BROWSER_VALIDATION_MESSAGE,
      ok: false as const,
    };
  }

  return {
    inspection: {
      contentLengthBytes: result.contentLengthBytes ?? undefined,
      contentType: result.contentType ?? undefined,
      finalUrl: result.finalUrl ?? locator,
      lastModified: result.lastModified ?? undefined,
      mediaType:
        normalizeHeaderContentType(result.contentType ?? null) ??
        inferMediaTypeFromUrl(locator),
      status,
      statusText: result.statusText ?? "",
    },
    ok: true as const,
  };
}

async function inspectExternalUrl(
  locator: string,
  signal: AbortSignal,
  inspectFromPortal: InspectSourceExternalUrl,
) {
  let url: URL;

  try {
    url = new URL(locator);
  } catch {
    return {
      message: "Use an HTTP or HTTPS URL.",
      ok: false as const,
    };
  }

  if (url.protocol !== "http:" && url.protocol !== "https:") {
    return {
      message: "Use an HTTP or HTTPS URL.",
      ok: false as const,
    };
  }

  try {
    let response = await fetchExternalUrl(url.toString(), "HEAD", signal);
    if (!response.ok && (response.status === 405 || response.status === 501)) {
      response = await fetchExternalUrl(url.toString(), "GET", signal);
    }

    if (!response.ok) {
      return {
        message: getExternalUrlHttpErrorMessage(response.status),
        ok: false as const,
      };
    }

    const contentType = response.headers.get("content-type") ?? undefined;
    const mediaType =
      normalizeHeaderContentType(contentType ?? null) ??
      inferMediaTypeFromUrl(url.toString());

    return {
      inspection: {
        contentLengthBytes: parseContentLength(
          response.headers.get("content-length"),
        ),
        contentType,
        finalUrl: response.url || url.toString(),
        lastModified: response.headers.get("last-modified") ?? undefined,
        mediaType,
        status: response.status,
        statusText: response.statusText,
      },
      ok: true as const,
    };
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") {
      throw error;
    }

    try {
      const portalResult = await inspectFromPortal(url.toString(), signal);
      return buildExternalUrlInspectionFromPortal(url.toString(), portalResult);
    } catch (portalError) {
      if (
        portalError instanceof DOMException &&
        portalError.name === "AbortError"
      ) {
        throw portalError;
      }
    }

    return {
      message: EXTERNAL_URL_BROWSER_VALIDATION_MESSAGE,
      ok: false as const,
    };
  }
}

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
  const inspectSourceExternalUrl = useInspectSourceExternalUrl();
  const updateSource = useUpdateSource(id ?? "");
  const initialLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;
  const [createMode, setCreateMode] = useState<"external" | "upload">(
    "external",
  );
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [externalUrlValidation, setExternalUrlValidation] =
    useState<ExternalUrlValidationState>({ status: "idle" });

  const form = useForm<SourceFormValues>({
    resolver: zodResolver(sourceFormSchema),
    defaultValues: {
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
  const locatorValue = form.watch("locator");
  const selectedLanguageOption =
    languageOptions.find((option) => option.value === selectedLanguageValue) ??
    null;
  const isUploadMode = mode === "create" && createMode === "upload";
  const isEditingUploadedSource =
    mode === "edit" && Boolean(sourceQuery.data?.storageKey);
  const shouldValidateExternalUrl = !isUploadMode && !isEditingUploadedSource;
  const showExternalUrlValidationLoader =
    shouldValidateExternalUrl &&
    externalUrlValidation.status === "validating" &&
    externalUrlValidation.locator === locatorValue?.trim();
  const isSubmitting =
    createSource.isPending ||
    updateSource.isPending ||
    createUploadIntent.isPending ||
    completeSourceUpload.isPending ||
    externalUrlValidation.status === "validating" ||
    (isUploadMode && uploadProgress > 0 && uploadProgress < 100);
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "locator",
      label: isUploadMode ? "File" : "Locator",
      description: isUploadMode
        ? "Choose the file that should become this source."
        : "Add a reachable HTTP or HTTPS URL for the source artifact.",
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
    if (
      !isUploadMode ||
      form.getValues("visibility") !== VisibilityScope.Public
    ) {
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

  useEffect(() => {
    if (!shouldValidateExternalUrl) {
      setExternalUrlValidation({ status: "idle" });
      form.clearErrors("locator");
      return;
    }

    const locator = locatorValue?.trim() ?? "";
    if (!locator) {
      setExternalUrlValidation({ status: "idle" });
      form.clearErrors("locator");
      return;
    }

    const controller = new AbortController();
    let isCancelled = false;
    setExternalUrlValidation({ locator, status: "scheduled" });
    const timeout = window.setTimeout(async () => {
      if (isCancelled) {
        return;
      }

      setExternalUrlValidation({ locator, status: "validating" });

      try {
        const result = await inspectExternalUrl(
          locator,
          controller.signal,
          (targetLocator, signal) =>
            inspectSourceExternalUrl({ locator: targetLocator }, signal),
        );

        if (!result.ok) {
          if (isCancelled) {
            return;
          }

          setExternalUrlValidation({
            locator,
            message: result.message,
            status: "invalid",
          });
          form.setError("locator", { message: translateText(result.message) });
          return;
        }

        const contentType =
          result.inspection.mediaType ?? inferMediaTypeFromUrl(locator);
        if (isCancelled) {
          return;
        }

        form.clearErrors("locator");
        form.setValue("mediaType", contentType, {
          shouldDirty: true,
          shouldValidate: true,
        });

        if (!form.getValues("label")) {
          form.setValue(
            "label",
            buildLabelFromExternalUrl(result.inspection.finalUrl),
            {
              shouldDirty: true,
              shouldValidate: true,
            },
          );
        }

        setSourceInspectionMetadata(
          form,
          buildExternalUrlMetadata(result.inspection),
        );
        setExternalUrlValidation({ locator, status: "valid" });
      } catch (error) {
        if (isCancelled) {
          return;
        }

        if (error instanceof DOMException && error.name === "AbortError") {
          return;
        }

        setExternalUrlValidation({
          locator,
          message: EXTERNAL_URL_BROWSER_VALIDATION_MESSAGE,
          status: "invalid",
        });
        form.setError("locator", {
          message: translateText(EXTERNAL_URL_BROWSER_VALIDATION_MESSAGE),
        });
      }
    }, 1000);

    return () => {
      isCancelled = true;
      window.clearTimeout(timeout);
      controller.abort();
    };
  }, [form, inspectSourceExternalUrl, locatorValue, shouldValidateExternalUrl]);

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
                    label: "Identity",
                    value: "Stable locator, label, and media type",
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
                      "Start with the locator and media type, then capture visibility and verification metadata.",
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

                    if (
                      shouldValidateExternalUrl &&
                      (externalUrlValidation.status !== "valid" ||
                        externalUrlValidation.locator !== values.locator.trim())
                    ) {
                      const message =
                        externalUrlValidation.status === "scheduled" ||
                        externalUrlValidation.status === "validating"
                          ? "Link check is still running."
                          : (externalUrlValidation.message ??
                            "Link check failed.");
                      form.setError("locator", {
                        message: translateText(message),
                      });
                      return;
                    }

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
                        language: values.language,
                        visibility,
                        label: values.label || undefined,
                        contextNote: values.contextNote || undefined,
                        externalId: values.externalId || undefined,
                        metadataJson: values.metadataJson?.trim() || undefined,
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
                        variant={
                          createMode === "external" ? "secondary" : "ghost"
                        }
                        size="sm"
                        onClick={() => setCreateMode("external")}
                      >
                        <Link2 className="size-4" />
                        {translateText("External URL")}
                      </Button>
                      <Button
                        type="button"
                        variant={
                          createMode === "upload" ? "secondary" : "ghost"
                        }
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
                    description="Capture the canonical locator first, then add the operator-facing label and media type."
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
                              const file =
                                event.currentTarget.files?.[0] ?? null;
                              setSelectedFile(file);
                              setUploadProgress(0);
                              setUploadError(null);

                              if (!file) {
                                form.setValue("locator", "", {
                                  shouldDirty: true,
                                  shouldValidate: true,
                                });
                                form.setValue("mediaType", "", {
                                  shouldDirty: true,
                                  shouldValidate: true,
                                });
                                return;
                              }

                              form.setValue("locator", file.name, {
                                shouldDirty: true,
                                shouldValidate: true,
                              });
                              form.setValue(
                                "mediaType",
                                file.type || "application/octet-stream",
                                {
                                  shouldDirty: true,
                                  shouldValidate: true,
                                },
                              );
                              setSourceInspectionMetadata(
                                form,
                                buildFileUploadMetadata(file),
                              );

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
                                size: (selectedFile.size / 1024 / 1024).toFixed(
                                  2,
                                ),
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
                        <FormField
                          control={form.control}
                          name="locator"
                          render={({ field }) => (
                            <FormItem>
                              <div className="flex items-center gap-1.5">
                                <FormLabel>
                                  {translateText("Locator")}
                                </FormLabel>
                                <ContextHint
                                  content={translateText(
                                    "Use a reachable HTTP or HTTPS URL for the source artifact.",
                                  )}
                                  label={translateText("Locator details")}
                                />
                              </div>
                              <FormControl>
                                <div className="relative">
                                  <Input
                                    {...field}
                                    className={
                                      showExternalUrlValidationLoader
                                        ? "pr-10"
                                        : undefined
                                    }
                                  />
                                  {showExternalUrlValidationLoader ? (
                                    <LoaderCircle
                                      className="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 animate-spin text-muted-foreground"
                                      aria-hidden="true"
                                    />
                                  ) : null}
                                </div>
                              </FormControl>
                              <FormDescription className="sr-only">
                                {translateText(
                                  "Use a reachable HTTP or HTTPS URL for the source artifact.",
                                )}
                              </FormDescription>
                              <FormMessage />
                            </FormItem>
                          )}
                        />
                      </div>
                    )}
                    <div className="lg:col-span-6">
                      <TextField
                        control={form.control}
                        name="label"
                        label="Label"
                        description="Human-readable name shown when operators choose this source."
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
                    description="Set who can reuse the source and review detected source metadata."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SelectField
                      control={form.control}
                      name="visibility"
                      label="Visibility"
                      description="Controls which audiences can see or reuse this source."
                      options={
                        isUploadMode
                          ? uploadVisibilityOptions
                          : visibilityOptions
                      }
                    />
                    <TextField
                      control={form.control}
                      name="mediaType"
                      label="Media type"
                      description="Detected MIME type from the file or reachable external URL."
                      readOnly
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
