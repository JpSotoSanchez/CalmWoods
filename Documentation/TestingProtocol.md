# Testing Protocol, Instruments and Raw Data — Calm Woods

| | |
|---|---|
| **Project** | Calm Woods: a procedurally generated VR forest for stress management and guided breathing |
| **Course** | Human Computer Interaction — Final Project |
| **Platform** | Meta Quest 2 / Meta Quest 3 · Unity |
| **Hardware used in testing** | Meta Quest 2 · Fitbit Inspire 3 (all sessions) |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |

> **Scope of this file.** It holds how the tests were run and the evidence behind them: goals, protocol, surveys, observation log, raw data, limitations and the Spanish versions given to participants. The participants, tasks, results and improvements are in [`UserTesting.md`](UserTesting.md).

---

## 1. Goals and success criteria

Testing is not a final approval step: it is evidence that must change the app. We decided what "success" means **before** running any session.

| # | Question | Metric | Success criterion |
|---|---|---|---|
| G1 | Can users complete the main flow without help? | Checkpoint completion, facilitator hints, errors, time per task | Every participant completes T1 and T2; on average ≤ 1 hint per participant |
| G2 | Do users feel less stressed after a session? | Pre vs. post scores for Q1–Q3 (1–10) and the overall-change item | Mean Q1 (stress) is lower after the session; any participant who feels worse is analyzed individually |
| G3 | Is physiological arousal lower after the session? | Smartwatch heart rate: baseline vs. end; exploration vs. breathing phase | Mean end HR ≤ mean baseline HR |
| G4 | Do users understand the weather feedback? | Post item C2 (1–5) + open answer D1 + observation | Mean C2 ≥ 4 and most participants link weather ↔ breathing without a hint |
| G5 | Is the experience comfortable in VR? | Post item C5 (1–5, lower is better) + observed discomfort | No participant scores C5 ≥ 4 and no session is stopped for discomfort |
| G6 | Is resting the controller on the chest natural, or awkward? | Post item C6 (1–5) + observation 2.2 | Mean C6 ≥ 4 and most participants place it correctly without a hint |
| G7 | Would users use the app again? | Open answer D6 | Most participants say they would use it again |

---

## 2. Participants and roles

**Target audience:** adults (18+) who feel stress regularly in everyday life, including at least one or two people with no VR experience (same definition as `UserResearch.md`).

**Target number:** the plan allowed 3–5 participants; the **final sample is 3**.

**Inclusion:** 18 or older · belongs to the target audience · gives informed consent.
**Exclusion:** history of seizures or photosensitive epilepsy · severe motion sickness · a respiratory condition that makes breathing exercises uncomfortable · feels unwell on the test day.

Participants are anonymized (P1, P2, P3). No names are stored. If a participant was also interviewed for `UserResearch.md`, they keep the same ID. Consent was given verbally in every session (Appendix B).

**Team roles:** in every session one team member, José Pablo Soto Sánchez, acted as facilitator, observer and heart-rate recorder.

- **Facilitator** – reads the script, gives hints only when the protocol allows, stays silent during T2.
- **Observer** – fills the observation log (Section 6.3) and times the tasks.
- **HR recorder** – reads the smartwatch every 30 s and fills the heart-rate sheet (Section 8.2). The participant never sees their own HR.

---

## 3. Test setup

- **Hardware:** Meta Quest 2 (headset + controllers) and Fitbit Inspire 3 smartwatch, in all sessions.
- **Breathing input used:** P1: controller on the chest **and** microphone together · P2: none (the builds tested had no working breathing input; the left controller was placed on the chest but did not drive anything) · P3: controller on the chest **and** microphone together
- **Position:** seated, comfortable posture, controller/hand resting on the chest for T2.
- **Commit tested:** P1: `bd448e3` · P2: `36e5a79` (run 1, no sound) and `0b34998` (run 2, with music and ambient sound) · P3: `bd448e3`, with the weather parameters adjusted in Unity before each run (not committed; values not recorded)

---

## 4. Tasks

The two tasks follow the real main flow: **Main menu → Setup → Forest → Breathing exercise (B button) / Options menu (Y button)**. A 5-minute session total, as stated in the project proposal.

