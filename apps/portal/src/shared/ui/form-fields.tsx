import { ReactNode, useState } from "react";
import { Sparkles, TriangleAlert } from "lucide-react";
import { Control, FieldValues, Path } from "react-hook-form";
import { translateMaybeString, usePortalI18n } from "@/shared/lib/i18n";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  Button,
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

type FieldConfirmationVariant = "primary" | "destructive";
type SwitchFieldConfirmationValue<T> = T | ((nextChecked: boolean) => T);

type SwitchFieldConfirmation = {
  title: SwitchFieldConfirmationValue<ReactNode>;
  description: SwitchFieldConfirmationValue<ReactNode>;
  confirmLabel?: SwitchFieldConfirmationValue<string>;
  cancelLabel?: string;
  variant?: SwitchFieldConfirmationValue<FieldConfirmationVariant>;
};

type SelectFieldOption = {
  value: string;
  label: string;
};

export type SelectFieldConfirmationContext = {
  currentValue?: string;
  nextValue: string;
  currentOption?: SelectFieldOption;
  nextOption?: SelectFieldOption;
};

type SelectFieldConfirmationValue<T> =
  | T
  | ((context: SelectFieldConfirmationContext) => T);

export type SelectFieldConfirmation = {
  title: SelectFieldConfirmationValue<ReactNode>;
  description: SelectFieldConfirmationValue<ReactNode>;
  confirmLabel?: SelectFieldConfirmationValue<string>;
  cancelLabel?: string;
  variant?: SelectFieldConfirmationValue<FieldConfirmationVariant>;
};

const activeStatusConfirmation: SwitchFieldConfirmation = {
  title: (nextChecked) =>
    nextChecked ? "Activate this record?" : "Deactivate this record?",
  description: (nextChecked) =>
    nextChecked
      ? "Active records can return to normal product flows and become available again to teammates or end users. Confirm this only when the content is ready to be used."
      : "Inactive records stay saved for editing and history, but should stop appearing in normal product flows. Confirm this when the content is outdated, under review, or temporarily unavailable.",
  confirmLabel: (nextChecked) => (nextChecked ? "Activate" : "Deactivate"),
  variant: (nextChecked) => (nextChecked ? "primary" : "destructive"),
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
  const { t } = usePortalI18n();
  const helpContent = buildFieldHelpContent({ description, hint });

  return (
    <div className="flex items-center gap-1.5">
      <FormLabel>{translateMaybeString(label, t)}</FormLabel>
      {helpContent ? (
        <ContextHint
          content={translateMaybeString(helpContent, t)}
          label={t("Field details")}
        />
      ) : null}
    </div>
  );
}

function resolveSwitchFieldConfirmationValue<T>(
  value: SwitchFieldConfirmationValue<T>,
  nextChecked: boolean,
) {
  return typeof value === "function"
    ? (value as (nextChecked: boolean) => T)(nextChecked)
    : value;
}

function resolveSelectFieldConfirmationValue<T>(
  value: SelectFieldConfirmationValue<T>,
  context: SelectFieldConfirmationContext,
) {
  return typeof value === "function"
    ? (value as (context: SelectFieldConfirmationContext) => T)(context)
    : value;
}

