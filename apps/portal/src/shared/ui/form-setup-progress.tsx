import type { ReactNode } from "react";
import {
  ProgressChecklistCard,
  type ProgressChecklistStep,
} from "@/shared/ui/progress-checklist";

export function FormSetupProgressCard({
  title,
  description,
  steps,
}: {
  title: ReactNode;
  description: ReactNode;
  steps: ProgressChecklistStep[];
}) {
  return (
    <ProgressChecklistCard
      eyebrow="Setup progress"
      title={title}
      description={description}
      steps={steps}
      className="shadow-none"
    />
  );
}
