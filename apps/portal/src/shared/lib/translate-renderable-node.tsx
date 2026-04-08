import {
  Children,
  cloneElement,
  isValidElement,
  type ReactNode,
} from "react";
import { translateText } from "@/shared/lib/i18n-core";

export function translateRenderableNode(node: ReactNode): ReactNode {
  if (typeof node === "string") {
    return translateText(node);
  }

  if (!isValidElement<{ children?: ReactNode }>(node)) {
    return node;
  }

  const children = node.props.children;

  if (children === undefined) {
    return node;
  }

  return cloneElement(node, {
    ...node.props,
    children: Children.map(children, (child) => translateRenderableNode(child)),
  });
}
