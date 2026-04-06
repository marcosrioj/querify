import { ReactNode } from "react";
import { Control, FieldValues, Path } from "react-hook-form";
import {
  Checkbox,
  ContextHint,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  Input,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SearchSelect,
  type SearchSelectOption,
  Switch,
  Textarea,
} from "@/shared/ui";

const EMPTY_SELECT_VALUE = "__empty_select_value__";

type BaseFieldProps<TFieldValues extends FieldValues> = {
  control: Control<TFieldValues>;
  name: Path<TFieldValues>;
  label: ReactNode;
  description?: ReactNode;
  hint?: ReactNode;
};

function buildFieldHelpContent({
  description,
  hint,
}: {
  description?: ReactNode;
  hint?: ReactNode;
}) {
  if (!description && !hint) {
    return null;
  }

  if (description && hint) {
    return (
      <div className="space-y-2">
        <div>{description}</div>
        <div className="border-t border-border pt-2">{hint}</div>
      </div>
    );
  }

  return description ?? hint;
}

function FieldLabel({
  label,
  description,
  hint,
}: {
  label: ReactNode;
  description?: ReactNode;
  hint?: ReactNode;
}) {
  const helpContent = buildFieldHelpContent({ description, hint });

  return (
    <div className="flex items-center gap-1.5">
      <FormLabel>{label}</FormLabel>
      {helpContent ? (
        <ContextHint content={helpContent} label="Field details" />
      ) : null}
    </div>
  );
}

export function TextField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  type = "text",
  placeholder,
  disabled = false,
}: BaseFieldProps<TFieldValues> & {
  type?: string;
  placeholder?: string;
  disabled?: boolean;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem>
          <FieldLabel label={label} description={description} hint={hint} />
          <FormControl>
            <Input
              {...field}
              type={type}
              placeholder={placeholder}
              disabled={disabled}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">{description}</FormDescription>
          ) : null}
          <FormMessage />
        </FormItem>
      )}
    />
  );
}

export function TextareaField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  placeholder,
  rows = 6,
  disabled = false,
}: BaseFieldProps<TFieldValues> & {
  placeholder?: string;
  rows?: number;
  disabled?: boolean;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem>
          <FieldLabel label={label} description={description} hint={hint} />
          <FormControl>
            <Textarea
              {...field}
              placeholder={placeholder}
              rows={rows}
              disabled={disabled}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">{description}</FormDescription>
          ) : null}
          <FormMessage />
        </FormItem>
      )}
    />
  );
}

export function SwitchField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  disabled = false,
}: BaseFieldProps<TFieldValues> & {
  disabled?: boolean;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem className="rounded-xl border border-border p-4">
          <div className="flex items-center justify-between gap-4">
            <div className="space-y-1">
              <FieldLabel label={label} description={description} hint={hint} />
              {description ? (
                <FormDescription className="sr-only">
                  {description}
                </FormDescription>
              ) : null}
            </div>
            <FormControl>
              <Switch
                checked={Boolean(field.value)}
                onCheckedChange={field.onChange}
                disabled={disabled}
              />
            </FormControl>
          </div>
          <FormMessage />
        </FormItem>
      )}
    />
  );
}

export function CheckboxField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  disabled = false,
}: BaseFieldProps<TFieldValues> & {
  disabled?: boolean;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem className="flex flex-row items-start gap-3 rounded-xl border border-border p-4">
          <FormControl>
            <Checkbox
              checked={Boolean(field.value)}
              onCheckedChange={field.onChange}
              disabled={disabled}
            />
          </FormControl>
          <div className="space-y-1 leading-none">
            <FieldLabel label={label} description={description} hint={hint} />
            {description ? (
              <FormDescription className="sr-only">
                {description}
              </FormDescription>
            ) : null}
            <FormMessage />
          </div>
        </FormItem>
      )}
    />
  );
}

export function SelectField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  options,
  placeholder,
  disabled = false,
}: BaseFieldProps<TFieldValues> & {
  options: Array<{ value: string; label: string }>;
  placeholder?: string;
  disabled?: boolean;
}) {
  const hasEmptyOption = options.some((option) => option.value === "");

  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => {
        const fieldValue =
          field.value === undefined || field.value === null
            ? undefined
            : String(field.value);
        const selectValue =
          fieldValue === ""
            ? hasEmptyOption
              ? EMPTY_SELECT_VALUE
              : undefined
            : fieldValue;

        return (
          <FormItem>
            <FieldLabel label={label} description={description} hint={hint} />
            <Select
              onValueChange={(value) =>
                field.onChange(value === EMPTY_SELECT_VALUE ? "" : value)
              }
              value={selectValue}
              disabled={disabled}
            >
              <FormControl>
                <SelectTrigger>
                  <SelectValue placeholder={placeholder} />
                </SelectTrigger>
              </FormControl>
              <SelectContent>
                {options.map((option) => {
                  const optionValue =
                    option.value === "" ? EMPTY_SELECT_VALUE : option.value;

                  return (
                    <SelectItem key={optionValue} value={optionValue}>
                      {option.label}
                    </SelectItem>
                  );
                })}
              </SelectContent>
            </Select>
            {description ? (
              <FormDescription className="sr-only">
                {description}
              </FormDescription>
            ) : null}
            <FormMessage />
          </FormItem>
        );
      }}
    />
  );
}

export function SearchSelectField<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  description,
  hint,
  options,
  selectedOption,
  placeholder,
  searchPlaceholder,
  emptyMessage,
  loading = false,
  disabled = false,
  allowClear = false,
  clearLabel,
  resultCountHint,
  searchValue,
  onSearchChange,
}: BaseFieldProps<TFieldValues> & {
  options: SearchSelectOption[];
  selectedOption?: SearchSelectOption | null;
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  loading?: boolean;
  disabled?: boolean;
  allowClear?: boolean;
  clearLabel?: string;
  resultCountHint?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem>
          <FieldLabel label={label} description={description} hint={hint} />
          <FormControl>
            <SearchSelect
              value={
                field.value === undefined || field.value === null
                  ? ""
                  : String(field.value)
              }
              onValueChange={field.onChange}
              options={options}
              selectedOption={selectedOption}
              placeholder={placeholder}
              searchPlaceholder={searchPlaceholder}
              emptyMessage={emptyMessage}
              loading={loading}
              disabled={disabled}
              allowClear={allowClear}
              clearLabel={clearLabel}
              resultCountHint={resultCountHint}
              searchValue={searchValue}
              onSearchChange={onSearchChange}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">{description}</FormDescription>
          ) : null}
          <FormMessage />
        </FormItem>
      )}
    />
  );
}
