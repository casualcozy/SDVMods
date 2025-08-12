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

    

    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        //XP at the start of the day
        public int[] startingxp = new int[6];

        // Difference between start of day XP and new XP
        public int xpDifferenceFarming;
        public int xpDifferenceFishing;
        public int xpDifferenceForaging;
        public int xpDifferenceMining;
        public int xpDifferenceCombat;

       
    
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // event += method to call

            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
        
           
        }


        /// <summary>
        /// At start of day, get starting XP value for each skill that has a relevant Skill Book (this mod does not include the Luck skill).
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
            startingxp[5] = Game1.player.experiencePoints[5];

            // prints star of day XP for each skill in SMAPI command panel
            Monitor.Log($"Current XP is {startingxp[0]}, {startingxp[1]}, {startingxp[2]}, {startingxp[3]}, {startingxp[4]}.", LogLevel.Debug);
         
        }

        /// <summary>
        /// On update, check to see if player gained experience by creating a new XP int array. 
        /// If every new XP value is the same as every starting XP value, nothing happens. 
        /// If any new XP value has changed since start of day, check if it is okay to gain experience in the given context (location).
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event data</param>

        private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;
           
            // Create new array with most recent XP values
            int[] newXP = Game1.player.experiencePoints.ToArray();


            // Checks if XP changed between current moment and start of day
            if (startingxp[0] == newXP[0] && startingxp[1] == newXP[1] && startingxp[2] == newXP[2] && startingxp[3] == newXP[3] && startingxp[4] == newXP[4])
                return;
            else
                OnExperienceGained(newXP);


        }

        /// <summary>
        /// Checks to see if the player is inside Clint's shop (Location name "Blacksmith") when the XP changes. 
        /// If the player is NOT inside Clint's shop, removes the XP that was gained.
        /// If the player is inside Clint's shop, updates starting XP to equal new XP so that UpdatedTick does nothing.
        /// </summary>
        /// <param name="int[] newXP">each function receives the new XP values from the UpdatedTick</param>

        private void OnExperienceGained(int[] newXP)
        {
            // The difference between new XP and start of day XP
            xpDifferenceFarming = newXP[0] - startingxp[0];
            xpDifferenceFishing = newXP[1] - startingxp[1];
            xpDifferenceForaging = newXP[2] - startingxp[2];
            xpDifferenceMining = newXP[3] - startingxp[3];
            xpDifferenceCombat = newXP[4] - startingxp[4];


            // If new XP is different and the player is NOT inside Clint's shop, remove the XP that was gained.
            if (Game1.player.currentLocation?.Name != "Blacksmith")
            {
                Game1.player.experiencePoints[0] -= xpDifferenceFarming;
                Game1.player.experiencePoints[1] -= xpDifferenceFishing;
                Game1.player.experiencePoints[2] -= xpDifferenceForaging;
                Game1.player.experiencePoints[3] -= xpDifferenceMining;
                Game1.player.experiencePoints[4] -= xpDifferenceCombat;

                // Prints current location and confirms XP did not increase in SMAPI command console
                Monitor.Log($"The Current location is {Game1.player.currentLocation?.Name}. The player's XP did not increase.", LogLevel.Debug);

                return;
            }

            // If new XP is different and the player is inside Clint's shop, update starting XP to equal new XP.
            if (Game1.player.currentLocation?.Name == "Blacksmith")
            {
                startingxp[0] += xpDifferenceFarming;
                startingxp[1] += xpDifferenceFishing;
                startingxp[2] += xpDifferenceForaging;
                startingxp[3] += xpDifferenceMining;
                startingxp[4] += xpDifferenceCombat;

                // Prints current location and confirms starting XP values updated in SMAPI command console
                Monitor.Log($"Current location is {Game1.player.currentLocation?.Name}. The player's XP increased. Starting XP now equals new XP.", LogLevel.Debug);

                return;

            }
        }



    }

}



