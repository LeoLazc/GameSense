---
version: 1
slug: "src-gamesense-web"
primary_target: "src/GameSense.Web"
related_targets: []
---

## Scope
Operate surface: the GameSense web flows, from authentication through reviewer qualification: sign-in/register, the main hub, the reviewer profile, and the quiz.

## Job and audience
Gaming reviewers need to complete a trustworthy knowledge assessment, understand whether they are eligible to review, and manage the account behind that eligibility.

## Outcome and proof
The user signs in or registers, lands on the main hub, opens the quiz from it, starts or resumes a real API-backed quiz, submits one answer at a time, and receives an honest scored result. The top bar shows the username and opens the profile, which shows the reviewer record and the sign-out control. No expected answers or evaluation rules appear in the client.

## Direction
Reviewer qualification console: a ruling-engine layout with a compact graphite instrument surface, bone-white reading field, hairline rules, and one restrained signal accent. The question panel is the fixed centre rail; progress and state annotations sit in the margin. The focal moment is the answer becoming an assessed record, not a game reward.

## Direction contract
THESIS: Make reviewer qualification feel like a precise assessment report, refusing game-like rewards and dashboard-card sprawl.
OWN-WORLD: Dark graphite chrome, warm bone content, iron ink, vermilion signal marks, hairline rules, square detents, measured labels, and no rounded card stack.
STORY: The reviewer establishes identity, resumes or begins a session, answers one question, sees evaluation in progress, then reads a pass/fail eligibility report.
FIRST VIEWPORT: A narrow top instrument bar frames a two-column console: status/progress rail at left and one dominant question record at right, with the answer field and submit action anchored beneath it.
FORM: Reference-setting page / ruling engine, assigned direction candidate 3, seed key 9c66fc11; rules and detents carry state instead of decoration.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## States and boundaries
- Required states: first visit, hub, profile, active question, submitting/evaluating, provider error with retry, pass, fail, resume, 401, 409, network failure, empty questions.
- Preserve API behavior; use only API question DTOs; persist token and session for refresh; remain usable at mobile widths.

## Constraints
- React + Vite + TypeScript under src/GameSense.Web; minimal dependencies; VITE_API_BASE_URL; accessible semantic controls, keyboard focus, live status, reduced motion.
- All visible UI copy is Spanish; the product name `GameSense` stays as-is.
