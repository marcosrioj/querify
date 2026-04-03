import { ReactNode } from 'react';
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
} from '@/shared/ui';

export type DataTableColumn<T> = {
  key: string;
  header: ReactNode;
  cell: (row: T) => ReactNode;
  className?: string;
};

export function DataTable<T>({
  title,
  description,
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
  title?: string;
  description?: string;
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
  return (
    <Card>
      {title || description || toolbar ? (
        <CardHeader className="gap-4 md:flex-row md:items-start md:justify-between">
          <CardHeading>
            {title ? <CardTitle>{title}</CardTitle> : null}
            {description ? <CardDescription>{description}</CardDescription> : null}
          </CardHeading>
          {toolbar ? <CardToolbar className="flex-wrap gap-2">{toolbar}</CardToolbar> : null}
        </CardHeader>
      ) : null}

      <CardContent className="space-y-4">
        {errorState ? (
          errorState
        ) : (
          <div className="overflow-hidden rounded-xl border border-border">
            <Table>
              <TableHeader className="bg-muted/50">
                <TableRow>
                  {columns.map((column) => (
                    <TableHead key={column.key} className={column.className}>
                      {column.header}
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
                        className={onRowClick ? 'cursor-pointer' : undefined}
                        onClick={() => onRowClick?.(row)}
                      >
                        {columns.map((column) => (
                          <TableCell key={column.key} className={column.className}>
                            {column.cell(row)}
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
        )}
        {footer}
      </CardContent>
    </Card>
  );
}
