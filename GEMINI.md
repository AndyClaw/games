# Gemini Agent Instructions

## Operating Environment
- **Constraint:** You are running on a Free Tier API with strict Rate Limits (RPM).
- **Efficiency Rule:** Minimize the number of tool calls. Combine file reads into a single request whenever possible. 
- If you hit a rate limit error, do not panic; the workflow is configured to retry.
  
