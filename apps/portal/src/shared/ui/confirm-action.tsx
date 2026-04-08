import { type ReactNode, useState } from "react";
import { LoaderCircle, Sparkles, TriangleAlert } from "lucide-react";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { translateMaybeString, usePortalI18n } from "@/shared/lib/i18n";

export function ConfirmAction({
  trigger,
  title,
  description,
  confirmLabel,
  cancelLabel = "Cancel",
  variant = "destructive",
  isPending = false,
  onConfirm,
}: {
  trigger: ReactNode;
  title: ReactNode;
  description: ReactNode;
  confirmLabel: string;
  cancelLabel?: string;
  variant?: "primary" | "destructive";
  isPending?: boolean;
  onConfirm: () => Promise<unknown> | unknown;
}) {
  const { t } = usePortalI18n();
  const [open, setOpen] = useState(false);

  const handleConfirm = async () => {
    try {
      await onConfirm();
      setOpen(false);
    } catch {
      // Keep the dialog open so the user can retry after an inline or toast error.
    }
  };

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <AlertDialogTrigger asChild>{trigger}</AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <div
            className={
              variant === "destructive"
                ? "flex size-11 items-center justify-center rounded-2xl border border-destructive/15 bg-destructive/10 text-destructive"
                : "flex size-11 items-center justify-center rounded-2xl border border-primary/15 bg-primary/10 text-primary"
            }
          >
            {variant === "destructive" ? (
              <TriangleAlert className="size-5" />
            ) : (
              <Sparkles className="size-5" />
            )}
          </div>
          <AlertDialogTitle>{translateMaybeString(title, t)}</AlertDialogTitle>
          <AlertDialogDescription>
            {translateMaybeString(description, t)}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>{t(cancelLabel)}</AlertDialogCancel>
          <Button variant={variant} disabled={isPending} onClick={() => void handleConfirm()}>
            {isPending ? <LoaderCircle className="size-4 animate-spin" /> : null}
            {t(confirmLabel)}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
