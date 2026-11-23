# Calm Woods: User Research and Customer Journey

| | |
|---|---|
| **Course** | Human Computer Interaction |
| **Platform** | Meta Quest 2 and Meta Quest 3 |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |

> **Scope of this file.** This file is about **understanding the users**: who they are, how they deal with stress, what they need, and how that shaped Calm Woods. The evaluation of the built app (tasks, surveys, heart rate, results and improvements) is in [`UserTesting.md`](UserTesting.md).

> **Note:** The persona and journey in this document are hypotheses. They come from the project proposal and general knowledge about stress and VR, not from real participants yet. The research below will confirm or correct them.

---

## 1. Research goal

Understand what people with everyday stress need, expect, and struggle with, so design decisions are backed by evidence.

### Research questions

| #   | Research question                                                                        | Project objective it informs                |
|-----|------------------------------------------------------------------------------------------|---------------------------------------------|
| RQ1 | How do people currently deal with stress, and what do they expect from a tool like this? | Environment and customization design        |
| RQ2 | What experience do they have with guided breathing or meditation?                        | Intuitive breathing interface               |
| RQ3 | How familiar are they with VR, and are they worried about motion sickness?               | Onboarding, VR comfort and usability        |

---

## 2. Participants

**Target users (3 to 5 participants):** Adults (18+) who say they feel stress regularly. Include at least one or two people with no VR experience, since "any person who suffers from stress" includes beginners.

**Exclusion:** history of seizures or photosensitive epilepsy, severe motion sickness, or a respiratory condition that makes breathing exercises uncomfortable. The same criteria apply in `UserTesting.md`.

Participants are anonymized (P1, P2, …). If the same people also test the app, they keep the same ID in `UserTesting.md`.

| ID | Age range | Occupation / context | VR experience (never / a few times / often) | Guided breathing or meditation before? |
|---|---|---|---|---|
| P1 | | | | |
| P2 | | | | |
| P3 | | | | |
| P4 | | | | |
| P5 | | | | |

---

## 3. Research method

**Short interview (about 3 min)**, held **before** the participant tries the app so the answers are not shaped by it.

| # | Question | Research question |
|---|---|---|
| R1 | When you feel stressed, what do you usually do to relax? | RQ1 |
| R2 | Have you ever done guided breathing or meditation (apps, videos, classes)? | RQ2 |
| R3 | Have you used VR before? How often? | RQ3 |
| R4 | Do you get motion sick easily? | RQ3 |
| R5 | What would you expect from a VR relaxation app? | RQ1 |

The interviewer writes down answers and exact quotes. No names are stored.

---

## 4. Persona (hypothesis to validate)

**"Ana," 27, a busy adult with everyday stress**

- **Context:** Juggles work or study, with constant deadlines and screen time. Her stress is frequent but not clinical.
- **Goals:** Calm down quickly, feel she is "doing something that works," and not waste time.
- **Current behavior:** Scrolls her phone, listens to music, or takes a walk. She has tried a meditation app but didn't stick with it because it felt boring or hard to measure.
- **Tech comfort:** Uses apps easily but has little or no VR experience.
- **Constraints:** Little time, skeptical that relaxation tools work, and slightly nervous about motion sickness.
- **Needs:** Zero learning curve, visible proof that it works, and a comfortable experience.

Interview questions R1 to R5 will tell us how closely real participants match Ana.

---

## 5. Customer Journey

Ana's journey from feeling stressed to deciding whether to come back to Calm Woods. Stages 1 and 6 happen outside the app. Stages 3 to 5 follow the real flow of the app: **MenuScene** (menus) → **procWorld** (forest). The thoughts, emotions and friction are hypotheses until they are checked against the findings in Section 6.

