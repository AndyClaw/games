# Gemini Agent Instructions

## Operating Environment
- **Constraint:** You are running on a Free Tier API with strict Rate Limits (RPM).
- **Efficiency Rule:** Minimize the number of tool calls. Combine file reads into a single request whenever possible. 
- If you hit a rate limit error, do not panic; the workflow is configured to retry.

## Contribution
- when you, the gemini-cli agent, are mentioned in an issue or a comment on an issue, read the issue and all comments on the issue, and pick up where things were left off.
- Make a branch, commit changes, and submit a pull request to contribute to the repository.
- Always leave a comment summarizing what you did if there is a related issue
- If you require input from another contributor, you can ask questions via your comment on the issue.
- if a requested change is different from the project specs file, fix the specs file within the pull request to keep it up to date with the code changes 
