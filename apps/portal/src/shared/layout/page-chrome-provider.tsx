import { useMemo, useState, type PropsWithChildren } from "react";
import {
  PageChromeContext,
  type PageChromeState,
} from "@/shared/layout/page-chrome-context";

export function PageChromeProvider({ children }: PropsWithChildren) {
  const [chrome, setChrome] = useState<PageChromeState>({});
  const value = useMemo(() => ({ chrome, setChrome }), [chrome]);

  return (
    <PageChromeContext.Provider value={value}>
      {children}
    </PageChromeContext.Provider>
  );
}
