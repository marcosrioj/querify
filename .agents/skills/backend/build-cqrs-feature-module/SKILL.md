# build-cqrs-feature-module

- Match the current project structure.
- Keep behavior in the module that owns it.
- Keep contracts and flow explicit.
- Keep write contracts simple: command handlers may return only `Guid`, `bool`, `string`, or `void`.
- Put complex response models in queries only.
- Do not add read DTO shaping to commands, command handlers, or write controllers.
- Keep command-side request DTOs flat.
- Do not inherit one write-side `*RequestDto` from another `*RequestDto`.
- Each write-side request DTO must declare its own properties explicitly.
- Query request DTOs that represent paged or sorted list reads may inherit the common pagination base used by the project pattern.
