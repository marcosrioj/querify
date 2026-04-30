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
import {
  AnswerKind,
  AnswerStatus,
  VisibilityScope,
  answerKindLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  useAnswer,
  useCreateAnswer,
  useUpdateAnswer,
} from "@/domains/answers/hooks";
import {
  answerFormSchema,
  type AnswerFormValues,
} from "@/domains/answers/schemas";
import { useQuestion, useQuestionList } from "@/domains/questions/hooks";
import { useSpace } from "@/domains/spaces/hooks";
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
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import {
  SearchSelectField,
  SelectField,
  TextField,
  TextareaField,
} from "@/shared/ui/form-fields";
import { translateText } from "@/shared/lib/i18n-core";

function buildQuestionOption(question: {
  id: string;
  title: string;
  spaceSlug: string;
}) {
  return {
    value: question.id,
    label: question.title,
    description: question.spaceSlug,
    keywords: [question.title, question.spaceSlug],
  };
}

export function AnswerFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const preselectedQuestionId = searchParams.get("questionId") ?? "";
  const [questionSearch, setQuestionSearch] = useState("");
  const deferredQuestionSearch = useDeferredValue(questionSearch.trim());
  const answerQuery = useAnswer(mode === "edit" ? id : undefined);
  const createAnswer = useCreateAnswer();
  const updateAnswer = useUpdateAnswer(id ?? "");

  const form = useForm<AnswerFormValues>({
    resolver: zodResolver(answerFormSchema),
    defaultValues: {
      questionId: preselectedQuestionId,
      headline: "",
      body: "",
      kind: AnswerKind.Official,
      visibility: VisibilityScope.Internal,
      contextNote: "",
      authorLabel: "",
      sort: 1,
    },
  });

  useEffect(() => {
    if (!answerQuery.data) {
      return;
    }

    form.reset({
      questionId: answerQuery.data.questionId,
      headline: answerQuery.data.headline,
      body: answerQuery.data.body ?? "",
      kind: answerQuery.data.kind,
      visibility: answerQuery.data.visibility,
      contextNote: answerQuery.data.contextNote ?? "",
      authorLabel: answerQuery.data.authorLabel ?? "",
      sort: answerQuery.data.sort,
    });
  }, [answerQuery.data, form]);

  const selectedQuestionId =
    form.watch("questionId") ||
    answerQuery.data?.questionId ||
    preselectedQuestionId;
  const selectedQuestionQuery = useQuestion(selectedQuestionId || undefined);
  const questionOptionsQuery = useQuestionList({
    page: 1,
    pageSize: 20,
    sorting: "Title ASC",
    searchText: deferredQuestionSearch || undefined,
  });
  const selectedQuestion =
    questionOptionsQuery.data?.items.find(
      (question) => question.id === selectedQuestionId,
    ) ?? selectedQuestionQuery.data;
  const selectedSpaceQuery = useSpace(selectedQuestion?.spaceId);
  const selectedVisibility = Number(
    form.watch("visibility"),
  ) as VisibilityScope;
  const currentAnswerStatus =
    mode === "create"
      ? AnswerStatus.Draft
      : (answerQuery.data?.status ?? AnswerStatus.Draft);
  const publicVisibilitySelected =
    selectedVisibility === VisibilityScope.Public;
  const invalidPublicStatus =
    publicVisibilitySelected && currentAnswerStatus !== AnswerStatus.Active;
  const spaceBlocksAnswers = selectedSpaceQuery.data?.acceptsAnswers === false;
  const questionOptions = (questionOptionsQuery.data?.items ?? []).map(
    buildQuestionOption,
  );
  const selectedQuestionOption = selectedQuestion
    ? buildQuestionOption(selectedQuestion)
    : null;
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "question",
      label: "Question",
      description: "Attach the answer to the right question.",
      complete: hasSetupText(setupValues.questionId),
    },
    {
      id: "headline",
      label: "Headline",
      description: "Write the answer title operators and customers will scan.",
      complete: hasSetupText(setupValues.headline, 3),
    },
    {
      id: "kind",
      label: "Answer kind",
      description: "Classify how official or contextual this answer is.",
      complete: hasSetupValue(setupValues.kind),
    },
    {
      id: "visibility",
      label: "Visibility",
      description:
        "Choose whether this answer stays internal or can be public.",
      complete: hasSetupValue(setupValues.visibility),
    },
  ];
  const isSubmitting = createAnswer.isPending || updateAnswer.isPending;
  const backTo =
    mode === "edit" && id
      ? `/app/answers/${id}`
      : selectedQuestionId
        ? `/app/questions/${selectedQuestionId}`
        : "/app/spaces";
  const pageTitle =
    mode === "create"
      ? "New answer"
      : answerQuery.data?.headline
        ? `${translateText("Edit")} ${answerQuery.data.headline}`
        : "Edit answer";

  if (mode === "create" && !preselectedQuestionId) {
    return (
      <DetailLayout
        header={
          <PageHeader
            title="New answer"
            description="An answer needs a parent Question before activation, sources, and accepted state are meaningful."
            descriptionMode="hint"
            backTo="/app/spaces"
          />
        }
      >
        <EmptyState
          title="Open a Question before creating an answer"
          description="Start in a Space, choose the question, then author the answer candidate with the right context already locked."
          action={{ label: "Open spaces", to: "/app/spaces" }}
        />
      </DetailLayout>
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={pageTitle}
          description="Author the answer candidate, then tune rank, visibility, and trust cues."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && answerQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Answers are ranked, activated, archived, and often grounded with evidence links after they are saved.",
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
                    label: "Question",
                    value: selectedQuestion?.title || "Choose in form",
                  },
                  {
                    label: "Ranking",
                    value: "Sort controls manual answer order",
                  },
                  { label: "Context", value: "Context note is optional" },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {answerQuery.isError ? (
        <ErrorState
          title="Unable to load answer"
          error={answerQuery.error}
          retry={() => void answerQuery.refetch()}
        />
      ) : mode === "edit" && answerQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Answer details")}</span>
                  <ContextHint
                    content={translateText(
                      "Write the answer first, then decide how visible and official it should be.",
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
                    const createBody = {
                      questionId: values.questionId,
                      headline: values.headline,
                      body: values.body || undefined,
                      contextNote: values.contextNote || undefined,
                      authorLabel: values.authorLabel || undefined,
                      kind: Number(values.kind) as AnswerKind,
                      status: currentAnswerStatus,
                      visibility: Number(values.visibility) as VisibilityScope,
                      sort: values.sort,
                    };

                    if (mode === "create") {
                      const createdId =
                        await createAnswer.mutateAsync(createBody);
                      navigate(`/app/answers/${createdId}`);
                      return;
                    }

                    await updateAnswer.mutateAsync({
                      headline: createBody.headline,
                      body: createBody.body,
                      contextNote: createBody.contextNote,
                      authorLabel: createBody.authorLabel,
                      kind: createBody.kind,
                      status: createBody.status,
                      visibility: createBody.visibility,
                      sort: createBody.sort,
                    });
                    navigate(`/app/answers/${id}`);
                  })}
                >
                  <FormSectionHeading
                    title="Placement"
                    description="Attach the answer to the right question before tuning visibility or trust."
                  />
                  <SearchSelectField
                    control={form.control}
                    name="questionId"
                    label="Question"
                    description="The question this answer candidate belongs to."
                    placeholder="Search and choose a question"
                    searchPlaceholder="Search questions"
                    emptyMessage={
                      deferredQuestionSearch
                        ? "No questions match this search."
                        : "No questions available."
                    }
                    options={questionOptions}
                    selectedOption={selectedQuestionOption}
                    loading={questionOptionsQuery.isFetching}
                    disabled={Boolean(preselectedQuestionId) || mode === "edit"}
                    searchValue={questionSearch}
                    onSearchChange={(value) =>
                      startTransition(() => setQuestionSearch(value))
                    }
                  />
                  <FormSectionHeading
                    title="Content"
                    description="Write the headline customers should trust first, then add the deeper answer body."
                  />
                  <div className="grid gap-4">
                    <TextField
                      control={form.control}
                      name="headline"
                      label="Headline"
                      description="The short answer that should appear as the candidate title."
                    />
                    <TextareaField
                      control={form.control}
                      name="body"
                      label="Body"
                      rows={8}
                      description="The full guidance, including steps, caveats, and nuance."
                    />
                  </div>
                  <FormSectionHeading
                    title="Trust, attribution, and order"
                    description="Control answer kind, audience exposure, author cue, and manual ordering together."
                  />
                  <div className="grid gap-4 md:grid-cols-3">
                    <SelectField
                      control={form.control}
                      name="kind"
                      label="Answer kind"
                      description="Classifies the answer so workflow and badges present it correctly."
                      options={Object.entries(answerKindLabels).map(
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
                      description="Controls internal, authenticated external, or public answer exposure."
                      options={Object.entries(visibilityScopeLabels).map(
                        ([value, label]) => ({
                          value,
                          label,
                        }),
                      )}
                    />
                    <TextField
                      control={form.control}
                      name="sort"
                      label="Sort"
                      description="Lower numbers surface first when acceptance does not override ordering."
                    />
                    <div className="md:col-span-3">
                      <TextField
                        control={form.control}
                        name="authorLabel"
                        label="Author label"
                        description="Optional attribution shown with the answer when authorship should be clear."
                      />
                    </div>
                    <div className="md:col-span-3">
                      <TextareaField
                        control={form.control}
                        name="contextNote"
                        label="Context note"
                        rows={4}
                        description="Optional note explaining why, when, or how this answer applies."
                      />
                    </div>
                  </div>
                  {spaceBlocksAnswers ? (
                    <p className="text-sm text-muted-foreground">
                      {translateText("This space does not accept new answers.")}
                    </p>
                  ) : null}
                  {invalidPublicStatus ? (
                    <p className="text-sm text-muted-foreground">
                      {translateText(
                        "Only active answers can be exposed publicly.",
                      )}
                    </p>
                  ) : null}
                  <div className="flex flex-wrap items-center gap-3">
                    <Button
                      type="submit"
                      disabled={
                        isSubmitting ||
                        spaceBlocksAnswers ||
                        invalidPublicStatus
                      }
                    >
                      {translateText(
                        mode === "create" ? "Create answer" : "Save changes",
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
            title={mode === "create" ? "Answer setup" : "Answer edit setup"}
            description="Complete the required answer fields before saving this candidate."
            steps={setupSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
