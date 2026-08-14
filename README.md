# Facility Escape

A small first-person puzzle game built for my games development module, on top of a custom engine (`GDEngine`) built up over the semester in labs. You wake up in a control room and have to clear five separate rooms, each one built around a different engine system, before you're done.

## Controls

- **WASD** — move
- **Mouse** — look around
- **Space** — jump (2 second cooldown so you can't spam it)

## How it plays

You start in the **Hub**, a control room with five doors. Walk into any of them to enter that zone, and there's always a door back to the Hub inside each one too — so you can bail out mid-puzzle and come back later, your progress in that zone is remembered.

- **Physics Lab** — push crates around and get one onto the pressure plate to open the vault gate. There's also a "scanner" that labels whatever you're currently looking at.
- **Audio Corridor** — walk between two looping sound sources (should be audible panning left/right as you pass), hit a pad for a sound effect, then activate the console at the end to swap the music track.
- **Observation Deck** — three floor pads switch your camera between first-person, an orbiting camera around a terminal, and a fixed security-cam angle. No key presses, just walk onto the pad.
- **Security Terminal** — a live HUD showing your position/speed/zones-remaining, a slider that actually changes gravity in real time, and a button that mutes the ambient sound.
- **Override Console** — walk up to it to flip a lockdown status from red to green and stop whatever music is playing.

Finishing any of the five updates the Hub's status board and plays a confirmation sound, no matter where you were when it happened — that's handled through a shared event bus rather than each zone knowing about the Hub directly (basically an Observer pattern, which was one of the marking criteria).

## Running it

Open `GDGame.sln` in Visual Studio, set `GDGame` as the startup project, and hit F5. Needs the .NET 8 SDK and only runs on Windows (MonoGame WindowsDX).

If Windows blocks the unsigned build the first time you run it (SmartScreen or Smart App Control), running it through Visual Studio's debugger instead of double-clicking the `.exe` usually gets around it.

## Project structure

- `GDEngine/` — the engine itself, mostly course-provided material (physics, audio, rendering, UI, the event bus, etc).
- `GDGame/FacilityEscape/` — the actual game: the hub and all five zones, built on top of the engine.
- `GDGame/Demos/` — leftover tutorial/demo code from the labs, not part of the real game. Kept around for reference, not touched.

A couple of small bug fixes went into the engine along the way too, mainly a physics crash that happened whenever a fast-moving object crossed a trigger volume (turned out to be a degenerate contact-spring setting in the collision code, not anything to do with the puzzles themselves).
