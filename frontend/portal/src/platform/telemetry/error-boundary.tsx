import { Component, ErrorInfo, PropsWithChildren } from 'react';
import { captureException } from '@/platform/telemetry/logger';
import { Button } from '@/components/ui/button';

type State = {
  hasError: boolean;
};

export class AppErrorBoundary extends Component<PropsWithChildren, State> {
  state: State = {
    hasError: false,
  };

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    captureException(error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-screen items-center justify-center bg-muted px-4">
          <div className="w-full max-w-xl rounded-2xl border border-border bg-background p-8 shadow-sm">
            <p className="text-sm font-medium uppercase tracking-[0.24em] text-primary">
              BaseFAQ Portal
            </p>
            <h1 className="mt-4 text-3xl font-semibold text-mono">
              An unexpected error interrupted the workspace.
            </h1>
            <p className="mt-3 text-sm text-muted-foreground">
              The error has been captured locally. Reload the app to continue,
              and connect telemetry before production rollout.
            </p>
            <Button
              className="mt-6"
              onClick={() => {
                window.location.reload();
              }}
            >
              Reload portal
            </Button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
