Risk Of Routes is the mod that adds routing system to the game inspired by Slay The Spire and FTL, aiming to add more replayability and decision-making by allowing you to choose your run's route.

## General

![](https://i.imgur.com/tw262cs.png)

After each stage completion a portals to the next stages will appear, each having its own modifiers for next stage. Stage modifiers are determined by the portal's node type, you can see the full layout for the current loop in the UI.

![](https://i.imgur.com/UBQ4MyU.png)

A new map is generated at the start of each loop.

![](https://i.imgur.com/goiNmly.png)
You can also reroll a portal's modifiers by using the bazaar seers.
## Nodes

| Icon                                 | Name      | Description                                                                                                       |
| ------------------------------------ | --------- | ----------------------------------------------------------------------------------------------------------------- |
| ![](https://i.imgur.com/US764Bf.png) | Damage    | Portal that has printer modifiers with 'Damage' tag.                                                              |
| ![](https://i.imgur.com/SZBNwYh.png) | Healing   | Portal that has printer modifiers with 'Healing' tag.                                                             |
| ![](https://i.imgur.com/dGUMAHg.png) | Utility   | Portal that has printer modifiers with 'Utility' tag.                                                             |
| ![](https://i.imgur.com/ijogRmP.png) | Drone     | Portal that has drone-related stage modifiers.                                                                    |
| ![](https://i.imgur.com/76wdgyT.png) | Rare      | Analogue of StS's 'elites', always has positive modifiers of high tier, balanced by more difficult negative ones. |
| ![](https://i.imgur.com/Hdfnnqq.png) | Chef      | Portal that always has Wandering CHEF modifier, if it has printer modifiers, they will be food-related.           |
| ![](https://i.imgur.com/zyE6bMG.png) | Encrypted | Encrypted portal leading to Conduit Canyon, always has drone-related modifier of high tier.                       |
| ![](https://i.imgur.com/yPvfmZN.png) | Colossus  | Portal for the False Son's route, always has exclusive modifier for teleporter reward.                            |

## Stage Modifiers

All modifiers are separated in 3 Tiers, each portal has it's randomly chosen set of slots for positive and negative modifiers to balance harder stages with better rewards. Some of the modifiers are stackable just like items. Stackable modifiers of lower tiers can take slot of higher tier modifier on the portal, by appearing with multiple stacks.

<details>
<summary><b>All Modifiers</b></summary>
<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Tier</th>
			<th>Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td colspan="3" align="center"><h3>Positive Modifiers</h3></td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/Tl63Lbh.png" width=128> </td>
			<td align="center">None</td>
			<td>
				Aurelionite Fragment Reward.<br>Replaces the teleporter reward with aurelionite fragment. Synergizes with other teleporter reward modifiers.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/Jxf1bsm.png" width=128> </td>
			<td align="center">None</td>
			<td>
				Wandering CHEF.<br>Spawns a Wandering CHEF somewhere on the stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/5KwT6ZH.png" width=128> </td>
			<td align="center">None</td>
			<td>
				Mountain.<br>Can appear on any portal with 50% chance if you have mountain stacks on the current stage. Allows you to carry them over to the next stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/4K2YcHM.png" width=128> </td>
			<td align="center">None</td>
			<td>
				Repair.<br>Can appear on any portal with 20% chance if any of the players have consumed items. Repairs all consumed items upon entering the stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/CY1aPUK.png" width=128> </td>
			<td align="center">1-3</td>
			<td>
				Printer.<br>Spawns a printer with certain item on the stage. Default configuration of items can be viewed and changed in config.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/1JxBw7Z.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Legendary Item Reward.<br>The teleporter boss will drop a legendary item.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/tLMIxdx.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Legendary Drone Reward.<br>The teleporter boss will drop a legendary drone.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/6cB1hYk.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Upgrade Drones.<br>Upgrades all drones you have upon entering the stage. Works the same way as drone assembly station.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/pF1OIIJ.png" width=128> </td>
			<td align="center">2</td>
			<td>
				Boss Item Reward.<br>The teleporter boss will drop a boss item.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/f4KS5Jp.png" width=128> </td>
			<td align="center">1</td>
			<td>
				Uncommon Drone Reward.<br>The teleporter boss will drop an uncommon drone.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/TkFqbYW.png" width=128> </td>
			<td align="center">1</td>
			<td>
				More Drones.<br>Increases the spawnrate of drone interactables on the stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/PWqCWL4.png" width=128> </td>
			<td align="center">1</td>
			<td>
				Boss Item Printer.<br>Appears only if any of the players have boss items. Item in printer is selected according to the portal type.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/ijBLjzq.png" width=128> </td>
			<td align="center">1</td>
			<td>
				Legendary Item Printer.<br>Appears only if any of the players have legendary items. Item in printer is selected according to the portal type.
			</td>
		</tr>
		<tr>
			<td colspan="3" align="center"><h3>Negative Modifiers</h3></td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/fLDg5g6.png" width=128> </td>
			<td align="center">1-3</td>
			<td>
				Artifact.<br>Enables one of the available artifacts for the stage. Full list of available ones can be viewed and changed in config.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/SHEKMKy.png" width=128> </td>
			<td align="center">1-3</td>
			<td>
				Enemy Items.<br>Enemies spawn with items. Default configuration of items can be viewed and changed in config. By default this modifier does not stack (configurable).
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/ooXI20F.png" width=128> </td>
			<td align="center">1-3</td>
			<td>
				Lunar.<br>Gives players the effect of certain lunar item for the stage. Default configuration of items can be seen and changed in config. By default items are purified and turned into pearls at the end of the stage (configurable).
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/Qc1AWFH.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Post-Loop Elites.<br>Allows Tier 2 elites to appear pre-loop. Stacks make them appear more frequently.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/rGaQcRw.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Lunar Enemies.<br>Allows Lunar enemies to spawn on the current stage. Stacks make them appear more frequently.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/EHJaIAm.png" width=128> </td>
			<td align="center">3</td>
			<td>
				Void Enemies.<br>Allows Void enemies to spawn on the current stage. Stacks make them appear more frequently.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/mCQGaw2.png" width=128> </td>
			<td align="center">2</td>
			<td>
				Soul Cost.<br>All gold purchases will stack a curse on your max HP for the current stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/9BkU56M.png" width=128> </td>
			<td align="center">2</td>
			<td>
				Only Flying.<br>Only flying enemies will spawn on this stage.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/yZAZTO8.png" width=128> </td>
			<td align="center">2</td>
			<td>
				Doppelganger.<br>Works the same way as the Vengeance artifact, but works each 4 minutes spent on stage. Additional stacks reduce the cooldown.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/t1l9SoB.png" width=128> </td>
			<td align="center">2</td>
			<td>
				Artifact of Mystery.<br>All item pickups on the stage are obscured. Also added as separate artifact.
			</td>
		</tr>
		<tr>
			<td> <img src="https://i.imgur.com/r6FcHTE.png" width=128> </td>
			<td align="center">1</td>
			<td>
				More Elites.<br>Increases the spawnrate of elite enemies on the stage.
			</td>
		</tr>
	</tbody>
</table>
</details>

## Configuration

![](https://i.imgur.com/LIlfbrx.png)

Mod is highly configurable, so you can adjust some of the visuals and modifiers numbers.

![](https://i.imgur.com/w6d167a.png)

You can also add items from other mods to item-related modifiers using config. Same for the modded artifacts (Mod also seems to work fine with modded locations, haven't tested lot of them).

## Punishment

![](https://i.imgur.com/86Zqp01.png)

By default the Punishment system is enabled. It punishes you for spending too much time on one stage by giving more stacks to negative modifiers after you finish the stage. Default time is 8 minutes. This feature can also be configured and disabled in config.
## TBD

Mod is still being developed, so it might feel rough in balance and have some bugs, more updates will be coming later.

![](https://i.imgur.com/60ZUbZv.png)


### Known Issues
- Mod seems to be broken in multiplayer for clients if they have one of the following mods:
`ArtificerExtended`
`DrifterBossGrab`
- Enemy Item Modifier outweighs other modifiers too much
- Extra stacks on the portal panel are not visible to clients
- Lunar items modifier can grey out other items in UI

## Planned
- Balance changes
- Visual tweaks to portals and UI elements.
- Replacement of a few icons, some of them are placeholders now.
- 1 new interactable and a bunch of items tied to routing mechanic.
- Some unique changes to how mechanics work on loop.
- Integration with some mods I like.
- Route map of full run on stats screen.

Also huge thanks to gergeroth, narcise yoku, bbborbo, 753 and snowy snowtime for sharing their feedback and suggestions on the mod!

Me on discord - @widekaramysh
