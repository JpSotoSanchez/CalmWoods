# User Testing and Iteration — Calm Woods

| | |
|---|---|
| **Project** | Calm Woods: a procedurally generated VR forest for stress management and guided breathing |
| **Course** | Human Computer Interaction — Final Project |
| **Platform** | Meta Quest 2 / Meta Quest 3 · Unity |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |
| **Build tested** | v0.__ (commit `_______`) |
| **Test dates** | ____ / ____ / 2026 → ____ / ____ / 2026 |

> **How to use this file.** Sections 1–7 are the test plan and the instruments (fill in the blanks before testing). Sections 8–10 are filled in *during and after* the tests with real data only. Every change listed in Section 9 must point to evidence in Section 8.

---

## 1. Goals and success criteria

Testing is not a final approval step: it is evidence that must change the app. We decided what "success" means **before** running any session.

| # | Question | Metric | Success criterion (adjust before testing) |
|---|---|---|---|
| G1 | Can users complete the main flow without help? | Checkpoint completion, facilitator hints, errors, time per task | Every participant completes T1 and T2; on average ≤ 1 hint per participant |
| G2 | Do users feel less stressed after a session? | Pre vs. post scores for Q1–Q3 (1–10) and the overall-change item | Mean Q1 (stress) is lower after the session; any participant who feels worse is analyzed individually |
| G3 | Is physiological arousal lower after the session? | Smartwatch heart rate: baseline vs. end; exploration vs. breathing phase | Mean end HR ≤ mean baseline HR |
| G4 | Do users understand the weather feedback? | Post item C2 (1–5) + open answer D1 + observation | Mean C2 ≥ 4 and most participants link weather ↔ breathing without a hint |
| G5 | Is the experience comfortable in VR? | Post item C5 (1–5, lower is better) + observed discomfort | No participant scores C5 ≥ 4 and no session is stopped for discomfort |

---

## 2. Participants

**Target audience:** _(copy from `UserResearch.md`; suggested: university students who experience everyday academic stress and have little or no experience with guided breathing)_ .

**Target number:** 3–5 participants (small qualitative + trend study; see Limitations).

**Inclusion:** 18 or older · belongs to the target audience · gives informed consent.
**Exclusion:** history of seizures or photosensitive epilepsy · severe motion sickness · feels unwell on the test day.

Participants are anonymized (P1, P2, …). No names are stored in this file.

| ID | Age range | Occupation / context | VR experience (never / a few times / often) | Caffeine or intense exercise in last 2 h? | Date | Facilitator | Observer | HR recorder |
|---|---|---|---|---|---|---|---|---|
| P1 | | | | | | | | |
| P2 | | | | | | | | |
| P3 | | | | | | | | |
| P4 | | | | | | | | |
| P5 | | | | | | | | |

---

## 3. Test setup

- **Hardware:** Meta Quest ☐ 2 ☐ 3 (headset + controllers), smartwatch model: __________, chair without wheels, quiet room, printed forms, stopwatch.
- **Breathing input used:** ☐ controller on the chest ☐ microphone (see Section 7 if comparing both).
- **Roles (one team member each):**
  - **Facilitator** – reads the script, gives hints only when the protocol allows, stays silent during T2.
  - **Observer** – fills the observation log (Section 6.3) and times the tasks.
  - **HR recorder** – reads the smartwatch every 30 s and fills the heart-rate sheet (Section 6.4). The participant never sees their own HR.
- **Position:** seated, comfortable posture, controller/hand resting on the chest for T2.

---

## 4. Tasks

The two tasks follow the real main flow: **Main menu → Setup → Forest → Audio menu / Breathing menu**. A 5-minute session total, as stated in the project proposal.

