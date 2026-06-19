import { RouteObject } from "react-router-dom";
import { McpPage } from "@/domains/mcp/mcp-page";

export const McpRoutes: RouteObject[] = [
  {
    path: "mcp",
    element: <McpPage />,
    handle: {
      title: "MCP",
      breadcrumb: "MCP",
      navKey: "mcp",
    },
  },
];
