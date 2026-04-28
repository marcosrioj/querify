import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { X } from "lucide-react";
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
  FormSetupProgressCard,
  FormCardSkeleton,
  FormSectionHeading,
  hasSetupText,
  hasSetupValue,
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

export function SpaceFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const spaceQuery = useSpace(mode === "edit" ? id : undefined);
  const createSpace = useCreateSpace();
  const updateSpace = useUpdateSpace(id ?? "");
  const initialLanguage = getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;

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

    form.reset({
      name: spaceQuery.data.name,
      slug: spaceQuery.data.slug,
      language: spaceQuery.data.language,
      summary: spaceQuery.data.summary ?? "",
      status: spaceQuery.data.status,
      visibility: spaceQuery.data.visibility,
      acceptsQuestions: spaceQuery.data.acceptsQuestions,
      acceptsAnswers: spaceQuery.data.acceptsAnswers,
    });
  }, [form, spaceQuery.data]);

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

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === "create" ? "New space" : "Edit space"}
          description="Define the QnA status and exposure before threads start accumulating."
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
                  className="space-y-5"
                  onSubmit={form.handleSubmit(async (values) => {
                    const body = {
                      ...values,
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
                    title="Identity"
                    description="Name the space clearly so operators know exactly what knowledge area they are configuring."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <TextField
                      control={form.control}
                      name="name"
                      label="Name"
                      placeholder="Product support space"
                      description="Use the operational name teammates will recognize."
                    />
                    <TextField
                      control={form.control}
                      name="slug"
                      label="Slug"
                      placeholder="product-support"
                      description="Use a stable slug for routing and integrations."
                    />
                  </div>
                  <div className="grid gap-4 md:grid-cols-2">
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
                  </div>
                  <TextareaField
                    control={form.control}
                    name="summary"
                    label="Summary"
                    rows={3}
                    description="Explain what the space covers and when teams should route content here."
                  />
                  <FormSectionHeading
                    title="Exposure"
                    description="Decide who can see the space."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
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
                  </div>
                  <FormSectionHeading
                    title="Workflow rules"
                    description="Tune whether the space accepts new threads."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
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
