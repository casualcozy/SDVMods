using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;
using StardewValley;
using System.Runtime.CompilerServices;
using Force.DeepCloner;

namespace LocationSpecificExperienceGain
    
{
    
    internal sealed class ModEntry : Mod
    {
        // XP at the start of the day for each skill
        public int[] startingxp = new int[5];

        // Level at the start of the day for each skill
        public int[] startingLevel = new int[5];

        /// <summary>The mod entry point.</summary>
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // event += method to call

            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
       
           
        }

        /// <summary>
        /// At start of day, get starting XP values and starting level values for each skill that has a relevant Skill Book.
        /// This mod does not change the Luck skill.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event data</param>

        public void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;


            // Create array with start of day XP
            startingxp[0] = Game1.player.experiencePoints[0];
            startingxp[1] = Game1.player.experiencePoints[1];
            startingxp[2] = Game1.player.experiencePoints[2];
            startingxp[3] = Game1.player.experiencePoints[3];
            startingxp[4] = Game1.player.experiencePoints[4];

            // Create array with start of day levels
            startingLevel[0] = Game1.player.GetSkillLevel(0);
            startingLevel[1] = Game1.player.GetSkillLevel(1);
            startingLevel[2] = Game1.player.GetSkillLevel(2);
            startingLevel[3] = Game1.player.GetSkillLevel(3);
            startingLevel[4] = Game1.player.GetSkillLevel(4);

            
            // Prints start of day level count, XP value for each skill, and level value for each skill in SMAPI command console. Level count should always be 0 at the start of day.
            Monitor.Log($"The current new level count is {Game1.player.newLevels.Count}. Current XP is {startingxp[0]}xp farming, {startingxp[1]}xp fishing, {startingxp[2]}xp foraging, {startingxp[3]}xp mining, {startingxp[4]}xp combat. Start of day levels are {startingLevel[0]} in farming, {startingLevel[1]} in fishing, {startingLevel[2]} in foraging, {startingLevel[3]} in mining, {startingLevel[4]} in combat.", LogLevel.Debug);
         
        }

        /// <summary>
        /// On update, collect current XP values for each skill by creating a new XP int array.
        /// On update, collect current level values for each skill by creating a new level int array.
        /// If the player is NOT located inside the Blacksmith shop: 
        /// - set the current XP for each skill equal to the relevant start of day XP value
        /// - set the current level value for each skill equal to the relevant start of day level value
        /// If the player performs a single action that would cause them to immediately gain a level while outside of Clint's:
        /// - Clear the "NewIdeas" HUD message that was triggered
        /// - Remove the last added item in the newLevels list to remove the end of day level up menu that was triggered
        /// While the player is inside the Blacksmith shop, if every current XP value is the same as every starting XP value, nothing happens. 
        /// If any skill experience points change while inside the Blacksmith shop, gain experience.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event data</param>

        private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // Create new array with most recent XP values and most recent level values for each skill
            int[] newXP = Game1.player.experiencePoints.ToArray();
            int[] newLevelGained = new int[5];
            newLevelGained[0] = Game1.player.GetSkillLevel(0);
            newLevelGained[1] = Game1.player.GetSkillLevel(1);
            newLevelGained[2] = Game1.player.GetSkillLevel(2);
            newLevelGained[3] = Game1.player.GetSkillLevel(3);
            newLevelGained[4] = Game1.player.GetSkillLevel(4);

            // If the player is NOT in Clint's shop, set XP and level values for each skill equal to the start of day values
            if (Game1.player.currentLocation?.Name != "Blacksmith")
            {

                Game1.player.experiencePoints[0] = startingxp[0];
                Game1.player.experiencePoints[1] = startingxp[1];
                Game1.player.experiencePoints[2] = startingxp[2];
                Game1.player.experiencePoints[3] = startingxp[3];
                Game1.player.experiencePoints[4] = startingxp[4];

                Game1.player.farmingLevel.Value = startingLevel[0];
                Game1.player.fishingLevel.Value = startingLevel[1];
                Game1.player.foragingLevel.Value = startingLevel[2];
                Game1.player.miningLevel.Value = startingLevel[3];
                Game1.player.combatLevel.Value = startingLevel[4];

                // If the level value for any skill increases while the player is NOT in Clint's shop, handle the HUD message and LevelUp menu triggers
                if (startingLevel[0] != newLevelGained[0] || startingLevel[1] != newLevelGained[1] || startingLevel[2] != newLevelGained[2] || startingLevel[3] != newLevelGained[3] || startingLevel[4] != newLevelGained[4])
                {
                    Game1.hudMessages.Clear();
                    Game1.player.newLevels.RemoveAt(Game1.player.newLevels.Count - 1);

                    // Prints current location and the new level count in SMAPI command console
                    Monitor.Log($"Current location is {Game1.player.currentLocation?.Name}. The current new level count is {Game1.player.newLevels.Count}.", LogLevel.Debug);

                }

            }
                 
            // If every current XP value is the same as the start of day while inside Clint's shop, do nothing
            // Else, any of the current XP values is different than its corresponding start of day XP value while inside Clint's shop, so gain experience
            if (startingxp[0] == newXP[0] && startingxp[1] == newXP[1] && startingxp[2] == newXP[2] && startingxp[3] == newXP[3] && startingxp[4] == newXP[4])
                return;

            else
            {
                OnExperienceGained();
            }


        }

        /// <summary>
        /// If the player is inside Clint's shop:
        /// - update the start of day XP values to equal new XP values
        /// - allow level up and update level values accordingly
        /// Editing note: the if condition here is probably not necessary but I've left it in just in case
        /// </summary>
        /// <param></param>

        private void OnExperienceGained()
        {
            if (Game1.player.currentLocation?.Name == "Blacksmith")
            {
                // Update start of day XP values
                startingxp[0] = Game1.player.experiencePoints[0];
                startingxp[1] = Game1.player.experiencePoints[1];
                startingxp[2] = Game1.player.experiencePoints[2];
                startingxp[3] = Game1.player.experiencePoints[3];
                startingxp[4] = Game1.player.experiencePoints[4];

                // Update start of day level values
                startingLevel[0] = Game1.player.farmingLevel.Value;
                startingLevel[1] = Game1.player.fishingLevel.Value;
                startingLevel[2] = Game1.player.foragingLevel.Value;
                startingLevel[3] = Game1.player.miningLevel.Value;
                startingLevel[4] = Game1.player.combatLevel.Value;

                // Prints current location, the new level count, and confirms XP changed in SMAPI command console
                Monitor.Log($"Current location is {Game1.player.currentLocation?.Name}. The current new level count is {Game1.player.newLevels.Count}. The player's XP changed.", LogLevel.Debug);

                return;

            }
        }



    }

}



