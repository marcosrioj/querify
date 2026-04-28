"use client";

import * as React from "react";
import { cn } from "@/lib/utils";
import { cva, type VariantProps } from "class-variance-authority";
import { translateRenderableNode } from "@/shared/lib/translate-renderable-node";

// Define CardContext
type CardContextType = {
  variant: "default" | "accent";
};

const CardContext = React.createContext<CardContextType>({
  variant: "default", // Default value
});

// Hook to use CardContext
const useCardContext = () => {
  const context = React.useContext(CardContext);
  if (!context) {
    throw new Error("useCardContext must be used within a Card component");
  }
  return context;
};

// Variants
const cardVariants = cva(
  "flex flex-col items-stretch rounded-xl text-card-foreground",
  {
    variants: {
      variant: {
        default:
          "border border-border/70 bg-card shadow-[var(--shadow-premium-card)] ring-1 ring-black/[0.015] dark:ring-white/[0.035]",
        accent: "bg-muted/60 shadow-xs p-1",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  },
);

const cardHeaderVariants = cva(
  "flex flex-col gap-3 px-5 py-4 md:flex-row md:items-start md:justify-between",
  {
    variants: {
      variant: {
        default: "border-b border-border/60",
        accent: "",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  },
);

const cardContentVariants = cva("grow p-5", {
  variants: {
    variant: {
      default: "",
      accent: "bg-card rounded-t-xl [&:last-child]:rounded-b-xl",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

const cardTableVariants = cva("grid grow", {
  variants: {
    variant: {
      default: "",
      accent: "bg-card rounded-xl",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

const cardFooterVariants = cva("flex items-center px-5 py-4", {
  variants: {
    variant: {
      default: "border-t border-border/60",
      accent: "bg-card rounded-b-lg mt-[2px]",
    },
  },
  defaultVariants: {
    variant: "default",
  },
});

// Card Component
function Card({
  className,
  variant = "default",
  ...props
}: React.HTMLAttributes<HTMLDivElement> & VariantProps<typeof cardVariants>) {
  return (
    <CardContext.Provider value={{ variant: variant || "default" }}>
      <div
        data-slot="card"
        className={cn(cardVariants({ variant }), className)}
        {...props}
      />
    </CardContext.Provider>
  );
}

// CardHeader Component
function CardHeader({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  const { variant } = useCardContext();
  return (
    <div
      data-slot="card-header"
      className={cn(cardHeaderVariants({ variant }), className)}
      {...props}
    />
  );
}

// CardContent Component
function CardContent({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  const { variant } = useCardContext();
  return (
    <div
      data-slot="card-content"
      className={cn(cardContentVariants({ variant }), className)}
      {...props}
    />
  );
}

// CardTable Component
function CardTable({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  const { variant } = useCardContext();
  return (
    <div
      data-slot="card-table"
      className={cn(cardTableVariants({ variant }), className)}
      {...props}
    />
  );
}

// CardFooter Component
function CardFooter({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  const { variant } = useCardContext();
  return (
    <div
      data-slot="card-footer"
      className={cn(cardFooterVariants({ variant }), className)}
      {...props}
    />
  );
}

// Other Components
function CardHeading({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="card-heading"
      className={cn("space-y-1.5", className)}
      {...props}
    />
  );
}

function CardToolbar({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="card-toolbar"
      className={cn("flex flex-wrap items-center gap-2.5", className)}
      {...props}
    />
  );
}

function CardTitle({
  className,
  ...props
}: React.HTMLAttributes<HTMLHeadingElement>) {
  return (
    <h3
      data-slot="card-title"
      className={cn("text-base font-semibold leading-5 text-mono", className)}
      {...props}
    >
      {translateRenderableNode(props.children)}
    </h3>
  );
}

function CardDescription({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="card-description"
      className={cn("text-sm leading-5 text-muted-foreground", className)}
      {...props}
    >
      {translateRenderableNode(props.children)}
    </div>
  );
}

// Exports
export {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardHeading,
  CardTable,
  CardTitle,
  CardToolbar,
};
