import { Link } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui';

export function ResetPasswordPlaceholderPage() {
  return (
    <Card className="border-white/10 bg-white/6 text-mono-foreground shadow-2xl backdrop-blur">
      <CardHeader>
        <CardTitle className="text-3xl text-white">Reset password</CardTitle>
        <CardDescription className="text-mono-foreground/70">
          Portal delegates password recovery to Auth0. Keep this route as a UI
          placeholder unless the identity provider requires a custom callback
          screen here.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Link className="text-sm text-mono-foreground/75 hover:text-white" to="/login">
          Back to sign in
        </Link>
      </CardContent>
    </Card>
  );
}