function ConfirmingSwitchControl({
  checked,
  disabled,
  onCheckedChange,
  confirmation,
}: {
  checked: boolean;
  disabled?: boolean;
  onCheckedChange: (checked: boolean) => void;
  confirmation?: SwitchFieldConfirmation;
}) {
  const { t } = usePortalI18n();
  const [pendingChecked, setPendingChecked] = useState<boolean | null>(null);
  const nextChecked = pendingChecked ?? checked;
  const isOpen = pendingChecked !== null;
  const title = confirmation
    ? resolveSwitchFieldConfirmationValue(confirmation.title, nextChecked)
    : null;
  const description = confirmation
    ? resolveSwitchFieldConfirmationValue(
        confirmation.description,
        nextChecked,
      )
    : null;
  const confirmLabel = confirmation
    ? resolveSwitchFieldConfirmationValue(
        confirmation.confirmLabel ?? "Confirm",
        nextChecked,
      )
    : "Confirm";
  const variant = confirmation
    ? resolveSwitchFieldConfirmationValue(
        confirmation.variant ?? "primary",
        nextChecked,
      )
    : "primary";
  const iconClassName =
    variant === "destructive"
      ? "flex size-11 items-center justify-center rounded-2xl border border-destructive/15 bg-destructive/10 text-destructive"
      : "flex size-11 items-center justify-center rounded-2xl border border-primary/15 bg-primary/10 text-primary";

  return (
    <>
      <Switch
        checked={checked}
        onCheckedChange={(value) => {
          if (!confirmation || value === checked) {
            onCheckedChange(value);
            return;
          }

          setPendingChecked(value);
        }}
        disabled={disabled}
      />
      <AlertDialog
        open={isOpen}
        onOpenChange={(open) => {
          if (!open) {
            setPendingChecked(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <div className={iconClassName}>
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
            <AlertDialogCancel>
              {t(confirmation?.cancelLabel ?? "Cancel")}
            </AlertDialogCancel>
            <Button
              variant={variant}
              onClick={() => {
                if (pendingChecked === null) {
                  return;
                }

                onCheckedChange(pendingChecked);
                setPendingChecked(null);
              }}
            >
              {t(confirmLabel)}
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
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
  const { t } = usePortalI18n();

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
              placeholder={placeholder ? t(placeholder) : undefined}
              disabled={disabled}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">
              {translateMaybeString(description, t)}
            </FormDescription>
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
  const { t } = usePortalI18n();

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
              placeholder={placeholder ? t(placeholder) : undefined}
              rows={rows}
              disabled={disabled}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">
              {translateMaybeString(description, t)}
            </FormDescription>
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
  confirmation,
}: BaseFieldProps<TFieldValues> & {
  disabled?: boolean;
  confirmation?: SwitchFieldConfirmation | false;
}) {
  const { t } = usePortalI18n();
  const normalizedFieldName = String(name).replace(/\./g, "").toLowerCase();
  const resolvedConfirmation =
    confirmation === false
      ? undefined
      : confirmation ??
        (normalizedFieldName.endsWith("isactive") ||
        normalizedFieldName.endsWith("active")
          ? activeStatusConfirmation
          : undefined);

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
                  {translateMaybeString(description, t)}
                </FormDescription>
              ) : null}
            </div>
            <FormControl>
              <ConfirmingSwitchControl
                checked={Boolean(field.value)}
                onCheckedChange={field.onChange}
                disabled={disabled}
                confirmation={resolvedConfirmation}
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
  const { t } = usePortalI18n();

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
                {translateMaybeString(description, t)}
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
  confirmation,
}: BaseFieldProps<TFieldValues> & {
  options: SelectFieldOption[];
  placeholder?: string;
  disabled?: boolean;
  confirmation?: SelectFieldConfirmation;
}) {
  const { t } = usePortalI18n();
  const [pendingValue, setPendingValue] = useState<string | null>(null);
  const [isUserInitiatedChange, setIsUserInitiatedChange] = useState(false);
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
        const resolvedPendingValue = pendingValue ?? "";
        const confirmationContext: SelectFieldConfirmationContext = {
          currentValue: fieldValue,
          nextValue: resolvedPendingValue,
          currentOption: options.find((option) => option.value === fieldValue),
          nextOption: options.find(
            (option) => option.value === resolvedPendingValue,
          ),
        };
        const confirmTitle = confirmation
          ? resolveSelectFieldConfirmationValue(
              confirmation.title,
              confirmationContext,
            )
          : null;
        const confirmDescription = confirmation
          ? resolveSelectFieldConfirmationValue(
              confirmation.description,
              confirmationContext,
            )
          : null;
        const confirmLabel = confirmation
          ? resolveSelectFieldConfirmationValue(
              confirmation.confirmLabel ?? "Confirm",
              confirmationContext,
            )
          : "Confirm";
        const confirmVariant = confirmation
          ? resolveSelectFieldConfirmationValue(
              confirmation.variant ?? "primary",
              confirmationContext,
            )
          : "primary";
        const iconClassName =
          confirmVariant === "destructive"
            ? "flex size-11 items-center justify-center rounded-2xl border border-destructive/15 bg-destructive/10 text-destructive"
            : "flex size-11 items-center justify-center rounded-2xl border border-primary/15 bg-primary/10 text-primary";

        return (
          <FormItem>
            <FieldLabel label={label} description={description} hint={hint} />
            <Select
              onOpenChange={(open) => {
                if (!open) {
                  setIsUserInitiatedChange(false);
                }
              }}
              onValueChange={(value) => {
                const nextValue =
                  value === EMPTY_SELECT_VALUE ? "" : value;

                if (!confirmation || nextValue === fieldValue) {
                  setIsUserInitiatedChange(false);
                  field.onChange(nextValue);
                  return;
                }

                if (!isUserInitiatedChange) {
                  return;
                }

                setPendingValue(nextValue);
                setIsUserInitiatedChange(false);
              }}
              value={selectValue}
              disabled={disabled}
            >
              <FormControl>
                  <SelectTrigger
                  onPointerDown={() => {
                    if (!disabled) {
                      setIsUserInitiatedChange(true);
                    }
                  }}
                  onKeyDown={() => {
                    if (!disabled) {
                      setIsUserInitiatedChange(true);
                    }
                  }}
                >
                  <SelectValue
                    placeholder={placeholder ? t(placeholder) : undefined}
                  />
                </SelectTrigger>
              </FormControl>
              <SelectContent>
                {options.map((option) => {
                  const optionValue =
                    option.value === "" ? EMPTY_SELECT_VALUE : option.value;

                  return (
                    <SelectItem key={optionValue} value={optionValue}>
                      {t(option.label)}
                    </SelectItem>
                  );
                })}
              </SelectContent>
            </Select>
            <AlertDialog
              open={pendingValue !== null}
              onOpenChange={(open) => {
                if (!open) {
                  setPendingValue(null);
                }
              }}
            >
              <AlertDialogContent>
                <AlertDialogHeader>
                  <div className={iconClassName}>
                    {confirmVariant === "destructive" ? (
                      <TriangleAlert className="size-5" />
                    ) : (
                      <Sparkles className="size-5" />
                    )}
                  </div>
                  <AlertDialogTitle>
                    {translateMaybeString(confirmTitle, t)}
                  </AlertDialogTitle>
                  <AlertDialogDescription>
                    {translateMaybeString(confirmDescription, t)}
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>
                    {t(confirmation?.cancelLabel ?? "Cancel")}
                  </AlertDialogCancel>
                  <Button
                    variant={confirmVariant}
                    onClick={() => {
                      if (pendingValue === null) {
                        return;
                      }

                      field.onChange(pendingValue);
                      setPendingValue(null);
                    }}
                  >
                    {t(confirmLabel)}
                  </Button>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
            {description ? (
              <FormDescription className="sr-only">
                {translateMaybeString(description, t)}
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
  const { t } = usePortalI18n();

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
              placeholder={placeholder ? t(placeholder) : undefined}
              searchPlaceholder={
                searchPlaceholder ? t(searchPlaceholder) : undefined
              }
              emptyMessage={emptyMessage ? t(emptyMessage) : undefined}
              loading={loading}
              disabled={disabled}
              allowClear={allowClear}
              clearLabel={clearLabel ? t(clearLabel) : undefined}
              resultCountHint={resultCountHint ? t(resultCountHint) : undefined}
              searchValue={searchValue}
              onSearchChange={onSearchChange}
            />
          </FormControl>
          {description ? (
            <FormDescription className="sr-only">
              {translateMaybeString(description, t)}
            </FormDescription>
          ) : null}
          <FormMessage />
        </FormItem>
      )}
    />
  );
}
