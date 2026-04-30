import {
  createContext,
  useContext,
  useLayoutEffect,
  type Dispatch,
  type ReactNode,
  type SetStateAction,
} from "react";

export type PageChromeBreadcrumb = {
  label: ReactNode;
  to?: string;
};

export type PageChromeState = {
  title?: ReactNode;
  description?: ReactNode;
  descriptionMode?: "inline" | "hint";
  backTo?: string;
  breadcrumbs?: PageChromeBreadcrumb[];
};

type PageChromeContextValue = {
  chrome: PageChromeState;
  setChrome: Dispatch<SetStateAction<PageChromeState>>;
};

export const PageChromeContext = createContext<
  PageChromeContextValue | undefined
>(undefined);

export function getPageChromeText(value: ReactNode) {
  if (typeof value === "string") {
    const text = value.trim();
    return text.length > 0 ? text : undefined;
  }

  if (typeof value === "number") {
    return String(value);
  }

  return undefined;
}

export function usePageChrome() {
  const context = useContext(PageChromeContext);

  if (!context) {
    throw new Error("usePageChrome must be used within PageChromeProvider.");
  }

  return context.chrome;
}

export function useRegisterPageChrome({
  title,
  description,
  descriptionMode,
  backTo,
  breadcrumbs,
}: PageChromeState) {
  const context = useContext(PageChromeContext);

  if (!context) {
    throw new Error(
      "useRegisterPageChrome must be used within PageChromeProvider.",
    );
  }

  const { setChrome } = context;

  useLayoutEffect(() => {
    setChrome({
      title,
      description,
      descriptionMode,
      backTo,
      breadcrumbs,
    });

    return () => setChrome({});
  }, [backTo, breadcrumbs, description, descriptionMode, setChrome, title]);
}
