import { LockKeyhole, MoveRight } from 'lucide-react';
import { Navigate, Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '@/platform/auth/auth-context';
import { RuntimeEnv } from '@/platform/runtime/env';
import { Alert, AlertDescription, Badge, Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';

const SWAGGER_UI_CLIENT_ID = 'fDJib60pSRhbtNRPhqYfZR9J8JqBCpz5';

export function LoginPage() {
  const { isConfigured, status, error, login } = useAuth();
  const [searchParams] = useSearchParams();
  const nextPath = searchParams.get('next') ?? '/app/dashboard';
  const callbackUrl =
    RuntimeEnv.auth0RedirectUri || `${window.location.origin}${RuntimeEnv.baseUrl}login`;
  const isUsingSwaggerClient = RuntimeEnv.auth0ClientId === SWAGGER_UI_CLIENT_ID;

  if (status === 'ready') {
    return <Navigate to={nextPath} replace />;
  }

  return (
    <Card className="border-white/10 bg-white/6 text-mono-foreground shadow-2xl backdrop-blur">
      <CardHeader className="space-y-4">
        <Badge variant="outline" className="w-fit border-white/15 bg-white/5 text-mono-foreground">
          BaseFAQ Portal
        </Badge>
        <div className="space-y-2">
          <CardTitle className="text-3xl text-white">
            Sign in to your tenant workspace
          </CardTitle>
          <CardDescription className="text-sm leading-6 text-mono-foreground/70">
            The Portal app authenticates against Auth0 and then calls only the
            Portal-side Tenant and FAQ APIs already present in this repository.
          </CardDescription>
        </div>
      </CardHeader>
      <CardContent className="space-y-5">
        {!isConfigured ? (
          <Alert variant="destructive">
            <AlertDescription>
              `VITE_AUTH0_CLIENT_ID` is not set. Auth0 domain and audience were
              derived from the backend config, but the Portal SPA client still
              needs to be configured in the frontend environment.
            </AlertDescription>
          </Alert>
        ) : null}

        {error ? (
          <Alert variant="destructive">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}

        {isUsingSwaggerClient ? (
          <Alert variant="destructive">
            <AlertDescription>
              The configured `VITE_AUTH0_CLIENT_ID` matches the Swagger UI Auth0
              client from the .NET APIs. That application is documented with
              Swagger callback URLs only, so Portal login will fail unless Auth0
              also allows <span className="font-medium">{callbackUrl}</span>.
            </AlertDescription>
          </Alert>
        ) : null}

        <div className="rounded-2xl border border-white/10 bg-black/10 p-4 text-sm text-mono-foreground/75">
          <div className="flex items-center gap-2 font-medium text-white">
            <LockKeyhole className="size-4" />
            Auth runtime
          </div>
          <dl className="mt-3 space-y-2">
            <div className="flex items-center justify-between gap-3">
              <dt>Authority</dt>
              <dd className="truncate text-right">{RuntimeEnv.auth0Domain}</dd>
            </div>
            <div className="flex items-center justify-between gap-3">
              <dt>Audience</dt>
              <dd className="truncate text-right">{RuntimeEnv.auth0Audience}</dd>
            </div>
            <div className="flex items-center justify-between gap-3">
              <dt>Callback</dt>
              <dd className="truncate text-right">{callbackUrl}</dd>
            </div>
          </dl>
        </div>

        <Button
          className="h-12 w-full"
          disabled={!isConfigured || status === 'booting'}
          onClick={() => void login(nextPath)}
        >
          Continue with Auth0
          <MoveRight className="size-4" />
        </Button>

        <div className="flex items-center justify-between text-sm text-mono-foreground/75">
          <Link className="hover:text-white" to="/forgot-password">
            Forgot password
          </Link>
          <span>{status === 'booting' ? 'Initializing session' : 'Portal login'}</span>
        </div>
      </CardContent>
    </Card>
  );
}
