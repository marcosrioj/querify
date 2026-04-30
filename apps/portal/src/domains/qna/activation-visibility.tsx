import { useCallback, useRef, useState } from "react";
import { Sparkles } from "lucide-react";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { VisibilityScope } from "@/shared/constants/backend-enums";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";

export function useActivationVisibilityPrompt() {
  const { t } = usePortalI18n();
  const [open, setOpen] = useState(false);
  const currentVisibilityRef = useRef<VisibilityScope>(
    VisibilityScope.Internal,
  );
  const resolveRef = useRef<((visibility: VisibilityScope) => void) | null>(
    null,
  );

  const resolveVisibility = useCallback((visibility: VisibilityScope) => {
    resolveRef.current?.(visibility);
    resolveRef.current = null;
    setOpen(false);
  }, []);

  const resolveActivationVisibility = useCallback(
    (currentVisibility: VisibilityScope) => {
      if (currentVisibility === VisibilityScope.Public) {
        return Promise.resolve(currentVisibility);
      }

      currentVisibilityRef.current = currentVisibility;
      setOpen(true);

      return new Promise<VisibilityScope>((resolve) => {
        resolveRef.current = resolve;
      });
    },
    [],
  );

  const handleOpenChange = (nextOpen: boolean) => {
    if (nextOpen) {
      setOpen(true);
      return;
    }

    resolveVisibility(currentVisibilityRef.current);
  };

  const ActivationVisibilityDialog = (
    <AlertDialog open={open} onOpenChange={handleOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <div className="flex size-11 items-center justify-center rounded-2xl border border-primary/15 bg-primary/10 text-primary">
            <Sparkles className="size-5" />
          </div>
          <AlertDialogTitle>
            {t("Make this item public while activating?")}
          </AlertDialogTitle>
          <AlertDialogDescription>
            {t(
              "Active items can stay internal or authenticated-only, or be made public now so customers can see them.",
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel
            onClick={() => resolveVisibility(currentVisibilityRef.current)}
          >
            {t("Keep current visibility")}
          </AlertDialogCancel>
          <Button
            variant="primary"
            onClick={() => resolveVisibility(VisibilityScope.Public)}
          >
            {t("Make public and activate")}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );

  return {
    resolveActivationVisibility,
    ActivationVisibilityDialog,
  };
}
