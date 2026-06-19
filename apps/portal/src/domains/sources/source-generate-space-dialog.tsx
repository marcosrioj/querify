import { useState } from "react";
import { Loader2, Sparkles } from "lucide-react";
import {
  Button,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Input,
  Label,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Switch,
  Textarea,
} from "@/shared/ui";
import {
  SourceGenerationTagMode,
  SourceRole,
  SpaceStatus,
  VisibilityScope,
  sourceGenerationTagModeLabels,
  sourceRoleLabels,
  spaceStatusLabels,
  visibilityScopeLabels,
} from "@/shared/constants/backend-enums";
import { translateText } from "@/shared/lib/i18n-core";
import type { SourceGenerateSpaceRequestDto } from "@/domains/sources/types";

type SourceGenerateSpaceDialogProps = {
  open: boolean;
  sourceLabel: string;
  isPending: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (request: SourceGenerateSpaceRequestDto) => Promise<void>;
};

export function SourceGenerateSpaceDialog({
  open,
  sourceLabel,
  isPending,
  onOpenChange,
  onSubmit,
}: SourceGenerateSpaceDialogProps) {
  const [spaceName, setSpaceName] = useState("");
  const [spaceSlug, setSpaceSlug] = useState("");
  const [language, setLanguage] = useState("en-US");
  const [visibility, setVisibility] = useState(VisibilityScope.Internal);
  const [status, setStatus] = useState(SpaceStatus.Draft);
  const [acceptsQuestions, setAcceptsQuestions] = useState(true);
  const [acceptsAnswers, setAcceptsAnswers] = useState(true);
  const [extractionGoal, setExtractionGoal] = useState("");
  const [maxTopLevelQuestions, setMaxTopLevelQuestions] = useState(3);
  const [maxFollowUpDepth, setMaxFollowUpDepth] = useState(1);
  const [maxAnswersPerQuestion, setMaxAnswersPerQuestion] = useState(1);
  const [includeFollowUpQuestions, setIncludeFollowUpQuestions] = useState(true);
  const [tagGenerationMode, setTagGenerationMode] = useState(
    SourceGenerationTagMode.CreateAndAttach,
  );
  const [sourceRole, setSourceRole] = useState(SourceRole.Origin);
  const [requireEveryAnswerToCiteSource, setRequireEveryAnswerToCiteSource] =
    useState(true);
  const [contentHint, setContentHint] = useState("");

  const submitDisabled = isPending || !spaceName.trim();

  async function handleSubmit() {
    if (submitDisabled) {
      return;
    }

    await onSubmit({
      spaceName: spaceName.trim(),
      spaceSlug: spaceSlug.trim() || null,
      language: language.trim() || "en-US",
      visibility,
      status,
      acceptsQuestions,
      acceptsAnswers,
      extractionGoal: extractionGoal.trim() || null,
      maxTopLevelQuestions,
      maxFollowUpDepth,
      maxAnswersPerQuestion,
      includeFollowUpQuestions,
      tagGenerationMode,
      sourceRole,
      requireEveryAnswerToCiteSource,
      contentHint: contentHint.trim() || null,
    });
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>{translateText("Generate Q&A space")}</DialogTitle>
          <DialogDescription>
            {translateText(
              "Create a draft internal Space from {source}. Generated questions and answers stay in review.",
              { source: sourceLabel },
            )}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          <section className="space-y-3">
            <h3 className="text-sm font-medium">
              {translateText("Space setup")}
            </h3>
            <div className="grid gap-3 md:grid-cols-2">
              <LabeledInput
                label="Space name"
                value={spaceName}
                onChange={setSpaceName}
                placeholder="Generated support knowledge"
              />
              <LabeledInput
                label="Slug"
                value={spaceSlug}
                onChange={setSpaceSlug}
                placeholder="generated-support-knowledge"
              />
              <LabeledInput
                label="Language"
                value={language}
                onChange={setLanguage}
                placeholder="en-US"
              />
              <EnumSelect
                label="Visibility"
                value={visibility}
                onValueChange={setVisibility}
                options={[VisibilityScope.Internal, VisibilityScope.Authenticated]}
                labels={visibilityScopeLabels}
              />
              <EnumSelect
                label="Space status"
                value={status}
                onValueChange={setStatus}
                options={[SpaceStatus.Draft, SpaceStatus.Active, SpaceStatus.Archived]}
                labels={spaceStatusLabels}
              />
              <div className="grid gap-3 sm:grid-cols-2">
                <SwitchRow
                  label="Accept questions"
                  checked={acceptsQuestions}
                  onCheckedChange={setAcceptsQuestions}
                />
                <SwitchRow
                  label="Accept answers"
                  checked={acceptsAnswers}
                  onCheckedChange={setAcceptsAnswers}
                />
              </div>
            </div>
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-medium">
              {translateText("Generation scope")}
            </h3>
            <div className="grid gap-3 md:grid-cols-3">
              <NumberInput
                label="Top-level questions"
                value={maxTopLevelQuestions}
                min={1}
                max={12}
                onChange={setMaxTopLevelQuestions}
              />
              <NumberInput
                label="Follow-up depth"
                value={maxFollowUpDepth}
                min={0}
                max={3}
                onChange={setMaxFollowUpDepth}
              />
              <NumberInput
                label="Answers per question"
                value={maxAnswersPerQuestion}
                min={1}
                max={3}
                onChange={setMaxAnswersPerQuestion}
              />
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <SwitchRow
                label="Include follow-up questions"
                checked={includeFollowUpQuestions}
                onCheckedChange={setIncludeFollowUpQuestions}
              />
              <EnumSelect
                label="Tag generation"
                value={tagGenerationMode}
                onValueChange={setTagGenerationMode}
                options={[
                  SourceGenerationTagMode.None,
                  SourceGenerationTagMode.SuggestOnly,
                  SourceGenerationTagMode.CreateAndAttach,
                ]}
                labels={sourceGenerationTagModeLabels}
              />
            </div>
            <LabeledTextarea
              label="Extraction goal"
              value={extractionGoal}
              onChange={setExtractionGoal}
              placeholder="Audience, product area, or support workflow to prioritize"
            />
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-medium">
              {translateText("Source and evidence")}
            </h3>
            <div className="grid gap-3 md:grid-cols-2">
              <EnumSelect
                label="Source role"
                value={sourceRole}
                onValueChange={setSourceRole}
                options={[
                  SourceRole.Origin,
                  SourceRole.Context,
                  SourceRole.Evidence,
                  SourceRole.Reference,
                ]}
                labels={sourceRoleLabels}
              />
              <SwitchRow
                label="Require answer citations"
                checked={requireEveryAnswerToCiteSource}
                onCheckedChange={setRequireEveryAnswerToCiteSource}
              />
            </div>
            <LabeledTextarea
              label="Content hint"
              value={contentHint}
              onChange={setContentHint}
              placeholder="Section, range, or topic inside a long source"
            />
          </section>

          <div className="rounded-md border border-border bg-muted/20 p-3 text-sm text-muted-foreground">
            {translateText(
              "Generated Space content starts as Draft/Internal by default. The request returns a run id; completion status appears on this Source page.",
            )}
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            disabled={isPending}
            onClick={() => onOpenChange(false)}
          >
            {translateText("Cancel")}
          </Button>
          <Button type="button" disabled={submitDisabled} onClick={handleSubmit}>
            {isPending ? (
              <Loader2 className="size-4 animate-spin" />
            ) : (
              <Sparkles className="size-4" />
            )}
            {translateText("Start generation")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function LabeledInput({
  label,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  value: string;
  placeholder?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-2">
      <Label>{translateText(label)}</Label>
      <Input
        value={value}
        placeholder={placeholder}
        onChange={(event) => onChange(event.target.value)}
      />
    </div>
  );
}

function LabeledTextarea({
  label,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  value: string;
  placeholder?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-2">
      <Label>{translateText(label)}</Label>
      <Textarea
        value={value}
        placeholder={placeholder}
        onChange={(event) => onChange(event.target.value)}
      />
    </div>
  );
}

function NumberInput({
  label,
  value,
  min,
  max,
  onChange,
}: {
  label: string;
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
}) {
  return (
    <div className="space-y-2">
      <Label>{translateText(label)}</Label>
      <Input
        type="number"
        min={min}
        max={max}
        value={value}
        onChange={(event) => {
          const parsed = Number.parseInt(event.target.value, 10);
          onChange(Number.isFinite(parsed) ? Math.min(max, Math.max(min, parsed)) : min);
        }}
      />
    </div>
  );
}

function EnumSelect<TValue extends number>({
  label,
  value,
  options,
  labels,
  onValueChange,
}: {
  label: string;
  value: TValue;
  options: TValue[];
  labels: Record<TValue, string>;
  onValueChange: (value: TValue) => void;
}) {
  return (
    <div className="space-y-2">
      <Label>{translateText(label)}</Label>
      <Select
        value={String(value)}
        onValueChange={(nextValue) => onValueChange(Number(nextValue) as TValue)}
      >
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option} value={String(option)}>
              {translateText(labels[option])}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

function SwitchRow({
  label,
  checked,
  onCheckedChange,
}: {
  label: string;
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
}) {
  return (
    <div className="flex min-h-10 items-center justify-between gap-3 rounded-md border border-border bg-background px-3 py-2">
      <Label className="text-sm">{translateText(label)}</Label>
      <Switch checked={checked} onCheckedChange={onCheckedChange} />
    </div>
  );
}
