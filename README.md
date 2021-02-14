# LOD-Planets-in-Unity

PLEASE READ THE LICENSE.txt FILE

Notes:
* When I say "your player", I mean whatever player controller that you have, preferably a simple one that just flies around without collision or gravity. An empty game object that you rename to "Player" will work fine for testing purposes, but you'll have to move it into position before you press play.

* The names of the game objects don't really matter, but are nice for organization.

Basic:
0. Drag all the scripts into Unity.
1. Put the Planet.cs and the NoiseFilter.cs scripts on an empty game object with the name "Planet".
2. Put the GameManagement.cs and Presets.cs scripts on another empty game object with the name "Manager".

In the GameManagement.cs script:
1. Add the planet to the "Objects to move" array.
2. Add your player to the "Player" variable.
3. Set "Max Travel Dist" to how far you want the player to be able to travel before being snapped back to the origin due to floating-point issues.
4. Drag the planet's Planet.cs script into the "Planet Script" variable.

In the Planet.cs script:
1. Add your player to the "Player" variable.
2. Add your own surface material to the "Surface Mat" variable.
3. Add the NoiseFilter.cs script that is attached to your planet to the "Noise Filter" variable.

In the NoiseFilter.cs script (this is really up to you, but here are my settings):
1. Set the "Strength" variable to 0.01
2. Set the "Octaves" slider to 5
3. Set the "Base Roughness" variable to 5
4. Set the "Roughness" variable to 3
5. Set the "Persistence" variable to 0.3
5.1. Ignore the variables "Center","Height Map" and "Normal Map". They are from a previous implementation, but since I'm still exploring that, I haven't deleted them yet.
6. Add the Planet.cs script that is attatched to your planet to the "Planet Script" variable.

Misc:
1. Add the "Player" tag to your player.
2. Move the player one planet-radius away from the planets origin. By default, this is 1000 units.

Enjoy :)
