# Calm Woods: Design Justification

| | |
|---|---|
| **Course** | Human Computer Interaction, Final Project (Universidad Panamericana) |
| **Platform** | Meta Quest 2 / Meta Quest 3 (standalone) · Unity 6000.5.7f1 |
| **Team** | Kenzo Matoo López · Jaime Rincón Burboa · José Pablo Soto Sánchez |

> **Scope of this file.** This file explains the main design decisions of Calm Woods, why they fit our users' needs, and which HCI concepts support them. Who the users are is in [`UserResearch.md`](UserResearch.md); how each decision is checked with real people is in [`UserTesting.md`](UserTesting.md).

> **Note on evidence.** The user needs quoted here come from our persona ("Ana", 27) and customer journey, which are still hypotheses until the research interviews are done. Each decision lists the test item that will confirm or correct it (Section 11).

---

## 1. Summary

Every decision in Calm Woods answers the same question: *does this help a stressed person calm down, or does it add one more thing to think about?*

| # | Decision | User need it serves | HCI concepts | Concept type |
|---|---|---|---|---|
| D1 | The app is a **VR** experience, not a web or phone app | Get away from the screens and distractions that cause the stress; an experience that does not feel boring | **Presence and immersion**; **selective attention** | Human factor (perception, psychology) |
| D2 | **Low-poly** visual style for every asset | A calm, easy-to-read world; a comfortable experience with no motion sickness | **Cognitive load**; **Gestalt (simplicity)**; **aesthetic and minimalist design**; frame rate and **cybersickness** | Human factor (cognition, perception) + heuristic |
| D3 | **Simple, soft typography and menus** with one shared theme and gentle fades | Zero learning curve; nothing on screen should feel urgent | **Consistency**; **feedback**; **Hick's law**; **Fitts's law**; **error prevention**; legibility | Interaction principles + human factor (perception) |
| D4 | The user can **explore the map freely** | The world should feel alive, not like a static screen; the user decides how to relax | **User control and freedom**; **constraints**; affordances / wayfinding | Interaction principles |
| D5 | A **breathing wheel** with soft colored phases, a center circle and a 5-second "GET READY" | Know what to do at every moment without thinking; time to get into position | **Color coding and contrast (figure-ground)**; **natural mapping**; **redundant coding**; **visibility of system status** | Human factor (perception) + interaction principles |
| D6 | **Clouds as feedback** for the user's breathing | Visible proof that it works, without numbers | **Feedback**; **metaphor / natural mapping**; **ambient (peripheral) display**; feedback timing | Interaction principle + human factor (perception) |
| D7 | **Controller on the chest** as the main breathing input, **microphone** as the secondary one | Use what the user already holds; keep working if one input fails | **Platform-specific input**; **redundancy / graceful degradation**; **error recovery** | Platform interaction + interaction principles |
| D8 | **Time of day** and **calm forest music and sounds** the user can choose | Relax in the way each person prefers; start right away if they do not want to choose | **Flexibility and personalization**; **sensible defaults**; **multisensory presence**; **memory load** | Heuristic + human factor (perception, cognition) |
| D9 | An **interactive tutorial** that can be **skipped** | Zero learning curve for beginners; no wasted time for people who already know | **Learnability (learning by doing)**; **help and documentation**; **user control and freedom**; **error prevention** | Interaction principles + human factor (cognition) |

This covers the rubric requirement: at least one human-factor concept (presence, cognitive load, color perception, cybersickness) and at least one interaction principle (feedback, consistency, constraints, error prevention).

---

## 2. D1 — Why virtual reality

### Decision

Calm Woods runs on Meta Quest 2 and 3 as a fully immersive VR experience instead of a website or a phone app.

### Why it fits our users

Our persona already tries to relax with her phone (scrolling, music, a meditation app) and dropped the meditation app because it felt boring. Her stress comes partly from deadlines and constant screen time. A phone app would put the relaxation tool on the **same device** that interrupts her with notifications, inside the same room full of reminders of her work.

VR changes that context:

| | Phone / web app | Calm Woods in VR |
|---|---|---|
| What the user sees | A small rectangle; the room, the desk and other apps are still in view | The forest fills the whole field of view; the room disappears |
| Distractions | Notifications, other tabs, people walking by | Visual distractions are blocked; ambient sound covers outside noise |
| How the exercise feels | Reading or watching instructions | Being *in* a place where the sky reacts to your breathing |
| Proof that it works | Numbers or nothing | The sky clears as you calm down, around you |

### HCI concepts

