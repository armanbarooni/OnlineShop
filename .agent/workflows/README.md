# Workflows Guide

## What are Workflows?

Workflows are reusable step-by-step instructions stored in `.agent/workflows/` that help automate common tasks.

## How to Use Workflows

### Method 1: Slash Commands
Use the filename as a slash command:
- `/example-deploy` - Runs the example deployment workflow
- `/full-auto-deploy` - Runs the fully automated deployment

### Method 2: Ask Me
Simply say: "Run the deployment workflow" or "Follow the example-deploy workflow"

## Turbo Mode Explained

### `// turbo` - Single Step Auto-Run
Place `// turbo` ABOVE a step to auto-run ONLY that step:

```markdown
// turbo
2. **List files**
   ```bash
   ls
   ```
```

### `// turbo-all` - Full Auto-Run
Place `// turbo-all` ANYWHERE in the workflow to auto-run ALL commands:

```markdown
// turbo-all

# My Workflow

1. **Step 1** (will auto-run)
2. **Step 2** (will auto-run)
3. **Step 3** (will auto-run)
```

## Creating Your Own Workflows

1. Create a new file: `.agent/workflows/your-workflow-name.md`
2. Add YAML frontmatter with description:
   ```yaml
   ---
   description: Brief description of what this workflow does
   ---
   ```
3. Write your steps in markdown
4. Add `// turbo` or `// turbo-all` annotations as needed
5. Use it with `/your-workflow-name`

## Example Workflows Included

- **example-deploy.md** - Demonstrates selective turbo mode (some steps auto-run)
- **full-auto-deploy.md** - Demonstrates full turbo mode (all steps auto-run)
- **complete-shop-automation.md** - Complete shop frontend automation with dynamic data (PRODUCTION WORKFLOW)

## Tips

- Use `// turbo-all` for workflows you trust completely (like deployments)
- Use `// turbo` for individual safe steps (like checking status, listing files)
- Workflows are great for onboarding new team members
- Keep workflows updated as your process changes
