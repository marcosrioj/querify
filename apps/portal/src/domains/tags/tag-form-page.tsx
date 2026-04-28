import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { X } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useCreateTag, useTag, useUpdateTag } from "@/domains/tags/hooks";
import { tagFormSchema, type TagFormValues } from "@/domains/tags/schemas";
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
  hasSetupText,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { ErrorState } from "@/shared/ui/placeholder-state";
import { TextField } from "@/shared/ui/form-fields";
import { translateText } from "@/shared/lib/i18n-core";

export function TagFormPage({ mode }: { mode: "create" | "edit" }) {
  const navigate = useNavigate();
  const { id } = useParams();
  const tagQuery = useTag(mode === "edit" ? id : undefined);
  const createTag = useCreateTag();
  const updateTag = useUpdateTag(id ?? "");

  const form = useForm<TagFormValues>({
    resolver: zodResolver(tagFormSchema),
    defaultValues: {
      name: "",
    },
  });

  useEffect(() => {
    if (!tagQuery.data) {
      return;
    }

    form.reset({ name: tagQuery.data.name });
  }, [form, tagQuery.data]);

  const isSubmitting = createTag.isPending || updateTag.isPending;
  const setupValues = form.watch();
  const setupSteps = [
    {
      id: "name",
      label: "Tag name",
      description: "Use a concise reusable label for taxonomy.",
      complete: hasSetupText(setupValues.name, 2),
    },
  ];
  const backTo = "/app/tags";

  return (
    <DetailLayout
      header={
        <PageHeader
          title={mode === "create" ? "New tag" : "Edit tag"}
          description="Keep the taxonomy concise and reusable across spaces and questions."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        mode === "edit" && tagQuery.isLoading ? (
          <SidebarSummarySkeleton />
        ) : (
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Quick notes")}</span>
                  <ContextHint
                    content={translateText(
                      "Tags should be reusable across multiple spaces and question threads, so avoid overly specific names.",
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
                    label: "Scope",
                    value: "Reusable taxonomy across spaces and questions",
                  },
                  {
                    label: "Spaces",
                    value: String(tagQuery.data?.spaceUsageCount ?? 0),
                  },
                  {
                    label: "Questions",
                    value: String(tagQuery.data?.questionUsageCount ?? 0),
                  },
                ]}
              />
            </CardContent>
          </Card>
        )
      }
    >
      {tagQuery.isError ? (
        <ErrorState
          title="Unable to load tag"
          error={tagQuery.error}
          retry={() => void tagQuery.refetch()}
        />
      ) : mode === "edit" && tagQuery.isLoading ? (
        <FormCardSkeleton fields={1} />
      ) : (
        <>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText("Tag details")}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent>
              <Form {...form}>
                <form
                  className="space-y-4"
                  onSubmit={form.handleSubmit(async (values) => {
                    if (mode === "create") {
                      await createTag.mutateAsync(values);
                      navigate("/app/tags");
                      return;
                    }

                    await updateTag.mutateAsync(values);
                    navigate("/app/tags");
                  })}
                >
                  <TextField
                    control={form.control}
                    name="name"
                    label="Tag name"
                    description="Use short reusable labels such as Billing, Activation, or API limits."
                  />
                  <div className="flex flex-wrap items-center gap-3">
                    <Button type="submit" disabled={isSubmitting}>
                      {translateText(
                        mode === "create" ? "Create tag" : "Save changes",
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
            title={mode === "create" ? "Tag setup" : "Tag edit setup"}
            description="Complete the required taxonomy field before saving this tag."
            steps={setupSteps}
          />
        </>
      )}
    </DetailLayout>
  );
}
