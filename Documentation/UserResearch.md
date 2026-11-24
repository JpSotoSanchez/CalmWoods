# Calm Woods: User Research and Customer Journey

| | |
|---|---|
| **Course** | Human Computer Interaction |
| **Platform** | Meta Quest 2 and Meta Quest 3 |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |

> **Scope of this file.** This file is about **understanding the users**: who they are, how they deal with stress, what they need, and how that shaped Calm Woods. The evaluation of the built app (tasks, surveys, heart rate, results and improvements) is in [`UserTesting.md`](UserTesting.md), and the raw data is in [`TestingProtocol.md`](TestingProtocol.md). How each need turned into a design decision is in [`DesignJustification.md`](DesignJustification.md).

> **Status (23/11/2026).** The persona and the journey started as hypotheses, based on the project proposal and general knowledge about stress and VR. They have now been checked against three participants (P1, P2, P3): a short research interview before each session (Section 3) and what they did and said while testing the app. The findings in Section 6 come from both.

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

**Target users:** Adults (18+) who say they feel stress regularly. The plan was to include at least one or two people with no VR experience, since "any person who suffers from stress" includes beginners. In the end, every participant had used VR at least once or twice before.

**Exclusion:** history of seizures or photosensitive epilepsy, severe motion sickness, or a respiratory condition that makes breathing exercises uncomfortable. The same criteria apply in `TestingProtocol.md`.

Participants are anonymized (P1, P2, P3) and keep the same ID in every document. Consent was given verbally.

| ID | Age | Occupation / context | VR experience | Guided breathing or meditation before? |
|---|---|---|---|---|
| P1 | 19 | Student | A little (a few times) | Tried guided breathing once; it did not work for them |
| P2 | 48 | Childcare worker | Very little (had tried a headset once or twice) | Tried it a couple of times with videos |
| P3 | 59 | Manager at a sports institution | Some (has used VR before and adapts easily) | No |

---

## 3. Research method

Two sources of evidence:

1. **Short interview (about 3 min)**, done with all three participants **before** they tried the app, so the answers were not shaped by it.

   | # | Question | Research question |
   |---|---|---|
   | R1 | When you feel stressed, what do you usually do to relax? | RQ1 |
   | R2 | Have you ever done guided breathing or meditation (apps, videos, classes)? | RQ2 |
   | R3 | Have you used VR before? How often? | RQ3 |
   | R4 | Do you get motion sick easily? | RQ3 |
   | R5 | What would you expect from a VR relaxation app? | RQ1 |

2. **Observation and open answers during the test sessions** (22–23/11/2026): what participants did, said and asked for while using the app, plus their open answers D1–D6 after it. These are used here only as evidence about **the users** (their needs, habits and constraints). Task scores and the evaluation of the app are in `UserTesting.md`.

The interviewer writes down answers and exact quotes. No names are stored.

### Interview answers

Answers as reported by the interviewer (summarized, not word for word).

| # | P1 | P2 | P3 |
|---|---|---|---|
| R1 How they relax | Listens to music | Watches a series | Goes for a walk |
| R2 Guided breathing or meditation | Tried it once, but it did not really work for them | Has tried it a couple of times with videos | Has not done it |
| R3 VR before | Yes, but very little | Barely, just a quick look | Yes, simple experiences, nothing complicated |
| R4 Gets motion sick easily | No | Yes, has gotten motion sick easily | No |
| R5 What they expect from the app | That it helps them relax | That it helps them control what they feel a little better | That it helps, but with reservations, because other apps they tried did not work for them |

---

## 4. Persona

**"Ana," 27, a busy adult with everyday stress** (initial hypothesis)

- **Context:** Juggles work or study, with constant deadlines and screen time. Her stress is frequent but not clinical.
- **Goals:** Calm down quickly, feel she is "doing something that works," and not waste time.
- **Current behavior:** Scrolls her phone, listens to music, or takes a walk. She has tried a meditation app but didn't stick with it because it felt boring or hard to measure.
- **Tech comfort:** Uses apps easily but has little or no VR experience.
- **Constraints:** Little time, skeptical that relaxation tools work, and slightly nervous about motion sickness.
- **Needs:** Zero learning curve, visible proof that it works, and a comfortable experience.

### How the real participants compare