| Task | Duration | What the participant is told | Success = |
|---|---|---|---|
| **T1 – Free exploration** | 3 min | "Explore the app however you like. Get to know it." *(Think-aloud allowed.)* | Starts the session from the main menu, chooses a time of day, adjusts at least one volume slider and enters the forest, all without a hint |
| **T2 – Calm your breathing** | 2 min | "Now dedicate yourself exclusively to calming your breathing. Please stay silent." | Opens the breathing menu, places the controller on the chest, follows the inhale / hold / exhale guide for the full 2 minutes |

---

## 5. Protocol (≈ 20 min per participant, ≈ 23 min with the research interview)

| Step | Time | What happens | Data |
|---|---|---|---|
| 1 | 2 min | Welcome, explain the study (Appendix B text), answer questions | Verbal consent |
| 2 | 3 min | *(Only if the participant was not interviewed before)* **Research interview** R1–R5 | `UserResearch.md`, Sections 3 and 6 |
| 3 | 2 min | **Pre-session survey** (3 questions + background) | Section 6.1 |
| 4 | 1 min | Put on smartwatch; sit still and quiet to record **baseline HR** | HR sheet |
| 5 | 1 min | Put on headset, adjust fit, place controllers. Short explanation of controls only | – |
| 6 | 3 min | **T1 – Free exploration** | Observation log, HR every 30 s |
| 7 | 2 min | **T2 – Calm your breathing** (breathing menu + weather feedback active) | Observation log, HR every 30 s |
| 8 | 1 min | Remove headset; sit still to record **post HR** | HR sheet |
| 9 | 3 min | **Post-session survey** | Section 6.2 |
| 10 | 3 min | Short interview: "What did you think the weather was telling you?", "What would you change?" | Quotes |

**Facilitator script (read exactly):**

> "We are testing the app, not you; there are no right or wrong answers. You can stop at any time. For the first 3 minutes, explore the app however you like. I will tell you when the 3 minutes are over. Then I will ask you to dedicate the next 2 minutes only to calming your breathing."

> At minute 3: "Now, for 2 minutes, focus only on calming your breathing. Please try not to speak."

**Hint rule:** wait at least 10 seconds of visible difficulty before helping. Log every hint.
**Stop criteria:** dizziness, nausea, eye strain, anxiety, or the participant asks to stop → remove the headset immediately, make sure they are comfortable, and record it.
**Note:** if the microphone is the breathing input, nobody talks during T2 (speech corrupts the signal).

### Deviations from the plan (actual sessions)

- The final sample is 3 participants (planned 3–5).
- Consent was given **verbally**; no signed form was collected.
- **Control condition:** P1 first sat quietly for 5 minutes without the app, and used the app 2 hours later. The order was not counterbalanced for P1. P2: **no control**. P2 did the full app session twice, with two builds one hour apart: run 1 without sound (`36e5a79`), then run 2 with music and ambient sound (`0b34998`), to see which version the user prefers. The order was not counterbalanced. · P3: **no control**. P3 did the full app session twice on `bd448e3`, two hours apart, with two weather settings: run 1 with the weather starting stormy and able to clear to very calm, then run 2 with the weather starting at a medium level and able to clear to much calmer. The order was not counterbalanced.
- P3's weather parameters were changed in the Unity Inspector before each run. The changes were not committed and the exact values were not recorded.
- P3 drank coffee about 2 hours before run 1 (no caffeine or exercise before run 2).
- P3 answered Survey 1 and Survey 2 Parts A and B before and after each run, and Parts C and D once, after both runs.
- P1 answered the earlier version of Survey 2 (items C1–C5 and open questions D1–D4), so **C6, D5 and D6 were not collected for P1** and G6 and G7 have no data for P1.
- **P2 tested earlier builds than P1.** In both P2 builds the weather did not react to breathing yet (the responsive climate came in `04dc20f`), so checkpoint 2.4 and items C2, C3 and D1 do not apply to P2, and G4 has no data from P2.
- P2 answered Survey 1 and Survey 2 Parts A and B before and after **each** run, and Parts C and D **once**, after both runs.
- The breathing guide was shown in English. P2 did not follow it in run 1, and the facilitator explained each step before run 2.
- P2's headset lens spacing was set wrong at first (slightly blurry vision). It was corrected before the test continued.
- The app has **no session summary screen** (planned in the project proposal, Section 4.3), so there is no app-reported breathing rate or time in calm state.

