# User Testing and Iteration — Calm Woods

| | |
|---|---|
| **Project** | Calm Woods: a procedurally generated VR forest for stress management and guided breathing |
| **Course** | Human Computer Interaction — Final Project |
| **Platform tested** | Meta Quest 2 · Fitbit Inspire 3 (heart rate) |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |
| **Commit tested** | P1: [`bd448e3`](https://github.com/JpSotoSanchez/CalmWoods/commit/bd448e3ae4c7b97be729d471a22159f7f81b043c) · P2: [`36e5a79`](https://github.com/JpSotoSanchez/CalmWoods/commit/36e5a7954c37914d01b84840f7c3ad9756d74e79) (run 1, no sound) and [`0b34998`](https://github.com/JpSotoSanchez/CalmWoods/commit/0b34998ae3b90ce611e1d88032f518c39d921ea6) (run 2, with sound) · P3: [`bd448e3`](https://github.com/JpSotoSanchez/CalmWoods/commit/bd448e3ae4c7b97be729d471a22159f7f81b043c) with the weather parameters adjusted for each run |
| **Test dates** | 22/11/2026 (P1) · 23/11/2026 (P2, P3) |

> **Scope of this file.** It reports **who tested** Calm Woods, **the tasks they completed**, **the results** and **the improvements** we made. How the tests were run (protocol, surveys, observation log, raw data, limitations) is in [`TestingProtocol.md`](TestingProtocol.md). Who the users are and what they need is in [`UserResearch.md`](UserResearch.md).

> **Status (23/11/2026).** Testing is complete with 3 participants. Each participant ran a different comparison. P1 compared the app with a control (sitting quietly). P2 compared a build without sound with a build with sound. P3 compared two weather settings: weather that starts stormy and clears to very calm, and weather that starts at a medium level and clears to much calmer. The changes in Section 5 have not been made yet.

---

## 1. Summary

- Participants from the target audience tested the app on a Meta Quest 2 in a **5-minute session** made of two tasks: **T1**, 3 minutes of free exploration, and **T2**, 2 minutes dedicated only to calming their breathing.
- We measured **task completion** (hints, errors, time), **how they felt before and after** (3 questions, 1–10), **heart rate** (Fitbit Inspire 3), **experience ratings** and **comments**.
- Each participant also did a second condition. P1 sat quietly for 5 minutes **without the app** (control), to see what the app adds beyond resting. P2 repeated the session one hour later, first with a build **without sound** and then with **music and ambient sound**. P3 repeated the session two hours later with two **weather settings**: starting stormy, then starting at a medium level.
- **Across all 5 app sessions:** stress fell from a mean of 5.0 to 4.0, and heart rate fell from a mean of 97.0 to 80.2 bpm. Nobody felt worse after a session. The main problem is the weather feedback: the two participants who had it (P1, P3) rated their understanding 2 and 3 of 5 and only understood it late in the session.
- **P1 (build `bd448e3`):** completed both tasks with 1 hint and 3 minor errors; felt "slightly better" after the app and "the same" after resting; heart rate fell from 95 to 78 bpm with the app and from 95 to 85 at rest. P1 did not understand the clouds and felt mild dizziness at the end.
- **P2 (builds `36e5a79` → `0b34998`, no responsive weather or breathing input yet):** completed T1 unaided but needed 2 hints in T2 and did not follow the breathing guide the first time, because it was shown in English. P2 felt "slightly better" without sound and "much better" with sound; heart rate fell 16 bpm without sound and 30 bpm with sound. Music was P2's favorite part.
- **P3 (build `bd448e3`, weather parameters adjusted):** completed every checkpoint unaided except placing the controller, where P3 first used the right one. P3 felt "the same" with the weather starting stormy and "slightly better" with it starting at a medium level. P3 first thought the weather was random and felt it did not help much. The observer noted that P3 did not relax much.

---

## 2. Who tested the solution

**Target audience:** adults (18+) who feel stress regularly in everyday life, including people with little or no VR experience (see [`UserResearch.md`](UserResearch.md)). Participants are anonymized. Consent was given verbally.

| ID | Age | Occupation | VR experience | Date | Commit tested |
|---|---|---|---|---|---|
| P1 | 19 | Student | A little (a few times) | 22/11/2026 | `bd448e3` |
| P2 | 48 | Childcare worker | Very little (had tried a headset once or twice) | 23/11/2026 | `36e5a79` (run 1) · `0b34998` (run 2) |
| P3 | 59 | Manager at a sports institution | Some (has used VR before and adapts easily) | 23/11/2026 | `bd448e3`, weather parameters adjusted (run 1, run 2) |

---

## 3. Tasks completed

The two tasks follow the real main flow of the app: **Main menu → Setup → Forest → Breathing exercise (B button) / Options menu (Y button)**.

| Task | Duration | What the participant was told | Success = |
|---|---|---|---|
| **T1 – Free exploration** | 3 min | "Explore the app however you like. Get to know it." | Starts the session from the main menu, chooses a time of day, adjusts at least one volume slider and enters the forest, all without a hint |
| **T2 – Calm your breathing** | 2 min | "Now dedicate yourself exclusively to calming your breathing. Please stay silent." | Opens the breathing menu, places the controller on the chest, follows the inhale / hold / exhale guide for the full 2 minutes |

An observer scored 10 checkpoints (5 per task) for each participant. The full checkpoint list, the surveys and the protocol are in [`TestingProtocol.md`](TestingProtocol.md).

**Builds tested by P2.** Both builds are from 21/11/2026, before the build P1 tested. `36e5a79` has the menus, volume sliders and breathing guide but no audio. `0b34998` is the next commit and adds the music playlist and the ambient soundscape (`Assets/CalmWoods/Scripts/Audio/`). In both builds the weather did not react to breathing yet (the responsive climate came later in `04dc20f`), and the chest controller did not drive anything. For P2, checkpoint 2.4 and items C2, C3 and D1 therefore do not apply, and 2.2 only scores where the controller was placed.

**Build tested by P3.** P3 used the latest build (`bd448e3`, the same one P1 used), with the chest controller and the microphone as inputs. Before each run the weather parameters were adjusted in Unity: in run 1 the weather started stormy and could clear to very calm, and in run 2 it started at a medium level and could clear to much calmer. These parameter changes were not committed, and the exact values were not recorded.

---

## 4. Results

### 4.1 Task completion and errors

✔ = done unaided · ◐ = done with a hint · ✘ = failed · n/a = does not apply. P1 times are approximate: about 7 s between consecutive checkpoints (range 6–8 s). P2 and P3 times are the seconds from the start of T1 or T2 when the checkpoint was reached. The P2 and P3 columns are the first run; notes about the second run are marked "run 2".

| Checkpoint | P1 | P2 | P3 |
|---|---|---|---|
| 1.1 Start from the main menu | ◐ ≈7 s (hint: go back to the menu to read the instructions again) | ✔ 5 s (said they liked it) | ✔ 3 s (knew how to move around) |
| 1.2 Choose a time of day | ✔ ≈7 s | ✔ 20 s (took a while to get used to pointing and selecting) | ✔ 10 s (chose Night quickly and moved on) |
| 1.3 Adjust the volume | ✔ ≈7 s | ✔ 25 s (run 2: said they liked the sound) | ✔ 15 s (ambient sound at medium, no music) |
| 1.4 Enter the forest | ✔ ≈7 s | ✔ 35 s (faster in run 2) | ✔ 20 s |
| 1.5 Look around comfortably | ✔ | ✔ 50 s (took a while to get used to the headset) | ✔ 30 s (used the headset easily) |
| 2.1 Open the breathing menu | ✔ ≈7 s | ◐ 27 s (hint: how to open the menu) | ✔ 10 s |
| 2.2 Place the controller on the chest | ✔ ≈7 s | ◐ 36 s (hint: it is the left controller) | ✘ 15 s (first used the right controller) |
| 2.3 Follow the breathing guide | ✔ ≈7 s | ✘ 45 s (did not follow it in run 1 because the guide was in English; each step was explained before run 2) | ✔ 25 s |
| 2.4 Notice the weather reacting | ✔ late: only in the last seconds of T2 | n/a (no responsive weather in this build) | ✔ 50 s |
| 2.5 Stay silent and still | ✔ | ✔ (apart from asking whether they were doing it right) | ✔ (no comments) |
| **Unaided / with hint / failed / n/a** | 9 / 1 / 0 / 0 | 6 / 2 / 1 / 1 | 9 / 0 / 1 / 0 |
| **Hints / errors** | 1 / 3 | 2 / 1 | 0 / 1 |
| **Crash or tracking lost** | No | No | No |

**Errors observed (P1):** pressed the audio-menu button twice by accident; pressed the breathing-menu button twice by accident (both because they pressed too fast); skipped the instructions the first time the app opened.

**Errors observed (P2):** did not follow the inhale / hold / exhale guide in run 1, because the labels were in English. Before run 2 the facilitator explained what each step meant. This explanation is not counted as a hint.

**Errors observed (P3):** placed the right controller on the chest first, instead of the left one.

### 4.2 How participants felt (before → after the app)

Scale 1–10, lower is better. P2 and P3 have one column per run. The mean is over all 5 app sessions.

| Item | P1 | P2 run 1 (no sound) | P2 run 2 (with sound) | P3 run 1 (weather starts stormy) | P3 run 2 (weather starts medium) | Mean before → after (Δ) |
|---|---|---|---|---|---|---|
| Q1 Stress | 7 → 5 (−2) | 4 → 3 (−1) | 5 → 3 (−2) | 5 → 5 (0) | 4 → 4 (0) | 5.0 → 4.0 (−1.0) |
| Q2 Body tension | 5 → 4 (−1) | 4 → 3 (−1) | 5 → 2 (−3) | 7 → 6 (−1) | 6 → 5 (−1) | 5.4 → 4.0 (−1.4) |
| Q3 Restless thoughts | 6 → 4 (−2) | 2 → 2 (0) | 6 → 2 (−4) | 8 → 6 (−2) | 3 → 3 (0) | 5.0 → 3.4 (−1.6) |
| Overall change (reported) | Slightly better | Slightly better | Much better | The same | Slightly better | 3 slightly better · 1 much better · 1 the same |

### 4.3 Heart rate

Fitbit Inspire 3, read every 30 s. "End" is the reading at minute 5. The full series is in [`TestingProtocol.md`](TestingProtocol.md), Section 8.2.

| Session | Baseline | Mean T1 | Mean T2 | End (5:00) | Post (+1 min) | Δ end − baseline |
|---|---|---|---|---|---|---|
| P1 | 95 | 89.3 | 80.0 | 78 | 79 | −17 |
| P2 run 1 (no sound) | 94 | 91.0 | 80.5 | 78 | 76 | −16 |
| P2 run 2 (with sound) | 102 | 92.7 | 75.0 | 72 | 73 | −30 |
| P3 run 1 (weather starts stormy) | 104 | 96.3 | 87.8 | 87 | 86 | −17 |
| P3 run 2 (weather starts medium) | 90 | 89.8 | 87.8 | 86 | 85 | −4 |
| **Mean (5 sessions)** | 97.0 | 91.8 | 82.2 | 80.2 | 79.8 | −16.8 |

P3 drank coffee about 2 hours before run 1, which may explain part of that run's high baseline (104 bpm).

### 4.4 Comparisons

#### App vs. sitting quietly (control, P1)

Control: 5 minutes seated without the app, with the same questions before and after and the same heart-rate readings. Only P1 did the control; P2 and P3 did a second app run instead (see below).

| Metric | P1 app | P1 control |
|---|---|---|
| Q1 stress, before → after (Δ) | 7 → 5 (−2) | 7 → 6 (−1) |
| Q2 body tension (Δ) | 5 → 4 (−1) | 5 → 6 (+1) |
| Q3 restless thoughts (Δ) | 6 → 4 (−2) | 6 → 6 (0) |
| Overall change | Slightly better | The same |
| Heart rate, baseline → end (Δ) | 95 → 78 (−17) | 95 → 85 (−10) |

**Reading of P1:** the app improved every measure more than resting alone (stress −2 vs. −1, body tension −1 vs. +1, thoughts −2 vs. 0, heart rate −17 vs. −10 bpm). Resting also lowered stress and heart rate, so part of the change comes from sitting quietly. P1 did the control first and the app 2 hours later, so order effects cannot be separated, and this is one participant.

#### Without sound vs. with sound (P2)

Same session (T1 + T2, 5 minutes) with two builds, one hour apart: run 1 without sound, then run 2 with music and ambient sound.

| Metric | Run 1: no sound (`36e5a79`) | Run 2: with sound (`0b34998`) |
|---|---|---|
| Q1 stress, before → after (Δ) | 4 → 3 (−1) | 5 → 3 (−2) |
| Q2 body tension (Δ) | 4 → 3 (−1) | 5 → 2 (−3) |
| Q3 restless thoughts (Δ) | 2 → 2 (0) | 6 → 2 (−4) |
| Overall change | Slightly better | Much better |
| Heart rate, baseline → end (Δ) | 94 → 78 (−16) | 102 → 72 (−30) |
| Mean HR during T2 | 80.5 | 75.0 |

**Reading of P2:** the build with sound improved every measure more than the build without sound, and P2 named music as the most helpful part (D2, D5). Three things limit this result. First, P2 started run 2 more stressed (5 / 5 / 6 vs. 4 / 4 / 2) and with a higher heart rate (102 vs. 94 bpm), so there was more room to drop; after both runs P2 ended at similar levels (stress 3 in both, 78 vs. 72 bpm). Second, run 2 always came second, so P2 already knew the controls and had the breathing guide explained, and this learning cannot be separated from the effect of the sound. Third, this is one participant.

#### Weather starting stormy vs. starting medium (P3)

Same session (T1 + T2, 5 minutes) on `bd448e3`, two hours apart, with the weather parameters adjusted before each run: run 1 started stormy and could clear to very calm; run 2 started at a medium level and could clear to much calmer.

| Metric | Run 1: starts stormy | Run 2: starts medium |
|---|---|---|
| Q1 stress, before → after (Δ) | 5 → 5 (0) | 4 → 4 (0) |
| Q2 body tension (Δ) | 7 → 6 (−1) | 6 → 5 (−1) |
| Q3 restless thoughts (Δ) | 8 → 6 (−2) | 3 → 3 (0) |
| Overall change | The same | Slightly better |
| Heart rate, baseline → end (Δ) | 104 → 87 (−17) | 90 → 86 (−4) |
| Mean HR during T2 | 87.8 | 87.8 |

**Reading of P3:** neither setting was clearly better. With the weather starting stormy, restless thoughts and heart rate dropped more, but P3 reported feeling "the same". With the weather starting at a medium level, the scores barely moved, but P3 reported feeling "slightly better". The large heart-rate drop in run 1 is hard to attribute to the app: P3 had coffee about 2 hours before run 1 and started at 104 bpm, while both runs reached the same mean during T2 (87.8 bpm). Run 2 also started calmer, so there was less room to improve, and it always came second. P3 rated the weather's help as 3 of 5 and said it did not help much (D6). Parts C and D were asked once, after both runs, so they cannot say which setting P3 preferred.

### 4.5 Experience ratings

Scale 1–5 (C5: lower is better). P2 and P3 answered once, after both runs. The mean is over the participants who answered (n).

| Item | P1 | P2 | P3 | Mean (n) |
|---|---|---|---|---|
| C1 The breathing guide was easy to follow | 5 | 5 | 5 | 5.0 (3) |
| C2 I understood how my breathing affected the weather | 2 | n/a (no responsive weather) | 3 | 2.5 (2) |
| C3 The weather changes helped me breathe more calmly | 4 | n/a (no responsive weather) | 3 | 3.5 (2) |
| C4 The menus were easy to use | 5 | 4 | 4 | 4.3 (3) |
| C5 Dizziness, nausea or eye strain | 3 | 2 (slightly blurry vision; lens spacing was set wrong and was corrected before continuing) | 1 | 2.0 (3) |
| C6 Resting the controller on my chest was comfortable | not asked | 4 (even though the chest input did not react yet) | 5 | 4.5 (2) |

### 4.6 What participants said

P2's and P3's answers were given in Spanish and are translated here. The originals are in [`TestingProtocol.md`](TestingProtocol.md), Section 8.1.

| ID | Participant | Quote or observation | Theme |
|---|---|---|---|
| O1 | P1 | "At the end, I understood that the weather was responsive" | Understanding the weather |
| O2 | P1 | "The comfy place" (most pleasant part) | Atmosphere and comfort |
| O3 | P1 | "I didn't understand the clouds" | Understanding the weather |
| O4 | P1 | "Maybe instructions for the weather" (what to change) | Onboarding |
| O5 | P1 | Mild dizziness at the end of the session, with the clouds moving. Afterwards: the image looked a little blurry and the headset may not have been fitted well | VR comfort and headset fit |
| O6 | P1 | Skipped the instructions when the app first opened and needed a hint to go back to them | Onboarding |
| O7 | P1 | Noticed the weather reacting only in the last seconds of T2 | Understanding the weather |
| O8 | P2 | "The music part helped me a lot, and it's an improvement" (most helpful part). During the session: the sound is very good | Music and sound |
| O9 | P2 | "Music, but I also liked being able to change the time of day" (favorite option) | Music and sound |
| O10 | P2 | "I still don't know whether I was breathing correctly, and that didn't give me the best feeling, but the UI is useful as a guide" | Breathing feedback |
| O11 | P2 | "Something to measure the breathing" (what to change) | Breathing feedback |
| O12 | P2 | Asked during T2 whether they were doing it right | Breathing feedback |
| O13 | P2 | "The headset is heavy and somewhat uncomfortable" | VR comfort and headset fit |
| O14 | P2 | Slightly blurry vision because the lens spacing was set wrong; corrected before continuing | VR comfort and headset fit |
| O15 | P2 | Took a while to get used to pointing and selecting (1.2) and to the headset (1.5); faster in run 2 | First-time VR |
| O16 | P2 | Needed hints to open the breathing menu and to know that the left controller goes on the chest (2.1, 2.2) | Onboarding |
| O17 | P2 | Did not follow the breathing guide in run 1 because it was in English (2.3) | Language |
| O18 | P2 | "Yes, for a while, if I have 5 or 10 minutes to go to that calm place" (would use it again) | Would use it again |
| O19 | P2 | Observer: P2 looked relaxed by the end of the session | Atmosphere and comfort |
| O20 | P3 | "After a while I saw that it depended on how I felt or breathed, although at first I thought it was random" (what the weather was telling them) | Understanding the weather |
| O21 | P3 | "I didn't understand the weather at the start" | Understanding the weather |
| O22 | P3 | "Maybe some indication of the weather, besides the clouds, would be good" (what to change) | Onboarding |
| O23 | P3 | Noticed the weather reacting 50 s into T2 (2.4) | Understanding the weather |
| O24 | P3 | "Seeing the forest itself" (most pleasant part) | Atmosphere and comfort |
| O25 | P3 | "I liked the time-of-day option" (favorite option). Chose Night quickly, set ambient sound to medium and turned no music on | Options |
| O26 | P3 | "Yes, it's interesting and the app felt safe, although I think the weather didn't help me much" (would use it again) | Would use it again |
| O27 | P3 | Placed the right controller on the chest first (2.2) | Chest controller |
| O28 | P3 | Knew how to move around and used the headset easily (1.1, 1.5) | VR experience |
| O29 | P3 | Observer: P3 did not relax much. P3 commented that maybe an app is not what works best for them | Fit with the user |
| O30 | P3 | Said the sound is very good | Music and sound |

### 4.7 Did we meet our goals?

The criteria were set before testing (see [`TestingProtocol.md`](TestingProtocol.md), Section 1). Means for G2 and G3 are over all 5 app sessions.

| Goal | Criterion | Result (P1, P2, P3) | Met? |
|---|---|---|---|
| G1 Main flow without help | Everyone completes T1 and T2; on average ≤ 1 hint | Everyone completed T1. Mean 1.0 hint (1, 2, 0). But T2 was not completed unaided by everyone: P2 did not follow the guide in run 1 and P3 first placed the wrong controller | ☐ Yes ☒ Partly ☐ No |
| G2 Less stress after a session | Mean Q1 lower after the session | Mean Q1 5.0 → 4.0. Nobody felt worse; P3 felt "the same" once and Q1 did not change in either of P3's runs | ☒ Yes ☐ Partly ☐ No |
| G3 Lower heart rate | Mean end HR ≤ mean baseline | Mean 97.0 → 80.2 bpm; lower in all 5 sessions (−4 to −30) | ☒ Yes ☐ Partly ☐ No |
| G4 Weather feedback understood | Mean C2 ≥ 4 and most link weather and breathing without a hint | Mean C2 = 2.5 (P1 2, P3 3; P2 had no responsive weather). Both understood it only late in the session | ☐ Yes ☐ Partly ☒ No |
| G5 Comfortable in VR | No C5 ≥ 4 and no session stopped | C5 = 3, 2, 1; no session stopped | ☒ Yes ☐ Partly ☐ No |
| G6 Chest controller feels natural | Mean C6 ≥ 4 and most place it correctly without a hint | Mean C6 = 4.5 (P2, P3; P1 not asked). Only P1 placed it correctly without help: P2 needed a hint and P3 used the wrong controller first | ☐ Yes ☒ Partly ☐ No |
| G7 Would use it again | Most say they would | P2 and P3 said yes (P1 not asked). P3 added that the weather did not help much | ☒ Yes ☐ Partly ☐ No |

**How far to trust these results:** the sample is small (3 people), one team member ran and observed every session, and heart rate comes from a smartwatch. Each participant did a different comparison, always in a fixed order, so each comparison rests on one person. P2 tested older builds than P1 and P3, and P3 had coffee before run 1. These results show trends and usability problems, not clinical effectiveness. Details in [`TestingProtocol.md`](TestingProtocol.md), Section 9.

---

## 5. Improvements made

Every row links a finding to its evidence and to the change that was made.

> **Status.** F1–F8 come from all three participants. *Change implemented*, *Where in the project* and *Re-test result* are filled in once a change exists. Severities are proposed.

**Severity scale:** 0 = not a problem · 1 = cosmetic · 2 = minor · 3 = major (fix before release) · 4 = blocker.

| ID | Finding (what happened) | Evidence | Severity | Change implemented | Where in the project | Re-test result |
|---|---|---|---|---|---|---|
| F1 | The weather feedback is understood only late. P1 skipped the initial instructions, did not understand the clouds (C2 = 2) and noticed the weather reacting only in the last seconds of T2. P3 thought the weather was random at first, did not understand it at the start (C2 = 3) and noticed it 50 s into T2. Both asked for some indication of what the weather means | 4.1 (1.1, 2.4) · 4.5 (C2) · 4.6 (O1, O3, O4, O6, O7, O20–O23) · P1, P3 | 3 (proposed) | | | |
| F2 | P1 pressed the buttons of both menus twice by accident, because they pressed too fast | 4.1 errors · P1 | 2 (proposed) | | | |
| F3 | Headset fit affected comfort. P1 felt mild dizziness and a slightly blurry image at the end of the session, while the clouds were moving, and the headset may not have been fitted well. P2 had slightly blurry vision because the lens spacing was set wrong (corrected during the session) and found the headset heavy and somewhat uncomfortable | 4.5 (C5 = 3 P1, 2 P2) · 4.6 (O5, O13, O14) · P1, P2 | 2 (proposed) | | | |
| F4 | It is not clear which controller goes on the chest. P2 needed a hint to know it was the left controller, and P3 placed the right one first, even with the tutorial in the build. Only P1 placed it correctly without help. P2 also needed a hint to open the breathing menu, but P1 and P3, whose builds had the tutorial (`815b5c7` onwards), opened it unaided | 4.1 (2.1, 2.2) · 4.6 (O16, O27) · P2, P3 | 2 (proposed) | | | |
| F5 | The breathing guide is only in English. P2 did not follow it in run 1 and needed each step explained | 4.1 (2.3) · 4.6 (O17) · P2 | 3 (proposed) | | | |
| F6 | P2 did not know whether they were breathing correctly, which made them uneasy, and asked for something that measures breathing. P2's builds had no working breathing input; in the later build the weather gives this feedback, but P1 and P3 understood it late (F1) | 4.1 (2.5) · 4.6 (O10, O11, O12) · P2 (links to F1) | 3 (proposed) | | | |
| F7 | Positive finding: the build with music and ambient sound improved every measure more than the build without sound, and music was P2's favorite part | 4.2 · 4.3 · 4.4 (P2) · 4.6 (O8, O9, O30) · P2 | 0 (keep) | None needed: music and ambient sound are in every build from `0b34998` on, including `bd448e3` | `Assets/CalmWoods/Scripts/Audio/` | |
| F8 | The weather did not help P3 calm down (C3 = 3; "the weather didn't help me much"), and P3 did not relax much. Starting the weather stormy or at a medium level gave mixed results, with no clear preference. P3 also said that maybe an app is not what works best for them | 4.2 · 4.3 · 4.4 (P3) · 4.5 (C3) · 4.6 (O26, O29) · P3 | 2 (proposed) | | | |

### Iteration log (before → after)

| Commit | Date | Changes | Motivated by |
|---|---|---|---|
| `36e5a79` (tested with P2, run 1) | 21/11/2026 | Build without music or ambient sound; weather not yet responsive | – |
| `0b34998` (tested with P2, run 2) | 21/11/2026 | Adds the music playlist and the ambient soundscape | – |
| `bd448e3` (tested with P1, and with P3 in two weather settings) | 22/11/2026 | Responsive weather and tutorial. For P3 the weather parameters were adjusted in Unity for each run; those values were not committed | – |
| `_______` | | | F__ |
| `_______` | | | F__ |

**Re-test:** ☐ Done with ___ participants ☐ Not possible (reason: ____________) · **Result:** ________________________________