| Task | Duration | What the participant is told | Success = |
|---|---|---|---|
| **T1 – Free exploration** | 3 min | "Explore the app however you like. Get to know it." *(Think-aloud allowed.)* | Starts the session from the main menu, chooses a time of day, adjusts at least one volume slider and enters the forest, all without a hint |
| **T2 – Calm your breathing** | 2 min | "Now dedicate yourself exclusively to calming your breathing. Please stay silent." | Opens the breathing menu, places the controller on the chest, follows the inhale / hold / exhale guide for the full 2 minutes |

---

## 5. Protocol (≈ 20 min per participant)

| Step | Time | What happens | Data |
|---|---|---|---|
| 1 | 2 min | Welcome, explain the study, read consent (Appendix B), answer questions | Signed consent |
| 2 | 2 min | **Pre-session survey** (3 questions + background) | Section 6.1 |
| 3 | 1 min | Put on smartwatch; sit still and quiet to record **baseline HR** | HR sheet |
| 4 | 1 min | Put on headset, adjust fit, place controllers. Short explanation of controls only | – |
| 5 | 3 min | **T1 – Free exploration** | Observation log, HR every 30 s |
| 6 | 2 min | **T2 – Calm your breathing** (breathing menu + weather feedback active) | Observation log, HR every 30 s, app session summary |
| 7 | 1 min | Remove headset; sit still to record **post HR** | HR sheet |
| 8 | 3 min | **Post-session survey** | Section 6.2 |
| 9 | 3 min | Short interview: "What did you think the weather was telling you?", "What would you change?" | Quotes |

**Facilitator script (read exactly):**

> "We are testing the app, not you; there are no right or wrong answers. You can stop at any time. For the first 3 minutes, explore the app however you like. I will tell you when the 3 minutes are over. Then I will ask you to dedicate the next 2 minutes only to calming your breathing."

> At minute 3: "Now, for 2 minutes, focus only on calming your breathing. Please try not to speak."

**Hint rule:** wait at least 10 seconds of visible difficulty before helping. Log every hint.
**Stop criteria:** dizziness, nausea, eye strain, anxiety, or the participant asks to stop → remove the headset immediately, make sure they are comfortable, and record it.
**Note:** if the microphone is the breathing input, nobody talks during T2 (speech corrupts the signal).

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

**C5. Did you feel dizziness, nausea or eye strain?** *(1 = not at all · 5 = very much)*
☐1 ☐2 ☐3 ☐4 ☐5

**Part D — In your own words**

- **D1.** What did you think the weather was telling you? ________________________________
- **D2.** What was the most helpful or pleasant part? ________________________________
- **D3.** What was confusing, annoying or uncomfortable? ________________________________
- **D4.** What would you change? ________________________________

### 6.3 Observation log (one copy per participant)

**Participant:** P__ · **Observer:** ______ · **Build:** v0.__ · **Input:** ☐ chest controller ☐ microphone

**Legend:** ✔ = done unaided · ◐ = done with a hint · ✘ = failed / gave up. **Error** = wrong action or hesitation > 5 s.

| # | Checkpoint | Result (✔ / ◐ / ✘) | Time (s) | Errors / hesitations | Notes and exact quotes |
|---|---|---|---|---|---|
| **T1** | **Free exploration (3 min)** | | | | |
| 1.1 | Presses Start on the main menu | | | | |
| 1.2 | Chooses a time of day (dawn / midday / afternoon / night) | | | | |
| 1.3 | Adjusts music and/or ambient volume | | | | |
| 1.4 | Presses Start and enters the forest | | | | |
| 1.5 | Looks around comfortably (no visible discomfort) | | | | |
| **T2** | **Calm your breathing (2 min)** | | | | |
| 2.1 | Finds and opens the breathing menu | | | | |
| 2.2 | Places the controller on the chest correctly | | | | |
| 2.3 | Follows the inhale / hold / exhale guide | | | | |
| 2.4 | Notices the weather reacting (looks at the sky, comments) | | | | |
| 2.5 | Stays silent and still during the 2 minutes | | | | |

