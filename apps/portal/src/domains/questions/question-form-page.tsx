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
  ChannelKind,
  QuestionStatus,
  VisibilityScope,
  channelKindLabels,
  questionStatusLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import {
  useQuestion,
  useCreateQuestion,
  useUpdateQuestion,
} from "@/domains/questions/hooks";
import { useActivationVisibilityPrompt } from "@/domains/qna/activation-visibility";
import {
  questionFormSchema,
  type QuestionFormValues,
} from "@/domains/questions/schemas";
import { useSpace, useSpaceList } from "@/domains/spaces/hooks";
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

function buildSpaceOption(space: { id: string; name: string; slug: string }) {
  return {
    value: space.id,
    label: space.name,
    description: space.slug,
    keywords: [space.name, space.slug],
  };
}

export function QuestionFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const preselectedSpaceId = searchParams.get("spaceId") ?? "";
  const [spaceSearch, setSpaceSearch] = useState("");
  const deferredSpaceSearch = useDeferredValue(spaceSearch.trim());
  const questionQuery = useQuestion(mode === "edit" ? id : undefined);
  const createQuestion = useCreateQuestion();
  const updateQuestion = useUpdateQuestion(id ?? "");
  const { resolveActivationVisibility, ActivationVisibilityDialog } =
    useActivationVisibilityPrompt();

  const form = useForm<QuestionFormValues>({
    resolver: zodResolver(questionFormSchema),
    defaultValues: {
      spaceId: preselectedSpaceId,
      title: "",
      summary: "",
      contextNote: "",
      status: QuestionStatus.Draft,
      visibility: VisibilityScope.Internal,
      originChannel: ChannelKind.Manual,
      sort: 0,
    },
  });

  useEffect(() => {
    if (!questionQuery.data) {
      return;
    }

    form.reset({
      spaceId: questionQuery.data.spaceId,
      title: questionQuery.data.title,
      summary: questionQuery.data.summary ?? "",
      contextNote: questionQuery.data.contextNote ?? "",
      status: questionQuery.data.status,
      visibility: questionQuery.data.visibility,
      originChannel: questionQuery.data.originChannel,
      sort: questionQuery.data.sort,
    });
  }, [form, questionQuery.data]);

  const selectedSpaceId =
    form.watch("spaceId") || questionQuery.data?.spaceId || preselectedSpaceId;
  const selectedSpaceQuery = useSpace(selectedSpaceId || undefined);
  const spaceOptionsQuery = useSpaceList({
    page: 1,
    pageSize: 20,
    sorting: "Name ASC",
    searchText: deferredSpaceSearch || undefined,
  });
  const selectedSpace =
    spaceOptionsQuery.data?.items.find(
      (space) => space.id === selectedSpaceId,
    ) ?? selectedSpaceQuery.data;
  const selectedVisibility = Number(
    form.watch("visibility"),
  ) as VisibilityScope;
  const selectedStatus = Number(form.watch("status")) as QuestionStatus;
  const publicVisibilitySelected =
    selectedVisibility === VisibilityScope.Public;
  const invalidPublicStatus =
    publicVisibilitySelected && selectedStatus !== QuestionStatus.Active;
  const questionStatusOptions = Object.entries(questionStatusLabels).map(
    ([value, label]) => ({
      value,
      label,
    }),
  );
  const spaceBlocksQuestions = selectedSpace?.acceptsQuestions === false;
  const spaceOptions = (spaceOptionsQuery.data?.items ?? []).map(
    buildSpaceOption,
  );
  const selectedSpaceOption = selectedSpace
    ? buildSpaceOption(selectedSpace)
    : null;
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "space",
      label: "Space",
      description: "Attach the question to its owning operating space.",
      complete: hasSetupText(setupValues.spaceId),
    },
    {
      id: "title",
      label: "Title",
      description: "Write the canonical question wording.",
      complete: hasSetupText(setupValues.title, 3),
    },
    {
      id: "status",
      label: "Status",
      description: "Set the starting lifecycle for the question.",
      complete: hasSetupValue(setupValues.status),
    },
    {
      id: "visibility",
      label: "Visibility",
      description:
        "Choose whether the question stays internal or can be public.",
      complete: hasSetupValue(setupValues.visibility),
    },
    {
      id: "origin",
      label: "Origin channel",
      description: "Record where this question came from.",
      complete: hasSetupValue(setupValues.originChannel),
    },
  ];
  const isSubmitting = createQuestion.isPending || updateQuestion.isPending;
  const backTo =
    mode === "edit" && id
      ? `/app/questions/${id}`
      : selectedSpaceId
        ? `/app/spaces/${selectedSpaceId}`
        : "/app/spaces";
  const pageTitle =
    mode === "create"
      ? "New question"
      : questionQuery.data?.title
        ? `${translateText("Edit")} ${questionQuery.data.title}`
        : "Edit question";

  if (mode === "create" && !preselectedSpaceId) {
    return (
      <DetailLayout
        header={
          <PageHeader
            title="New question"
            description="A question needs a Space before it can inherit intake, visibility, tags, sources, and activity context."
            descriptionMode="hint"
            backTo="/app/spaces"
          />
        }
      >
        <EmptyState
          title="Open a Space before creating a question"
          description="Choose the operating Space first. The creation form will then keep the parent context fixed and only ask for question details."
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
          description="Capture the question, its operational status, and the context needed for accurate answers."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && questionQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Questions own workflow, accepted answers, sources, tags, and public feedback.",
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
                    label: "Space",
                    value: selectedSpace?.name || "Choose in form",
                  },
                  {
                    label: "Origin",
                    value: "Manual, widget, API, help center, and more",
                  },
                  {
                    label: "Workflow",
                    value: "Draft, active, or archived",
                  },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {ActivationVisibilityDialog}
      {questionQuery.isError ? (
        <ErrorState
          title="Unable to load question"
          error={questionQuery.error}
          retry={() => void questionQuery.refetch()}
        />
      ) : mode === "edit" && questionQuery.isLoading ? (
        <FormCardSkeleton fields={12} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Question details")}</span>
                  <ContextHint
                    content={translateText(
                      "Start with the space, then set lifecycle, visibility, and question context so downstream answers inherit the right guardrails.",
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
                    const nextStatus = Number(values.status) as QuestionStatus;
                    let nextVisibility = Number(
                      values.visibility,
                    ) as VisibilityScope;
                    const isActivating =
                      nextStatus === QuestionStatus.Active &&
                      (mode === "create" ||
                        questionQuery.data?.status !== QuestionStatus.Active);

                    if (
                      isActivating &&
                      nextVisibility !== VisibilityScope.Public
                    ) {
                      const resolvedVisibility =
                        await resolveActivationVisibility(nextVisibility);
                      if (resolvedVisibility === null) {
                        return;
                      }

                      nextVisibility = resolvedVisibility;
                    }

                    const createBody = {
                      spaceId: values.spaceId,
                      title: values.title,
                      summary: values.summary || undefined,
                      contextNote: values.contextNote || undefined,
                      status: nextStatus,
                      visibility: nextVisibility,
                      originChannel: Number(
                        values.originChannel,
                      ) as ChannelKind,
                      sort: values.sort,
                    };

                    if (mode === "create") {
                      const createdId =
                        await createQuestion.mutateAsync(createBody);
                      navigate(`/app/questions/${createdId}`);
                      return;
                    }

                    await updateQuestion.mutateAsync({
                      title: createBody.title,
                      summary: createBody.summary,
                      contextNote: createBody.contextNote,
                      status: createBody.status,
                      visibility: createBody.visibility,
                      originChannel: createBody.originChannel,
                      sort: createBody.sort,
                      acceptedAnswerId:
                        questionQuery.data?.acceptedAnswerId || undefined,
                    });
                    navigate(`/app/questions/${id}`);
                  })}
                >
                  <FormSectionHeading
                    title="Placement"
                    description="Pick the owning space first so the question inherits the right exposure and intake rules."
                  />
                  <SearchSelectField
                    control={form.control}
                    name="spaceId"
                    label="Space"
                    description="The space controls exposure and how the question should be operated."
                    placeholder="Search and choose the owning space"
                    searchPlaceholder="Search spaces"
                    emptyMessage={
                      deferredSpaceSearch
                        ? "No spaces match this search."
                        : "No spaces available."
                    }
                    options={spaceOptions}
                    selectedOption={selectedSpaceOption}
                    loading={spaceOptionsQuery.isFetching}
                    disabled={Boolean(preselectedSpaceId) || mode === "edit"}
                    searchValue={spaceSearch}
                    onSearchChange={(value) =>
                      startTransition(() => setSpaceSearch(value))
                    }
                  />
                  <FormSectionHeading
                    title="Identity"
                    description="Use the wording customers or operators will actually search for."
                  />
                  <div className="grid gap-4">
                    <TextField
                      control={form.control}
                      name="title"
                      label="Title"
                      placeholder="How do I activate the workspace for a new tenant?"
                      description="Use the canonical question wording."
                    />
                    <TextareaField
                      control={form.control}
                      name="summary"
                      label="Summary"
                      rows={3}
                      description="A compact explanation of the question before the full context."
                    />
                    <TextareaField
                      control={form.control}
                      name="contextNote"
                      label="Context note"
                      rows={4}
                      description="Operational nuance that answer authors should understand."
                    />
                  </div>
                  <FormSectionHeading
                    title="Workflow and order"
                    description="Set lifecycle, audience exposure, intake channel, and manual ordering together."
                  />
                  <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                    <SelectField
                      control={form.control}
                      name="status"
                      label="Status"
                      description="Controls whether the question is draft, active, or archived."
                      options={questionStatusOptions}
                    />
                    <SelectField
                      control={form.control}
                      name="visibility"
                      label="Visibility"
                      description="Controls internal, authenticated external, or public question exposure."
                      options={Object.entries(visibilityScopeLabels).map(
                        ([value, label]) => ({
                          value,
                          label,
                        }),
                      )}
                    />
                    <SelectField
                      control={form.control}
                      name="originChannel"
                      label="Origin channel"
                      description="Records where the question came from for reporting and routing."
                      options={Object.entries(channelKindLabels).map(
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
                      description="Lower values appear earlier in curated ordering."
                    />
                  </div>
                  {spaceBlocksQuestions ? (
                    <p className="text-sm text-muted-foreground">
                      {translateText(
                        "This space does not accept new questions.",
                      )}
                    </p>
                  ) : null}
                  {invalidPublicStatus ? (
                    <p className="text-sm text-muted-foreground">
                      {translateText(
                        "Public visibility requires status Active.",
                      )}
                    </p>
                  ) : null}
                  <div className="flex flex-wrap items-center gap-3">
                    <Button
                      type="submit"
                      disabled={
                        isSubmitting ||
                        spaceBlocksQuestions ||
                        invalidPublicStatus
                      }
                    >
                      {translateText(
                        mode === "create" ? "Create question" : "Save changes",
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
            title={mode === "create" ? "Question setup" : "Question edit setup"}
            description="Complete the required question fields before saving this question."
            steps={setupSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