---

## 6. Data-collection instruments

### 6.1 Survey 1 — Before the session (3 questions)

**Background (not part of the 3 questions)**
Participant ID: ______ · Age range: ☐ 18–20 ☐ 21–23 ☐ 24–26 ☐ 27+
VR experience: ☐ Never ☐ A few times ☐ Often
Caffeine or intense exercise in the last 2 hours: ☐ No ☐ Yes

**Answer about how you feel *right now*, at this moment.**

**Q1. How stressed do you feel right now?**
1 = completely calm · 10 = extremely stressed
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Q2. How tense does your body feel right now (shoulders, jaw, chest)?**
1 = not tense at all · 10 = extremely tense
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Q3. How restless or racing are your thoughts right now?**
1 = calm and clear · 10 = extremely restless
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

*(Q1 measures overall stress, Q2 the body, Q3 the mind. They are repeated in Survey 2 with identical wording so the change can be compared.)*

### 6.2 Survey 2 — After the session

**Part A — How do you feel right now?** *(same questions as Survey 1)*

**Q1. How stressed do you feel right now?** (1 = completely calm · 10 = extremely stressed)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Q2. How tense does your body feel right now?** (1 = not tense at all · 10 = extremely tense)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Q3. How restless or racing are your thoughts right now?** (1 = calm and clear · 10 = extremely restless)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Part B — Overall change**

**Compared with before the session, I feel…**
☐ Much worse ☐ Slightly worse ☐ The same ☐ Slightly better ☐ Much better

**Part C — Your experience** *(1 = strongly disagree · 5 = strongly agree)*

| | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|
| **C1.** The breathing guide (inhale / hold / exhale) was easy to follow | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C2.** I understood how my breathing affected the weather | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C3.** The weather changes helped me breathe more calmly | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C4.** Moving through the menus (start, time of day, volume, breathing) was easy | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C6.** Resting the controller on my chest was comfortable | ☐ | ☐ | ☐ | ☐ | ☐ |

**C5. Did you feel dizziness, nausea or eye strain?** *(1 = not at all · 5 = very much)*
☐1 ☐2 ☐3 ☐4 ☐5

**Part D — In your own words**

- **D1.** What did you think the weather was telling you? ________________________________
- **D2.** What was the most helpful or pleasant part? ________________________________
- **D3.** What was confusing, annoying or uncomfortable? ________________________________
- **D4.** What would you change? Would you prefer another way to measure your breathing? ________________________________
- **D5.** Which option did you like most: time of day, music, or nature sounds? ________________________________
- **D6.** Would you use it again? When, and for how long? ________________________________

### 6.3 Observation log (one copy per participant)

*(Completed logs are in Section 8.3.)*

**Participant:** P__ · **Observer:** ______ · **Commit:** `_______` · **Input:** ☐ chest controller ☐ microphone ☐ both

**Legend:** ✔ = done unaided · ◐ = done with a hint · ✘ = failed / gave up. **Error** = wrong action or hesitation > 5 s.

| # | Checkpoint | Result (✔ / ◐ / ✘) | Time (s) | Errors / hesitations | Notes and exact quotes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | | | | |
| 1.2 | Chooses a time of day (Dawn / Afternoon / Sunset / Night) | | | | |
| 1.3 | Adjusts music and/or ambient volume | | | | |
| 1.4 | Presses Start and enters the forest | | | | |
| 1.5 | Looks around comfortably (no adjusting the headset, rubbing eyes or removing it) | | | | |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | | | | |
| 2.2 | Places the controller on the chest correctly (note if without prompting) | | | | |
| 2.3 | Follows the inhale / hold / exhale guide | | | | |
| 2.4 | Notices the weather reacting (looks at the sky, smiles, surprise, confusion, comments) | | | | |
| 2.5 | Stays silent and still during the 2 minutes | | | | |

