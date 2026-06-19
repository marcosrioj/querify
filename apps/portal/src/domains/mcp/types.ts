export type McpAvailability = "available" | "future";

export type McpToolGroup = {
  module: string;
  tools: string[];
  boundary: string;
  availability: McpAvailability;
};

export type McpReadinessItem = {
  module: string;
  state: McpAvailability;
  note: string;
};
