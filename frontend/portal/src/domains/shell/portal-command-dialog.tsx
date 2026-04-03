import { Command, Search } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Button,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/shared/ui';

const shortcuts = [
  { label: 'Go to dashboard', to: '/app/dashboard' },
  { label: 'Open FAQs', to: '/app/faq' },
  { label: 'Open FAQ items', to: '/app/faq-items' },
  { label: 'Open content refs', to: '/app/content-refs' },
];

export function PortalCommandDialog() {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button
        variant="outline"
        className="hidden min-w-[180px] justify-start text-muted-foreground md:flex"
        onClick={() => setOpen(true)}
      >
        <Search className="size-4" />
        Search portal
      </Button>
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>Portal command search</DialogTitle>
            <DialogDescription>
              The command surface is intentionally lean for now. Use it as a
              quick launcher while full global search stays a placeholder.
            </DialogDescription>
          </DialogHeader>
          <div className="rounded-2xl border border-border p-2">
            <div className="flex items-center gap-2 rounded-xl bg-muted/60 px-3 py-2 text-sm text-muted-foreground">
              <Search className="size-4" />
              Search is route-launch only in this foundation build.
            </div>
            <div className="mt-3 grid gap-2">
              {shortcuts.map((shortcut) => (
                <Button
                  asChild
                  variant="ghost"
                  className="justify-start rounded-xl"
                  key={shortcut.to}
                  onClick={() => setOpen(false)}
                >
                  <Link to={shortcut.to}>
                    <Command className="size-4" />
                    {shortcut.label}
                  </Link>
                </Button>
              ))}
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
