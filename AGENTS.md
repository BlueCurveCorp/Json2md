# Objectives
You are doing a dotnet rewrite of the library that is in this directory 'original/'.
- The port MUST be in `dotnet 10`
- The port MUST enforce dontnet best practices while being simple
- The port MUST pass all tests of the original version
- ALL tests of the original version MUST be port to dotnet

- The dotnet code MUST be put in the folder 'src/'
- The dotnet solution is 'Json2md.slnx'
- The dotnet solution use centrally manage package version

## graphify

This project has a graphify knowledge graph at graphify-out/.

Rules:

- Before answering architecture or codebase questions, read graphify-out/GRAPH_REPORT.md for god nodes and community structure
- If graphify-out/wiki/index.md exists, navigate it instead of reading raw files
- For cross-module "how does X relate to Y" questions, prefer `graphify query "<question>"`, `graphify path "<A>" "<B>"`, or `graphify explain "<concept>"` over grep — these traverse the graph's EXTRACTED + INFERRED edges instead of scanning files
- After modifying code files in this session, run `graphify update . --no-viz` to keep the graph current (AST-only, no API cost)
- graphify can take a long time to finish on a large codebase so just wait until it finish

## MASTER SKILL REGISTRY & PROTOCOL

**ALWAYS load the `master-skill-registry-protocol` skill**, if you can't find a skill, check if the `master-skill-registry-protocol` can route you to an approriate skill.
**When listing skills also ALWAYS take into account the `master-skill-registry-protocol` , `MASTER SKILL REGISTRY`** 

## Progress tracking

Always create an issue in `Linear` to track what you are doing

- ALWAYS use the team `BlueCurve` - You are part of this team
- ALWAYS use the workspace `BlueCurve` - You are part of this workspace
- If an issue should be split use sub-issues
- Always update the issues status
- Use the issues comments to track you progress
- In doubt always referer to the issue, if the issue is a sub-issue start from the master issue and read it then all sub-issues before the one you are working on so you can refresh your understanding

**If Linear is not reacheable create a file `<TASK_CONTENT>-<DATE:ddMMyyyy>-MEMORY.md` in a directory named `./Memories` to keep track of your progress**

## Testing

- `TUnit`: Unit test
- `Reqnroll`: Behavior Driven Development (BDD)
- `Reqnroll.TUnit`: Reqnroll integration with TUnit

**Always use Gherkin** for Behavior Driven Development (BDD)
