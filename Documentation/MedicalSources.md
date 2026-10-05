# Medical source register

## Exact reference

**S1 — IFRC International First Aid, Resuscitation and Education Guidelines 2025.** International Federation of Red Cross and Red Crescent Societies. Official 2026 publication of the 2025 edition.

- [IFRC document page](https://www.ifrc.org/document/ifrc-international-first-aid-resuscitation-and-education-guidelines-2025)
- [Official PDF](https://www.ifrc.org/sites/default/files/2026-03/IFRC%20International%20First%20Aid%2C%20Resuscitation%20and%20Education%20Guidelines%202025.pdf)
- Level 1: **General approach → Assess the scene**, printed pp. **115 and 117** (PDF indices 114 and 116); access-help context p. 116.
- Accessed 2026-10-04. No bulk copyrighted passages reproduced.

## Level 1 audit

`Assets/Data/ScenarioData/Level01_SceneSafety.asset` is the rule source, using source ID `ifrc2025`. The Ink file holds concise feedback paraphrases. Fictional setting: a maintenance bay, damaged equipment, a spill, one person down and two bystanders. These observations are authored scene facts, not medical deductions or electrical safety thresholds.

| Rule | Purpose | Basis |
|---|---|---|
| R01_uncontrolled_hazard | Derive scene_unsafe | S1, Assess the scene |
| R02_distant_support | Recognize the option of support from outside | S1, Assess the scene |
| R03_unsafe_entry | Flag an approach into the known hazard | S1, Assess the scene |
| R04_safe_response | Recognize staying outside and requesting help | S1, Assess the scene / access help |
| R05_assessment_recorded | Record observations across the three clue groups | Pedagogical completion marker based on S1 assessment topics; not clinical clearance |

The responder never handles the cable or claims that a dry spot guarantees safe access. Early help is allowed. The simulated call has no external effect. Completion does not imply treatment, rescue or hazard control. Scoring weights and the three-observation target are educational design choices, not guideline recommendations.

Review status: source-linked engineering implementation; **human first-aid educator review remains outstanding** before public educational release.

## Planned level source table (no later-level content implemented)

S1 above is the exact guideline document selected for all rows. For Levels 2–20 these are planned references only: relevant subsections and rules require review when each scenario is authored. This table does not claim medical validation of unbuilt levels.

| Level | Topic | Guideline document |
|---|---|---|
| 01 | Scene safety | S1 — IFRC Guidelines 2025; audited above |
| 02 | Recognizing when to seek help | S1 — IFRC Guidelines 2025 |
| 03 | Minor bleeding | S1 — IFRC Guidelines 2025 |
| 04 | Nosebleed | S1 — IFRC Guidelines 2025 |
| 05 | Minor burn | S1 — IFRC Guidelines 2025 |
| 06 | Sprain | S1 — IFRC Guidelines 2025 |
| 07 | Eye-related minor incident | S1 — IFRC Guidelines 2025 |
| 08 | Fall injury | S1 — IFRC Guidelines 2025 |
| 09 | Possible fracture | S1 — IFRC Guidelines 2025 |
| 10 | Multiple observations/symptoms | S1 — IFRC Guidelines 2025 |
| 11 | Severe bleeding | S1 — IFRC Guidelines 2025 |
| 12 | Breathing emergency | S1 — IFRC Guidelines 2025 |
| 13 | Unresponsive person | S1 — IFRC Guidelines 2025 |
| 14 | Choking emergency | S1 — IFRC Guidelines 2025 |
| 15 | Multiple-injury scenario | S1 — IFRC Guidelines 2025 |
| 16 | Outdoor emergency | S1 — IFRC Guidelines 2025 |
| 17 | Sports incident | S1 — IFRC Guidelines 2025 |
| 18 | Two-person scenario | S1 — IFRC Guidelines 2025 |
| 19 | Limited-resource scenario | S1 — IFRC Guidelines 2025 |
| 20 | Mixed challenge | S1 — IFRC Guidelines 2025 |
