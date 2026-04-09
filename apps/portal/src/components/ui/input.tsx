import * as React from 'react';
import { cn } from '@/lib/utils';
import { type VariantProps } from 'class-variance-authority';
import {
  inputAddonVariants,
  inputGroupVariants,
  inputVariants,
  inputWrapperVariants,
} from '@/components/ui/input.variants';
import { translateText } from '@/shared/lib/i18n-core';

function Input({
  className,
  type,
  variant,
  placeholder,
  ...props
}: React.ComponentProps<'input'> & VariantProps<typeof inputVariants>) {
  return (
    <input
      data-slot="input"
      type={type}
      placeholder={typeof placeholder === 'string' ? translateText(placeholder) : placeholder}
      className={cn(inputVariants({ variant }), className)}
      {...props}
    />
  );
}

function InputAddon({
  className,
  variant,
  mode,
  ...props
}: React.ComponentProps<'div'> & VariantProps<typeof inputAddonVariants>) {
  return <div data-slot="input-addon" className={cn(inputAddonVariants({ variant, mode }), className)} {...props} />;
}

function InputGroup({ className, ...props }: React.ComponentProps<'div'> & VariantProps<typeof inputGroupVariants>) {
  return <div data-slot="input-group" className={cn(inputGroupVariants(), className)} {...props} />;
}

function InputWrapper({
  className,
  variant,
  ...props
}: React.ComponentProps<'div'> & VariantProps<typeof inputWrapperVariants>) {
  return (
    <div
      data-slot="input-wrapper"
      className={cn(inputVariants({ variant }), inputWrapperVariants({ variant }), className)}
      {...props}
    />
  );
}

export { Input, InputAddon, InputGroup, InputWrapper };
