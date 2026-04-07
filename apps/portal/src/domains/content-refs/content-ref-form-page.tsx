import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import {
  useContentRef,
  useCreateContentRef,
  useUpdateContentRef,
} from "@/domains/content-refs/hooks";
import {
  contentRefFormSchema,
  type ContentRefFormValues,
} from "@/domains/content-refs/schemas";
import {
  ContentRefKind,
  contentRefKindLabels,
} from "@/shared/constants/backend-enums";
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
import { SelectField, TextField } from "@/shared/ui/form-fields";

export function ContentRefFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id: faqId, contentRefId } = useParams();
  const [searchParams] = useSearchParams();
  const originatingFaqItemId = searchParams.get("faqItemId") ?? "";
  const resolvedContentRefId = contentRefId;
  const contentRefQuery = useContentRef(
    mode === "edit" ? resolvedContentRefId : undefined,
  );
  const createContentRef = useCreateContentRef();
  const updateContentRef = useUpdateContentRef(resolvedContentRefId ?? "");
  const backTo = faqId ? `/app/faq/${faqId}` : "/app/faq";
  const detailPath =
    mode === "edit" && faqId && resolvedContentRefId
      ? `/app/faq/${faqId}/content-refs/${resolvedContentRefId}`
      : backTo;
  const buildDetailPath = (nextContentRefId: string) =>
    faqId
      ? `/app/faq/${faqId}/content-refs/${nextContentRefId}${originatingFaqItemId ? `?faqItemId=${originatingFaqItemId}` : ""}`
      : "/app/faq";
  const contextualBackTo =
    mode === "create" && faqId && originatingFaqItemId
      ? `/app/faq/${faqId}/items/${originatingFaqItemId}`
      : detailPath;

  const form = useForm<ContentRefFormValues>({
    resolver: zodResolver(contentRefFormSchema),
    defaultValues: {
      kind: ContentRefKind.Web,
      locator: "",
      label: "",
      scope: "",
    },
  });

  useEffect(() => {
    if (!contentRefQuery.data) {
      return;
    }

    form.reset({
      kind: contentRefQuery.data.kind,
      locator: contentRefQuery.data.locator,
      label: contentRefQuery.data.label ?? "",
      scope: contentRefQuery.data.scope ?? "",
    });
  }, [contentRefQuery.data, form]);

  const kindValue = Number(form.watch("kind")) as ContentRefKind;
  const locatorValue = form.watch("locator");
  const labelValue = form.watch("label");
  const formSteps = [
    {
      id: "kind",
      label: "Choose the source type",
      description: contentRefKindLabels[kindValue]
        ? `Current type: ${contentRefKindLabels[kindValue]}.`
        : "Classify the source so teammates know what kind of reference they are opening.",
      complete: Number.isFinite(kindValue),
    },
    {
      id: "locator",
      label: "Add the stable reference",
      description: locatorValue
        ? "The source reference is filled in."
        : "Use the canonical URL, document path, or repository location.",
      complete: Boolean(locatorValue?.trim()),
    },
    {
      id: "label",
      label: "Give it a reusable label",
      description: labelValue
        ? `Current label: ${labelValue}`
        : "A good label helps people find this source again without opening it first.",
      complete: Boolean(labelValue?.trim()),
    },
  ];

  const isSubmitting = createContentRef.isPending || updateContentRef.isPending;

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="Sources"
          title={mode === "create" ? "New source" : "Edit source"}
          description="Add a page, file, or doc your Q&A items can use."
          descriptionMode="hint"
          backTo={contextualBackTo}
        />
      }
      sidebar={
        mode === "edit" && contentRefQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>Quick notes</span>
                  <ContextHint
                    content="Good source records stay durable, labeled, and easy to reuse."
                    label="Quick notes details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <KeyValueList
                items={[
                  {
                    label: "Kinds",
                    value: "Web, PDF, document, video, repository, manual",
                  },
                  {
                    label: "Reference",
                    value: "Use a stable URI or file path reference",
                  },
                  {
                    label: "Scope",
                    value: "Optional grouping label for the workspace",
                  },
                  {
                    label: "Selected type",
                    value: contentRefKindLabels[kindValue] ?? "Choose in form",
                  },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {contentRefQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          error={contentRefQuery.error}
          retry={() => void contentRefQuery.refetch()}
        />
      ) : mode === "edit" && contentRefQuery.isLoading ? (
        <FormCardSkeleton fields={4} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>Details</span>
                  <ContextHint
                    content="Make this source easy to find and reuse."
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
                      kind: Number(values.kind) as ContentRefKind,
                      label: values.label || undefined,
                      scope: values.scope || undefined,
                    };

                    if (mode === "create") {
                      const createdId = await createContentRef.mutateAsync(body);
                      navigate(buildDetailPath(createdId));
                      return;
                    }

                    await updateContentRef.mutateAsync(body);
                    navigate(detailPath);
                  })}
                >
                  {originatingFaqItemId ? (
                    <Alert variant="info" appearance="light">
                      <div className="space-y-1">
                        <AlertTitle>Creating a source for a Q&A item</AlertTitle>
                        <AlertDescription>
                          Save this source first, then link it back to the Q&A
                          item from the next screen.
                        </AlertDescription>
                      </div>
                    </Alert>
                  ) : null}
                  <FormSectionHeading
                    title="Define the source"
                    description="Make the reference durable first. That keeps the source usable when people revisit it later."
                    className="pt-2"
                  />
                  <SelectField
                    control={form.control}
                    name="kind"
                    label="Type"
                    description="Choose the format teammates will recognize fastest."
                    options={Object.entries(contentRefKindLabels).map(
                      ([value, label]) => ({
                        value,
                        label,
                      }),
                    )}
                  />
                  <TextField
                    control={form.control}
                    name="locator"
                    label="Reference"
                    description="Use the canonical URL, file path, repository URI, or document locator."
                    hint="Examples: URL, PDF path, repository URI, or document locator."
                    placeholder="https://docs.example.com/onboarding"
                  />
                  <FormSectionHeading
                    title="Make it reusable"
                    description="Add the naming context people need so this source is easy to find without opening it first."
                    className="pt-2"
                  />
                  <div className="grid gap-4 md:grid-cols-2">
                    <TextField
                      control={form.control}
                      name="label"
                      label="Label"
                      description="Name the source the way teammates will search for it later."
                      placeholder="Onboarding setup guide"
                    />
                    <TextField
                      control={form.control}
                      name="scope"
                      label="Scope"
                      description="Optional workspace grouping such as Billing, Product docs, or Support."
                      placeholder="Product docs"
                    />
                  </div>
                  <div className="flex flex-wrap items-center gap-3">
                    <Button type="submit" disabled={isSubmitting}>
                      {mode === "create" ? "Create source" : "Save changes"}
                    </Button>
                    <Button asChild variant="outline">
                      <Link to={contextualBackTo}>Cancel</Link>
                    </Button>
                  </div>
                </form>
              </Form>
            </CardContent>
          </Card>
          <ProgressChecklistCard
            eyebrow={mode === "create" ? "Start here" : "Progress"}
            title="Make the source easy to trust and reuse"
            description="Start with the source type, a durable locator, and a label teammates can recognize at a glance."
            steps={formSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
