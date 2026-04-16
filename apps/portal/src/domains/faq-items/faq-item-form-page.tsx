import { zodResolver } from "@hookform/resolvers/zod";
import { startTransition, useDeferredValue, useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { X } from "lucide-react";
import {
  Link,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import { useContentRef, useContentRefList } from "@/domains/content-refs/hooks";
import type { ContentRefDto } from "@/domains/content-refs/types";
import { useFaq, useFaqList } from "@/domains/faq/hooks";
import type { FaqDto } from "@/domains/faq/types";
import {
  useCreateFaqItem,
  useFaqItem,
  useUpdateFaqItem,
} from "@/domains/faq-items/hooks";
import { FaqItemAnswersCard } from "@/domains/faq-items/faq-item-answers-card";
import {
  faqItemFormSchema,
  type FaqItemFormValues,
} from "@/domains/faq-items/schemas";
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
import { ErrorState } from "@/shared/ui/placeholder-state";
import {
  SearchSelectField,
  SwitchField,
  TextField,
  TextareaField,
} from "@/shared/ui/form-fields";
import {
  contentRefKindLabels,
  faqStatusLabels,
} from "@/shared/constants/backend-enums";
import { translateText } from "@/shared/lib/i18n-core";

function buildFaqOption(faq: FaqDto) {
  const statusLabel = faqStatusLabels[faq.status];

  return {
    value: faq.id,
    label: faq.name,
    description: `${faq.language} • ${statusLabel}`,
    keywords: [faq.name, faq.language, statusLabel],
  };
}

function buildContentRefOption(contentRef: ContentRefDto) {
  const primaryLabel = contentRef.label || contentRef.locator;
  const secondaryDetails = [
    contentRefKindLabels[contentRef.kind],
    contentRef.label ? contentRef.locator : contentRef.scope || undefined,
  ].filter(Boolean);

  return {
    value: contentRef.id,
    label: primaryLabel,
    description: secondaryDetails.join(" • "),
    keywords: [
      primaryLabel,
      contentRef.locator,
      contentRef.scope ?? "",
      contentRefKindLabels[contentRef.kind],
    ],
  };
}

export function FaqItemFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id: faqId, itemId } = useParams();
  const [searchParams] = useSearchParams();
  const resolvedItemId = itemId;
  const preselectedFaqId = faqId ?? searchParams.get("faqId") ?? "";
  const preselectedContentRefId = searchParams.get("contentRefId") ?? "";
  const [faqSearch, setFaqSearch] = useState("");
  const [contentRefSearch, setContentRefSearch] = useState("");
  const deferredFaqSearch = useDeferredValue(faqSearch.trim());
  const deferredContentRefSearch = useDeferredValue(contentRefSearch.trim());
  const itemQuery = useFaqItem(mode === "edit" ? resolvedItemId : undefined);
  const createFaqItem = useCreateFaqItem();
  const updateFaqItem = useUpdateFaqItem(resolvedItemId ?? "");

  const form = useForm<FaqItemFormValues>({
    resolver: zodResolver(faqItemFormSchema),
    defaultValues: {
      question: "",
      additionalInfo: "",
      ctaTitle: "",
      ctaUrl: "",
      sort: 10,
      isActive: true,
      faqId: preselectedFaqId,
      contentRefId: preselectedContentRefId,
    },
  });

  const watchedFaqId = form.watch("faqId");
  const watchedContentRefId = form.watch("contentRefId");
  const currentFaqId =
    watchedFaqId || itemQuery.data?.faqId || preselectedFaqId;
  const currentContentRefId =
    watchedContentRefId ??
    itemQuery.data?.contentRefId ??
    preselectedContentRefId ??
    "";
  const selectedFaqQuery = useFaq(currentFaqId || undefined);
  const selectedContentRefQuery = useContentRef(
    currentContentRefId || undefined,
  );
  const faqOptionsQuery = useFaqList({
    page: 1,
    pageSize: 20,
    sorting: "Name ASC",
    searchText: deferredFaqSearch || undefined,
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 20,
    sorting: "Label ASC",
    searchText: deferredContentRefSearch || undefined,
  });

  useEffect(() => {
    if (!itemQuery.data) {
      return;
    }

    form.reset({
      question: itemQuery.data.question,
      additionalInfo: itemQuery.data.additionalInfo ?? "",
      ctaTitle: itemQuery.data.ctaTitle ?? "",
      ctaUrl: itemQuery.data.ctaUrl ?? "",
      sort: itemQuery.data.sort,
      isActive: itemQuery.data.isActive,
      faqId: itemQuery.data.faqId,
      contentRefId:
        preselectedContentRefId || itemQuery.data.contentRefId || "",
    });
  }, [form, itemQuery.data, preselectedContentRefId]);

  const selectedFaq =
    faqOptionsQuery.data?.items.find((faq) => faq.id === currentFaqId) ??
    selectedFaqQuery.data;
  const selectedContentRef =
    contentRefQuery.data?.items.find(
      (contentRef) => contentRef.id === currentContentRefId,
    ) ?? selectedContentRefQuery.data;
  const faqOptions = (faqOptionsQuery.data?.items ?? []).map(buildFaqOption);
  const contentRefOptions = (contentRefQuery.data?.items ?? []).map(
    buildContentRefOption,
  );
  const selectedFaqOption = selectedFaq ? buildFaqOption(selectedFaq) : null;
  const selectedContentRefOption = selectedContentRef
    ? buildContentRefOption(selectedContentRef)
    : null;
  const questionValue = form.watch("question");
  const backTo =
    mode === "edit" && currentFaqId && resolvedItemId
      ? `/app/faq/${currentFaqId}/items/${resolvedItemId}`
      : currentFaqId
        ? `/app/faq/${currentFaqId}`
        : "/app/faq";
  const faqSettingsPath = currentFaqId
    ? `/app/faq/${currentFaqId}/edit`
    : "/app/faq";
  const faqResultHint = faqOptionsQuery.data
    ? faqOptionsQuery.data.totalCount > faqOptions.length
      ? translateText(
          "Showing {shown} of {total} FAQs. Keep typing to narrow the list.",
          {
            shown: faqOptions.length,
            total: faqOptionsQuery.data.totalCount,
          },
        )
      : faqOptionsQuery.data.totalCount === 1
        ? translateText("1 FAQ ready to pick.")
        : translateText("{count} FAQs ready to pick.", {
            count: faqOptionsQuery.data.totalCount,
          })
    : undefined;
  const contentRefResultHint = contentRefQuery.data
    ? contentRefQuery.data.totalCount > contentRefOptions.length
      ? translateText(
          "Showing {shown} of {total} sources. Keep typing to narrow the list.",
          {
            shown: contentRefOptions.length,
            total: contentRefQuery.data.totalCount,
          },
        )
      : contentRefQuery.data.totalCount === 1
        ? translateText("1 source ready to pick.")
        : translateText("{count} sources ready to pick.", {
            count: contentRefQuery.data.totalCount,
          })
    : undefined;
  const formSteps = [
    {
      id: "question",
      label: translateText("Write the question"),
      description: questionValue
        ? translateText("Current question: {value}", { value: questionValue })
        : translateText(
            "Use the exact wording a customer would search or ask.",
          ),
      complete: Boolean(questionValue?.trim()),
    },
    {
      id: "answers",
      label: translateText("Manage answer variants"),
      description:
        mode === "edit"
          ? itemQuery.data?.answers.length
            ? translateText("{count} answers are already attached.", {
                count: itemQuery.data.answers.length,
              })
            : translateText(
                "Add the first answer variant after saving the item.",
              )
          : translateText(
              "Create the item first, then manage answers on the item page.",
            ),
      complete:
        mode === "edit" ? Boolean(itemQuery.data?.answers.length) : false,
    },
    {
      id: "faq",
      label: translateText("Attach the right FAQ"),
      description: selectedFaq
        ? translateText("Currently attached to {name}.", {
            name: selectedFaq.name,
          })
        : translateText("Pick the FAQ that controls visibility for this item."),
      complete: Boolean(currentFaqId),
    },
  ];

  const isSubmitting = createFaqItem.isPending || updateFaqItem.isPending;

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === "create" ? "New Q&A item" : "Edit Q&A item"}
          description={translateText(
            "Write the question, item metadata, and source link. Answers are managed inside the Q&A item.",
          )}
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && itemQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Keep each Q&A item clear, ranked, and linked to a FAQ.",
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
                    label: "Associations",
                    value: "FAQ required, source optional",
                  },
                  {
                    label: "Scoring",
                    value: "Sort, feedback, and confidence affect ranking",
                  },
                  {
                    label: "Selected FAQ",
                    value:
                      selectedFaq?.name ||
                      (mode === "create"
                        ? "Choose in form"
                        : itemQuery.data?.faqId || "Loading"),
                  },
                  {
                    label: "Selected source",
                    value:
                      selectedContentRef?.label ||
                      selectedContentRef?.locator ||
                      "Optional",
                  },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {itemQuery.isError ? (
        <ErrorState
          title="Unable to load Q&A item"
          error={itemQuery.error}
          retry={() => void itemQuery.refetch()}
        />
      ) : mode === "edit" && itemQuery.isLoading ? (
        <FormCardSkeleton fields={8} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Details")}</span>
                  <ContextHint
                    content={translateText(
                      "Write the question and metadata first. Answer variants are managed separately on this item.",
                    )}
                    label={translateText("Form details")}
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
                      additionalInfo: values.additionalInfo || undefined,
                      ctaTitle: values.ctaTitle || undefined,
                      ctaUrl: values.ctaUrl || undefined,
                      contentRefId: values.contentRefId || undefined,
                    };

                    if (mode === "create") {
                      const createdId = await createFaqItem.mutateAsync(body);
                      navigate(
                        body.faqId
                          ? `/app/faq/${body.faqId}/items/${createdId}`
                          : "/app/faq",
                      );
                      return;
                    }

                    await updateFaqItem.mutateAsync(body);
                    navigate(
                      body.faqId && resolvedItemId
                        ? `/app/faq/${body.faqId}/items/${resolvedItemId}`
                        : "/app/faq",
                    );
                  })}
                >
                  <TextField
                    control={form.control}
                    name="question"
                    label="Question"
                    description="Phrase it the way a customer would naturally ask it."
                    placeholder="How do I connect my knowledge base?"
                  />
                  <TextareaField
                    control={form.control}
                    name="additionalInfo"
                    label="Additional info"
                    rows={4}
                    description="Optional notes, caveats, or internal context."
                  />
                  <FormSectionHeading
                    title={translateText("Connect it")}
                    description={translateText(
                      "Pick the FAQ that controls visibility for this item.",
                    )}
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SearchSelectField
                      control={form.control}
                      name="faqId"
                      label="FAQ"
                      description="Required. The parent FAQ controls publish state."
                      placeholder="Search and choose the parent FAQ"
                      searchPlaceholder="Search by FAQ name, language, or status"
                      emptyMessage={
                        deferredFaqSearch
                          ? "No FAQs match this search."
                          : "No FAQs available."
                      }
                      options={faqOptions}
                      selectedOption={selectedFaqOption}
                      loading={faqOptionsQuery.isFetching}
                      resultCountHint={faqResultHint}
                      searchValue={faqSearch}
                      onSearchChange={(value) =>
                        startTransition(() => setFaqSearch(value))
                      }
                    />
                    <SearchSelectField
                      control={form.control}
                      name="contentRefId"
                      label="Source"
                      description="Optional, but linking a source improves traceability."
                      placeholder="Search and link a source"
                      searchPlaceholder="Search by source label, type, scope, or URL"
                      emptyMessage={
                        deferredContentRefSearch
                          ? "No sources match this search."
                          : "No sources available."
                      }
                      options={contentRefOptions}
                      selectedOption={selectedContentRefOption}
                      loading={contentRefQuery.isFetching}
                      allowClear
                      clearLabel="Remove linked source"
                      resultCountHint={contentRefResultHint}
                      searchValue={contentRefSearch}
                      onSearchChange={(value) =>
                        startTransition(() => setContentRefSearch(value))
                      }
                    />
                  </div>
                  <FormSectionHeading
                    title={translateText("Tune ranking")}
                    description={translateText(
                      "These values control where this answer appears relative to other items in the FAQ.",
                    )}
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-3">
                    <TextField
                      control={form.control}
                      name="sort"
                      label="Sort"
                      type="number"
                      description="Lower values usually surface earlier."
                    />
                    <div className="space-y-2">
                      <p className="text-sm font-medium">
                        {translateText("Feedback score")}
                      </p>
                      <p className="flex h-9 w-full items-center rounded-md border border-input bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                        {mode === "edit"
                          ? (itemQuery.data?.feedbackScore ?? "—")
                          : "—"}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {translateText("Set automatically by user feedbacks.")}
                      </p>
                    </div>
                    <div className="space-y-2">
                      <p className="text-sm font-medium">
                        {translateText("Confidence")}
                      </p>
                      <p className="flex h-9 w-full items-center rounded-md border border-input bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
                        {mode === "edit"
                          ? (itemQuery.data?.confidenceScore ?? "—")
                          : "—"}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {translateText("Set automatically by the scoring pipeline.")}
                      </p>
                    </div>
                  </div>
                  <FormSectionHeading
                    title={translateText("CTA behavior")}
                    description={translateText(
                      "Optional call-to-action for this Q&A item. Leave blank if no next step is needed.",
                    )}
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <TextField
                      control={form.control}
                      name="ctaTitle"
                      label="CTA title"
                      description="Label the next step users should take."
                      placeholder="Start setup"
                    />
                    <TextField
                      control={form.control}
                      name="ctaUrl"
                      label="CTA URL"
                      description="Must start with http:// or https://"
                      placeholder="https://example.com/setup"
                    />
                  </div>
                  <SwitchField
                    control={form.control}
                    name="isActive"
                    label="Active"
                    hint="Inactive Q&A items stay saved but stay hidden from end users."
                  />
                  <div className="flex flex-wrap items-center gap-3">
                    <Button type="submit" disabled={isSubmitting}>
                      {translateText(
                        mode === "create" ? "Create Q&A item" : "Save changes",
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
          {mode === "edit" && resolvedItemId && itemQuery.data ? (
            <FaqItemAnswersCard
              faqItemId={resolvedItemId}
              answers={itemQuery.data.answers}
              question={itemQuery.data.question}
            />
          ) : (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex flex-wrap items-center gap-2">
                    <span>{translateText("Answers")}</span>
                    <ContextHint
                      content={translateText(
                        "Save the item first, then manage answer variants from the Q&A item page.",
                      )}
                      label={translateText("Answer setup details")}
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  {translateText(
                    "Answers now live as separate records under each Q&A item. Create the item first, then add and rank answer variants.",
                  )}
                </p>
              </CardContent>
            </Card>
          )}
          <ProgressChecklistCard
            eyebrow={translateText(
              mode === "create" ? "Start here" : "Progress",
            )}
            title={translateText("Complete the essentials first")}
            description={translateText(
              "Save the item shell, attach it to the right FAQ, then manage the answer variants.",
            )}
            steps={formSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
