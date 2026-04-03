import { Link } from 'react-router-dom';

export function ResetPasswordPlaceholderPage() {
  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <h2 className="text-3xl font-semibold text-mono">Reset password</h2>
        <p className="text-sm leading-6 text-muted-foreground">
          Portal delegates password recovery to Auth0. Keep this route as a UI
          placeholder unless the identity provider requires a custom callback
          screen here.
        </p>
      </div>

      <div className="text-sm text-muted-foreground">
        <Link className="hover:text-foreground" to="/login">
          Back to sign in
        </Link>
      </div>
    </div>
  );
}
