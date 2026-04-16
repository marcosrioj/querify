# Intent Routing

- If the request changes state, start in the owning command handler.
- If the request changes response shape, start in the owning query handler.
- If the request changes composition, start in the project extension and API registration.
- If the request changes persistence behavior, inspect the entity, configuration, context rules, and tests together.
