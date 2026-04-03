import { ChevronDown, LogOut, Moon } from 'lucide-react';
import { useTheme } from 'next-themes';
import { Link } from 'react-router-dom';
import { useAuth } from '@/platform/auth/auth-context';
import {
  Avatar,
  AvatarFallback,
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  Switch,
} from '@/shared/ui';
import { RoleBadge } from '@/shared/ui/status-badges';
import { getInitials } from '@/lib/helpers';

export function UserMenu({
  variant = 'full',
}: {
  variant?: 'full' | 'compact';
}) {
  const { user, logout } = useAuth();
  const { resolvedTheme, setTheme } = useTheme();
  const initials = getInitials(user?.name ?? user?.email, 2);

  const handleThemeToggle = (checked: boolean) => {
    setTheme(checked ? 'dark' : 'light');
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        {variant === 'compact' ? (
          <button
            type="button"
            className="cursor-pointer rounded-full border-2 border-secondary"
          >
            <Avatar className="size-9">
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
          </button>
        ) : (
          <Button variant="outline" className="h-11 gap-3 rounded-full px-3">
            <Avatar className="size-8">
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
            <div className="hidden min-w-0 text-left md:block">
              <p className="truncate text-sm font-medium text-mono">
                {user?.name ?? 'Portal user'}
              </p>
              <p className="truncate text-xs text-muted-foreground">
                {user?.email ?? 'No email in token'}
              </p>
            </div>
            <ChevronDown className="size-4 text-muted-foreground" />
          </Button>
        )}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-72">
        <DropdownMenuLabel className="space-y-2">
          <div className="text-sm font-semibold text-mono">
            {user?.name ?? 'Portal user'}
          </div>
          <div className="text-xs font-normal text-muted-foreground">
            {user?.email ?? 'Email claim unavailable'}
          </div>
          {user ? <RoleBadge role={user.role} /> : null}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <Link to="/app/settings/profile">Profile settings</Link>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <Link to="/app/settings/security">Security</Link>
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          className="gap-2"
          onSelect={(event) => event.preventDefault()}
        >
          <Moon className="size-4" />
          <div className="flex grow items-center justify-between gap-2">
            <span>Dark mode</span>
            <Switch
              size="sm"
              checked={resolvedTheme === 'dark'}
              aria-label="Toggle dark mode"
              onCheckedChange={handleThemeToggle}
            />
          </div>
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          className="text-destructive focus:text-destructive"
          onClick={() => {
            void logout();
          }}
        >
          <LogOut className="size-4" />
          Sign out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
