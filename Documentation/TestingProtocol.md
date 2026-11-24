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

**Target number:** the plan allowed 3–5 participants. **Four sessions were run**, each on a newer build of the app. **P1 and P2 are verified and reported.** P3 and P4 are pending: the build they used has not yet been matched to the repository history, so their data is left out until it is confirmed.

**Numbering:** IDs go in reverse order of testing. The highest ID used the oldest build and P1 used the final build (`bd448e3`). P2 was tested before P1.

**Inclusion:** 18 or older · belongs to the target audience · gives informed consent.
**Exclusion:** history of seizures or photosensitive epilepsy · severe motion sickness · a respiratory condition that makes breathing exercises uncomfortable · feels unwell on the test day.

Participants are anonymized (P1–P4). No names are stored. If a participant was also interviewed for `UserResearch.md`, they keep the same ID. Consent was given verbally in every session (Appendix B).

**Team roles:** in every session one team member, José Pablo Soto Sánchez, acted as facilitator, observer and heart-rate recorder.

- **Facilitator** – reads the script, gives hints only when the protocol allows, stays silent during T2.
- **Observer** – fills the observation log (Section 6.3) and times the tasks.
- **HR recorder** – reads the smartwatch every 30 s and fills the heart-rate sheet (Section 8.2). The participant never sees their own HR.

---

## 3. Test setup

- **Hardware:** Meta Quest 2 (headset + controllers) and Fitbit Inspire 3 smartwatch, in all sessions.
- **Breathing input used:** P1: controller on the chest **and** microphone together · P2: controller on the chest (left controller) **and** microphone together · P3, P4: pending verification
- **Position:** seated, comfortable posture, controller/hand resting on the chest for T2.
- **Build tested:** P1: `bd448e3` · P2: a build between `815b5c7` and `40db2f4` (tutorial, breathing guide and breathing tracking; no reactive weather) · P3, P4: pending verification

---

## 4. Tasks

The two tasks follow the real main flow: **Main menu → Setup → Forest → Breathing exercise (B button) / Options menu (Y button)**. A 5-minute session total, as stated in the project proposal.

| Task | Duration | What the participant is told | Success = |
|---|---|---|---|
| **T1 – Free exploration** | 3 min | "Explore the app however you like. Get to know it." *(Think-aloud allowed.)* | Starts the session from the main menu, chooses a time of day, adjusts at least one volume slider and enters the forest, all without a hint |
| **T2 – Calm your breathing** | 2 min | "Now dedicate yourself exclusively to calming your breathing. Please stay silent." | Opens the breathing menu, places the controller on the chest, follows the inhale / hold / exhale guide for the full 2 minutes |

Because each participant used a newer build, a checkpoint or survey item about a feature that was not in that build yet is recorded as **N/A**, not as a failure.

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
| 7 | 2 min | **T2 – Calm your breathing** (breathing menu + weather feedback active, when the build has them) | Observation log, HR every 30 s |
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

- Four sessions were run (planned 3–5); two are reported so far (P1, P2) and two are pending verification (P3, P4).
- Each participant used a **different build**, because the app kept changing between sessions. This makes the sessions an iteration record, but it also means results are not fully comparable across participants.
- **Build and date of P2:** the session notes did not record the commit and had a wrong date. The build was identified from the repository history: it had the tutorial (`815b5c7`) and breathing tracking, but not the reactive weather (`04dc20f`), so it was built from a commit between `815b5c7` and `40db2f4`. The session took place on 22/11/2026, before P1. The exact commit was not recorded.
- Consent was given **verbally**; no signed form was collected.
- **Control condition:** P1 first sat quietly for 5 minutes without the app, and used the app 2 hours later. P2 used the app first and did the control 2 hours later. Across P1 and P2 the order is therefore balanced. P3, P4: pending verification.
- **P1 was not asked items C6, D5 and D6** of Survey 2, so G6 and G7 have no data for P1.
- The app has **no session summary screen** (planned in the project proposal, Section 4.3), so there is no app-reported breathing rate or time in calm state. P2's build showed a live on-screen sensor label during T2; it was removed in `bd448e3`, and its values were not recorded.

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