| Persona trait | What we saw | Result |
|---|---|---|
| Age 27, works or studies | Ages 19, 48 and 59: a student and two working adults | **Wider than expected.** The audience is not one age group; the persona should not suggest a young adult only (K8) |
| Relaxes with the phone, music or a walk | Music (P1), a series (P2), a walk (P3) | **Confirmed:** passive media or a walk, no structured technique (K10) |
| Tried a meditation app and dropped it | P1 tried guided breathing once and it did not work; P2 tried it with videos; P3 tried other apps that did not work | **Confirmed** for P1 and P3 (K10) |
| Little or no VR experience | A little (P1), very little (P2), some (P3). P2 needed time to get used to the headset and to pointing | **Partly confirmed.** Little VR experience, but nobody was a complete beginner (K6) |
| Little time | P2 would use it "if I have 5 or 10 minutes" | **Confirmed** (K7) |
| Needs visible proof that it works | P2 did not know whether they were breathing correctly and asked for "something to measure the breathing". P1 and P3 asked for an explanation of the weather | **Confirmed, and stronger than expected** (K1, K2) |
| Skeptical that relaxation tools work | P3 expected the app to help "but with reservations" because other apps had not worked, and said afterwards that "maybe an app is not what works best for me" | **Confirmed** for one participant (K7, K10) |
| Slightly nervous about motion sickness | P2 said they get motion sick easily (R4) but rated dizziness 2 of 5. Mild dizziness for P1 at the end; blurry vision for P1 and P2, linked to headset fit | **Partly confirmed:** discomfort came from headset fit more than from the app (K6) |
| Listens to music to relax | Music was P2's favorite part, and P2 and P3 said the sound was very good | **Confirmed** (K3) |
| Reads English (implicit) | P2 could not follow the breathing guide in English | **Contradicted:** some users need the guide in Spanish (K4) |

**Updated persona:** an adult of any age (roughly 19 to 60) who feels everyday stress, has tried VR once or twice at most, may need the app in Spanish, has only a few minutes, and wants clear proof that they are breathing well before they trust the app.

---

## 5. Customer Journey

The journey from feeling stressed to deciding whether to come back to Calm Woods. Stages 1 and 6 happen outside the app. Stages 3 to 5 follow the real flow of the app: **MenuScene** (menus) → **procWorld** (forest). The first five rows are the original hypotheses. The **Evidence from research and testing** row shows what the three participants said in the interview (Stage 1) and did and said in the sessions (Stages 2 to 6).

| | **1. Stress builds up** | **2. Getting into VR** | **3. Menu and setup** | **4. Exploring the forest** | **5. Guided breathing** | **6. Ending and coming back** |
|---|---|---|---|---|---|---|
| **Actions** | Deals with deadlines and screens all day. Tries to relax by scrolling her phone, listening to music or taking a walk | Sits down, puts on the Quest and picks up the controllers | Reads *About us*, presses *Start*, picks a time of day (Dawn, Afternoon, Sunset, Night), sets music and ambient volume, presses *Start Relaxing* | Follows the tutorial (or skips it), looks around the forest, the lake and the dock while music and nature sounds play. May open the options menu (Y) to change the time of day or the volume | Opens the exercise (B), waits through "GET READY", rests the left controller on her chest, follows the inhale / hold / exhale wheel and watches the clouds | Closes the exercise (B), opens options (Y) → *Return to menu* → *Exit*, takes off the headset and decides whether to use it again |
| **Thoughts** | "I need a break, but I don't have time." "Meditation apps are boring." | "Will this actually help me?" "Will I get dizzy?" | "Where do I point? Is this the right option?" | "This is calm." "What do I do now?" | "Am I doing it right?" "Is the storm because of me?" | "Did it work? Do I feel better?" "Would I do this again?" |
| **Emotions** | Tense, tired, skeptical | Curious but skeptical, slightly nervous | Excited, slightly unsure | Relaxed, curious | Focused, then calm, or frustrated if the feedback seems random | Relieved, or unsure whether it helped |
| **Touchpoints** | Phone, music apps, meditation apps | Headset, controllers, chair | Main menu, About panel, setup panel, controller pointing | Tutorial, procedural forest, music, ambient sounds (forest, water near the lake), options menu | Breathing wheel, left controller on the chest (microphone as backup), clouds, audio | Options menu, main menu |
| **Friction** | Current tools feel boring and give no proof that they work | Headset fit and focus; fear of motion sickness; setting up VR just to relax may feel like too much effort | Unfamiliar controller pointing; too many choices before relaxing | Not knowing how to start the breathing exercise; being stopped at the edge of the map or the lake without knowing why | Not knowing which controller goes on the chest; weather that seems random; not knowing whether the breathing is right; a guide in another language | No clear end to the session and no summary, so progress is only what she noticed in the weather |
| **Evidence from research and testing** | They relax with music (P1), a series (P2) or a walk (P3). P1 tried guided breathing once and it did not work; P3's earlier apps did not work, so P3 came in with reservations | P2's lens spacing was set wrong (blurry vision) and P2 found the headset heavy. P1 felt mild dizziness at the end, with a headset that may not have been fitted well | Everyone finished the setup. P2 took about 20 s to get used to pointing and selecting; P3 chose Night in 10 s. P1 skipped the instructions and needed a hint | P1 and P3, whose build had the tutorial, opened the breathing menu unaided; P2, without the tutorial, needed a hint. Most pleasant parts: "the comfy place" (P1), "seeing the forest itself" (P3) | The guide was easy to follow (C1 = 5 for all). P1 understood the weather only at the very end and P3 thought it was random at first. P2 needed a hint about which controller goes on the chest, and P3 used the wrong one first. P2 could not follow the English guide and was unsure whether they were breathing correctly | 4 of 5 sessions ended "slightly" or "much better", 1 "the same". P2 would come back for 5 to 10 minutes; P3 would too, but said the weather did not help much and that maybe an app is not for them |
| **How Calm Woods responds** | A calm natural place where progress is visible in the weather | Seated session (used in every test) | Afternoon and 70% volume are preselected, so she can start right away; *Go Back* on the About and setup panels | A skippable tutorial teaches the controls; a new landscape every session; a clearing around the starting point; volumes and time of day can be changed at any moment | The weather shows progress without numbers; the calm score is smoothed so changes are gradual; a short "GET READY" before the first inhale | Volumes are remembered for the next session; a new forest each time |