**Totals:** hints given: ___ · errors: ___ · tracking lost / crash: ☐ No ☐ Yes → ________
**Spontaneous comments (with timestamp):** ________________________________
**Emotional reactions / body language observed:** ________________________________

### 6.4 Heart-rate recording

Reading source: Fitbit Inspire 3 worn on the same wrist for every participant. The HR recorder notes the value shown every 30 s: a baseline before VR, readings at 0:30, 1:00 … 5:00 during the session, and a post reading 1 minute after removing the headset. The participant does not look at it. **"End" means the 5:00 reading**, which is comparable to the end of the 5-minute control condition. The data are in Section 8.2.

### 6.5 Validated questionnaires (open decision)

The project proposal mentions a "short validated anxiety questionnaire" and the System Usability Scale (SUS). Decide whether to add them to Surveys 1 and 2:

- **Anxiety:** the recommendation is the **STAI-6 (short state-anxiety form)**, because it measures how a person feels right now, which fits a before/after comparison of a single session. The popular GAD-7 asks about the past two weeks, so it would not change after 5 minutes.
- **Usability:** the **SUS (10 items)**, answered once in Survey 2.

**Decision:** ☐ STAI-6 ☐ SUS ☒ Neither (reason: there was no time for them in P2's sessions, and P3 was not interested in answering them) · No participant answered either questionnaire.

---

## 7. Comparison design (control condition)

A change in stress or heart rate could happen simply from sitting still, so the study includes a comparison, as stated in the project proposal. Options considered were the control, input method (chest controller vs. microphone) and feedback on/off. **We chose the control condition.**

- **Condition A:** Calm Woods session (T1 + T2, 5 minutes).
- **Condition B (control):** sit quietly for 5 minutes with the watch, no app. Same Survey 1 before and Survey 2 (Parts A and B) after, and the same heart-rate readings.

**Order:** P1 B → A (control first, the app 2 hours later; not counterbalanced) · P2 A1 → A2, no B (see below) · P3 A1 → A2, no B (see below)

**P2: sound off vs. sound on.** P2 did not do the control. P2 did Condition A twice with two builds, one hour apart:

- **A1:** build without sound (`36e5a79`).
- **A2:** build with music and ambient sound (`0b34998`), the commit right after A1.

Everything else was the same (T1 + T2, Survey 1 and Survey 2 Parts A and B around each run, the same heart-rate readings). Parts C and D were asked once, after A2. The order was fixed (A1 first), so learning from run 1 cannot be separated from the effect of the sound.

**P3: weather starting stormy vs. starting medium.** P3 did not do the control. P3 did Condition A twice on `bd448e3`, two hours apart, with the weather parameters adjusted in Unity before each run:

- **A1:** the weather starts stormy and can clear to very calm.
- **A2:** the weather starts at a medium level and can clear to much calmer.

Everything else was the same as for P2. Parts C and D were asked once, after A2. The order was fixed (A1 first), and P3 had coffee about 2 hours before A1.

The comparison results are in `UserTesting.md`, Section 4.4.

---

## 8. Raw data

> Real data only. A blank cell means not recorded or not yet collected.

### 8.1 Survey responses

Control = 5 minutes seated without the app (only Survey 1, Survey 2 Part A and Part B were asked). "n/a" = not asked in that condition, or does not apply to the build tested. "not asked" = the participant answered the earlier version of the survey. P2 and P3 have no control: their two columns are two app runs. P2: run 1 = build without sound `36e5a79`, run 2 = build with sound `0b34998`, one hour later. P3: both runs on `bd448e3`, run 1 = weather starts stormy, run 2 = weather starts at a medium level, two hours later. P2 and P3 answered Parts C and D once, after both runs, so those answers are in the run 2 column and run 1 shows "–". P2's and P3's open answers are in the original Spanish.

| Item | P1 app | P1 control | P2 run 1 (no sound) | P2 run 2 (with sound) | P3 run 1 (weather starts stormy) | P3 run 2 (weather starts medium) |
|---|---|---|---|---|---|---|
| Age · VR experience | 19 · a little | same | 48 · very little (had tried a headset once or twice) | same | 59 · some (has used VR before and adapts easily) | same |
| Caffeine or exercise in last 2 h | No | – | No | No | Yes (coffee about 2 h before) | No |
| Survey 1 Q1 / Q2 / Q3 | 7 / 5 / 6 | 7 / 5 / 6 | 4 / 4 / 2 | 5 / 5 / 6 | 5 / 7 / 8 | 4 / 6 / 3 |
| Survey 2 Q1 / Q2 / Q3 | 5 / 4 / 4 | 6 / 6 / 6 | 3 / 3 / 2 | 3 / 2 / 2 | 5 / 6 / 6 | 4 / 5 / 3 |
| Part B overall change | Slightly better | The same | Slightly better | Much better | The same | Slightly better |
| C1 guide | 5 | n/a | – | 5 | – | 5 |
| C2 understood weather | 2 | n/a | – | n/a (no responsive weather) | – | 3 |
| C3 weather helped | 4 | n/a | – | n/a (no responsive weather) | – | 3 |
| C4 menus | 5 | n/a | – | 4 | – | 4 |
| C5 dizziness / eye strain | 3 | n/a | – | 2 | – | 1 |
| C6 controller on chest | not asked | n/a | – | 4 | – | 5 |
| D1 weather telling you | "At the end, I understood that the weather was responsive" | n/a | – | n/a (no responsive weather) | – | "Después de un rato vi que era según cómo me sentía o respiraba, aunque al principio pensé que era aleatorio" |
| D2 most helpful or pleasant | "The comfy place" | n/a | – | "La parte de la música me ayudó mucho y es una mejora" | – | "El ver el bosque como tal" |
| D3 confusing or uncomfortable | "I didn't understand the clouds" | n/a | – | "Todavía no sé si de alguna manera estaba haciendo bien el respirar y eso no me daba la mejor sensación del mundo, pero la UI es útil como guía, y los lentes son pesados y algo incómodos" | – | "No entendí el clima al inicio" |
| D4 what to change | "Maybe instructions for the weather" | n/a | – | "Algo para medir la respiración" | – | "Puede que alguna indicación del clima estuviera bien, además de las nubes" |
| D5 favorite option | not asked | n/a | – | "Música, pero la parte de cambiar el horario también me gustó" | – | "Me gustó la opción del día" |
| D6 use it again | not asked | n/a | – | "Sí, por un rato, si tengo unos 5 o 10 min para irme a ese lugar tranquilo" | – | "Sí, está interesante y se sintió segura la app, aunque el clima creo que no me ayudó mucho" |

Additional comment from P1 (app): "a little dizziness at the end of the session"; afterwards the image looked a little blurry and the headset may not have been fitted as well as possible.

Additional comments from P2: C5 = 2 because of slightly blurry vision, caused by the lens spacing being set wrong; it was corrected before the test continued. During the session P2 said the sound was very good.

Additional comment from P3: after the session P3 commented that maybe an app is not what works best for them.

### 8.2 Heart-rate sheet

| Moment | P1 | P2 run 1 (no sound) | P2 run 2 (with sound) | P3 run 1 (weather starts stormy) | P3 run 2 (weather starts medium) |
|---|---|---|---|---|---|
| Baseline (seated, before VR) | 95 | 94 | 102 | 104 | 90 |
| 0:30 (T1) | 89 | 92 | 102 | 100 | 90 |
| 1:00 (T1) | 92 | 93 | 102 | 99 | 90 |
| 1:30 (T1) | 91 | 93 | 94 | 100 | 90 |
| 2:00 (T1) | 90 | 92 | 90 | 94 | 90 |
| 2:30 (T1) | 89 | 90 | 85 | 93 | 90 |
| 3:00 (T1 ends) | 85 | 86 | 83 | 92 | 89 |
| 3:30 (T2) | 83 | 84 | 80 | 89 | 90 |
| 4:00 (T2) | 80 | 81 | 76 | 87 | 88 |
| 4:30 (T2) | 79 | 79 | 72 | 88 | 87 |
| 5:00 (T2 ends) | 78 | 78 | 72 | 87 | 86 |
| Post (1 min after removing headset) | 79 | 76 | 73 | 86 | 85 |
| **Mean T1** (0:30–3:00) | 89.3 | 91.0 | 92.7 | 96.3 | 89.8 |
| **Mean T2** (3:30–5:00) | 80.0 | 80.5 | 75.0 | 87.8 | 87.8 |
| **Δ** T2 mean − baseline | −15.0 | −13.5 | −27.0 | −16.3 | −2.3 |
| **Δ** 5:00 − baseline | −17 | −16 | −30 | −17 | −4 |
| **Δ** post − baseline | −16 | −18 | −29 | −18 | −5 |
| **Control:** baseline | 95 | not run | not run | not run | not run |
| **Control:** end (5 min) | 85 | not run | not run | not run | not run |
| **Control:** Δ end − baseline | −10 | not run | not run | not run | not run |

### 8.3 Completed observation logs

#### P1

**Participant:** P1 · **Observer:** José Pablo Soto Sánchez · **Commit:** `bd448e3` · **Input:** controller on the chest and microphone together · **Date:** 22/11/2026

| # | Checkpoint | Result | Time (s) | Errors / hesitations | Notes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | ◐ | ≈7 | Skipped the initial instructions | Hint at the start: go back to the menu to see the instructions again |
| 1.2 | Chooses a time of day | ✔ | ≈7 | | |
| 1.3 | Adjusts music and/or ambient volume | ✔ | ≈7 | | |
| 1.4 | Presses Start and enters the forest | ✔ | ≈7 | | |
| 1.5 | Looks around comfortably | ✔ | ≈7 | | |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | ✔ | ≈7 | | |
| 2.2 | Places the controller on the chest correctly | ✔ | ≈7 | | |
| 2.3 | Follows the inhale / hold / exhale guide | ✔ | ≈7 | | |
| 2.4 | Notices the weather reacting | ✔ | last seconds of T2 | | Noticed late |
| 2.5 | Stays silent and still during the 2 minutes | ✔ | – | | |

**Totals:** hints given: 1 · errors: 3 · tracking lost / crash: No
**Errors (3):**
1. Pressed the audio-menu button twice by accident (pressed too fast).
2. Pressed the breathing-menu button twice by accident (pressed too fast).
3. Skipped the instructions on how to use the app the first time it opened.

**App session summary:** not available (the app has no session summary).
**Emotional reactions / body language observed:** not recorded.

#### P2

**Participant:** P2 · **Observer:** José Pablo Soto Sánchez · **Commit:** `36e5a79` (run 1, no sound) · `0b34998` (run 2, with sound) · **Input:** none (no working breathing input in these builds) · **Date:** 23/11/2026

Results and times are for run 1. Times are seconds from the start of T1 or T2. Notes about run 2 are marked.

| # | Checkpoint | Result | Time (s) | Errors / hesitations | Notes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | ✔ | 5 | | Said they liked it |
| 1.2 | Chooses a time of day | ✔ | 20 | | Took a while to get used to pointing and selecting |
| 1.3 | Adjusts music and/or ambient volume | ✔ | 25 | | Run 2: said they liked the sound |
| 1.4 | Presses Start and enters the forest | ✔ | 35 | | Did it better in run 2 |
| 1.5 | Looks around comfortably | ✔ | 50 | | Took a while to get used to the headset |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | ◐ | 27 | | Hint: how to open the menu |
| 2.2 | Places the controller on the chest correctly | ◐ | 36 | | Hint: it is the left controller (chest input not responsive in this build) |
| 2.3 | Follows the inhale / hold / exhale guide | ✘ | 45 | Did not follow the guide in run 1 | The guide was in English; each step was explained before run 2 |
| 2.4 | Notices the weather reacting | n/a | – | | No responsive weather in this build |
| 2.5 | Stays silent and still during the 2 minutes | ✔ | rest of T2 | | Silent except for asking whether they were doing it right |

**Totals:** done unaided: 6 · with a hint: 2 · failed: 1 · n/a: 1 · hints given: 2 (plus the explanation of the guide between runs) · errors: 1 · tracking lost / crash: No
**Errors (1):**
1. Did not follow the inhale / hold / exhale guide in run 1, because it was shown in English.

**Spontaneous comments:** the sound is very good (no other important comments recorded).
**App session summary:** not available (the app has no session summary).
**Emotional reactions / body language observed:** P2 looked relaxed by the end of the session.

#### P3

**Participant:** P3 · **Observer:** José Pablo Soto Sánchez · **Commit:** `bd448e3`, weather parameters adjusted before each run (run 1: starts stormy · run 2: starts at a medium level) · **Input:** controller on the chest and microphone together · **Date:** 23/11/2026

Results and times are for run 1. Times are seconds from the start of T1 or T2.

| # | Checkpoint | Result | Time (s) | Errors / hesitations | Notes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | ✔ | 3 | | Knew how to move around |
| 1.2 | Chooses a time of day | ✔ | 10 | | Chose Night quickly and moved on |
| 1.3 | Adjusts music and/or ambient volume | ✔ | 15 | | Ambient sound at medium; did not turn music on |
| 1.4 | Presses Start and enters the forest | ✔ | 20 | | |
| 1.5 | Looks around comfortably | ✔ | 30 | | Used the headset easily |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | ✔ | 10 | | No comments |
| 2.2 | Places the controller on the chest correctly | ✘ | 15 | Used the right controller first | No hint recorded |
| 2.3 | Follows the inhale / hold / exhale guide | ✔ | 25 | | Followed it correctly |
| 2.4 | Notices the weather reacting | ✔ | 50 | | |
| 2.5 | Stays silent and still during the 2 minutes | ✔ | rest of T2 | | No comments |

**Totals:** done unaided: 9 · with a hint: 0 · failed: 1 · n/a: 0 · hints given: 0 · errors: 1 · tracking lost / crash: No
**Errors (1):**
1. Placed the right controller on the chest first, instead of the left one.

**Spontaneous comments:** none recorded.
**App session summary:** not available (the app has no session summary).
**Emotional reactions / body language observed:** P3 did not relax much, and commented that maybe an app is not what works best for them.

---

## 9. Limitations

- Small sample (3 people): results show **trends and usability problems**, not statistical or clinical effectiveness.
- One team member was facilitator, observer and heart-rate recorder in every session, so observations come from a single rater and may be biased.
- The control condition was not counterbalanced for P1 (control first, app 2 hours later), so order effects cannot be separated from the effect of the app.
- P1 answered an earlier version of Survey 2 (without C6, D5 and D6), so those items have no data for P1.
- P2 did not do the control. P2 compared a build without sound and a build with sound in a fixed order (no sound first, one hour apart), so learning from run 1 cannot be separated from the effect of the sound. P2 also started run 2 more stressed and with a higher heart rate than run 1.
- P2 tested earlier builds than P1, without responsive weather or a working breathing input, so P2 gives no data on the weather feedback (G4) and the participants did not all test the same app.
- P2 answered Parts C and D once for both runs, so those ratings cannot be split by build.
- P3 did not do the control either. P3 compared two weather settings in a fixed order (stormy start first, two hours apart). P3 drank coffee about 2 hours before run 1, which may have raised that run's baseline heart rate (104 bpm) and exaggerated its drop. The weather parameter values were not recorded or committed, so the two settings cannot be reproduced exactly.
- Each participant did a different comparison (control, sound, weather start), so each comparison rests on a single person.
- The app has no session summary screen, so there is no app-reported breathing rate or time in calm state to compare with the heart rate.
- Fitbit Inspire 3 heart rate is less precise than medical equipment and can change from movement, caffeine or nerves.
- Self-reported scores can be affected by novelty, wanting to please the team, or the presence of an observer.
- Participants are mostly acquaintances (one student aged 19 and two working adults aged 48 and 59); results may not generalize to other groups.
- Calm Woods is a relaxation aid, **not a medical treatment**.

---

## Appendix A — Survey versions in Spanish (for participants)

### Encuesta 1 — Antes de la sesión

**Datos generales:** ID: ______ · Rango de edad: ☐ 18–20 ☐ 21–23 ☐ 24–26 ☐ 27+
Experiencia con realidad virtual: ☐ Ninguna ☐ Algunas veces ☐ Frecuente
Cafeína o ejercicio intenso en las últimas 2 horas: ☐ No ☐ Sí

**Responde pensando en cómo te sientes *ahora mismo*.**

**P1. ¿Qué tan estresado/a te sientes en este momento?**
1 = totalmente tranquilo/a · 10 = extremadamente estresado/a
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**P2. ¿Qué tan tenso sientes tu cuerpo en este momento (hombros, mandíbula, pecho)?**
1 = nada tenso · 10 = extremadamente tenso
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**P3. ¿Qué tan inquietos o acelerados sientes tus pensamientos en este momento?**
1 = calmados y claros · 10 = extremadamente inquietos
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

### Encuesta 2 — Después de la sesión

**Parte A — ¿Cómo te sientes ahora mismo?** *(mismas preguntas que la Encuesta 1)*

**P1. ¿Qué tan estresado/a te sientes en este momento?** (1 = totalmente tranquilo/a · 10 = extremadamente estresado/a)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**P2. ¿Qué tan tenso sientes tu cuerpo en este momento?** (1 = nada tenso · 10 = extremadamente tenso)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**P3. ¿Qué tan inquietos o acelerados sientes tus pensamientos en este momento?** (1 = calmados y claros · 10 = extremadamente inquietos)
☐1 ☐2 ☐3 ☐4 ☐5 ☐6 ☐7 ☐8 ☐9 ☐10

**Parte B — Cambio general**

**En comparación con antes de la sesión, ahora me siento…**
☐ Mucho peor ☐ Un poco peor ☐ Igual ☐ Un poco mejor ☐ Mucho mejor

**Parte C — Tu experiencia** *(1 = totalmente en desacuerdo · 5 = totalmente de acuerdo)*

| | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|
| **C1.** La guía de respiración (inhalar / sostener / exhalar) fue fácil de seguir | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C2.** Entendí cómo mi respiración afectaba el clima | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C3.** Los cambios del clima me ayudaron a respirar con más calma | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C4.** Fue fácil moverme por los menús (inicio, hora del día, volumen, respiración) | ☐ | ☐ | ☐ | ☐ | ☐ |
| **C6.** Fue cómodo apoyar el control en mi pecho | ☐ | ☐ | ☐ | ☐ | ☐ |

**C5. ¿Sentiste mareo, náusea o cansancio en los ojos?** *(1 = nada · 5 = mucho)*
☐1 ☐2 ☐3 ☐4 ☐5

**Parte D — Con tus propias palabras**

- **D1.** ¿Qué pensaste que te estaba diciendo el clima? ________________________________
- **D2.** ¿Qué fue lo más útil o agradable? ________________________________
- **D3.** ¿Qué te pareció confuso, molesto o incómodo? ________________________________
- **D4.** ¿Qué cambiarías? ¿Preferirías otra forma de medir tu respiración? ________________________________
- **D5.** ¿Qué opción te gustó más: la hora del día, la música o los sonidos de la naturaleza? ________________________________
- **D6.** ¿Lo volverías a usar? ¿Cuándo y por cuánto tiempo? ________________________________

---

## Appendix B — Consent text

**English.** *Calm Woods is a university project (Human Computer Interaction). You will use a VR app for about 5 minutes, wear a smartwatch that records your heart rate, and answer two short surveys. Participation is voluntary and you may stop at any time, for any reason, without consequence. Your answers will be anonymized and used only for this course. The app is a relaxation aid, not a medical treatment. If you feel dizzy, nauseous or uncomfortable, tell us and we will stop immediately.*
☐ I understand and agree to participate. · Signature / initials: ______ · Date: ______

**Español.** *Calm Woods es un proyecto universitario (Interacción Humano-Computadora). Usarás una app de realidad virtual durante unos 5 minutos, llevarás un reloj inteligente que registra tu ritmo cardiaco y responderás dos encuestas cortas. La participación es voluntaria y puedes detenerte en cualquier momento, por cualquier motivo, sin ninguna consecuencia. Tus respuestas serán anónimas y se usarán únicamente para este curso. La app es un apoyo para relajarse, no un tratamiento médico. Si sientes mareo, náusea o incomodidad, avísanos y nos detendremos de inmediato.*
☐ Entiendo y acepto participar. · Firma / iniciales: ______ · Fecha: ______

> **In practice:** in every session the study was explained to the participant and consent was given **verbally**; no signed form was collected.
