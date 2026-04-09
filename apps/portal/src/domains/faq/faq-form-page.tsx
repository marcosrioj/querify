import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { X } from "lucide-react";
import { Link } from "react-router-dom";
import { faqFormSchema, type FaqFormValues } from "@/domains/faq/schemas";
import { useCreateFaq, useFaq, useUpdateFaq } from "@/domains/faq/hooks";
import {
  faqStatusLabels,
  FaqStatus,
} from "@/shared/constants/backend-enums";
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
  ProgressChecklistCard,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import {
  SearchSelectField,
  SelectField,
  TextField,
  type SelectFieldConfirmation,
} from "@/shared/ui/form-fields";
import { ErrorState } from "@/shared/ui/placeholder-state";
import { translateText } from "@/shared/lib/i18n-core";
import {
  DEFAULT_PORTAL_LANGUAGE,
  getStoredPortalLanguage,
  portalLanguageOptions,
} from "@/shared/lib/language";

const faqStatusConfirmation: SelectFieldConfirmation = {
  title: ({ nextOption }) =>
    translateText("Change FAQ status to {status}?", {
      status: nextOption?.label ?? translateText("this option"),
    }),
  description: ({ nextValue }) => {
    switch (Number(nextValue)) {
      case FaqStatus.Published:
        return translateText(
          "Published FAQs are treated as ready for customer-facing use. Confirm this only when the answers and sources are ready.",
        );
      case FaqStatus.Archived:
        return translateText(
          "Archived FAQs stay saved for history, but should stop being used as active content. Confirm this when the FAQ is obsolete or intentionally retired.",
        );
      case FaqStatus.Draft:
      default:
        return translateText(
          "Draft keeps the FAQ in a working state while the team is still reviewing answers and sources.",
        );
    }
  },
  confirmLabel: ({ nextOption }) =>
    translateText("Set as {status}", {
      status: nextOption?.label ?? translateText("selected status"),
    }),
  variant: ({ nextValue }) =>
    Number(nextValue) === FaqStatus.Archived ? "destructive" : "primary",
};