- **Presence and immersion (perception / psychology).** Immersion is what the system offers (a 360° view, head tracking, spatial sound); presence is the user's feeling of *being there*. A higher sense of presence makes the forest feel like a real break from the user's surroundings, which is the whole purpose of the app.
- **Selective attention (cognition).** Attention is limited, and anything in the field of view competes for it. By replacing the user's surroundings with a calm scene, VR removes competing stimuli, so the user's attention can go to their breathing and the environment.

### Costs of the platform and how we address them

VR has risks that a phone does not, so the choice comes with obligations:

| Risk | How the design responds |
|---|---|
| Motion sickness | Seated session, stable frame rate (D2), menus that do not tilt with the head (D3), movement constrained to safe areas (D4) |
| New users do not know the controls | Interactive tutorial at the start of the session (D9) |
| Setting up VR feels like effort | Defaults are preselected (Afternoon, 70% volume) so the user can start right away (D8) |

> 📷 **Screenshot:** `Images/dj_vr_overview.png` — the user's view inside the forest (TODO: capture in headset).

---

## 3. D2 — Low-poly visual style

### Decision

All assets in the forest (terrain, trees, rocks, bushes, grass, wildflowers, dock, water) use a **low-poly** style: simple geometric shapes, flat colors and a soft, limited palette. Several of these meshes are built by our own `LowPolyMeshBuilder` with a shared color palette.

### 3.1 The visual side: a world that feels simple

A stressed user should not have to *work* to understand where they are. A low-poly forest shows the idea of a tree, a rock or a lake with very few shapes and no noisy textures, so the scene can be understood at a glance.

