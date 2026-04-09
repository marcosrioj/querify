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
  toolbar,
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
  toolbar?: ReactNode;
  footer?: ReactNode;
  onRowClick?: (row: T) => void;
}) {
  const { t } = usePortalI18n();

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
    <Card>
      {title || description || toolbar ? (
        <CardHeader className="gap-4 md:flex-row md:items-center md:justify-between">
          <CardHeading>
            {title ? (
              <CardTitle className="flex flex-wrap items-start gap-2">
                <span>{translateMaybeString(title, t)}</span>
                {titleHint}
              </CardTitle>
            ) : null}
            {description && descriptionMode === "inline" ? (
              <CardDescription>
                {translateMaybeString(description, t)}
              </CardDescription>
            ) : null}
          </CardHeading>
          {toolbar ? (
            <CardToolbar className="flex-wrap gap-2">{toolbar}</CardToolbar>
          ) : null}
        </CardHeader>
      ) : null}

      <CardContent className="space-y-5">
        {errorState ? (
          errorState
        ) : (
          <>
            <div className="space-y-3 lg:hidden">
              {loading
                ? Array.from({ length: 4 }, (_, index) => (
                    <div
                      key={`mobile-loading-${index}`}
                      className="rounded-xl border border-border/80 bg-card p-4"
                    >
                      <div className="space-y-3">
                        {columns.map((column) => (
                          <div key={column.key} className="space-y-1.5">
                            <Skeleton className="h-3 w-20" />
                            <Skeleton className="h-5 w-[75%]" />
                          </div>
                        ))}
                      </div>
                    </div>
                  ))
                : rows.map((row) => (
                    <div
                      key={getRowId(row)}
                      className="rounded-xl border border-border/80 bg-card p-4"
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
                      <div className="space-y-3">
                        {columns.map((column) => (
                          <div
                            key={column.key}
                            className="border-b border-border/60 pb-3 last:border-b-0 last:pb-0"
                          >
                            <p className="text-[0.6875rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                              {mobileHeaderLabel(column)}
                            </p>
                            <div className="mt-1.5 min-w-0 break-words text-sm text-foreground">
                              {translateMaybeString(column.cell(row), t)}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}

              {!loading && rows.length === 0 ? emptyState : null}
            </div>

            <div className="hidden overflow-hidden rounded-xl border border-border/80 bg-card lg:block">
              <Table>
                <TableHeader className="bg-muted/40">
                  <TableRow>
                    {columns.map((column) => (
                      <TableHead key={column.key} className={column.className}>
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
                          className={onRowClick ? "cursor-pointer" : undefined}
                          onClick={() => onRowClick?.(row)}
                        >
                          {columns.map((column) => (
                            <TableCell
                              key={column.key}
                              className={column.className}
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
