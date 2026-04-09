import { zodResolver } from "@hookform/resolvers/zod";
import { Pencil, Plus, ThumbsUp, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  useCreateFaqItemAnswer,
  useDeleteFaqItemAnswer,
  useUpdateFaqItemAnswer,
} from "@/domains/faq-items/hooks";
import type {
  FaqItemAnswerCreateRequestDto,
  FaqItemAnswerDto,
  FaqItemAnswerUpdateRequestDto,
} from "@/domains/faq-items/types";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ConfirmAction,
  ContextHint,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  Form,
  FormSectionHeading,
} from "@/shared/ui";
import { SwitchField, TextField, TextareaField } from "@/shared/ui/form-fields";
import { translateText } from "@/shared/lib/i18n-core";

const faqItemAnswerFormSchema = z.object({
  shortAnswer: z.string().min(3, "Short answer is required."),
  answer: z.string().optional(),
  sort: z.coerce.number().int(),
  isActive: z.boolean(),
});

type FaqItemAnswerFormValues = z.infer<typeof faqItemAnswerFormSchema>;

type FaqItemAnswersCardProps = {
  faqItemId: string;
  answers: FaqItemAnswerDto[];
  question: string;
};

export function FaqItemAnswersCard({
  faqItemId,
  answers,
  question,
}: FaqItemAnswersCardProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [editingAnswer, setEditingAnswer] = useState<FaqItemAnswerDto | null>(
    null,
  );
  const createFaqItemAnswer = useCreateFaqItemAnswer();
  const updateFaqItemAnswer = useUpdateFaqItemAnswer(editingAnswer?.id ?? "");
  const deleteFaqItemAnswer = useDeleteFaqItemAnswer();
  const nextSort =
    answers.reduce((max, answer) => Math.max(max, answer.sort), 0) + 1;
  const isSubmitting =
    createFaqItemAnswer.isPending ||
    updateFaqItemAnswer.isPending ||
    deleteFaqItemAnswer.isPending;

  const form = useForm<FaqItemAnswerFormValues>({
    resolver: zodResolver(faqItemAnswerFormSchema),
    defaultValues: {
      shortAnswer: "",
      answer: "",
      sort: nextSort,
      isActive: true,
    },
  });

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    form.reset({
      shortAnswer: editingAnswer?.shortAnswer ?? "",
      answer: editingAnswer?.answer ?? "",
      sort: editingAnswer?.sort ?? nextSort,
      isActive: editingAnswer?.isActive ?? true,
    });
  }, [editingAnswer, form, isOpen, nextSort]);

  async function onSubmit(values: FaqItemAnswerFormValues) {
    const body: FaqItemAnswerCreateRequestDto | FaqItemAnswerUpdateRequestDto =
      {
        shortAnswer: values.shortAnswer,
        answer: values.answer || undefined,
        sort: values.sort,
        isActive: values.isActive,
        faqItemId,
      };

    if (editingAnswer) {
      await updateFaqItemAnswer.mutateAsync(body);
    } else {
      await createFaqItemAnswer.mutateAsync(body);
    }

    setIsOpen(false);
    setEditingAnswer(null);
  }

  function openCreateDialog() {
    setEditingAnswer(null);
    setIsOpen(true);
  }

  function openEditDialog(answer: FaqItemAnswerDto) {
    setEditingAnswer(answer);
    setIsOpen(true);
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex flex-wrap items-start justify-between gap-3">
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>{translateText("Answers")}</span>
                <Badge variant="outline">
                  {translateText("{count} answers", { count: answers.length })}
                </Badge>
                <ContextHint
                  content={translateText(
                    "Manage every answer variant for this question here. Votes help surface the strongest answer first.",
                  )}
                  label={translateText("Answer management details")}
                />
              </CardTitle>
            </CardHeading>
            <Button type="button" size="sm" onClick={openCreateDialog}>
              <Plus className="size-4" />
              {translateText("Add answer")}
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {answers.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-border bg-muted/10 p-4">
              <p className="font-medium text-mono">
                {translateText("No answers yet")}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {translateText(
                  'Save the first answer variant for "{question}".',
                  {
                    question,
                  },
                )}
              </p>
            </div>
          ) : (
            answers.map((answer, index) => (
              <div
                key={answer.id}
                className="rounded-2xl border border-border bg-muted/10 p-4"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="space-y-2">
                    <div className="flex flex-wrap items-center gap-2">
                      {index === 0 ? (
                        <Badge variant="success">
                          {translateText("Top answer")}
                        </Badge>
                      ) : null}
                      <Badge variant={answer.isActive ? "outline" : "mono"}>
                        {translateText(answer.isActive ? "Active" : "Inactive")}
                      </Badge>
                      <Badge variant="outline">
                        {translateText("Sort {value}", { value: answer.sort })}
                      </Badge>
                      <Badge variant="outline" className="gap-1">
                        <ThumbsUp className="size-3.5" />
                        {translateText("Votes {value}", {
                          value: answer.voteScore,
                        })}
                      </Badge>
                    </div>
                    <div>
                      <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                        {translateText("Short answer")}
                      </p>
                      <p className="mt-2 text-sm leading-6">
                        {answer.shortAnswer}
                      </p>
                    </div>
                    {answer.answer ? (
                      <div>
                        <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                          {translateText("Full answer")}
                        </p>
                        <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                          {answer.answer}
                        </p>
                      </div>
                    ) : null}
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => openEditDialog(answer)}
                    >
                      <Pencil className="size-4" />
                      {translateText("Edit")}
                    </Button>
                    <ConfirmAction
                      title={translateText("Delete this answer?")}
                      description={translateText(
                        "This removes the answer variant and its vote history from the item.",
                      )}
                      confirmLabel={translateText("Delete answer")}
                      isPending={deleteFaqItemAnswer.isPending}
                      onConfirm={() =>
                        deleteFaqItemAnswer.mutateAsync(answer.id)
                      }
                      trigger={
                        <Button type="button" size="sm" variant="destructive">
                          <Trash2 className="size-4" />
                          {translateText("Delete")}
                        </Button>
                      }
                    />
                  </div>
                </div>
              </div>
            ))
          )}
        </CardContent>
      </Card>

      <Dialog
        open={isOpen}
        onOpenChange={(open) => {
          setIsOpen(open);
          if (!open) {
            setEditingAnswer(null);
          }
        }}
      >
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {translateText(editingAnswer ? "Edit answer" : "Add answer")}
            </DialogTitle>
            <DialogDescription>
              {translateText(
                editingAnswer
                  ? "Update the answer variant shown for this question."
                  : "Create another answer variant for this question.",
              )}
            </DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <TextField
                control={form.control}
                name="shortAnswer"
                label={translateText("Short answer")}
                description={translateText(
                  "This summary can surface first in lists and previews.",
                )}
                placeholder={translateText(
                  "Give the fast answer users should see first.",
                )}
              />
              <TextareaField
                control={form.control}
                name="answer"
                label={translateText("Full answer")}
                rows={6}
                description={translateText(
                  "Optional deeper guidance for users who need more detail.",
                )}
              />
              <FormSectionHeading
                title={translateText("Answer settings")}
                description={translateText(
                  "Sort controls ordering, and votes help the strongest answer rise to the top.",
                )}
              />
              <div className="grid gap-4 md:grid-cols-2">
                <TextField
                  control={form.control}
                  name="sort"
                  label={translateText("Sort")}
                  type="number"
                  description={translateText(
                    "Lower values appear first before vote score is considered.",
                  )}
                />
                <SwitchField
                  control={form.control}
                  name="isActive"
                  label={translateText("Active")}
                  hint={translateText(
                    "Inactive answers stay saved but hidden from end users.",
                  )}
                />
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <Button type="submit" disabled={isSubmitting}>
                  {translateText(
                    editingAnswer ? "Save answer" : "Create answer",
                  )}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    setIsOpen(false);
                    setEditingAnswer(null);
                  }}
                >
                  {translateText("Cancel")}
                </Button>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
