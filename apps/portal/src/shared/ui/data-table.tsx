import { ReactNode } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardToolbar,
  CardTitle,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui";
import { ContextHint } from "@/shared/ui/context-hint";
import { translateMaybeString } from "@/shared/lib/i18n-render";
import { usePortalI18n } from "@/shared/lib/use-portal-i18n";
import { cn } from "@/lib/utils";

export type DataTableColumn<T> = {
  key: string;
  header: ReactNode;
  cell: (row: T) => ReactNode;
  className?: string;
  mobileLabel?: ReactNode;
};

export function DataTable<T>({
  title,
  description,
  descriptionMode = "inline",
  columns,
  rows,
  getRowId,
  loading,
  emptyState,
  errorState,
  headingControl,
  toolbar,
  toolbarPlacement = "inline",
  footer,
  onRowClick,
}: {
  title?: ReactNode;
  description?: ReactNode;
  descriptionMode?: "inline" | "hint";
  columns: DataTableColumn<T>[];
  rows: T[];
  getRowId: (row: T) => string;
  loading?: boolean;
  emptyState?: ReactNode;
  errorState?: ReactNode;
  headingControl?: ReactNode;
  toolbar?: ReactNode;
  toolbarPlacement?: "inline" | "below";
  footer?: ReactNode;
  onRowClick?: (row: T) => void;
}) {
  const { t } = usePortalI18n();
  const primaryColumn = columns[0];
  const actionColumn = columns.find((column) => column.key === "actions");
  const detailColumns = columns.filter(
    (column) => column !== primaryColumn && column !== actionColumn,
  );

  const mobileHeaderLabel = (column: DataTableColumn<T>) => {
    if (column.mobileLabel) {
      return translateMaybeString(column.mobileLabel, t);
    }

    if (typeof column.header === "string") {
      return t(column.header);
    }

    return column.key;
  };

  const titleHint =
    description && descriptionMode === "hint" ? (
      <ContextHint
        content={translateMaybeString(description, t)}
        label={t("Table details")}
        className="mt-0.5"
      />
    ) : null;

  return (
    <Card className="min-w-0 overflow-hidden bg-linear-to-b from-background to-muted/10">
      {title || description || headingControl || toolbar ? (
        <CardHeader className="gap-4 bg-linear-to-b from-muted/20 to-transparent px-4 py-4 md:flex-col md:items-stretch sm:px-5">
          <CardHeading
            className={
              headingControl && toolbarPlacement === "inline"
                ? "w-full min-w-0"
                : "min-w-0"
            }
          >
            {title ? (
              <CardTitle className="flex min-w-0 flex-wrap items-start gap-2">
                <span className="min-w-0 break-words">
                  {translateMaybeString(title, t)}
                </span>
                {titleHint}
              </CardTitle>
            ) : null}
            {description && descriptionMode === "inline" ? (
              <CardDescription>
                {translateMaybeString(description, t)}
              </CardDescription>
            ) : null}
            {headingControl ? (
              <div className="min-w-0 rounded-lg border border-border/70 bg-background/80 p-3 shadow-xs shadow-black/5">
                {headingControl}
              </div>
            ) : null}
          </CardHeading>
          {toolbar ? (
            <CardToolbar className="w-full min-w-0 rounded-lg border border-border/70 bg-muted/15 p-3 shadow-xs shadow-black/5">
              {toolbar}
            </CardToolbar>
          ) : null}
        </CardHeader>
      ) : null}

      <CardContent className="min-w-0 space-y-5 p-4 sm:p-5">
        {errorState ? (
          errorState
        ) : (
          <>
            <div className="space-y-3 xl:hidden">
              {loading
                ? Array.from({ length: 4 }, (_, index) => (
                    <div
                      key={`mobile-loading-${index}`}
                      className="min-w-0 max-w-full overflow-hidden rounded-lg border border-border/70 bg-background/90 shadow-xs shadow-black/5"
                    >
                      <div className="border-b border-border/60 bg-muted/15 p-4">
                        <Skeleton className="h-3 w-24" />
                        <Skeleton className="mt-2 h-5 w-[78%]" />
                        <Skeleton className="mt-2 h-4 w-[52%]" />
                      </div>
                      <div className="grid gap-2 p-3 sm:grid-cols-2">
                        {detailColumns.length
                          ? detailColumns.map((column) => (
                              <div
                                key={column.key}
                                className="rounded-md border border-border/60 bg-muted/10 p-3"
                              >
                                <Skeleton className="h-3 w-20" />
                                <Skeleton className="mt-2 h-5 w-[75%]" />
                              </div>
                            ))
                          : columns.slice(1).map((column) => (
                              <div
                                key={column.key}
                                className="rounded-md border border-border/60 bg-muted/10 p-3"
                              >
                                <Skeleton className="h-3 w-20" />
                                <Skeleton className="mt-2 h-5 w-[75%]" />
                              </div>
                            ))}
                      </div>
                    </div>
                  ))
                : rows.map((row) => (
                    <div
                      key={getRowId(row)}
                      className={cn(
                        "min-w-0 max-w-full overflow-hidden rounded-lg border border-border/70 bg-background/90 shadow-xs shadow-black/5 transition-[background-color,border-color,box-shadow,transform]",
                        onRowClick &&
                          "hover:-translate-y-0.5 hover:border-primary/25 hover:bg-primary/[0.025] hover:shadow-[var(--shadow-premium-elevated)] focus-visible:border-primary/35 focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
                      )}
                      onClick={() => onRowClick?.(row)}
                      onKeyDown={(event) => {
                        if (!onRowClick) {
                          return;
                        }

                        if (event.key === "Enter" || event.key === " ") {
                          event.preventDefault();
                          onRowClick(row);
                        }
                      }}
                      role={onRowClick ? "button" : undefined}
                      tabIndex={onRowClick ? 0 : undefined}
                    >
                      {primaryColumn ? (
                        <div className="min-w-0 border-b border-border/60 bg-muted/15 p-4">
                          <p className="min-w-0 break-words text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                            {mobileHeaderLabel(primaryColumn)}
                          </p>
                          <div className="mt-2 min-w-0 max-w-full break-words text-sm text-foreground [overflow-wrap:anywhere] [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
                            {translateMaybeString(primaryColumn.cell(row), t)}
                          </div>
                        </div>
                      ) : null}

                      {detailColumns.length ? (
                        <div className="grid min-w-0 gap-2 p-3 sm:grid-cols-2">
                          {detailColumns.map((column) => (
                            <div
                              key={column.key}
                              className="min-w-0 rounded-md border border-border/60 bg-muted/10 p-3"
                            >
                              <p className="min-w-0 break-words text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                                {mobileHeaderLabel(column)}
                              </p>
                              <div className="mt-1.5 min-w-0 max-w-full break-words text-sm text-foreground [overflow-wrap:anywhere] [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
                                {translateMaybeString(column.cell(row), t)}
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : null}

                      {actionColumn ? (
                        <div className="min-w-0 border-t border-border/60 bg-muted/10 p-3">
                          <p className="mb-2 min-w-0 break-words text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                            {mobileHeaderLabel(actionColumn)}
                          </p>
                          <div className="min-w-0 max-w-full break-words text-sm text-foreground [overflow-wrap:anywhere] [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
                            {translateMaybeString(actionColumn.cell(row), t)}
                          </div>
                        </div>
                      ) : null}

                      {!primaryColumn &&
                      !detailColumns.length &&
                      !actionColumn ? (
                        <div className="space-y-3 p-4">
                          {columns.map((column) => (
                            <div
                              key={column.key}
                              className="min-w-0 border-b border-border/60 pb-3 last:border-b-0 last:pb-0"
                            >
                              <p className="min-w-0 break-words text-[0.6875rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                                {mobileHeaderLabel(column)}
                              </p>
                              <div className="mt-1.5 min-w-0 max-w-full break-words text-sm text-foreground [overflow-wrap:anywhere] [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal">
                                {translateMaybeString(column.cell(row), t)}
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : null}
                    </div>
                  ))}

              {!loading && rows.length === 0 ? emptyState : null}
            </div>

            <div className="hidden min-w-0 overflow-x-auto overflow-y-hidden rounded-lg border border-border/70 bg-background/90 shadow-xs shadow-black/5 xl:block">
              <Table className="table-fixed">
                <TableHeader className="bg-muted/35">
                  <TableRow>
                    {columns.map((column) => (
                      <TableHead
                        key={column.key}
                        className={cn(
                          "min-w-0 whitespace-normal break-words text-[0.6875rem] font-semibold uppercase tracking-[0.14em] [overflow-wrap:anywhere]",
                          column === primaryColumn && "w-auto",
                          column.className,
                        )}
                      >
                        {translateMaybeString(column.header, t)}
                      </TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {loading
                    ? Array.from({ length: 6 }, (_, index) => (
                        <TableRow key={`loading-${index}`}>
                          {columns.map((column) => (
                            <TableCell key={column.key}>
                              <Skeleton className="h-5 w-[75%]" />
                            </TableCell>
                          ))}
                        </TableRow>
                      ))
                    : rows.map((row) => (
                        <TableRow
                          key={getRowId(row)}
                          className={
                            onRowClick
                              ? "cursor-pointer transition-colors hover:bg-primary/[0.025] focus-visible:bg-primary/[0.035] focus-visible:outline-hidden"
                              : undefined
                          }
                          onClick={() => onRowClick?.(row)}
                          onKeyDown={(event) => {
                            if (!onRowClick) {
                              return;
                            }

                            if (event.key === "Enter" || event.key === " ") {
                              event.preventDefault();
                              onRowClick(row);
                            }
                          }}
                          role={onRowClick ? "button" : undefined}
                          tabIndex={onRowClick ? 0 : undefined}
                        >
                          {columns.map((column) => (
                            <TableCell
                              key={column.key}
                              className={cn(
                                "min-w-0 align-top break-words py-5 [overflow-wrap:anywhere] [&_[data-slot=button]]:min-w-0 [&_[data-slot=button]]:whitespace-normal",
                                column === primaryColumn && "w-auto",
                                column.className,
                              )}
                            >
                              {translateMaybeString(column.cell(row), t)}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))}

                  {!loading && rows.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={columns.length}
                        className="h-40 text-center align-middle"
                      >
                        {emptyState}
                      </TableCell>
                    </TableRow>
                  ) : null}
                </TableBody>
              </Table>
            </div>
          </>
        )}
        {footer}
      </CardContent>
    </Card>
  );
}
