import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import { useFaqList } from "@/domains/faq/hooks";
import { useContentRefList } from "@/domains/content-refs/hooks";
import {
  useCreateFaqItem,
  useFaqItem,
  useUpdateFaqItem,
} from "@/domains/faq-items/hooks";
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
  Alert,
  AlertDescription,
  AlertTitle,
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
  SelectField,
  SwitchField,
  TextField,
  TextareaField,
} from "@/shared/ui/form-fields";

export function FaqItemFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id: faqId, itemId } = useParams();
  const [searchParams] = useSearchParams();
  const resolvedItemId = itemId;
  const preselectedFaqId = faqId ?? searchParams.get("faqId") ?? "";
  const preselectedContentRefId = searchParams.get("contentRefId") ?? "";
  const itemQuery = useFaqItem(mode === "edit" ? resolvedItemId : undefined);
  const faqOptionsQuery = useFaqList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: "Label ASC",
  });
  const createFaqItem = useCreateFaqItem();
  const updateFaqItem = useUpdateFaqItem(resolvedItemId ?? "");

  const form = useForm<FaqItemFormValues>({
    resolver: zodResolver(faqItemFormSchema),
    defaultValues: {
      question: "",
      shortAnswer: "",
      answer: "",
      additionalInfo: "",
      ctaTitle: "",
      ctaUrl: "",
      sort: 10,
      voteScore: 0,
      aiConfidenceScore: 0,
      isActive: true,
      faqId: preselectedFaqId,
      contentRefId: preselectedContentRefId,
    },
  });

  useEffect(() => {
    if (!itemQuery.data) {
      return;
    }

    form.reset({
      question: itemQuery.data.question,
      shortAnswer: itemQuery.data.shortAnswer,
      answer: itemQuery.data.answer ?? "",
      additionalInfo: itemQuery.data.additionalInfo ?? "",
      ctaTitle: itemQuery.data.ctaTitle ?? "",
      ctaUrl: itemQuery.data.ctaUrl ?? "",
      sort: itemQuery.data.sort,
      voteScore: itemQuery.data.voteScore,
      aiConfidenceScore: itemQuery.data.aiConfidenceScore,
      isActive: itemQuery.data.isActive,
      faqId: itemQuery.data.faqId,
      contentRefId:
        preselectedContentRefId || itemQuery.data.contentRefId || "",
    });
  }, [form, itemQuery.data, preselectedContentRefId]);

  const selectedFaq = faqOptionsQuery.data?.items.find(
    (faq) => faq.id === form.watch("faqId"),
  );
  const selectedContentRef = contentRefQuery.data?.items.find(
    (contentRef) => contentRef.id === form.watch("contentRefId"),
  );
  const questionValue = form.watch("question");
  const shortAnswerValue = form.watch("shortAnswer");
  const currentFaqId =
    form.watch("faqId") || itemQuery.data?.faqId || preselectedFaqId;
  const faqAllowsCta = Boolean(selectedFaq?.ctaEnabled);
  const backTo =
    mode === "edit" && currentFaqId && resolvedItemId
      ? `/app/faq/${currentFaqId}/items/${resolvedItemId}`
      : currentFaqId
        ? `/app/faq/${currentFaqId}`
        : "/app/faq";
  const faqSettingsPath = currentFaqId ? `/app/faq/${currentFaqId}/edit` : "/app/faq";
  const isEditingCurrentFaq = currentFaqId === itemQuery.data?.faqId;
  const formSteps = [
    {
      id: "question",
      label: "Write the question",
      description: questionValue
        ? `Current question: ${questionValue}`
        : "Use the exact wording a customer would search or ask.",
      complete: Boolean(questionValue?.trim()),
    },
    {
      id: "short-answer",
      label: "Add the short answer",
      description: shortAnswerValue
        ? "The summary answer is in place."
        : "Give the user the fast answer before they need the longer one.",
      complete: Boolean(shortAnswerValue?.trim()),
    },
    {
      id: "faq",
      label: "Attach the right FAQ",
      description: selectedFaq
        ? `Currently attached to ${selectedFaq.name}.`
        : "Pick the FAQ that controls visibility and CTA behavior.",
      complete: Boolean(currentFaqId),
    },
  ];

  const isSubmitting = createFaqItem.isPending || updateFaqItem.isPending;

  useEffect(() => {
    if (faqAllowsCta) {
      return;
    }

    form.clearErrors(["ctaTitle", "ctaUrl"]);

    if (mode === "create" || !isEditingCurrentFaq) {
      form.setValue("ctaTitle", "");
      form.setValue("ctaUrl", "");
    }
  }, [faqAllowsCta, form, isEditingCurrentFaq, mode]);

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="Q&A items"
          title={mode === "create" ? "New Q&A item" : "Edit Q&A item"}
          description="Write the question and answer, set score, and link a source."
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
                  <span>Quick notes</span>
                  <ContextHint
                    content="Keep each Q&A item clear, ranked, and linked to a FAQ."
                    label="Quick notes details"
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
                    value: "Sort, vote, and AI confidence affect ranking",
                  },
                  {
                    label: "CTA policy",
                    value: selectedFaq
                      ? selectedFaq.ctaEnabled
                        ? "Inherited from FAQ and currently enabled"
                        : "Disabled by the selected FAQ"
                      : "Select an FAQ to see CTA policy",
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
          description="The Q&A item request failed."
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
                  <span>Details</span>
                  <ContextHint
                    content="Write the question, answer, and source details people rely on."
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
                      answer: values.answer || undefined,
                      additionalInfo: values.additionalInfo || undefined,
                      ctaTitle: faqAllowsCta
                        ? values.ctaTitle || undefined
                        : mode === "edit" && isEditingCurrentFaq
                          ? itemQuery.data?.ctaTitle || undefined
                          : undefined,
                      ctaUrl: faqAllowsCta
                        ? values.ctaUrl || undefined
                        : mode === "edit" && isEditingCurrentFaq
                          ? itemQuery.data?.ctaUrl || undefined
                          : undefined,
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
                  <TextField
                    control={form.control}
                    name="shortAnswer"
                    label="Short answer"
                    description="This is the fast summary users should see first."
                    placeholder="Connect your content source, then publish the FAQ."
                  />
                  <FormSectionHeading
                    title="Connect it"
                    description="Pick the FAQ first. It controls visibility rules and whether this item can use a CTA."
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <SelectField
                      control={form.control}
                      name="faqId"
                      label="FAQ"
                      description="Required. The parent FAQ controls publish state and CTA behavior."
                      options={(faqOptionsQuery.data?.items ?? []).map((faq) => ({
                        value: faq.id,
                        label: faq.name,
                      }))}
                    />
                    <SelectField
                      control={form.control}
                      name="contentRefId"
                      label="Source"
                      description="Optional, but linking a source improves traceability."
                      options={[
                        { value: "", label: "None" },
                        ...(contentRefQuery.data?.items ?? []).map(
                          (contentRef) => ({
                            value: contentRef.id,
                            label: contentRef.label || contentRef.locator,
                          }),
                        ),
                      ]}
                    />
                  </div>
                  <FormSectionHeading
                    title="Tune ranking"
                    description="These values control where this answer appears relative to other items in the FAQ."
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
                    <TextField
                      control={form.control}
                      name="voteScore"
                      label="Vote score"
                      type="number"
                      description="Use this to reflect answer quality or team preference."
                    />
                    <TextField
                      control={form.control}
                      name="aiConfidenceScore"
                      label="AI confidence"
                      type="number"
                      description="Keeps AI ranking logic visible to the team."
                    />
                  </div>
                  <FormSectionHeading
                    title="Expand the answer"
                    description="Use the short answer for speed, then add fuller guidance or supporting notes if needed."
                    className="pt-2"
                  />
                  <TextareaField
                    control={form.control}
                    name="answer"
                    label="Full answer"
                    rows={7}
                    description="Optional deeper answer for users who need more than the summary."
                  />
                  <TextareaField
                    control={form.control}
                    name="additionalInfo"
                    label="Additional info"
                    rows={4}
                    description="Optional notes, caveats, or internal context."
                  />
                  <div className="space-y-3 pt-2">
                    <FormSectionHeading
                      title="CTA behavior"
                      description="Item-level CTA fields inherit from the selected FAQ. If the FAQ disables CTA, these fields are locked."
                    />
                    {!currentFaqId ? (
                      <Alert variant="info" appearance="light">
                        <div className="space-y-1">
                          <AlertTitle>Select an FAQ first</AlertTitle>
                          <AlertDescription>
                            CTA fields stay unavailable until you choose the FAQ that controls this answer.
                          </AlertDescription>
                        </div>
                      </Alert>
                    ) : !faqAllowsCta ? (
                      <div className="space-y-3">
                        <Alert variant="warning" appearance="light">
                          <div className="space-y-1">
                            <AlertTitle>CTA disabled by FAQ</AlertTitle>
                            <AlertDescription>
                              Enable CTA on the parent FAQ before you can add or edit CTA values for this Q&A item.
                            </AlertDescription>
                          </div>
                        </Alert>
                        <Button asChild variant="outline" size="sm">
                          <Link to={faqSettingsPath}>Open FAQ CTA settings</Link>
                        </Button>
                      </div>
                    ) : (
                      <Alert variant="success" appearance="light">
                        <div className="space-y-1">
                          <AlertTitle>CTA available</AlertTitle>
                          <AlertDescription>
                            This FAQ allows item-level CTA values, so you can define the next step below.
                          </AlertDescription>
                        </div>
                      </Alert>
                    )}
                  </div>
                  <div className="grid gap-4 md:grid-cols-2">
                    <TextField
                      control={form.control}
                      name="ctaTitle"
                      label="CTA title"
                      description={
                        faqAllowsCta
                          ? "Label the next step users should take."
                          : "Enable CTA on the FAQ to edit this field."
                      }
                      placeholder="Start setup"
                      disabled={!faqAllowsCta}
                    />
                    <TextField
                      control={form.control}
                      name="ctaUrl"
                      label="CTA URL"
                      description={
                        faqAllowsCta
                          ? "Must start with http:// or https://"
                          : "Enable CTA on the FAQ to edit this field."
                      }
                      placeholder="https://example.com/setup"
                      disabled={!faqAllowsCta}
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
                      {mode === "create" ? "Create Q&A item" : "Save changes"}
                    </Button>
                    <Button asChild variant="outline">
                      <Link to={backTo}>Cancel</Link>
                    </Button>
                  </div>
                </form>
              </Form>
            </CardContent>
          </Card>
          <ProgressChecklistCard
            eyebrow={mode === "create" ? "Start here" : "Progress"}
            title="Complete the essentials first"
            description="Get the core answer in place before tuning ranking, source links, or CTA behavior."
            steps={formSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
