import { ArrowUpRight, KeyRound } from 'lucide-react';
import { Link } from 'react-router-dom';
import { RuntimeEnv } from '@/platform/runtime/env';
import { Alert, AlertDescription, Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';

export function ForgotPasswordPage() {
  const hasResetUrl = Boolean(RuntimeEnv.auth0PasswordResetUrl);

  return (
    <Card className="border-white/10 bg-white/6 text-mono-foreground shadow-2xl backdrop-blur">
      <CardHeader className="space-y-4">
        <div className="inline-flex size-12 items-center justify-center rounded-2xl bg-white/10 text-white">
          <KeyRound className="size-5" />
        </div>
        <div className="space-y-2">
          <CardTitle className="text-3xl text-white">
            Password reset
          </CardTitle>
          <CardDescription className="text-sm leading-6 text-mono-foreground/70">
            There is no Portal-owned password reset endpoint in the repo. This
            flow should stay with the external identity provider.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent className="space-y-5">
        {hasResetUrl ? (
          <Button asChild className="w-full">
            <a href={RuntimeEnv.auth0PasswordResetUrl} target="_blank" rel="noreferrer">
              Open Auth0 reset flow
              <ArrowUpRight className="size-4" />
            </a>
          </Button>
        ) : (
          <Alert>
            <AlertDescription>
              Set `VITE_AUTH0_PASSWORD_RESET_URL` when your Auth0 reset screen is
              provisioned for the Portal SPA.
            </AlertDescription>
          </Alert>
        )}

        <div className="text-sm text-mono-foreground/75">
          <Link className="hover:text-white" to="/login">
            Back to sign in
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}
