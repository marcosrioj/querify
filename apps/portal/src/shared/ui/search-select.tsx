import * as React from "react";
import { Check, ChevronsUpDown, LoaderCircle, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";

export type SearchSelectOption = {
  value: string;
  label: string;
  description?: string;
  keywords?: string[];
};

export type SearchSelectProps = Omit<
  React.ComponentProps<"button">,
  "value" | "onChange"
> & {
  value?: string;
  onValueChange: (value: string) => void;
  options: SearchSelectOption[];
  selectedOption?: SearchSelectOption | null;
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  loading?: boolean;
  allowClear?: boolean;
  clearLabel?: string;
  resultCountHint?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
};

export const SearchSelect = React.forwardRef<
  HTMLButtonElement,
  SearchSelectProps
>(function SearchSelect(
  {
    value = "",
    onValueChange,
    options,
    selectedOption,
    placeholder = "Select an option",
    searchPlaceholder = "Search...",
    emptyMessage = "No results found.",
    loading = false,
    disabled = false,
    allowClear = false,
    clearLabel = "Clear selection",
    resultCountHint,
    searchValue,
    onSearchChange,
    className,
    ...triggerProps
  },
  ref,
) {
  const [open, setOpen] = React.useState(false);
  const [internalSearchValue, setInternalSearchValue] = React.useState("");
  const listId = React.useId();
  const resolvedSearchValue = searchValue ?? internalSearchValue;

  const setSearchValue = (nextValue: string) => {
    if (onSearchChange) {
      onSearchChange(nextValue);
      return;
    }

    setInternalSearchValue(nextValue);
  };

  const resetSearch = () => {
    setSearchValue("");
  };

  const currentOption =
    selectedOption ?? options.find((option) => option.value === value) ?? null;
  const currentOptionIncluded = currentOption
    ? options.some((option) => option.value === currentOption.value)
    : false;
  const showCurrentSelection =
    Boolean(currentOption) &&
    !currentOptionIncluded &&
    !resolvedSearchValue.trim();
  const canClear = allowClear && Boolean(value);

  return (
    <Popover
      open={open}
      onOpenChange={(nextOpen) => {
        setOpen(nextOpen);

        if (!nextOpen) {
          resetSearch();
        }
      }}
    >
      <PopoverTrigger asChild>
        <Button
          asChild
          variant="outline"
          mode="input"
          size="lg"
          autoHeight
          placeholder={!currentOption}
          disabled={disabled}
          selected={open}
          className={cn("w-full px-3 py-2 text-left", className)}
        >
          <button
            ref={ref}
            type="button"
            role="combobox"
            aria-autocomplete="list"
            aria-controls={listId}
            aria-expanded={open}
            aria-haspopup="listbox"
            {...triggerProps}
          >
            <span className="flex min-w-0 flex-1 flex-col items-start gap-0.5 py-0.5">
              <span
                className={cn(
                  "w-full truncate",
                  !currentOption ? "font-normal" : "font-medium",
                )}
              >
                {currentOption?.label ?? placeholder}
              </span>
              {currentOption?.description ? (
                <span className="w-full truncate text-xs text-muted-foreground">
                  {currentOption.description}
                </span>
              ) : null}
            </span>
            <span className="ml-3 flex items-center gap-2">
              {currentOption ? (
                <Badge variant="secondary" className="hidden sm:inline-flex">
                  Selected
                </Badge>
              ) : null}
              <ChevronsUpDown className="size-4 opacity-50" />
            </span>
          </button>
        </Button>
      </PopoverTrigger>
      <PopoverContent
        className="w-[var(--radix-popover-trigger-width)] min-w-[18rem] p-0"
        align="start"
      >
        <Command shouldFilter={!onSearchChange}>
          <CommandInput
            value={resolvedSearchValue}
            onValueChange={setSearchValue}
            placeholder={searchPlaceholder}
            autoFocus
          />
          <CommandList id={listId}>
            {showCurrentSelection && currentOption ? (
              <>
                <CommandGroup heading="Current selection">
                  <SearchSelectItem
                    option={currentOption}
                    isSelected
                    onSelect={() => {
                      setOpen(false);
                      resetSearch();
                    }}
                  />
                </CommandGroup>
                <CommandSeparator />
              </>
            ) : null}

            {loading && options.length === 0 ? (
              <div className="flex items-center gap-2 px-3 py-6 text-sm text-muted-foreground">
                <LoaderCircle className="size-4 animate-spin" />
                Searching results...
              </div>
            ) : (
              <CommandEmpty>{emptyMessage}</CommandEmpty>
            )}

            <CommandGroup
              heading={resolvedSearchValue.trim() ? "Matches" : "Options"}
            >
              {options.map((option) => (
                <SearchSelectItem
                  key={option.value}
                  option={option}
                  isSelected={option.value === value}
                  onSelect={() => {
                    onValueChange(option.value);
                    setOpen(false);
                    resetSearch();
                  }}
                />
              ))}
            </CommandGroup>

            {canClear ? (
              <>
                <CommandSeparator />
                <CommandGroup heading="Actions">
                  <CommandItem
                    onSelect={() => {
                      onValueChange("");
                      setOpen(false);
                      resetSearch();
                    }}
                  >
                    <X className="size-4 text-muted-foreground" />
                    <span>{clearLabel}</span>
                  </CommandItem>
                </CommandGroup>
              </>
            ) : null}
          </CommandList>
        </Command>

        {resultCountHint ? (
          <div className="border-t border-border px-3 py-2 text-xs text-muted-foreground">
            {resultCountHint}
          </div>
        ) : null}
      </PopoverContent>
    </Popover>
  );
});

function SearchSelectItem({
  option,
  isSelected,
  onSelect,
}: {
  option: SearchSelectOption;
  isSelected: boolean;
  onSelect: () => void;
}) {
  return (
    <CommandItem
      value={option.label}
      keywords={option.keywords}
      onSelect={onSelect}
      className="items-start"
    >
      <span
        className={cn(
          "mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-sm border",
          isSelected
            ? "border-primary bg-primary text-primary-foreground"
            : "border-input text-transparent",
        )}
      >
        <Check className="size-3" />
      </span>
      <span className="flex min-w-0 flex-1 flex-col gap-0.5">
        <span className="truncate font-medium">{option.label}</span>
        {option.description ? (
          <span className="truncate text-xs text-muted-foreground">
            {option.description}
          </span>
        ) : null}
      </span>
    </CommandItem>
  );
}
