# TODO

## 1. Core Gameplay Loop

### Implement enemy spawning and progression system
   - Enemy Alert system ✅
   - Make a Hostile baseclass:
	  - OnAlert() function, what the enemy should do when notices the player ✅
	  - MoveToPlayer, MoveUntilPlayerVisible, etc. ✅
   - Enemy ability system 
   - Ranged and Melee attacks
   - NPC communication system (Make an "overlord")
   - PlayerSight Matrix: :✅
	  - körben raycast minden irányban, leírva azt a pontot amit a player még "lát"
	  - minél több raycast, annál pontosabb kör
	  - caster ellenfelek a legközelebbi raycast point-player vonal legközelebbi pontjára ha mozognak, akkor még látják a playert, és nem futnak oda ahol legutoljára látták
	  - lehet csinálni ellenfeleket, amik előszeretettel mozognak az "árnyékban"/ fedezékben


### Basic level design

### Add player and enemy health, damage, and death mechanics


## 2. Combat & Abilities

### Expand spell system (more spells, cooldowns, effects)
Ötlet: 
- Spell upgrades (Thorgast szerűen)
- Spell modifiers (modifier, amit bármelyik spellre rá lehet rakni, de csak egyet) pl- manát ad vissza, következő damage spell CD lecsökken, etc
- 6 ability: 
   Q - damage (Red)
   W - defensive (blue)
   E - interrupt - cc  (purple)
   R - big damage (orange/ fire red)
   D - movement (green)
   F - utility (silver)
   spellek color code-olva vannak, minden slotba csak a megfelelő spell kerülhet 
- itemek, RoR, Slay the Spire
### Implement spell upgrades/modifiers

### Add melee/ranged weapon support (optional)

## 3. Level Generation

### Create procedural or modular dungeon/room generation

### Add doors, keys, and simple puzzles/obstacles

## 4. User Interface

### Add HUD (health, mana, spell icons, minimap)

### Implement menus (pause, game over, settings)

## 5. Audio & Visuals

### Add sound effects (movement, spells, hits, deaths)

### Add basic animations (player, enemies)

### Improve visual feedback (particles, screen shake, etc.)

## 6. Progression & Replayability

### Implement progression system (score, currency, upgrades)

### Add unlockable spells, characters, or items

### Add random events/modifiers for replay value

## 7. Polish & Optimization

### Playtest and balance gameplay

### Optimize performance

### Fix bugs and improve code structure

## 8. Content Expansion

### Add more enemy types, bosses, and spell variety

### Expand level themes and visuals