**Legend:** ✔ = done unaided · ◐ = done with a hint · ✘ = failed / gave up · N/A = not in that build. **Error** = wrong action or hesitation > 5 s.

| # | Checkpoint | Result (✔ / ◐ / ✘ / N/A) | Time (s) | Errors / hesitations | Notes and exact quotes |
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

**Decision:** ☐ STAI-6 ☐ SUS ☐ Neither (reason: ____________) · P1 and P2 did not answer either questionnaire.

---

## 7. Comparison design (control condition)

A change in stress or heart rate could happen simply from sitting still, so the study includes a comparison, as stated in the project proposal. Options considered were the control, input method (chest controller vs. microphone) and feedback on/off. **We chose the control condition.**

- **Condition A:** Calm Woods session (T1 + T2, 5 minutes).
- **Condition B (control):** sit quietly for 5 minutes with the watch, no app. Same Survey 1 before and Survey 2 (Parts A and B) after, and the same heart-rate readings.

**Order:** P1 B → A (control first, the app 2 hours later) · P2 A → B (the app first, control 2 hours later) · P3, P4: pending verification

The comparison results are in `UserTesting.md`, Section 4.4.

---

## 8. Raw data

> Real data only. A blank cell means not recorded or not yet collected. P3 and P4 are left blank until the build they used is verified.

### 8.1 Survey responses

Control = 5 minutes seated without the app (only Survey 1, Survey 2 Part A and Part B were asked). "n/a" = not asked in that condition. "not asked" = the item was not asked to that participant. "N/A (build)" = the feature was not in that participant's build. P2 answered in Spanish; quotes are translated.

| Item | P1 app | P1 control | P2 app | P2 control | P3 app | P3 control | P4 app | P4 control |
|---|---|---|---|---|---|---|---|---|
| Age · VR experience | 19 · a little | same | 25 · never | same | | | | |
| Caffeine or exercise in last 2 h | No | – | No | – | | | | |
| Survey 1 Q1 / Q2 / Q3 | 7 / 5 / 6 | 7 / 5 / 6 | 7 / 7 / 6 | 7 / 6 / 6 | | | | |
| Survey 2 Q1 / Q2 / Q3 | 5 / 4 / 4 | 6 / 6 / 6 | 4 / 4 / 4 | 6 / 6 / 5 | | | | |
| Part B overall change | Slightly better | The same | Much better | Slightly better | | | | |
| C1 guide | 5 | n/a | 5 | n/a | | | | |
| C2 understood weather | 2 | n/a | N/A (build) | n/a | | | | |
| C3 weather helped | 4 | n/a | N/A (build) | n/a | | | | |
| C4 menus | 5 | n/a | 5 | n/a | | | | |
| C5 dizziness / eye strain | 3 | n/a | 1 | n/a | | | | |
| C6 controller on chest | not asked | n/a | 4 | n/a | | | | |
| D1 weather telling you | "At the end, I understood that the weather was responsive" | n/a | N/A (build) | n/a | | | | |
| D2 most helpful or pleasant | "The comfy place" | n/a | "The sunrise atmosphere, and feeling that the guided breathing really did slow me down" | n/a | | | | |
| D3 confusing or uncomfortable | "I didn't understand the clouds" | n/a | "I liked putting the controller on my chest, but I didn't see any change in the forest that confirmed whether I was doing it right" | n/a | | | | |
| D4 what to change | "Maybe instructions for the weather" | n/a | "Some change in the environment or the sky to see my progress without having to read numbers" | n/a | | | | |
| D5 favorite option | not asked | n/a | "The relaxing music and the sunrise" | n/a | | | | |
| D6 use it again | not asked | n/a | "Yes, definitely after finishing work, about 5 to 10 minutes" | n/a | | | | |

Additional comment from P1 (app): "a little dizziness at the end of the session"; afterwards the image looked a little blurry and the headset may not have been fitted as well as possible.

### 8.2 Heart-rate sheet