**Where the emotional curve dipped:** the hypothesis was that the lowest points would be at the start (Stages 2 and 3) and in Stage 5 if the weather felt disconnected from the breathing. Testing confirmed the **Stage 5 dip**: P1 and P3 did not understand the clouds at first, and P2 felt uneasy not knowing whether they were breathing well. **Stage 3** was shorter than expected: everyone completed the setup and rated the menus 4 or 5. **Stage 4** was the high point (the forest and the sound were the favorite parts). **Stage 6** stayed open: with no summary, P3 left unsure whether it had helped.

---

## 6. Key findings

> Real data only. R1–R5 are the interview answers (Section 3). Evidence IDs (O1–O30) refer to `UserTesting.md`, Section 4.6; checkpoint and survey item codes refer to `TestingProtocol.md`.

| ID | Finding | Evidence | Confirms or contradicts the hypothesis? |
|---|---|---|---|
| K1 | Users want to **know whether they are breathing correctly**; not knowing makes them uneasy | P2: "I still don't know whether I was breathing correctly, and that didn't give me the best feeling" (O10), asked for "something to measure the breathing" (O11) and asked during T2 whether they were doing it right (O12) | **Confirms** the need for visible proof that it works |
| K2 | The weather **metaphor is not self-explanatory**: users understand it late or think it is random | P1 understood it only at the end and "didn't understand the clouds" (O1, O3, O7); P3 "at first I thought it was random" (O20, O21). C2 = 2 and 3. Both asked for an indication of what the weather means (O4, O22) | **Contradicts** the assumption that a storm that clears needs no explanation |
| K3 | **Sound and music** matter a lot for these users | P2 felt "much better" with sound and "slightly better" without it; music was P2's favorite part (O8, O9). P2 and P3 said the sound was very good (O30) | **Confirms** the persona's habit of relaxing with music; stronger than expected |
| K4 | Not every user reads English, and an **English-only guide** stops them | P2 did not follow the guide in run 1 because the labels were in English (O17). P2 and P3 gave all their open answers in Spanish | **New:** the persona did not consider language |
| K5 | Resting a controller on the chest is **comfortable**, but **which controller** is not obvious | C6 = 4 and 5. P2 needed a hint that it was the left controller (O16); P3 placed the right one first (O27) | **Partly confirms** the chest-controller friction: comfort is fine, placement is not |
| K6 | People with little VR experience need **time to adapt**, and **headset fit** drives comfort | R3: all three had little or simple VR experience. P2, who gets motion sick easily (R4), took a while to get used to pointing and to the headset and was faster in run 2 (O15). P1 and P2 had blurry vision linked to fit or lens spacing (O5, O14); P2 found the headset heavy (O13) | **Confirms** Stage 2 and 3 friction; discomfort came from the hardware more than from the app |
| K7 | Users would come back for **short sessions**, but not everyone believes an app is the right tool | P3 came in with reservations (R5). P2: "Yes, for a while, if I have 5 or 10 minutes to go to that calm place" (O18); P3: yes, "it felt safe", but "maybe an app is not what works best for me" (O26, O29) | **Confirms** "little time" and the persona's skepticism |
| K8 | The audience is **not one age group** | Participants were 19, 48 and 59, a student and two working adults | **Extends** the persona (27) |
| K9 | Some users **skip the instructions** at the start | P1 skipped the instructions the first time the app opened and needed a hint to go back (O6) | **New:** the tutorial alone does not guarantee that users learn the controls or the meaning of the weather |
| K10 | Users relax with **passive, familiar activities**, and earlier breathing or relaxation tools **did not work** for some of them | R1: music, a series, a walk. R2: P1 tried guided breathing once and it did not work. R5: P3's other apps did not work, so P3 had reservations | **Confirms** the persona's habits and her abandoned meditation app |
| K11 | Everyone expects the app to **help them relax or manage what they feel** | R5 from P1, P2 and P3 | **Confirms** the goal of the app; P3's reservations raise the bar for proving it works (K1) |

