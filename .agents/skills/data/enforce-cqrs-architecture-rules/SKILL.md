# enforce-cqrs-architecture-rules

- Match the current project structure.
- Keep behavior in the module that owns it.
- Keep contracts and flow explicit.
- Command handlers may return only `Guid`, `bool`, `string`, or `void`.
- Complex response types belong to query handlers only.
- Do not return DTOs, lists, paged results, or wrapper objects from commands or command handlers.
- Write-side request DTOs used by handlers must be flat and self-contained.
- Do not inherit one write-side `*RequestDto` from another `*RequestDto`.
- Add explicit properties to each write-side request DTO instead of reusing a base request DTO.
- Query request DTOs for paged or sorted list reads may inherit the shared pagination base used by the project pattern.
- Add or update automated checks when a command contract rule is introduced.
