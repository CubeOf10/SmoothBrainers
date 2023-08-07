# Agamotto Demos
Demonstrations of the possibilities offered by Agamotto.
Don't forget to play with the TimeStone's controller from the editor to record/simulate and move in time.

---

**SimplePhysicsDemo**
Demonstrates how to
- record and simulate physics simulation.
- use path visualizer

---

**ComplexPhysicsDemo**
A demonstration similar to SimplePhysicsDemo but with much more objects to demonstrate determinism possibility even in complex scene.

---

**CustomTimeAgentDemo**
A target object follows waypoints and a follower object follows the target.
Move the waypoints or play with objects speed.

Use in game button to stop objects in time and simulate their paths.
Use TimeStone's Editor controller to move in time after a simulation

Demonstrates how to :
- record and simulate code execution
- record and simulate Animator
- record and simulate CharacterController
- use path visualizer
- keep simulated objects visible and with specific material
- Have a child TimeAgent discovered dynamically and using local position and rotation

---

**ParticlesDemo**
A demonstration of the possibilities using Agamotto with particles.

Demonstrates how to :
- Simulate particles with or without children
- Use TimeStone's UnityEvent to freeze agents on start (from editor)

---

**PoolDemo**
Select the ball, set the direction and force and simulate the result.

Demonstrates how to :
- Simulate physics
- Act on simulation (to simulate ball hit)
- use path visualizer

---

**SorcererDemo**
Using a First-person view, catch cubes, throw cubes, stop time and move in time.

Demonstrates how to :
- record and simulate physics
- stop and start time from code
- move in time using TimeStone's playback capabilities

---

**TowerDefenseDemo**
Spawn towers, compare real and simulations result on the wave.
The towers are controlled by code, but generate physics based projectiles.

Demonstrates how to :
- simulate mixed code execution and physics
- persist active state to use objects Pool
- Add custom material/color to cloned objects