- **Cognitive load (cognition).** Detailed, realistic textures add visual information the user has to process. Flat shapes and a soft palette lower that load, leaving mental resources for the breathing exercise.
- **Gestalt: simplicity / Prägnanz (perception).** People perceive scenes in the simplest possible way. Low-poly objects are already simple forms, so trees group naturally into clusters and the clearing, the trail and the lake read as clear areas of the scene.
- **Aesthetic and minimalist design (Nielsen's heuristic #8).** Nothing in the scene competes with what matters. The clouds, which are our main feedback channel (D6), stay visible against a calm background.
- **Consistency (visual).** Every asset shares the same style and palette, so nothing looks out of place and breaks the feeling of being in one coherent world.

### 3.2 The technical side: a smooth experience on mobile hardware

The Quest is a standalone headset with mobile hardware. In VR, a low or unstable frame rate is not just a visual problem; it is a **comfort and safety problem**, because a lag between head movement and the image is one of the main causes of **cybersickness**. For a relaxation app, feeling dizzy would defeat the purpose.

Low-poly assets keep the app light, and the generator adds limits so the scene can never get too heavy:

| Measure (in the code) | What it does | Why it matters for the user |
|---|---|---|
| Very simple meshes (a grass tuft is 6 triangles) | Thousands of objects cost little to draw | Dense, alive-looking forest without frame drops |
| GPU instancing for grass, in 16 m chunks (`GrassInstancedRenderer`) | Draws many tufts in a few calls instead of one object each | Keeps the frame rate stable |
| Grass drawn only within 25 m of the camera | Far grass is not drawn | Saves work where the user cannot see detail anyway |
| Hard caps: 600 trees, 120,000 grass tufts, a global density scale | The generated world can never exceed a known budget | Every procedurally generated forest runs smoothly, not just most of them |

> 📷 **Screenshot:** `Images/dj_lowpoly_trees.png` — close-up of trees, rocks and grass (TODO).
> 📷 **Screenshot:** `Images/dj_lowpoly_lake.png` — lake and dock (TODO).
> TODO: add the frame rate measured on Quest 2 / Quest 3 (OVR Metrics Tool or the Unity profiler).

---

## 4. D3 — Simple, soft typography and menus

### Decision

All menus (main menu, setup, options, breathing UI and tutorial) share one visual language defined in a single `UITheme`:

| Element | Value |
|---|---|
| Font | TODO: font name — a simple, rounded sans-serif |
| Panels | Dark forest green (`#1E2B22`) at 88% opacity, so the forest shows through |
| Text | Cream (`#F3EDE0`) instead of pure white, to avoid harsh contrast |
| Main action / selected option | Soft amber (`#E0A040`) |
| Sliders | Sage green fill (`#8FBF8A`) with a round handle |
| Text sizes | Title 72 · heading 48 · body 36 · buttons 40 (canvas units, sized to be read from about 1.5 m) |
| Transitions | Panels fade in and out (0.25–0.3 s) instead of appearing suddenly |

### Why it fits our users

Our persona needs **zero learning curve** and is skeptical; a cluttered or flashy menu would make the app feel like another task. Soft colors, round shapes and slow fades make every screen feel as calm as the forest, so the interface does not break the mood.

### HCI concepts

- **Consistency (interaction principle).** One theme controls the font, colors and sizes of every canvas, so a button always looks like a button and the selected option is always amber. The controls are also consistent: **B** always opens and closes the breathing exercise, **Y** always opens and closes the options, and only one of them can be open at a time.
- **Feedback (interaction principle).** Buttons change tint when pointed at and when pressed; the selected time of day turns amber; panels fade in and out so the user sees the change happen instead of being surprised by it.
- **Hick's law (cognition).** Decision time grows with the number of choices. Each screen shows only a few options: the setup screen has four time-of-day buttons and two sliders, with defaults already selected, so the user can simply press *Start Relaxing*.
- **Fitts's law (motor control).** Pointing with a controller ray is less precise than a mouse. Buttons are large and the slider handle is bigger than the bar, so targets are easy to hit.
- **Legibility in VR (perception).** VR displays have lower pixel density than a phone, and thin or decorative fonts blur. A simple sans-serif at large sizes, with light text on a dark panel, stays readable.
- **Error prevention (interaction principle).** While a panel is fading, its buttons are blocked, so the user cannot press something half-visible by accident.
- **Comfort.** Menus appear about 1.2 m in front of the user, at eye level, and only follow the horizontal direction of the head, so they never tilt and the user does not need to strain their neck.

> 📷 **Screenshots (TODO):** `Images/dj_main_menu.png` · `Images/dj_setup.png` · `Images/dj_options.png`

---

## 5. D4 — Free exploration of the map

### Decision

Inside the forest, the user can walk with the left joystick and turn with the right joystick to explore the whole generated world: the clearing where they start, the meadows, the forest clusters, the trail and the lake with its dock.

### Why it fits our users

If the user could only stand still in one spot, the forest would feel like a backdrop, and the app would risk feeling as boring as the meditation app our persona abandoned. Letting the user move makes the world feel **alive** and increases immersion: it is a place they can visit, not a picture. Because the forest is generated again each session, there is something new to discover every time.

### HCI concepts

- **User control and freedom (Nielsen's heuristic #3).** The user decides how to relax: walking to the lake, sitting in a meadow, or staying in the clearing and opening the breathing exercise right away. Exploration is optional; the breathing exercise is always one button away.
- **Affordances and wayfinding.** A dirt trail starts at the clearing, winds through the flat part of the world and ends at the dock. The trail invites the user to follow it without any sign or instruction, and leads them to the lake, one of the calmest spots in the scene.
- **Constraints (interaction principle) and error prevention.** Free movement could lead the user into confusing or unsafe situations, so the world limits it:
  - A clearing is always kept free around the starting point, so the user never starts inside a tree or on a slope.
  - A ring of mountains frames the scene and hides the edge of the world.
  - Invisible walls and the `PlayerAreaLimiter` keep the user inside the map.
  - The user cannot walk into the lake, only onto the dock; if they try, they return to the last dry position.

### Trade-off: exploration and motion sickness

Moving with a joystick while seated is a known cause of motion sickness. We accept this risk because exploration adds immersion, and we limit it with the seated setup, a stable frame rate (D2) and optional movement. TODO: confirm the turn type (snap or smooth) and movement speed, and consider a comfort vignette if testing shows discomfort. This is checked with survey item C5 and observation 1.5.

The customer journey also predicts a friction here: the user might be stopped at the edge of the map or the lake without knowing why. The mountains and the shoreline make these limits visible as part of the landscape; testing will show whether that is enough.

> 📷 **Screenshot:** `Images/dj_trail.png` — the trail leaving the clearing toward the lake (TODO).

---

## 6. D5 — The breathing wheel

### Decision

The breathing exercise is shown as a wheel in front of the user, following a **4-2-4-2** pattern (inhale 4 s, hold 2 s, exhale 4 s, hold 2 s):

| Part of the wheel | What it does |
|---|---|
| **Ring of colored arcs** | One arc per phase: inhale in soft orange (`#ED9452`), hold in lavender (`#B37AF5`), exhale in soft teal (`#61C2DB`). The current phase is fully visible; the others are faded to 35% |
| **Marker** | A dot that travels around the ring, so the user sees where they are in the whole cycle |
| **Center circle** | Grows during inhale, stays full during hold, shrinks during exhale, with eased motion |
| **Label and countdown** | "INHALE", "HOLD" or "EXHALE" and the seconds left in the phase |
| **"GET READY" (5 s)** | Shown before the first inhale |

### Why it fits our users

The colors were chosen because they look **comfortable and soft**, but still **stand out** a little and **contrast** with the dark panel and the forest behind it. The user should be able to follow the exercise with a glance, without reading or counting.

The **center circle** lets the user see at which part of the breathing cycle they are: it fills like the lungs fill and empties like the lungs empty.

The **5 seconds of "GET READY"** give the user time to place the controller on their chest before the exercise starts. They also match the 5 seconds that the controller tracker ignores at the start (D7), so the movement of placing the controller is not read as a breath.

### HCI concepts

- **Color coding and figure-ground contrast (perception).** Each phase has its own hue, and hue differences are noticed before the user reads anything. Fading the inactive arcs makes the current phase the "figure" and everything else the "ground", so attention goes to the right place.
- **Natural mapping (interaction principle).** The circle growing and shrinking maps directly to the body: bigger means air coming in, smaller means air going out. No explanation is needed.
- **Redundant coding (perception / accessibility).** The same information is shown in several ways: color, the label, the countdown, the circle size and the marker position. A user who cannot tell the colors apart, or who does not want to read, can still follow the exercise.
- **Visibility of system status (Nielsen's heuristic #1).** The marker and the countdown always tell the user where they are and how long is left, so they never have to guess.
- **Error prevention.** The "GET READY" pause avoids a false start while the user is still placing the controller.

> 📷 **Screenshot:** `Images/dj_breathing_wheel.png` — the wheel during an inhale (TODO).

---

## 7. D6 — Clouds as feedback

### Decision

While the breathing exercise is open, the sky reacts to the user's **calm score** (0 = agitated, 1 = calm):

| User state | What the clouds do |
|---|---|
| Agitated, fast or irregular breathing | Many clouds (up to 16), dark, big, low and close to the user, circling faster |
| Breathing slowing down | Clouds lighten, shrink, rise and move away |
| Calm, steady breathing | Only a couple of small, white, distant clouds remain |

The calm score is smoothed over about 6 s, and the clouds follow it with another short delay (about 2 s), so the sky changes gradually instead of jumping with every breath. When the exercise is closed, the sky returns to its clearest state.

There is no separate wind effect (the trees do not sway and there is no wind sound); the only hint of wind is that the clouds circle faster when the user is agitated.

### Why it fits our users

Weather fits the **forest theme**: a storm that clears is something everyone understands in a natural setting, so the feedback feels like part of the world and not like an extra screen. Our persona needs **visible proof** that the exercise is working, but she dislikes things that feel like measuring and charts. The clouds show progress without a single number.

### HCI concepts

- **Feedback (interaction principle).** The user acts (slows their breathing) and sees a result (the sky clears). This closes the loop between action and outcome, which is the core idea of the app.
- **Metaphor and natural mapping.** "Stormy" and "clear" are already common ways to describe a person's mood, so a dark, crowded sky reads as agitation and a clear sky reads as calm.
- **Ambient (peripheral) display (perception).** The feedback is in the environment around the user, not in a number they have to read. They can notice it with peripheral vision while they keep their focus on the breathing wheel.
- **Feedback timing.** Feedback that reacts to every small change would look random and could make the user anxious. Smoothing makes the change slow and continuous, which also matches the calm pace of the exercise. The smoothing time is a value we will tune with test results.

> 📷 **Screenshots (TODO):** `Images/dj_clouds_storm.png` (agitated) · `Images/dj_clouds_clear.png` (calm) — ideally the same view, before and after.

---

## 8. D7 — Breathing input: controller first, microphone second

### Decision

| Input | Role | How it works |
|---|---|---|
| **Controller on the chest** | Main input | The user rests the left controller on their chest. The app follows the rise and fall of the chest, counts the breaths and measures how regular they are |
| **Headset microphone** | Secondary input | Listens to the sound of the breathing. It is used when the controller gives no estimate |

Both inputs give the same two values, **breaths per minute** and **regularity**, which are combined into the calm score (70% rate, 30% regularity).

### Why it fits our users

The **controller** measures the movement of the chest and, from it, how often the user breathes, so the app can tell when the breathing becomes slow and calm. It uses hardware the user already has in their hand, with no extra devices.

The **microphone** covers the cases the controller misses. Agitated breathing is usually strong enough to be **heard**, so if the controller is moved or its tracking restarts, the microphone can still pick up the breathing and the feedback does not stop.

### HCI concepts

- **Platform-specific input.** The Quest controllers are tracked very precisely in space, so a few millimeters of chest movement are enough to detect a breath. This uses a capability of the platform instead of adding a sensor.
- **Redundancy and graceful degradation.** With two inputs, the experience keeps working when one of them fails, instead of the clouds freezing.
- **Error recovery.** If the controller moves more than 5 cm (for example, the user adjusts it), tracking restarts by itself instead of producing wrong readings. The first 5 seconds are ignored, matching the "GET READY" pause (D5).
- **Known constraints.** The microphone needs a quiet room and asks for permission the first time; talking interferes with it. These are stated in the README's recommended setup.

---

## 9. D8 — Time of day and audio

### Decision

- **Time of day:** four presets (Dawn, Afternoon, Sunset and Night), each with its own sky, lighting and mood. It can be chosen before the session and changed inside the forest from the options menu.
- **Audio:** a playlist of calm, forest-themed relaxing music with crossfades between tracks, plus ambient sounds (day or night forest, and water that gets louder near the lake). Music and ambient sounds have separate volume sliders, and the volumes are remembered between sessions.
- **Defaults:** Afternoon and 70% volume are preselected.

### Why it fits our users

People relax in different ways. Some prefer the light of the morning, others the middle of the day, the evening or the night, so each user can choose the moment that relaxes them most. The music was chosen to be calm and to match the forest theme, so it supports the relaxation instead of distracting from it. A user who does not want to choose can start right away with the defaults.

### HCI concepts

- **Flexibility and personalization (Nielsen's heuristic #7).** The user adapts the experience to their own taste, which makes the app feel like *their* place to relax.
- **Sensible defaults.** The options never block the user: the defaults let the persona, who has little time, skip every decision.
- **Multisensory presence (perception).** Sound adds to the sense of being in the forest. Ambient sounds that change with the place (water near the lake) and with the time of day (night forest) make the world feel real, and they also mask noise from outside.
- **Reduced memory load.** The volumes are saved, so the user does not have to remember or set them again every time.

> 📷 **Screenshots (TODO):** `Images/dj_time_of_day.png` — the same view at Dawn, Afternoon, Sunset and Night.

---

## 10. D9 — Interactive, skippable tutorial

### Decision

When the user enters the forest, a short tutorial appears in front of them. It has six steps: welcome, walk (left joystick), turn (right joystick), open and close the breathing guide (B), open and close the options (Y), and a final "You're ready". Each action step **waits for the user to actually do it** and shows "Done!" when they do. Text steps continue with the trigger. Holding the trigger for 2 seconds **skips the whole tutorial**.

### Why it fits our users

Many people in our target audience have little or no VR experience, so the tutorial teaches them how to use the app and how to move inside it. People who already know how to use it can skip it, so it never becomes a wall between them and the forest.

### HCI concepts

- **Learnability: learning by doing (cognition).** People remember an action better when they perform it than when they only read about it. Each step asks the user to press the real button or move the real joystick.
- **Help and documentation (Nielsen's heuristic #10).** The help appears where and when it is needed, inside the experience, instead of in a manual.
- **User control and freedom (heuristic #3).** Experienced users can skip it. The tutorial also hides while the breathing or options panel is open, so panels never overlap.
- **Visibility of system status.** A progress label (for example "2 / 6") and the "Done!" message tell the user how far along they are.
- **Error prevention.** A text step cannot be continued during its first second, so the user cannot skip it by accident. Skipping the whole tutorial needs a 2-second hold, not a single press, and a countdown shows while it is being held.

> 📷 **Screenshot:** `Images/dj_tutorial.png` — the "Walk" step (TODO).

---

## 11. How these decisions will be checked

| Decision | What would show it works | Where (`UserTesting.md`) |
|---|---|---|
| D1 VR | Lower stress after the session; users say it felt different from their usual tools | Q1–Q3 pre/post, D2, D6 |
| D2 Low-poly | No dizziness or eye strain; stable frame rate; users describe the scene as calm | C5, observation 1.5, D2 / D3 |
| D3 Menus and typography | Users complete the setup without hints or long hesitations; menus rated easy | Observation 1.1–1.4, C4, hints count |
| D4 Exploration | Users explore without getting lost or feeling sick; exploration mentioned as pleasant | T1 observation, C5, D2 / D3 |
| D5 Breathing wheel | Users follow the guide without hints | C1, observation 2.3 |
| D6 Clouds as feedback | Users link the clouds to their breathing without being told | C2, C3, D1, observation 2.4 (goal G4) |
| D7 Breathing input | Users place the controller correctly; the controller feels comfortable | C6, observation 2.2, D4, input-method comparison (Section 7) |
| D8 Time of day and audio | Users choose an option or start quickly with the defaults; they name a favorite option | Observation 1.2–1.3, D5 |
| D9 Tutorial | Users open the breathing guide and the options without hints | Hints count, observation 2.1 |