**Totals:** hints given: ___ · errors: ___ · tracking lost / crash: ☐ No ☐ Yes → ________
**App session summary (if available):** breathing rate start → end: ___ → ___ · time in calm state: ___ %
**Emotional reactions / body language observed:** ________________________________

### 6.4 Heart-rate sheet

Reading source: smartwatch worn on the same wrist for every participant. The HR recorder notes the value shown every 30 s. The participant does not look at it.

| Moment | P1 | P2 | P3 | P4 | P5 |
|---|---|---|---|---|---|
| Baseline (seated, before VR) | | | | | |
| 0:30 (T1) | | | | | |
| 1:00 (T1) | | | | | |
| 1:30 (T1) | | | | | |
| 2:00 (T1) | | | | | |
| 2:30 (T1) | | | | | |
| 3:00 (T1 ends) | | | | | |
| 3:30 (T2) | | | | | |
| 4:00 (T2) | | | | | |
| 4:30 (T2) | | | | | |
| 5:00 (T2 ends) | | | | | |
| Post (1 min after removing headset) | | | | | |

**Derived values** (computed after testing): mean T1 · mean T2 · Δ (T2 mean − baseline) · Δ (post − baseline).

---

## 7. Comparison / A/B condition

A change in stress or heart rate could happen simply from sitting still, so the study includes at least one comparison (as stated in the project proposal). Choose **one** before testing and keep the order **counterbalanced** (half of the participants do A first, half do B first).

| Option | Condition A | Condition B | What it tells us |
|---|---|---|---|
| **Control** | Calm Woods session (T1 + T2) | Sit quietly for 5 minutes with the watch, no app | Does the app reduce stress more than resting alone? |
| **Input method** | T2 with chest controller | T2 with microphone | Which breathing input is more reliable and preferred? |
| **Feedback** | T2 with weather feedback on | T2 with the guide only (feedback off) | Does the weather feedback add value? |

**Chosen comparison:** __________________________ · **Order:** P1 __ · P2 __ · P3 __ · P4 __ · P5 __

| Metric | Condition A | Condition B | Difference / observation |
|---|---|---|---|
| Δ Q1 stress (post − pre) | | | |
| Δ HR (end − baseline) | | | |
| Completion / hints | | | |
| Preferred by (count of participants) | | | |

---

## 8. Results

> *Fill with real data only. Leave a cell blank rather than estimating it.*

### 8.1 Participant summary

| ID | T1 done? | T2 done? | Hints | Errors | Q1 pre → post | HR baseline → post | Overall change (B) | Comfort (C5) | Condition |
|---|---|---|---|---|---|---|---|---|---|
| P1 | | | | | | | | | |
| P2 | | | | | | | | | |
| P3 | | | | | | | | | |
| P4 | | | | | | | | | |
| P5 | | | | | | | | | |
| **Mean / total** | | | | | | | | | |

### 8.2 Self-reported state (before vs. after)

| Item | Mean before | Mean after | Δ | Improved / same / worse (count) |
|---|---|---|---|---|
| Q1 Stress | | | | |
| Q2 Body tension | | | | |
| Q3 Restless thoughts | | | | |

### 8.3 Task performance

| Task / checkpoint | Completed unaided | With hint | Failed | Mean time (s) | Most common error |
|---|---|---|---|---|---|
| 1.1 Start from main menu | | | | | |
| 1.2 Choose time of day | | | | | |
| 1.3 Adjust volume | | | | | |
| 1.4 Enter the forest | | | | | |
| 2.1 Open breathing menu | | | | | |
| 2.2 Controller on chest | | | | | |
| 2.3 Follow breathing guide | | | | | |
| 2.4 Notice weather reacting | | | | | |

### 8.4 Experience ratings (Survey 2, Part C)

