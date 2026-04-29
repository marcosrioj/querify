import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { RefreshCw, X } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  spaceStatusLabels,
  visibilityScopeLabels,
  SpaceStatus,
  VisibilityScope,
} from "@/shared/constants/backend-enums";
import {
  useCreateSpace,
  useSpace,
  useUpdateSpace,
} from "@/domains/spaces/hooks";
import {
  spaceFormSchema,
  type SpaceFormValues,
} from "@/domains/spaces/schemas";
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
  FormSetupProgressCard,
  FormCardSkeleton,
  FormSectionHeading,
  FormMessage,
  hasSetupText,
  hasSetupValue,
  Input,
  SidebarSummarySkeleton,
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

function slugify(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 64);
}

export function SpaceFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const spaceQuery = useSpace(mode === "edit" ? id : undefined);
  const createSpace = useCreateSpace();
  const updateSpace = useUpdateSpace(id ?? "");
  const initialLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;
  const [autoSlugFromName, setAutoSlugFromName] = useState(mode === "create");

  const form = useForm<SpaceFormValues>({
    resolver: zodResolver(spaceFormSchema),
    defaultValues: {
      name: "",
      slug: "",
      language: initialLanguage,
      summary: "",
      status: SpaceStatus.Active,
      visibility: VisibilityScope.Public,
      acceptsQuestions: true,
      acceptsAnswers: true,
    },
  });

  useEffect(() => {
    if (!spaceQuery.data) {
      return;
    }

    const loadedSlug = spaceQuery.data.slug ?? "";
    setAutoSlugFromName(!loadedSlug.trim());

    form.reset({
      name: spaceQuery.data.name,
      slug: loadedSlug,
      language: spaceQuery.data.language,
      summary: spaceQuery.data.summary ?? "",
      status: spaceQuery.data.status,
      visibility: spaceQuery.data.visibility,
      acceptsQuestions: spaceQuery.data.acceptsQuestions,
      acceptsAnswers: spaceQuery.data.acceptsAnswers,
    });
  }, [form, spaceQuery.data]);

  const nameValue = form.watch("name");
  const slugValue = form.watch("slug") ?? "";

  useEffect(() => {
    if (!autoSlugFromName) {
      return;
    }

    const nextSlug = slugify(nameValue);
    if (slugValue !== nextSlug) {
      form.setValue("slug", nextSlug, {
        shouldDirty: mode === "create",
        shouldValidate: true,
      });
    }
  }, [autoSlugFromName, form, mode, nameValue, slugValue]);

  const slugifyFromName = () => {
    setAutoSlugFromName(false);
    form.setValue("slug", slugify(form.getValues("name")), {
      shouldDirty: true,
      shouldValidate: true,
    });
  };

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
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "identity",
      label: "Name and slug",
      description: "Give the space a clear name and stable slug.",
      complete:
        hasSetupText(setupValues.name, 2) && hasSetupText(setupValues.slug, 2),
    },
    {
      id: "language",
      label: "Language",
      description: "Choose the main locale used by this space.",
      complete: hasSetupText(setupValues.language, 2),
    },
    {
      id: "status",
      label: "Status",
      description: "Choose whether the space is draft, active, or archived.",
      complete: hasSetupValue(setupValues.status),
    },
    {
      id: "visibility",
      label: "Visibility",
      description: "Set the audience exposure for the space.",
      complete: hasSetupValue(setupValues.visibility),
    },
  ];
  const isSubmitting = createSpace.isPending || updateSpace.isPending;
  const backTo = mode === "edit" && id ? `/app/spaces/${id}` : "/app/spaces";
  const pageTitle =
    mode === "create"
      ? "New space"
      : spaceQuery.data?.name
        ? `${translateText("Edit")} ${spaceQuery.data.name}`
        : "Edit space";

  return (
    <DetailLayout
      header={
        <PageHeader
          title={pageTitle}
          description="Define the QnA status and exposure before questions start accumulating."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && spaceQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Spaces define status, exposure, and whether questions and answers can be collected.",
                    )}
                    label={translateText("Quick notes details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  { label: "Status", value: "Draft, active, or archived" },
                  { label: "Visibility", value: "Authenticated or public" },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {spaceQuery.isError ? (
        <ErrorState
          title="Unable to load space"
          error={spaceQuery.error}
          retry={() => void spaceQuery.refetch()}
        />
      ) : mode === "edit" && spaceQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Configuration")}</span>
                  <ContextHint
                    content={translateText(
                      "Start with status, then decide who can see the space and whether it accepts submissions.",
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
                      ...values,
                      slug: values.slug?.trim() || undefined,
                      summary: values.summary || undefined,
                      status: Number(values.status) as SpaceStatus,
                      visibility: Number(values.visibility) as VisibilityScope,
                    };

                    if (mode === "create") {
                      const createdId = await createSpace.mutateAsync(body);
                      navigate(`/app/spaces/${createdId}`);
                      return;
                    }

                    await updateSpace.mutateAsync(body);
                    navigate(`/app/spaces/${id}`);
                  })}
                >
                  <FormSectionHeading
                    title="Identity and locale"
                    description="Start with the human-facing identity, then tune the routing and locale details."
                  />
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-6">
                    <div className="md:col-span-2 lg:col-span-6">
                      <TextField
                        control={form.control}
                        name="name"
                        label="Name"
                        placeholder="Product support space"
                        description="Use the operational name teammates will recognize."
                      />
                    </div>
                    <div className="md:col-span-2 lg:col-span-6">
                      <TextareaField
                        control={form.control}
                        name="summary"
                        label="Summary"
                        rows={4}
                        description="Explain what the space covers and when teams should route content here."
                      />
                    </div>
                    <div className="lg:col-span-3">
                      <FormField
                        control={form.control}
                        name="slug"
                        render={({ field }) => (
                          <FormItem>
                            <div className="flex flex-wrap items-center justify-between gap-2">
                              <div className="flex items-center gap-1.5">
                                <FormLabel>{translateText("Slug")}</FormLabel>
                                <ContextHint
                                  content={translateText(
                                    "Use a stable slug for routing and integrations.",
                                  )}
                                  label={translateText("Slug details")}
                                />
                              </div>
                              <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={slugifyFromName}
                                className="shrink-0"
                              >
                                <RefreshCw className="size-4" />
                                {translateText("Slugify from name")}
                              </Button>
                            </div>
                            <FormControl>
                              <Input
                                {...field}
                                value={field.value ?? ""}
                                placeholder="product-support"
                                onChange={(event) => {
                                  setAutoSlugFromName(false);
                                  field.onChange(event);
                                }}
                              />
                            </FormControl>
                            <FormDescription className="sr-only">
                              {translateText(
                                "Use a stable slug for routing and integrations.",
                              )}
                            </FormDescription>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                    <div className="lg:col-span-3">
                      <SearchSelectField
                        control={form.control}
                        name="language"
                        label="Language"
                        description="Use the main locale for the questions and answers in this space."
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
                    </div>
                  </div>
                  <FormSectionHeading
                    title="Publishing rules"
                    description="Set lifecycle, audience exposure, and whether this space accepts new content."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SelectField
                      control={form.control}
                      name="status"
                      label="Status"
                      description="Public spaces must be active before they are exposed."
                      options={Object.entries(spaceStatusLabels).map(
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
                      description="Choose the strongest audience exposure the space should allow."
                      options={Object.entries(visibilityScopeLabels).map(
                        ([value, label]) => ({
                          value,
                          label,
                        }),
                      )}
                    />
                    <SwitchField
                      control={form.control}
                      name="acceptsQuestions"
                      label="Accept questions"
                      description="Disable this for frozen or read-only knowledge spaces."
                    />
                    <SwitchField
                      control={form.control}
                      name="acceptsAnswers"
                      label="Accept answers"
                      description="Disable this if questions should route elsewhere instead of collecting answers."
                    />
                  </div>
                  <div className="flex flex-wrap items-center gap-3">
                    <Button type="submit" disabled={isSubmitting}>
                      {translateText(
                        mode === "create" ? "Create space" : "Save changes",
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
            title={mode === "create" ? "Space setup" : "Space edit setup"}
            description="Complete the required configuration before saving this space."
            steps={setupSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
