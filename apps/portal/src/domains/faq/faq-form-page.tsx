import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { faqFormSchema, type FaqFormValues } from "@/domains/faq/schemas";
import { useCreateFaq, useFaq, useUpdateFaq } from "@/domains/faq/hooks";
import {
  ctaTargetLabels,
  faqSortStrategyLabels,
  faqStatusLabels,
  CtaTarget,
  FaqSortStrategy,
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
  ProgressChecklistCard,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { SelectField, SwitchField, TextField } from "@/shared/ui/form-fields";
import { ErrorState } from "@/shared/ui/placeholder-state";

export function FaqFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(mode === "edit" ? id : undefined);
  const createFaq = useCreateFaq();
  const updateFaq = useUpdateFaq(id ?? "");

  const form = useForm<FaqFormValues>({
    resolver: zodResolver(faqFormSchema),
    defaultValues: {
      name: "",
      language: "en-US",
      status: FaqStatus.Draft,
      sortStrategy: FaqSortStrategy.Sort,
      ctaEnabled: false,
      ctaTarget: CtaTarget.Self,
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
      sortStrategy: faqQuery.data.sortStrategy,
      ctaEnabled: faqQuery.data.ctaEnabled,
      ctaTarget: faqQuery.data.ctaTarget,
    });
  }, [faqQuery.data, form]);

  const isSubmitting = createFaq.isPending || updateFaq.isPending;
  const nameValue = form.watch("name");
  const languageValue = form.watch("language");
  const statusValue = form.watch("status");
  const ctaEnabled = form.watch("ctaEnabled");
  const ctaTargetValue = form.watch("ctaTarget");
  const formSteps = [
    {
      id: "name",
      label: "Name the FAQ",
      description: nameValue
        ? `Current title: ${nameValue}`
        : "Use a specific title so teammates and customers know what this FAQ covers.",
      complete: Boolean(nameValue?.trim()),
    },
    {
      id: "language",
      label: "Confirm the language",
      description: languageValue
        ? `Current locale: ${languageValue}`
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
    {
      id: "cta",
      label: "Decide CTA behavior",
      description: ctaEnabled
        ? "CTA is enabled, so choose where linked actions should open."
        : "Leave CTA off until this FAQ needs a next-step action.",
      complete: !ctaEnabled || Boolean(ctaTargetValue),
    },
  ];

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="FAQ"
          title={mode === "create" ? "New FAQ" : "Edit FAQ"}
          description="Set the name, status, and CTA rules for this FAQ."
          descriptionMode="hint"
          backTo={mode === "edit" && id ? `/app/faq/${id}` : "/app/faq"}
        />
      }
      sidebar={
        mode === "edit" && faqQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <>
            <ProgressChecklistCard
              title="Set up this FAQ without guesswork"
              description="Complete the basics first, then decide visibility and CTA behavior. The form stays lightweight until those decisions matter."
              steps={formSteps}
            />
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex flex-wrap items-center gap-2">
                    <span>Quick notes</span>
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
                    { label: "CTA", value: "Optional next step for Q&A items" },
                  ]}
                />
              </CardContent>
            </Card>
          </>
        )
      }
    >
      {faqQuery.isError ? (
        <ErrorState
          title="Unable to load FAQ"
          description="The FAQ detail request failed."
          retry={() => void faqQuery.refetch()}
        />
      ) : mode === "edit" && faqQuery.isLoading ? (
        <FormCardSkeleton fields={6} />
      ) : (
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>Details</span>
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
                    sortStrategy: Number(
                      values.sortStrategy,
                    ) as FaqSortStrategy,
                    ctaTarget: Number(values.ctaTarget) as CtaTarget,
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
                <div className="space-y-1">
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
                    Start here
                  </p>
                  <h3 className="text-lg font-semibold tracking-tight text-mono">
                    Basics
                  </h3>
                  <p className="text-sm leading-6 text-muted-foreground">
                    Name the FAQ clearly and set the language people will search in.
                  </p>
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <TextField
                    control={form.control}
                    name="name"
                    label="FAQ name"
                    placeholder="e.g. Product onboarding FAQ"
                    description="Keep the name specific enough that teammates can find it quickly."
                  />
                  <TextField
                    control={form.control}
                    name="language"
                    label="Language"
                    description="Use the locale customers will search in, for example en-US."
                    placeholder="en-US"
                  />
                </div>
                <div className="space-y-1 pt-2">
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
                    Configure behavior
                  </p>
                  <h3 className="text-lg font-semibold tracking-tight text-mono">
                    Visibility and ranking
                  </h3>
                  <p className="text-sm leading-6 text-muted-foreground">
                    Decide when this FAQ should be visible and how its answers should be ordered.
                  </p>
                </div>
                <div className="grid gap-4 md:grid-cols-2">
                  <SelectField
                    control={form.control}
                    name="status"
                    label="Status"
                    description="Draft keeps the FAQ private while you build. Publish only when the answers are ready."
                    options={Object.entries(faqStatusLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                  <SelectField
                    control={form.control}
                    name="sortStrategy"
                    label="Sort strategy"
                    description="Choose whether answers follow manual order or other ranking logic."
                    options={Object.entries(faqSortStrategyLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                </div>
                <div className="space-y-1 pt-2">
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-primary">
                    Optional
                  </p>
                  <h3 className="text-lg font-semibold tracking-tight text-mono">
                    CTA behavior
                  </h3>
                  <p className="text-sm leading-6 text-muted-foreground">
                    Add a next step only if answers in this FAQ should route users somewhere else.
                  </p>
                </div>
                <SwitchField
                  control={form.control}
                  name="ctaEnabled"
                  label="Enable CTA"
                  description="Controls whether Q&A items can show CTA links."
                />
                <SelectField
                  control={form.control}
                  name="ctaTarget"
                  label="CTA target"
                  description={
                    ctaEnabled
                      ? "Choose where the CTA should open when a Q&A item includes a link."
                      : "Enable CTA first to choose the target behavior."
                  }
                  disabled={!ctaEnabled}
                  options={Object.entries(ctaTargetLabels).map(
                    ([value, label]) => ({
                      value,
                      label,
                    }),
                  )}
                />
                <div className="flex flex-wrap items-center gap-3">
                  <Button type="submit" disabled={isSubmitting}>
                    {mode === "create" ? "Create FAQ" : "Save changes"}
                  </Button>
                  <Button asChild variant="outline">
                    <Link
                      to={mode === "edit" && id ? `/app/faq/${id}` : "/app/faq"}
                    >
                      Cancel
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
