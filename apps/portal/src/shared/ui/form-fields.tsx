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

function FieldLabel({ label, hint }: { label: ReactNode; hint?: ReactNode }) {
  return (
    <div className="flex items-center gap-1.5">
      <FormLabel>{label}</FormLabel>
      {hint ? <ContextHint content={hint} label="Field details" /> : null}
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
}: BaseFieldProps<TFieldValues> & {
  type?: string;
  placeholder?: string;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem>
          <FieldLabel label={label} hint={hint} />
          <FormControl>
            <Input {...field} type={type} placeholder={placeholder} />
          </FormControl>
          {description ? (
            <FormDescription>{description}</FormDescription>
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
}: BaseFieldProps<TFieldValues> & {
  placeholder?: string;
  rows?: number;
}) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem>
          <FieldLabel label={label} hint={hint} />
          <FormControl>
            <Textarea {...field} placeholder={placeholder} rows={rows} />
          </FormControl>
          {description ? (
            <FormDescription>{description}</FormDescription>
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
}: BaseFieldProps<TFieldValues>) {
  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => (
        <FormItem className="rounded-xl border border-border p-4">
          <div className="flex items-center justify-between gap-4">
            <div className="space-y-1">
              <FieldLabel label={label} hint={hint} />
              {description ? (
                <FormDescription>{description}</FormDescription>
              ) : null}
            </div>
            <FormControl>
              <Switch
                checked={Boolean(field.value)}
                onCheckedChange={field.onChange}
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
}: BaseFieldProps<TFieldValues>) {
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
            />
          </FormControl>
          <div className="space-y-1 leading-none">
            <FieldLabel label={label} hint={hint} />
            {description ? (
              <FormDescription>{description}</FormDescription>
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
}: BaseFieldProps<TFieldValues> & {
  options: Array<{ value: string; label: string }>;
  placeholder?: string;
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
            <FieldLabel label={label} hint={hint} />
            <Select
              onValueChange={(value) =>
                field.onChange(value === EMPTY_SELECT_VALUE ? "" : value)
              }
              value={selectValue}
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
              <FormDescription>{description}</FormDescription>
            ) : null}
            <FormMessage />
          </FormItem>
        );
      }}
    />
  );
}