| | **1. Stress builds up** | **2. Getting into VR** | **3. Menu and setup** | **4. Exploring the forest** | **5. Guided breathing** | **6. Ending and coming back** |
|---|---|---|---|---|---|---|
| **Actions** | Deals with deadlines and screens all day. Tries to relax by scrolling her phone, listening to music or taking a walk | Sits down, puts on the Quest and picks up the controllers | Reads *About us*, presses *Start*, picks a time of day (Dawn, Afternoon, Sunset, Night), sets music and ambient volume, presses *Start Relaxing* | Looks around the forest, the lake and the dock while music and nature sounds play. May open the options menu (Y) to change the time of day or the volume | Opens the exercise (B), waits through "GET READY", rests the controller on her chest, follows the inhale / hold / exhale wheel and watches the clouds | Closes the exercise (B), opens options (Y) → *Return to menu* → *Exit*, takes off the headset and decides whether to use it again |
| **Thoughts** | "I need a break, but I don't have time." "Meditation apps are boring." | "Will this actually help me?" "Will I get dizzy?" | "Where do I point? Is this the right option?" | "This is calm." "What do I do now?" | "Am I doing it right?" "Is the storm because of me?" | "Did it work? Do I feel better?" "Would I do this again?" |
| **Emotions** | Tense, tired, skeptical | Curious but skeptical, slightly nervous | Excited, slightly unsure | Relaxed, curious | Focused, then calm, or frustrated if the feedback seems random | Relieved, or unsure whether it helped |
| **Touchpoints** | Phone, music apps, meditation apps | Headset, controllers, chair | Main menu, About panel, setup panel, controller pointing | Procedural forest, music, ambient sounds (forest, water near the lake), options menu | Breathing wheel, controller on the chest (microphone as backup), clouds and wind, audio | Options menu, main menu |
| **Friction** | Current tools feel boring and give no proof that they work | Headset fit and focus; fear of motion sickness; setting up VR just to relax may feel like too much effort | Unfamiliar controller pointing; too many choices before relaxing | Not knowing how to start the breathing exercise; being stopped at the edge of the map or the lake without knowing why | Controller placement feels odd; moving the controller restarts tracking; weather lags or jumps; the guide distracts from the scenery | No clear end to the session and no summary, so progress is only what she noticed in the weather |
| **How Calm Woods responds** | A calm natural place where progress is visible in the weather | Seated experience (TODO: confirm) | Afternoon and 70% volume are preselected, so she can start right away; *Go Back* on the About and setup panels | A new landscape every session; a clearing around the starting point; volumes and time of day can be changed at any moment | The weather shows progress without numbers; the calm score is smoothed so changes are gradual; 5 s to get ready before the first inhale | Volumes are remembered for the next session; a new forest each time |

**Where the emotional curve should dip:** before the app (Stage 1), when uncertainty peaks at the start (Stages 2 and 3), and in Stage 5 if the weather feels disconnected from her breathing. The high point should come in Stages 4 and 5, when the forest is calm and the sky clears. Stage 6 is open: with no summary, whether she leaves relieved or unsure depends only on what she noticed.

---

## 6. Key findings

> *Fill with real interview data only. Leave a row blank rather than estimating it.*

| ID | Finding | Evidence (participant, answer or quote) | Confirms or contradicts the hypothesis? |
|---|---|---|---|
| K1 | | | |
| K2 | | | |
| K3 | | | |

After filling this table, update the persona (Section 4) and the journey (Section 5) with what was actually observed. Findings that contradict the hypotheses are the most valuable result.

---

## 7. User needs

| Need | Source | Confirmed by findings? |
|---|---|---|
| Zero learning curve | Persona hypothesis | ☐ Yes ☐ Partly ☐ No (K__) |
| Visible proof that it works | Persona hypothesis | ☐ Yes ☐ Partly ☐ No (K__) |
| A comfortable experience (no motion sickness) | Persona hypothesis | ☐ Yes ☐ Partly ☐ No (K__) |
| Calm down quickly, without wasting time | Persona hypothesis | ☐ Yes ☐ Partly ☐ No (K__) |
| | Finding K__ | |

---

## 8. How the research influenced the solution

Each hypothesized friction leads to a design decision in the app, which is then checked during user testing.

| Hypothesized friction                      | Design decision                                                                  | Status in the app | How we check it (`UserTesting.md`) |
|--------------------------------------------|----------------------------------------------------------------------------------|---|---|
| Too many choices at setup                  | Offer a default time of day and preset volumes so the user can start immediately | Implemented: Afternoon and 70% music/ambient volume are preselected | Observation log 1.1–1.4 (time and hesitations) |
| Chest controller feels awkward             | Keep the microphone as an alternative; ask preference                            | Implemented: the microphone is used when the controller gives no estimate | Survey C6, observation 2.2, input-method comparison (Section 7) |
| Weather feels random or laggy              | Tune the smoothing window of the calm score                                      | Implemented: the calm score is smoothed over 6 s (value to tune with test results) | Survey C2 and D1, observation 2.4 |
| Breathing guide distracts from the scenery | Keep the UI minimal and semi-transparent                                         | TODO: confirm | Survey C1 and D3 |
| No clear end or summary of the session     | Show a short summary in plain language ("You were calm for 2 of 5 minutes")     | Not implemented: the app has no session summary yet | Survey Part B, interview |
| Motion discomfort                          | Prefer seated mode, stable frame rate                                            | TODO: confirm | Survey C5, observation 1.5 |