The persona (Section 4) and the journey (Section 5) have been updated with these findings.

---

## 7. User needs

| Need | Source | Confirmed by findings? |
|---|---|---|
| Zero learning curve | Persona hypothesis | ☐ Yes ☒ Partly ☐ No (K6, K9: the setup was easy for everyone, but first-time users needed time and hints) |
| Visible proof that it works | Persona hypothesis | ☒ Yes ☐ Partly ☐ No (K1, K2: confirmed, and the clouds alone did not provide it) |
| A comfortable experience (no motion sickness) | Persona hypothesis | ☐ Yes ☒ Partly ☐ No (K6: no session was stopped; discomfort came from headset fit) |
| Calm down quickly, without wasting time | Persona hypothesis | ☒ Yes ☐ Partly ☐ No (K7: short sessions of 5 to 10 minutes) |
| Something that feels different from tools that already failed | Findings K10, K11 | New need |
| Understand what the feedback means | Finding K2 | New need |
| Instructions and breathing guide in Spanish | Finding K4 | New need |
| Know which controller goes on the chest | Finding K5 | New need |
| Calming music and nature sounds | Finding K3 | New need (stronger than the hypothesis) |

---

## 8. How the research influenced the solution

Each friction leads to a design decision in the app (explained in `DesignJustification.md`), which was then checked with the participants.

| Friction | Design decision | Status in the app (`bd448e3`) | What testing showed |
|---|---|---|---|
| Too many choices at setup | Offer a default time of day and preset volumes so the user can start immediately | Implemented: Afternoon and 70% music/ambient volume are preselected (D8) | **Worked.** Everyone completed the setup; menus rated 4.3 of 5 (C4). P3 finished it in 20 s |
| New users do not know the controls | Interactive, skippable tutorial (D9) | Implemented from `815b5c7` | **Mostly worked.** With the tutorial, P1 and P3 opened the breathing menu unaided; without it, P2 needed a hint. P1 skipped the instructions (K9) |
| Chest controller feels awkward | Keep the microphone as an alternative; ask preference (D7) | Implemented: the microphone is used when the controller gives no estimate | **Comfort OK** (C6 = 4.5), but users did not know which controller to use (K5, UserTesting F4). The input-method comparison was not run |
| Weather feels random or laggy | Smooth the calm score so the weather changes gradually (D6) | Implemented: the calm score is smoothed over 2 s and the clouds follow with a 1 s delay | **Not solved.** The problem was the meaning, not the lag: users did not know what the weather meant (K2, F1). G4 not met |
| Breathing guide distracts from the scenery | Keep the UI minimal and semi-transparent (D3, D5) | Implemented: panels at 88% opacity, inactive arcs faded | **Worked.** The guide was rated 5 of 5 by everyone (C1); nobody said it was distracting |
| No clear end or summary of the session | Show a short summary in plain language ("You were calm for 2 of 5 minutes") | Not implemented: the app has no session summary yet | **The need is confirmed** (K1): P2 did not know whether they were breathing correctly (F6) |
| Motion discomfort | Seated mode, stable frame rate (D1, D2, D4) | Seated in every session; frame rate not measured | **Mostly worked.** No session was stopped; C5 mean 2.0. P1's mild dizziness was linked to headset fit (K6, F3) |
| Guide only in English | Translate the guide and tutorial into Spanish | Not implemented | **New need** from testing (K4, F5) |
| Sound matters | Music playlist and ambient soundscape (D8) | Implemented from `0b34998` | **Worked.** With sound, P2 improved every measure more than without (K3, F7) |
