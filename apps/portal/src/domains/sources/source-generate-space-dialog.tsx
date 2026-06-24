import { useState } from "react";
import { Loader2, Sparkles } from "lucide-react";
import {
  Button,
  ContextHint,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Label,
  Textarea,
} from "@/shared/ui";
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
  const [extractionGoal, setExtractionGoal] = useState("");
  const [contentHint, setContentHint] = useState("");

  async function handleSubmit() {
    if (isPending) {
      return;
    }

    await onSubmit({
      extractionGoal: extractionGoal.trim() || null,
      contentHint: contentHint.trim() || null,
    });
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>{translateText("Generate Q&A space")}</DialogTitle>
          <DialogDescription>
            {translateText(
              "Create a draft internal Space from {source}. Generated questions and answers stay in review.",
              { source: sourceLabel },
            )}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-5">
          <section className="rounded-md border border-border bg-muted/20 p-4">
            <div className="flex items-start gap-3">
              <div className="inline-flex size-9 shrink-0 items-center justify-center rounded-md bg-primary/10 text-primary">
                <Sparkles className="size-4" aria-hidden="true" />
              </div>
              <div className="min-w-0 space-y-2">
                <h3 className="text-sm font-medium">
                  {translateText("Automatic generation plan")}
                </h3>
                <p className="text-sm text-muted-foreground">
                  {translateText(
                    "Querify derives the Space setup and graph size from the Source before starting the run.",
                  )}
                </p>
                <ul className="grid gap-1.5 text-sm text-muted-foreground sm:grid-cols-2">
                  <li>
                    {translateText(
                      "Space name, slug, and language are identified automatically.",
                    )}
                  </li>
                  <li>
                    {translateText(
                      "Draft/Internal review state is enforced until activation for public exposure.",
                    )}
                  </li>
                  <li>
                    {translateText(
                      "Questions, answers, follow-up depth, and answer count are planned automatically.",
                    )}
                  </li>
                  <li>
                    {translateText(
                      "Source links use Evidence and every answer must cite the Source.",
                    )}
                  </li>
                </ul>
              </div>
            </div>
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-medium">
              {translateText("Manual guidance")}
            </h3>
            <div className="grid gap-4">
              <LabeledTextarea
                label="Extraction goal"
                hint="Use this to tell the agent which audience, product area, or support workflow to prioritize."
                value={extractionGoal}
                onChange={setExtractionGoal}
                placeholder="Audience, product area, or support workflow to prioritize"
              />
              <LabeledTextarea
                label="Content hint"
                hint="Use this to point the agent to a section, range, or topic inside long source material."
                value={contentHint}
                onChange={setContentHint}
                placeholder="Section, range, or topic inside a long source"
              />
            </div>
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
          <Button type="button" disabled={isPending} onClick={handleSubmit}>
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

function LabeledTextarea({
  label,
  hint,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  hint: string;
  value: string;
  placeholder?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-2">
      <div className="flex items-center gap-1.5">
        <Label>{translateText(label)}</Label>
        <ContextHint content={translateText(hint)} />
      </div>
      <Textarea
        value={value}
        placeholder={placeholder ? translateText(placeholder) : undefined}
        onChange={(event) => onChange(event.target.value)}
      />
    </div>
  );
}
