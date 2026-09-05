// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.BattlePassModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class BattlePassModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public uint BeginDate { get; set; }

        public uint EndedDate { get; set; }

        public bool Enable { get; set; }

        public List<PassBoxModel> Cards { get; set; }

        public BattlePassModel() => this.Name = "";

        
        public bool SeasonIsEnabled()
        {
            uint num = uint.Parse(DateTime.Now.ToString("yyMMddHHmm"));
            return this.BeginDate <= num && num < this.EndedDate;
        }


        public List<PassItemModel> GetReward(int Level, bool IsPremium)
        {
            List<PassItemModel> reward = new List<PassItemModel>();
            if (IsPremium)
            {
                reward.Add(this.Cards[Level].Normal);
                reward.Add(this.Cards[Level].PremiumA);
                reward.Add(this.Cards[Level].PremiumB);
            }
            else
                reward.Add(this.Cards[Level].Normal);
            return reward;
        }


        public void SetBoxCounts()
        {
            for (int index = 0; index < 99; ++index)
                this.Cards[index].SetCount();
        }

        public int GetCardCount()
        {
            int cardCount = 0;
            foreach (PassBoxModel card in this.Cards)
            {
                if (card.RewardCount > 0)
                    ++cardCount;
            }
            return cardCount;
        }

        //public (int, int, int, int) GetLevelProgression(int PassPoints)
        //{
        //    // ISSUE: object of a compiler-generated type is created
        //    // ISSUE: variable of a compiler-generated type
        //    BattlePassModel.Class15 class15 = new BattlePassModel.Class15();
        //    int num1 = 0;
        //    // ISSUE: erence to a compiler-generated field
        //    class15.Field0 = 0;
        //    int num2 = 0;
        //    int num3 = 0;
        //    int num4 = 0;
        //    List<PassBoxModel> list = this.Cards.OrderBy<PassBoxModel, int>((Func<PassBoxModel, int>)(A_1 => A_1.Card)).ToList<PassBoxModel>();
        //    foreach (PassBoxModel passBoxModel in list)
        //    {
        //        if (PassPoints >= passBoxModel.RequiredPoints)
        //        {
        //            num1 = passBoxModel.Card;
        //            num2 = passBoxModel.RequiredPoints;
        //        }
        //        else
        //        {
        //            // ISSUE: erence to a compiler-generated field
        //            class15.Field0 = passBoxModel.Card;
        //            num3 = passBoxModel.RequiredPoints - (PassPoints - num2);
        //            num4 = num2 + passBoxModel.RequiredPoints;
        //            break;
        //        }
        //    }
        //    // ISSUE: erence to a compiler-generated field
        //    if (class15.Field0 == 0 && list.Any<PassBoxModel>())
        //    {
        //        // ISSUE: erence to a compiler-generated field
        //        class15.Field0 = list.Last<PassBoxModel>().Card + 1;
        //        num3 = 0;
        //        num4 = num2;
        //    }
        //    else
        //    {
        //        // ISSUE: erence to a compiler-generated field
        //        if (num1 > 0 && class15.Field0 == 0)
        //        {
        //            // ISSUE: erence to a compiler-generated field
        //            class15.Field0 = num1 + 1;
        //        }
        //        else
        //        {
        //            // ISSUE: erence to a compiler-generated field
        //            if (num1 == 0 && class15.Field0 == 0 && list.Any<PassBoxModel>())
        //            {
        //                // ISSUE: erence to a compiler-generated field
        //                class15.Field0 = list.First<PassBoxModel>().Card;
        //                num3 = list.First<PassBoxModel>().RequiredPoints - PassPoints;
        //                num4 = list.First<PassBoxModel>().RequiredPoints;
        //            }
        //            else
        //            {
        //                // ISSUE: erence to a compiler-generated field
        //                if (class15.Field0 <= 0)
        //                {
        //                    // ISSUE: erence to a compiler-generated field
        //                    if (num1 > 0 && class15.Field0 == 0 && !list.Any<PassBoxModel>())
        //                    {
        //                        num3 = 0;
        //                        num4 = num2;
        //                    }
        //                    else
        //                    {
        //                        // ISSUE: erence to a compiler-generated field
        //                        if (num1 == 0 && class15.Field0 > 0)
        //                        {
        //                            // ISSUE: erence to a compiler-generated method
        //                            int index = list.FindIndex(new Predicate<PassBoxModel>(class15.Method2));
        //                            if (index >= 0)
        //                            {
        //                                num3 = list[index].RequiredPoints - PassPoints;
        //                                num4 = list[index].RequiredPoints;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // ISSUE: erence to a compiler-generated method
        //                    int index = list.FindIndex(new Predicate<PassBoxModel>(class15.Method0));
        //                    if (index >= 0)
        //                    {
        //                        int requiredPoints = list[index].RequiredPoints;
        //                        int num5 = 0;
        //                        if (index > 0)
        //                            num5 = list.Take<PassBoxModel>(index).Sum<PassBoxModel>((Func<PassBoxModel, int>)(A_1 => A_1.RequiredPoints));
        //                        num3 = requiredPoints - (PassPoints - num5);
        //                        num4 = num5 + requiredPoints;
        //                    }
        //                    else
        //                    {
        //                        // ISSUE: erence to a compiler-generated method
        //                        if (num1 > 0 && !list.Any<PassBoxModel>(new Func<PassBoxModel, bool>(class15.Method1)))
        //                        {
        //                            num3 = 0;
        //                            num4 = num2;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    // ISSUE: erence to a compiler-generated field
        //    return (num1, class15.Field0, num3, num4);
        //}
        /// <summary>
        /// Calcula la progresión del nivel del Battle Pass basado en los puntos acumulados
        /// </summary>
        /// <param name="PassPoints">Puntos totales del jugador</param>
        /// <returns>
        /// Tupla con:
        /// - Item1: Nivel actual alcanzado
        /// - Item2: Siguiente nivel a alcanzar
        /// - Item3: Puntos que faltan para el siguiente nivel
        /// - Item4: Puntos totales necesarios para el siguiente nivel
        /// </returns>
        /// 
        public (int CurrentLevel, int NextLevel, int PointsToNextLevel, int NextLevelTotalPoints) GetLevelProgression(int PassPoints)
        {
            int currentLevel = 0;
            int nextLevel = 0;
            int currentLevelTotalPoints = 0;
            int pointsToNextLevel = 0;
            int nextLevelTotalPoints = 0;

           
            List<PassBoxModel> sortedCards = this.Cards.OrderBy(card => card.Card).ToList();

            
            foreach (PassBoxModel passBox in sortedCards)
            {
                if (PassPoints >= passBox.RequiredPoints)
                {
                   
                    currentLevel = passBox.Card;
                    currentLevelTotalPoints = passBox.RequiredPoints;
                }
                else
                {
                   
                    nextLevel = passBox.Card;
                    pointsToNextLevel = passBox.RequiredPoints - (PassPoints - currentLevelTotalPoints);
                    nextLevelTotalPoints = currentLevelTotalPoints + passBox.RequiredPoints;
                    break;
                }
            }

          
            if (nextLevel == 0 && sortedCards.Any())
            {
                
                nextLevel = sortedCards.Last().Card + 1;
                pointsToNextLevel = 0;
                nextLevelTotalPoints = currentLevelTotalPoints;
            }
            else if (currentLevel > 0 && nextLevel == 0)
            {
                
                nextLevel = currentLevel + 1;
            }
            else if (currentLevel == 0 && nextLevel == 0 && sortedCards.Any())
            {
                
                nextLevel = sortedCards.First().Card;
                pointsToNextLevel = sortedCards.First().RequiredPoints - PassPoints;
                nextLevelTotalPoints = sortedCards.First().RequiredPoints;
            }
            else if (nextLevel <= 0)
            {
                if (currentLevel > 0 && nextLevel == 0 && !sortedCards.Any())
                {
                    pointsToNextLevel = 0;
                    nextLevelTotalPoints = currentLevelTotalPoints;
                }
                else if (currentLevel == 0 && nextLevel > 0)
                {
                    
                    int index = sortedCards.FindIndex(card => card.Card == nextLevel);
                    if (index >= 0)
                    {
                        pointsToNextLevel = sortedCards[index].RequiredPoints - PassPoints;
                        nextLevelTotalPoints = sortedCards[index].RequiredPoints;
                    }
                }
            }
            else
            {
                // Recalcular puntos necesarios basados en el siguiente nivel
                int index = sortedCards.FindIndex(card => card.Card == nextLevel);
                if (index >= 0)
                {
                    int requiredPointsForNextLevel = sortedCards[index].RequiredPoints;
                    int accumulatedPointsBeforeNextLevel = 0;

                    if (index > 0)
                    {
                        // Sumar todos los puntos requeridos antes de este nivel
                        accumulatedPointsBeforeNextLevel = sortedCards.Take(index).Sum(card => card.RequiredPoints);
                    }

                    pointsToNextLevel = requiredPointsForNextLevel - (PassPoints - accumulatedPointsBeforeNextLevel);
                    nextLevelTotalPoints = accumulatedPointsBeforeNextLevel + requiredPointsForNextLevel;
                }
                else if (currentLevel > 0 && !sortedCards.Any(card => card.Card == nextLevel))
                {
                    pointsToNextLevel = 0;
                    nextLevelTotalPoints = currentLevelTotalPoints;
                }
            }

            return (currentLevel, nextLevel, pointsToNextLevel, nextLevelTotalPoints);
        }
        public bool IsCompleted(int PassPoints)
        {
            if (this.Cards == null || !this.Cards.Any<PassBoxModel>())
                return true;
            int num = this.Cards.Sum<PassBoxModel>((Func<PassBoxModel, int>)(A_1 => A_1.RequiredPoints));
            return PassPoints >= num;
        }
    }
}