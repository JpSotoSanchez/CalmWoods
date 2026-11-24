# User Testing and Iteration — Calm Woods

| | |
|---|---|
| **Project** | Calm Woods: a procedurally generated VR forest for stress management and guided breathing |
| **Course** | Human Computer Interaction — Final Project |
| **Platform tested** | Meta Quest 2 · Fitbit Inspire 3 (heart rate) |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |
| **Commit tested** | P1: [`bd448e3`](https://github.com/JpSotoSanchez/CalmWoods/commit/bd448e3ae4c7b97be729d471a22159f7f81b043c) · P2: a build between [`815b5c7`](https://github.com/JpSotoSanchez/CalmWoods/commit/815b5c7) and [`40db2f4`](https://github.com/JpSotoSanchez/CalmWoods/commit/40db2f4) · P3, P4: pending verification |
| **Test dates** | 22/11/2026 (P2, then P1) · P3, P4: pending verification |

> **Scope of this file.** It reports **who tested** Calm Woods, **the tasks they completed**, **the results** and **the improvements** we made. How the tests were run (protocol, surveys, observation log, raw data, limitations) is in [`TestingProtocol.md`](TestingProtocol.md). Who the users are and what they need is in [`UserResearch.md`](UserResearch.md).

> **Status (23/11/2026).** Four sessions were run, each on a newer build of the app. IDs go in reverse order of testing: the highest ID used the oldest build, and P1 used the final build. **P1 and P2 are verified and reported here.** P3 and P4 are pending: their notes do not match any build in the repository history yet, so their data is not included until the build they used is confirmed.

---

## 1. Summary

- Participants from the target audience tested the app on a Meta Quest 2 in a **5-minute session** made of two tasks: **T1**, 3 minutes of free exploration, and **T2**, 2 minutes dedicated only to calming their breathing.
- We measured **task completion** (hints, errors, time), **how they felt before and after** (3 questions, 1–10), **heart rate** (Fitbit Inspire 3), **experience ratings** and **comments**.
- Every participant also sat quietly for 5 minutes **without the app** (control), so we can tell what the app adds beyond resting.
- Each participant tested a newer build, so the sessions also show the **iteration**: P2 tested the build with breathing tracking but no reactive weather; P2 asked for visual confirmation in the environment; P1 tested the build with reactive weather.
- **Results so far (P1, P2):** both completed both tasks with 1 hint each. Both felt better after the app than after resting (stress −2 and −3 with the app, −1 and −1 at rest). Heart rate fell 17 and 12 bpm with the app, and 10 and 4 bpm at rest. P2 wanted proof that the app was detecting their breathing; P1, with the weather in place, still did not understand the clouds (C2 = 2) and felt mild dizziness.

---

## 2. Who tested the solution

**Target audience:** adults (18+) who feel stress regularly in everyday life, including people with little or no VR experience (see [`UserResearch.md`](UserResearch.md)). Participants are anonymized. Consent was given verbally.

| ID | Age | Occupation | VR experience | Date | Build tested | What the build had |
|---|---|---|---|---|---|---|
| P1 | 19 | Student | A little (a few times) | 22/11/2026 | `bd448e3` | Forest, menus, audio, tutorial, breathing guide, breathing tracking, **reactive weather** |
| P2 | 25 | Junior analyst | Never | 22/11/2026 | between `815b5c7` and `40db2f4` | Forest, menus, audio, tutorial, breathing guide, **breathing tracking** (chest controller + microphone, with an on-screen sensor label). No reactive weather yet |
| P3 | | | | pending | pending verification | |
| P4 | | | | pending | pending verification | |

---

## 3. Tasks completed

The two tasks follow the real main flow of the app: **Main menu → Setup → Forest → Breathing exercise (B button) / Options menu (Y button)**.

| Task | Duration | What the participant was told | Success = |
|---|---|---|---|
| **T1 – Free exploration** | 3 min | "Explore the app however you like. Get to know it." | Starts the session from the main menu, chooses a time of day, adjusts at least one volume slider and enters the forest, all without a hint |
| **T2 – Calm your breathing** | 2 min | "Now dedicate yourself exclusively to calming your breathing. Please stay silent." | Opens the breathing menu, places the controller on the chest, follows the inhale / hold / exhale guide for the full 2 minutes |

An observer scored 10 checkpoints (5 per task) for each participant. A checkpoint is **N/A** when the feature did not exist yet in the build that participant used; N/A is not a failure. The full checkpoint list, the surveys and the protocol are in [`TestingProtocol.md`](TestingProtocol.md).

---

## 4. Results

### 4.1 Task completion and errors

✔ = done unaided · ◐ = done with a hint · ✘ = failed · N/A = not in that build. Times are approximate. For P1, about 7 s between consecutive checkpoints (range 6–8 s), counted again from the start of T1 and of T2.

| Checkpoint | P1 | P2 | P3 | P4 |
|---|---|---|---|---|
| 1.1 Start from the main menu | ◐ ≈7 s (hint: go back to the menu to read the instructions again) | ✔ ≈9 s | | |
| 1.2 Choose a time of day | ✔ ≈7 s | ✔ ≈7 s (Dawn) | | |
| 1.3 Adjust the volume | ✔ ≈7 s | ✔ ≈8 s (music at 70 %, ambient raised) | | |
| 1.4 Enter the forest | ✔ ≈7 s | ✔ ≈6 s | | |
| 1.5 Look around comfortably | ✔ | ✔ (stayed mostly seated looking around, walked a little on the path) | | |
| 2.1 Open the breathing menu | ✔ ≈7 s | ✔ ≈7 s (remembered it from the tutorial) | | |
| 2.2 Place the controller on the chest | ✔ ≈7 s | ◐ ≈11 s (unsure whether the right or left controller; told it was the left) | | |
| 2.3 Follow the breathing guide | ✔ ≈7 s | ✔ ≈6 s (very steady for the 2 minutes) | | |
| 2.4 Notice the weather reacting | ✔ late: only in the last seconds of T2 | N/A (no reactive weather in this build) | | |
| 2.5 Stay silent and still | ✔ | ✔ | | |
| **Hints / errors** | 1 / 3 | 1 / 1 | | |
| **Crash or tracking lost** | No | No | | |

**Errors observed**
- **P1:** pressed the audio-menu button twice by accident; pressed the breathing-menu button twice by accident (both because they pressed too fast); skipped the instructions the first time the app opened.
- **P2:** at the start of T2, moved the chest controller to get comfortable, which restarted the tracking; settled quickly.

### 4.2 How participants felt (before → after the app)

Scale 1–10, lower is better.

| Item | P1 | P2 | P3 | P4 | Mean Δ (P1–P2) |
|---|---|---|---|---|---|
| Q1 Stress | 7 → 5 (−2) | 7 → 4 (−3) | | | −2.5 |
| Q2 Body tension | 5 → 4 (−1) | 7 → 4 (−3) | | | −2.0 |
| Q3 Restless thoughts | 6 → 4 (−2) | 6 → 4 (−2) | | | −2.0 |
| Overall change (reported) | Slightly better | Much better | | | |

### 4.3 Heart rate

Fitbit Inspire 3, read every 30 s. "End" is the reading at minute 5. The full series is in [`TestingProtocol.md`](TestingProtocol.md), Section 8.2.

| Participant | Baseline | Mean T1 | Mean T2 | End (5:00) | Post (+1 min) | Δ end − baseline |
|---|---|---|---|---|---|---|
| P1 | 95 | 89.3 | 80.0 | 78 | 79 | −17 |
| P2 | 80 | 77.2 | 69.8 | 68 | 69 | −12 |
| P3 | | | | | | |
| P4 | | | | | | |
| **Mean (P1–P2)** | 87.5 | 83.3 | 74.9 | 73.0 | 74.0 | −14.5 |

### 4.4 App vs. sitting quietly (control)

Control: 5 minutes seated without the app, with the same questions before and after and the same heart-rate readings. P1 did the control first (app 2 hours later); P2 did the app first (control 2 hours later), so across these two the order is balanced.

| Metric | P1 app | P1 control | P2 app | P2 control | P3 app | P3 control | P4 app | P4 control |
|---|---|---|---|---|---|---|---|---|
| Q1 stress, before → after (Δ) | 7 → 5 (−2) | 7 → 6 (−1) | 7 → 4 (−3) | 7 → 6 (−1) | | | | |
| Q2 body tension (Δ) | 5 → 4 (−1) | 5 → 6 (+1) | 7 → 4 (−3) | 6 → 6 (0) | | | | |
| Q3 restless thoughts (Δ) | 6 → 4 (−2) | 6 → 6 (0) | 6 → 4 (−2) | 6 → 5 (−1) | | | | |
| Overall change | Slightly better | The same | Much better | Slightly better | | | | |
| Heart rate, baseline → end (Δ) | 95 → 78 (−17) | 95 → 85 (−10) | 80 → 68 (−12) | 78 → 74 (−4) | | | | |

**Reading so far:** for both participants the app improved every measure more than resting alone. Mean change with the app vs. at rest: stress −2.5 vs. −1.0, body tension −2.0 vs. +0.5, thoughts −2.0 vs. −0.5, heart rate −14.5 vs. −7.0 bpm. Resting also lowered stress and heart rate, so part of the change comes from sitting quietly. These are two participants, so this is a trend, not proof.

### 4.5 Experience ratings

Scale 1–5 (C5: lower is better). N/A = the feature was not in that participant's build.

| Item | P1 | P2 | P3 | P4 | Mean (n) |
|---|---|---|---|---|---|
| C1 The breathing guide was easy to follow | 5 | 5 | | | 5.0 (2) |
| C2 I understood how my breathing affected the weather | 2 | N/A | | | 2.0 (1) |
| C3 The weather changes helped me breathe more calmly | 4 | N/A | | | 4.0 (1) |
| C4 The menus were easy to use | 5 | 5 | | | 5.0 (2) |
| C5 Dizziness, nausea or eye strain | 3 | 1 | | | 2.0 (2) |
| C6 Resting the controller on my chest was comfortable | not asked | 4 | | | 4.0 (1) |

### 4.6 What participants said

P2's answers were given in Spanish and are translated here.

| ID | Participant | Quote or observation | Theme |
|---|---|---|---|
| O1 | P1 | "At the end, I understood that the weather was responsive" | Understanding the weather |
| O2 | P1 | "The comfy place" (most pleasant part) | Atmosphere and comfort |
| O3 | P1 | "I didn't understand the clouds" | Understanding the weather |
| O4 | P1 | "Maybe instructions for the weather" (what to change) | Onboarding |
| O5 | P1 | Mild dizziness at the end of the session, with the clouds moving. Afterwards: the image looked a little blurry and the headset may not have been fitted well | VR comfort and headset fit |
| O6 | P1 | Skipped the instructions when the app first opened and needed a hint to go back to them | Onboarding |
| O7 | P1 | Noticed the weather reacting only in the last seconds of T2 | Understanding the weather |
| O8 | P2 | "The sunrise atmosphere, and feeling that the guided breathing really did slow me down" (most pleasant part) | Atmosphere and comfort |
| O9 | P2 | "I liked putting the controller on my chest, but I didn't see any change in the forest that confirmed whether I was doing it right" | Feedback on breathing |
| O10 | P2 | "Some change in the environment or the sky to see my progress without having to read numbers" (what to change) | Feedback on breathing |
| O11 | P2 | After T2, asked: "Did it detect it properly? I didn't see if anything changed" | Feedback on breathing |
| O12 | P2 | Unsure which controller goes on the chest; needed a hint | Chest controller |
| O13 | P2 | Favorite options: "The relaxing music and the sunrise." Would use it again "definitely, after finishing work, about 5 to 10 minutes" | Use again |
| O14 | P2 | Looked very comfortable from the start; no dizziness | VR comfort |

### 4.7 Did we meet our goals?

The criteria were set before testing (see [`TestingProtocol.md`](TestingProtocol.md), Section 1). The verdict is marked once the sample is closed.

| Goal | Criterion | P1–P2 so far | Met (all participants)? |
|---|---|---|---|
| G1 Main flow without help | Everyone completes T1 and T2; on average ≤ 1 hint | Both completed both tasks; 1 hint each (mean 1.0) | ☐ Yes ☐ Partly ☐ No |
| G2 Less stress after a session | Mean Q1 lower after the session | Mean Q1 7.0 → 4.5 | ☐ Yes ☐ Partly ☐ No |
| G3 Lower heart rate | Mean end HR ≤ mean baseline | Mean 87.5 → 73.0 | ☐ Yes ☐ Partly ☐ No |
| G4 Weather feedback understood | Mean C2 ≥ 4 and most link weather and breathing without a hint | C2 = 2 (P1 only; P2's build had no reactive weather) | ☐ Yes ☐ Partly ☐ No |
| G5 Comfortable in VR | No C5 ≥ 4 and no session stopped | C5 = 3 and 1; no session stopped | ☐ Yes ☐ Partly ☐ No |
| G6 Chest controller feels natural | Mean C6 ≥ 4 | C6 = 4 (P2 only); P2 needed a hint on which controller | ☐ Yes ☐ Partly ☐ No |
| G7 Would use it again | Most say they would | P2: yes (P1 not asked) | ☐ Yes ☐ Partly ☐ No |

**How far to trust these results:** the reported sample is small (2 people so far), one team member ran and observed every session, heart rate comes from a smartwatch, and each participant used a different build. These results show trends and usability problems, not clinical effectiveness. Details in [`TestingProtocol.md`](TestingProtocol.md), Section 9.

---

## 5. Improvements made

Every row links a finding to its evidence and to the change that was made.

> **Preliminary.** Findings come from P1 and P2 only. *Change implemented*, *Where in the project* and *Re-test result* are filled in once a change exists. Severities are proposed.

**Severity scale:** 0 = not a problem · 1 = cosmetic · 2 = minor · 3 = major (fix before release) · 4 = blocker.

| ID | Finding (what happened) | Evidence | Severity | Change implemented | Where in the project | Re-test result |
|---|---|---|---|---|---|---|
| F1 | P1 skipped the initial instructions and needed a hint. P1 did not understand the clouds, rated C2 = 2 and noticed the weather reacting only in the last seconds of T2 | 4.1 (1.1, 2.4) · 4.5 (C2) · 4.6 (O1, O3, O4, O6, O7) · P1 | 3 (proposed) | | | |
| F2 | P1 pressed the buttons of both menus twice by accident, because they pressed too fast | 4.1 errors · P1 | 2 (proposed) | | | |
| F3 | P1 felt mild dizziness and a slightly blurry image at the end of the session, while the clouds were moving; the headset may not have been fitted well | 4.5 (C5 = 3) · 4.6 (O5) · P1 | 2 (proposed) | | | |
| F4 | P2 did not know which controller goes on the chest and needed a hint; moving it to get comfortable restarted the tracking | 4.1 (2.2, errors) · 4.6 (O12) · P2 | 2 (proposed) | | | |
| F5 | P2 had no feedback in the environment that their breathing was being detected; the only feedback was an on-screen sensor label with numbers | 4.6 (O9, O10, O11) · P2 | 3 (proposed) | Reactive weather finished: clouds darken and come closer when breathing is agitated, and clear and move away as it calms. The sensor label was removed | [`04dc20f`](https://github.com/JpSotoSanchez/CalmWoods/commit/04dc20f) (`WeatherController.cs`, `CalmScoreCalculator.cs`) · [`bd448e3`](https://github.com/JpSotoSanchez/CalmWoods/commit/bd448e3ae4c7b97be729d471a22159f7f81b043c) (label removed) | P1: the weather reacted and helped (C3 = 4), but P1 understood it only at the end (C2 = 2) → leads to F1 |

### Iteration log (before → after)

| Commit | Date | Changes | Motivated by |
|---|---|---|---|
| between `815b5c7` and `40db2f4` (tested with P2) | 22/11/2026 | Build with tutorial, breathing guide and breathing tracking (chest controller + microphone); no reactive weather yet | – |
| [`04dc20f`](https://github.com/JpSotoSanchez/CalmWoods/commit/04dc20f) | 22/11/2026 | Reactive weather finished (clouds respond to the calm score). It was already planned in the proposal and started in [`0b4ae70`](https://github.com/JpSotoSanchez/CalmWoods/commit/0b4ae70); P2's session confirmed it was needed | F5 |
| [`bd448e3`](https://github.com/JpSotoSanchez/CalmWoods/commit/bd448e3ae4c7b97be729d471a22159f7f81b043c) (tested with P1) | 22/11/2026 | On-screen sensor label removed | F5 |
| `_______` | | | F1 / F2 / F3 / F4 |

**Re-test:** ☐ Done with ___ participants ☐ Not possible (reason: ____________) · **Result:** ________________________________