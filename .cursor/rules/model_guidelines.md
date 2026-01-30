## AI Model Usage Guidelines
- **Complex Tasks:** Use Claude 3.5 Sonnet or GPT-4o for architectural changes, cross-project refactoring, and complex logic in POS.Core or POS.UI.
- **Simple Tasks:** For documentation, unit tests, simple UI tweaks, or boilerplate, remind the user to switch to "cursor-small" or "gpt-4o-mini" to save fast requests.
- **Agent/Composer:** Before executing a multi-file plan, the AI should state if the task is "Complex" or "Simple" so the user can select the appropriate model.