| Moment | P1 | P2 | P3 | P4 |
|---|---|---|---|---|
| Baseline (seated, before VR) | 95 | 80 | | |
| 0:30 (T1) | 89 | 81 | | |
| 1:00 (T1) | 92 | 79 | | |
| 1:30 (T1) | 91 | 78 | | |
| 2:00 (T1) | 90 | 76 | | |
| 2:30 (T1) | 89 | 75 | | |
| 3:00 (T1 ends) | 85 | 74 | | |
| 3:30 (T2) | 83 | 72 | | |
| 4:00 (T2) | 80 | 70 | | |
| 4:30 (T2) | 79 | 69 | | |
| 5:00 (T2 ends) | 78 | 68 | | |
| Post (1 min after removing headset) | 79 | 69 | | |
| **Mean T1** (0:30–3:00) | 89.3 | 77.2 | | |
| **Mean T2** (3:30–5:00) | 80.0 | 69.8 | | |
| **Δ** T2 mean − baseline | −15.0 | −10.3 | | |
| **Δ** 5:00 − baseline | −17 | −12 | | |
| **Δ** post − baseline | −16 | −11 | | |
| **Control:** baseline | 95 | 78 | | |
| **Control:** end (5 min) | 85 | 74 | | |
| **Control:** Δ end − baseline | −10 | −4 | | |

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

**Participant:** P2 · **Observer:** José Pablo Soto Sánchez · **Commit:** build between `815b5c7` and `40db2f4` (exact commit not recorded) · **Input:** controller on the chest (left) and microphone together · **Date:** 22/11/2026, before P1

| # | Checkpoint | Result | Time (s) | Errors / hesitations | Notes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | ✔ | ≈9 | | |
| 1.2 | Chooses a time of day | ✔ | ≈7 | | Chose Dawn |
| 1.3 | Adjusts music and/or ambient volume | ✔ | ≈8 | | Left music at 70 %, raised ambient a little |
| 1.4 | Presses Start and enters the forest | ✔ | ≈6 | | |
| 1.5 | Looks around comfortably | ✔ | – | | Preferred to stay seated looking around; walked only a little along the path |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | ✔ | ≈7 | | Remembered it from the tutorial |
| 2.2 | Places the controller on the chest correctly | ◐ | ≈11 | Unsure whether the right or left controller | Hint: it is the left controller. Placed it during "GET READY" |
| 2.3 | Follows the inhale / hold / exhale guide | ✔ | ≈6 | | Very steady for the 2 minutes |
| 2.4 | Notices the weather reacting | N/A | – | | No reactive weather in this build |
| 2.5 | Stays silent and still during the 2 minutes | ✔ | – | | |

**Totals:** hints given: 1 · errors: 1 · tracking lost / crash: No
**Errors (1):**
1. At the start of T2, moved the chest controller to get comfortable, which restarted the tracking; settled quickly.

**Spontaneous comments:** after T2: "Did it detect it properly? I didn't see if anything changed."
**App session summary:** not available. A live sensor label was visible during T2; its values were not recorded.
**Emotional reactions / body language observed:** looked very comfortable from the start; no dizziness.

---

## 9. Limitations

- Small sample: 2 reported participants so far (4 sessions run). Results show **trends and usability problems**, not statistical or clinical effectiveness.
- Each participant used a different build, so differences between participants mix the effect of the person with the effect of the version.
- The exact build commit of P2 was not recorded at the time; it was reconstructed from the repository history as a range of commits.
- One team member was facilitator, observer and heart-rate recorder in every session, so observations come from a single rater and may be biased.
- The control order was balanced across P1 (control first) and P2 (app first), but with one participant per order, order effects cannot be separated from the effect of the app.
- P1 was not asked C6, D5 and D6, so those items have no data for P1.
- The app has no session summary screen, so there is no app-reported breathing rate or time in calm state to compare with the heart rate.
- Fitbit Inspire 3 heart rate is less precise than medical equipment and can change from movement, caffeine or nerves.
- Self-reported scores can be affected by novelty, wanting to please the team, or the presence of an observer.
- Participants are mostly acquaintances and students; results may not generalize to other groups.
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