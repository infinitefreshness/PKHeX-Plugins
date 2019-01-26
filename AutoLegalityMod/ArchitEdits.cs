﻿using System.Linq;
using PKHeX.Core;
using static PKHeX.Core.LegalityCheckStrings;

namespace AutoLegalityMod
{
    public static class ArchitEdits
    {
        /// <summary>
        /// Set Nature and Ability of the pokemon
        /// </summary>
        /// <param name="pk">PKM to modify</param>
        /// <param name="SSet">Showdown Set to refer</param>
        public static void SetNatureAbility(this PKM pk, ShowdownSet SSet)
        {
            // Values that are must for showdown set to work, IVs should be adjusted to account for this
            pk.Nature = SSet.Nature;
            pk.SetAbility(SSet.Ability);
        }

        /// <summary>
        /// Sets shiny value to whatever boolean is specified
        /// </summary>
        /// <param name="pk">PKM to modify</param>
        /// <param name="isShiny">Shiny value that needs to be set</param>
        public static void SetShinyBoolean(this PKM pk, bool isShiny)
        {
            if (!isShiny)
            {
                pk.SetUnshiny();
            }
            else
            {
                if (pk.GenNumber > 5)
                    pk.SetShiny();
                else if (pk.VC)
                    pk.SetIsShiny(true);
                else
                    pk.SetShinySID();
            }
        }

        /// <summary>
        /// Set a valid Pokeball incase of an incorrect ball issue arising with GeneratePKM
        /// </summary>
        /// <param name="pk"></param>
        public static void SetSpeciesBall(this PKM pk)
        {
            if (!new LegalityAnalysis(pk).Report().Contains(LBallEncMismatch))
                return;
            if (pk.GenNumber == 5 && pk.Met_Location == 75)
                pk.Ball = (int)Ball.Dream;
            else
                pk.Ball = 4;
        }

        public static void ClearRelearnMoves(this PKM Set)
        {
            Set.RelearnMove1 = 0;
            Set.RelearnMove2 = 0;
            Set.RelearnMove3 = 0;
            Set.RelearnMove4 = 0;
        }

        public static void SetMarkings(this PKM pk)
        {
            if (pk.Format >= 7)
            {
                if (pk.IV_HP == 30 || pk.IV_HP == 29) pk.MarkCircle = 2;
                if (pk.IV_ATK == 30 || pk.IV_ATK == 29) pk.MarkTriangle = 2;
                if (pk.IV_DEF == 30 || pk.IV_DEF == 29) pk.MarkSquare = 2;
                if (pk.IV_SPA == 30 || pk.IV_SPA == 29) pk.MarkHeart = 2;
                if (pk.IV_SPD == 30 || pk.IV_SPD == 29) pk.MarkStar = 2;
                if (pk.IV_SPE == 30 || pk.IV_SPE == 29) pk.MarkDiamond = 2;
            }
            if (pk.IV_HP == 31) pk.MarkCircle = 1;
            if (pk.IV_ATK == 31) pk.MarkTriangle = 1;
            if (pk.IV_DEF == 31) pk.MarkSquare = 1;
            if (pk.IV_SPA == 31) pk.MarkHeart = 1;
            if (pk.IV_SPD == 31) pk.MarkStar = 1;
            if (pk.IV_SPE == 31) pk.MarkDiamond = 1;
        }

        public static void ClearHyperTraining(this PKM pk)
        {
            if (pk is IHyperTrain h)
            {
                h.HT_HP = false;
                h.HT_ATK = false;
                h.HT_DEF = false;
                h.HT_SPA = false;
                h.HT_SPD = false;
                h.HT_SPE = false;
            }
        }

        public static void SetHappiness(this PKM pk)
        {
            pk.CurrentFriendship = pk.Moves.Contains(218) ? 0 : 255;
        }

        public static void SetBelugaValues(this PKM pk)
        {
            if (pk is PB7 pb7)
                pb7.ResetCalculatedValues();
        }

        public static void RestoreIVs(this PKM pk, int[] IVs)
        {
            pk.IVs = IVs;
            pk.ClearHyperTraining();
        }

        public static bool NeedsHyperTraining(this PKM pk)
        {
            int flawless = 0;
            int minIVs = 0;
            foreach (int i in pk.IVs)
            {
                if (i == 31) flawless++;
                if (i == 0 || i == 1) minIVs++; //ignore IV value = 0/1 for intentional IV values (1 for hidden power cases)
            }
            return flawless + minIVs != 6;
        }

        public static void HyperTrain(this PKM pk)
        {
            if (!(pk is IHyperTrain h) || !NeedsHyperTraining(pk))
                return;

            pk.CurrentLevel = 100; // Set level for HT before doing HT

            h.HT_HP = (pk.IV_HP != 0 && pk.IV_HP != 1 && pk.IV_HP != 31);
            h.HT_ATK = (pk.IV_ATK != 0 && pk.IV_ATK != 1 && pk.IV_ATK != 31);
            h.HT_DEF = (pk.IV_DEF != 0 && pk.IV_DEF != 1 && pk.IV_DEF != 31);
            h.HT_SPA = (pk.IV_SPA != 0 && pk.IV_SPA != 1 && pk.IV_SPA != 31);
            h.HT_SPD = (pk.IV_SPD != 0 && pk.IV_SPD != 1 && pk.IV_SPD != 31);
            h.HT_SPE = (pk.IV_SPE != 0 && pk.IV_SPE != 1 && pk.IV_SPE != 31);
        }

        public static void FixMemoriesPKM(this PKM pk)
        {
            switch (pk)
            {
                case PK7 pk7:
                    if (!pk.IsUntraded)
                        pk7.TradeMemory(true);
                    pk7.FixMemories();
                    break;
                case PK6 pk6:
                    if (!pk.IsUntraded)
                        pk6.TradeMemory(true);
                    pk6.FixMemories();
                    break;
            }
        }

        /// <summary>
        /// Set TID, SID and OT
        /// </summary>
        /// <param name="pk">PKM to set trainer data to</param>
        /// <param name="trainer">Trainer data</param>
        /// <param name="APILegalized">Was the <see cref="pk"/> legalized by the API</param>
        public static void SetTrainerData(this PKM pk, SimpleTrainerInfo trainer, bool APILegalized = false)
        {
            if (APILegalized)
            {
                if ((pk.TID == 12345 && pk.OT_Name == "PKHeX") || (pk.TID == 34567 && pk.SID == 0 && pk.OT_Name == "TCD"))
                {
                    bool Shiny = pk.IsShiny;
                    pk.TID = trainer.TID;
                    pk.SID = trainer.SID;
                    pk.OT_Name = trainer.OT;
                    pk.OT_Gender = trainer.Gender;
                    pk.SetShinyBoolean(Shiny);
                }
                return;
            }
            pk.TID = trainer.TID;
            pk.SID = trainer.SID;
            pk.OT_Name = trainer.OT;
        }
    }
}