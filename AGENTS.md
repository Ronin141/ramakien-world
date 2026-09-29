# AGENTS.md

## Project

Project name: ramakien-world
Engine: Unity 6
Genre: Third-person 3D action adventure
Theme: Ramakien-inspired fantasy world

## Development Principles

- Keep the architecture simple and maintainable.
- Prefer small, focused components.
- Do not over-engineer systems before they are needed.
- Do not introduce external assets or packages unless explicitly requested.
- Do not modify unrelated files.
- Avoid generated Unity folders such as Library, Temp, Logs, and obj.
- Keep gameplay code under Assets/Scripts.
- Use clear namespaces and descriptive class names.
- Explain architectural decisions when introducing a new system.

## Current Milestone

Only implement:

- Basic third-person player movement
- Camera control
- Running
- Jumping
- Simple test ground

Do NOT implement:

- Combat
- Inventory
- Quests
- Enemies
- Dialogue
- Save system
- Skill system
- Final art assets

Placeholder primitives are preferred for this milestone.

## Workflow

Before modifying files:

1. Read this AGENTS.md.
2. Inspect the existing Unity project structure.
3. Explain the implementation plan.
4. List the files expected to be created or modified.
5. Wait for approval before making significant architectural changes.

After implementation:

1. Report all files created or modified.
2. Explain any Unity Editor setup required.
3. Report assumptions or known limitations.
4. Do not expand the requested scope.