| Item | P1 | P2 | P3 | P4 | P5 | Mean |
|---|---|---|---|---|---|---|
| C1 Guide easy to follow | | | | | | |
| C2 Understood weather feedback | | | | | | |
| C3 Weather helped me calm down | | | | | | |
| C4 Menus easy to use | | | | | | |
| C5 Dizziness / eye strain (lower is better) | | | | | | |

### 8.5 Success criteria check

| Goal | Criterion met? | Evidence |
|---|---|---|
| G1 Main flow completed unaided | ☐ Yes ☐ Partly ☐ No | |
| G2 Lower self-reported stress | ☐ Yes ☐ Partly ☐ No | |
| G3 Lower heart rate | ☐ Yes ☐ Partly ☐ No | |
| G4 Weather feedback understood | ☐ Yes ☐ Partly ☐ No | |
| G5 Comfortable in VR | ☐ Yes ☐ Partly ☐ No | |

### 8.6 Qualitative observations and quotes

| ID | Participant | Quote or observation | Theme |
|---|---|---|---|
| O1 | P_ | | |
| O2 | P_ | | |
| O3 | P_ | | |

---

## 9. Findings and improvements

Every row links a finding to its evidence and to the change that was made.

**Severity scale:** 0 = not a problem · 1 = cosmetic · 2 = minor (fix if time allows) · 3 = major (fix before release) · 4 = blocker (users cannot continue).

| ID | Finding (what happened) | Evidence (where / who) | Severity | Change implemented | Where in the project | Re-test result |
|---|---|---|---|---|---|---|
| F1 | | e.g. 8.3 row __, O__, P__ | | | scene / script / UI element | |
| F2 | | | | | | |
| F3 | | | | | | |

### Iteration log (before → after)

| Version | Date | Changes | Motivated by |
|---|---|---|---|
| v0.__ (tested) | | Build used in the first round | – |
| v0.__ | | | F__ |
| v0.__ | | | F__ |

**Re-test:** ☐ Done with ___ participants ☐ Not possible (reason: ____________) · **Result:** ________________________________

---

## 10. Limitations

- Small sample (3–5 people): results show **trends and usability problems**, not statistical or clinical effectiveness.
- Smartwatch heart rate is less precise than medical equipment and can change from movement, caffeine or nerves.
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

**C5. ¿Sentiste mareo, náusea o cansancio en los ojos?** *(1 = nada · 5 = mucho)*
☐1 ☐2 ☐3 ☐4 ☐5

**Parte D — Con tus propias palabras**

- **D1.** ¿Qué pensaste que te estaba diciendo el clima? ________________________________
- **D2.** ¿Qué fue lo más útil o agradable? ________________________________
- **D3.** ¿Qué te pareció confuso, molesto o incómodo? ________________________________
- **D4.** ¿Qué cambiarías? ________________________________

---

## Appendix B — Consent text

**English.** *Calm Woods is a university project (Human Computer Interaction). You will use a VR app for about 5 minutes, wear a smartwatch that records your heart rate, and answer two short surveys. Participation is voluntary and you may stop at any time, for any reason, without consequence. Your answers will be anonymized and used only for this course. The app is a relaxation aid, not a medical treatment. If you feel dizzy, nauseous or uncomfortable, tell us and we will stop immediately.*
☐ I understand and agree to participate. · Signature / initials: ______ · Date: ______

**Español.** *Calm Woods es un proyecto universitario (Interacción Humano-Computadora). Usarás una app de realidad virtual durante unos 5 minutos, llevarás un reloj inteligente que registra tu ritmo cardiaco y responderás dos encuestas cortas. La participación es voluntaria y puedes detenerte en cualquier momento, por cualquier motivo, sin ninguna consecuencia. Tus respuestas serán anónimas y se usarán únicamente para este curso. La app es un apoyo para relajarse, no un tratamiento médico. Si sientes mareo, náusea o incomodidad, avísanos y nos detendremos de inmediato.*
☐ Entiendo y acepto participar. · Firma / iniciales: ______ · Fecha: ______