export function FaqFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(mode === "edit" ? id : undefined);
  const createFaq = useCreateFaq();
  const updateFaq = useUpdateFaq(id ?? "");
  const createDefaultLanguage =
    getStoredPortalLanguage() ?? DEFAULT_PORTAL_LANGUAGE;

  const form = useForm<FaqFormValues>({
    resolver: zodResolver(faqFormSchema),
    defaultValues:
      mode === "create"
        ? {
            name: "",
            language: createDefaultLanguage,
            status: FaqStatus.Draft,
          }
        : {
            name: "",
            language: DEFAULT_PORTAL_LANGUAGE,
            status: FaqStatus.Draft,
          },
  });

  useEffect(() => {
    if (!faqQuery.data) {
      return;
    }

    form.reset({
      name: faqQuery.data.name,
      language: faqQuery.data.language,
      status: faqQuery.data.status,
    });
  }, [faqQuery.data, form]);

  useEffect(() => {
    if (mode !== "create") {
      return;
    }

    form.reset({
      name: "",
      language: createDefaultLanguage,
      status: FaqStatus.Draft,
    });
  }, [createDefaultLanguage, form, mode]);

  const isSubmitting = createFaq.isPending || updateFaq.isPending;
  const nameValue = form.watch("name");
  const languageValue = form.watch("language");
  const statusValue = form.watch("status");
  const languageOptions = portalLanguageOptions.map((option) => ({
    value: option.code,
    label: option.label,
    description: translateText("{code} • {direction}", {
      code: option.code,
      direction: option.direction.toUpperCase(),
    }),
    keywords: [option.code, option.direction, option.label],
  }));
  const selectedLanguageOption =
    languageOptions.find((option) => option.value === languageValue) ??
    (languageValue?.trim()
      ? {
          value: languageValue,
          label: languageValue,
        }
      : null);
  const formSteps = [
    {
      id: "name",
      label: "Name the FAQ",
      description: nameValue
        ? translateText("Current title: {value}", { value: nameValue })
        : "Use a specific title so teammates and customers know what this FAQ covers.",
      complete: Boolean(nameValue?.trim()),
    },
    {
      id: "language",
      label: "Confirm the language",
      description: languageValue
        ? translateText("Current locale: {value}", { value: languageValue })
        : "Pick the locale customers will actually search in.",
      complete: Boolean(languageValue?.trim()),
    },
    {
      id: "status",
      label: "Choose visibility",
      description:
        Number(statusValue) === FaqStatus.Published
          ? "This FAQ is configured to be customer-facing."
          : "Keep it in draft until the first answers and sources are in place.",
      complete: statusValue !== undefined && statusValue !== null,
    },
  ];

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === "create" ? "New FAQ" : "Edit FAQ"}
          description="Set the name, language, and status for this FAQ."
          descriptionMode="hint"
          backTo={mode === "edit" && id ? `/app/faq/${id}` : "/app/faq"}
        />
      }
      sidebar={
        mode === "edit" && faqQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content="Keep the FAQ simple, searchable, and ready for publication."
                    label="Quick notes details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Language",
                    value: "Use the locale customers will search in",
                  },
                  { label: "Status", value: "Draft, published, or archived" },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {faqQuery.isError ? (
        <ErrorState
          title="Unable to load FAQ"
          error={faqQuery.error}
          retry={() => void faqQuery.refetch()}
        />
      ) : mode === "edit" && faqQuery.isLoading ? (
        <FormCardSkeleton fields={3} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Details")}</span>
                  <ContextHint
                    content="Set how this FAQ should behave across the portal."
                    label="Form details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <Form {...form}>
                <form
                  className="space-y-4"
                  onSubmit={form.handleSubmit(async (values) => {
                    const body = {
                      ...values,
                      status: Number(values.status) as FaqStatus,
                    };

                    if (mode === "create") {
                      const createdId = await createFaq.mutateAsync(body);
                      navigate(`/app/faq/${createdId}`);
                      return;
                    }

                    await updateFaq.mutateAsync(body);
                    navigate(`/app/faq/${id}`);
                  })}
                >
                  <FormSectionHeading
                    title="Basics"
                    description="Name the FAQ clearly and set the language people will search in."
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <TextField
                      control={form.control}
                      name="name"
                      label="FAQ name"
                      placeholder="e.g. Product onboarding FAQ"
                      description="Keep the name specific enough that teammates can find it quickly."
                    />
                    <SearchSelectField
                      control={form.control}
                      name="language"
                      label="Language"
                      description="Use the locale customers will search in, for example en-US."
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
                  <FormSectionHeading
                    title="Visibility"
                    description="Decide when this FAQ should be visible."
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SelectField
                      control={form.control}
                      name="status"
                      label="Status"
                      description="Draft keeps the FAQ private while you build. Publish only when the answers are ready."
                      confirmation={faqStatusConfirmation}
                      options={Object.entries(faqStatusLabels).map(
                        ([value, label]) => ({
                          value,
                          label,
                        }),
                      )}
                    />
                  </div>
                  <div className="flex flex-wrap items-center gap-3">
                    <Button type="submit" disabled={isSubmitting}>
                      {translateText(mode === "create" ? "Create FAQ" : "Save changes")}
                    </Button>
                    <Button asChild variant="outline">
                      <Link
                        to={
                          mode === "edit" && id ? `/app/faq/${id}` : "/app/faq"
                        }
                      >
                        <X className="size-4" />
                        {translateText("Cancel")}
                      </Link>
                    </Button>
                  </div>
                </form>
              </Form>
            </CardContent>
          </Card>
          <ProgressChecklistCard
            title="Set up this FAQ without guesswork"
            description="Complete the basics first, then decide visibility. The form stays lightweight until those decisions matter."
            steps={formSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
