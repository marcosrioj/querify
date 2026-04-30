import { Activity, Link2 } from "lucide-react";
import { Link } from "react-router-dom";
import type { ActivityDto } from "@/domains/activity/types";
import { Button } from "@/shared/ui";
import { translateText } from "@/shared/lib/i18n-core";

export function ActivityRelationshipActions({ event }: { event: ActivityDto }) {
  const subjectHref = event.answerId
    ? `/app/answers/${event.answerId}`
    : `/app/questions/${event.questionId}`;
  const subjectLabel = event.answerId ? "Open answer" : "Open question";

  return (
    <div className="flex flex-wrap gap-2">
      <Button asChild variant="outline" size="sm">
        <Link to={`/app/activity/${event.id}`}>
          <Activity className="size-4" />
          {translateText("Open event")}
        </Link>
      </Button>
      <Button asChild variant="ghost" size="sm">
        <Link to={subjectHref}>
          <Link2 className="size-4" />
          {translateText(subjectLabel)}
        </Link>
      </Button>
    </div>
  );
}
