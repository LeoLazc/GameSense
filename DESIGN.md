---
name: GameSense
description: A precise knowledge qualification console for gaming reviewers.
colors:
  graphite: "#171817"
  bone: "#e7e3d9"
  ink: "#222421"
  muted: "#6c7069"
  rule: "#bbb8ad"
  signal: "#bd4937"
  soft-signal: "#f0d5ce"
typography:
  display:
    fontFamily: "DM Sans, Arial, sans-serif"
    fontSize: "clamp(2.2rem, 4vw, 4.5rem)"
    fontWeight: 500
    lineHeight: ".98"
    letterSpacing: "-.065em"
  body:
    fontFamily: "DM Sans, Arial, sans-serif"
    fontSize: "1rem"
    lineHeight: "1.65"
  label:
    fontFamily: "IBM Plex Mono, monospace"
    fontSize: "11px"
    letterSpacing: ".09em"
rounded:
  none: "0"
spacing:
  sm: "8px"
  md: "18px"
  lg: "30px"
components:
  button-primary:
    backgroundColor: "{colors.graphite}"
    textColor: "{colors.bone}"
    rounded: "{rounded.none}"
    padding: "14px 18px"
---

# Design System: GameSense

## Overview

**Creative North Star: "The Ruling Desk"**

GameSense is a compact qualification instrument for reviewers. Its visual language treats knowledge eligibility as a serious assessment record: dark graphite chrome frames a warm bone reading field, while a restrained vermilion signal marks focus, progress, and exceptions.

The interface is measured rather than game-like. Hairline rules, square detents, monospaced annotations, and a fixed question rail create a report surface that makes the next decision legible without turning the workflow into a reward dashboard.

**Key Characteristics:**
- Graphite instrument chrome and bone content field
- Vermilion used as a scarce state signal
- Square geometry, hairline rules, and dense annotations
- Spanish UI copy with a precise, trustworthy voice

## Colors

The palette is warm, low-chroma, and editorial: graphite and bone establish the instrument, iron ink carries reading, and vermilion is reserved for state.

### Primary
- **Vermilion Signal** (#bd4937): Progress, focus, active detents, and recovery states.

### Neutral
- **Graphite** (#171817): Instrument bar, primary actions, and high-contrast chrome.
- **Bone** (#e7e3d9): Main reading field and light text on graphite.
- **Iron Ink** (#222421): Primary content text and rules.
- **Muted Field** (#6c7069): Labels, metadata, and secondary copy.
- **Hairline Rule** (#bbb8ad): Dividers and progress tracks.
- **Soft Signal** (#f0d5ce): Error and exception background.

**The Scarce Signal Rule.** Vermilion is a signal, not a fill color. Use it where the reviewer needs to notice state or action.

## Typography

**Display Font:** DM Sans (with Arial sans-serif fallback)
**Body Font:** DM Sans (with Arial sans-serif fallback)
**Label/Mono Font:** IBM Plex Mono

**Character:** DM Sans keeps questions and decisions readable; IBM Plex Mono makes status, metadata, and system annotations feel recorded and measurable.

### Hierarchy
- **Display** (500, clamp(2.2rem, 4vw, 4.5rem), .98): Primary question, result, and page headings.
- **Title** (500, 1.45rem, normal): Panel headings.
- **Body** (400, 1rem, 1.65): Explanatory copy and answer content.
- **Label** (400, 11px, .09em, uppercase where used): State, field, and progress annotations.

## Layout

The desktop console uses a centered 90vw reading frame with a two-column composition: a margin or progress rail beside one dominant content panel. The main question rail remains the focal area. On screens below 700px, columns become a vertical sequence and the progress rule becomes horizontal while preserving the same reading order.

### Game index and detail
- **Home:** The home surface contains only the API-backed recent-game carousel. Each slide places a real game image on the left and release metadata on the right; the media area and content region keep stable dimensions across slides, while controls remain manual and visible.
- **Detail:** The game page uses separate image and information sections. Title, release facts, description, source link, and community reviews remain readable without stacking content into decorative cards.
- **Responsive:** The image leads on mobile, followed by the game information and reviews. Carousel controls remain text-labelled and keyboard accessible.

## Elevation & Depth

The system is flat by default. Depth comes from tonal contrast, hairline rules, and the graphite instrument bar rather than shadows or floating surfaces.

**The Flat-By-Default Rule.** A surface should earn separation through structure and state before adding decoration.

## Shapes

All controls and panels use square corners (0px). Borders are generally 1px and quiet. Detents are small square marks; progress is a ruled track with a filled signal segment. There are no rounded card stacks.

## Components

### Buttons
- **Shape:** Square, hairline border (0px radius).
- **Primary:** Graphite background with bone text and compact 14px 18px padding.
- **Hover / Focus:** Primary action inverts to bone with graphite text; focus uses a vermilion outer ring.
- **Quiet:** Text-only action with an offset underline.

### Cards / Containers
- **Corner Style:** Square (0px).
- **Background:** Bone field or transparent panel over the field.
- **Shadow Strategy:** No shadows; use top and bottom rules.
- **Border:** 1px ink or hairline rule.
- **Internal Padding:** Compact 28px top and 30px bottom for form panels.

### Inputs / Fields
- **Style:** Square 1px gray stroke, lightly translucent bone-white fill, 14px padding.
- **Focus:** Ink border with a vermilion outer ring.
- **Error / Disabled:** Soft signal background for errors; disabled actions reduce opacity.

### Navigation
 - **Style:** A 64px graphite instrument bar with GameSense identity at left, a light-orange `Quiz` action for the quiz surface, and reviewer identity at right. Metadata uses IBM Plex Mono; identity uses DM Sans.
- **Mobile:** Context metadata and rule collapse, preserving brand and user control.

### Qualification Rail
The progress rail is a vertical ruled index on desktop and a horizontal ruled index on mobile. It exposes position and state without becoming a progress-ring reward device.

## Do's and Don'ts

### Do:
- **Do** keep one dominant question or ruling in the content field.
- **Do** use rules, labels, and detents to communicate state.
- **Do** preserve accessible focus rings and reduced-motion behavior.
- **Do** keep visible UI copy in Spanish.

### Don't:
- **Don't** use rounded card grids, gamified rewards, or dashboard sprawl.
- **Don't** use gradients, decorative shadows, or vermilion as a general background.
- **Don't** expose expected answers or provider evaluation rules in the client.
