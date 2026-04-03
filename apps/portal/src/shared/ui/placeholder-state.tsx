import { Link } from 'react-router-dom';
import { AlertTriangle, ArrowRight, Inbox } from 'lucide-react';
import { Button, Card, CardContent } from '@/shared/ui';

export function EmptyState({
  title,
  description,
  action,
}: {
  title: string;
  description: string;
  action?: { label: string; to: string };
}) {
  return (
    <Card className="border-dashed bg-muted/10">
      <CardContent className="flex flex-col items-center gap-4 py-12 text-center">
        <div className="rounded-full border border-primary/15 bg-primary/10 p-3 text-primary">
          <Inbox className="size-5" />
        </div>
        <div className="space-y-2">
          <h3 className="text-base font-semibold text-mono">{title}</h3>
          <p className="max-w-lg text-sm leading-6 text-muted-foreground">
            {description}
          </p>
        </div>
        {action ? (
          <Button asChild>
            <Link to={action.to}>
              {action.label}
              <ArrowRight className="size-4" />
            </Link>
          </Button>
        ) : null}
      </CardContent>
    </Card>
  );
}

export function ErrorState({
  title,
  description,
  retry,
}: {
  title: string;
  description: string;
  retry?: () => void;
}) {
  return (
    <Card className="border-destructive/30 bg-destructive/5">
      <CardContent className="flex flex-col items-center gap-4 py-10 text-center">
        <div className="rounded-full border border-destructive/15 bg-destructive/10 p-3 text-destructive">
          <AlertTriangle className="size-5" />
        </div>
        <div className="space-y-2">
          <h3 className="text-base font-semibold text-mono">{title}</h3>
          <p className="max-w-lg text-sm leading-6 text-muted-foreground">
            {description}
          </p>
        </div>
        {retry ? <Button onClick={retry}>Try again</Button> : null}
      </CardContent>
    </Card>
  );
}

export function NotFoundPage() {
  return (
    <Card className="w-full max-w-xl">
      <CardContent className="space-y-5 p-8 text-center">
        <p className="text-xs font-medium uppercase tracking-[0.24em] text-primary">
          BaseFAQ Portal
        </p>
        <div className="space-y-2">
          <h1 className="text-3xl font-semibold text-mono">Route not found</h1>
          <p className="text-sm leading-6 text-muted-foreground">
            This route is outside the Portal surface or has not been mapped yet.
          </p>
        </div>
        <Button asChild>
          <Link to="/app/dashboard">Go to dashboard</Link>
        </Button>
      </CardContent>
    </Card>
  );
}
