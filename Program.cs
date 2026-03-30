using System.Threading.Tasks;
using TeamHeroCoderLibrary;

namespace PlayerCoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting...");
            GameClientConnectionManager connectionManager;
            connectionManager = new GameClientConnectionManager();
            connectionManager.SetExchangePath(MyAI.FolderExchangePath);
            connectionManager.onHeroHasInitiative = MyAI.ProcessAI;
            connectionManager.StartListeningToGameClientForHeroPlayRequests();
        }
    }

    public static class MyAI
    {
        public static string FolderExchangePath = "C:/Users/Bohdan/AppData/LocalLow/Ludus Ventus/Team Hero Coder";

        static public void ProcessAI()
        {
            Console.WriteLine("Processing AI!");

            //#region SampleCode

            //if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Fighter)
            //{
            //    //The character with initiative is a figher, do something here...

            //    Console.WriteLine("this is a fighter");
            //}
            //else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric)
            //{
            //    //The character with initiative is a figher, do something here...

            //    Console.WriteLine("this is a cleric");
            //}
            //else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard)
            //{

            //}

            //foreach (InventoryItem ii in TeamHeroCoder.BattleState.allyInventory)
            //{

            //}

            ////Find the foe with the lowest health
            //Hero target = null;

            //foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes)
            //{
            //    if (hero.health > 0)
            //    {
            //        if (target == null)
            //            target = hero;
            //        else if (hero.health < target.health)
            //            target = hero;
            //    }
            //}

            ////This is the line of code that tells FG that we want to perform the attack action and target the foe with the lowest HP
            //TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);

            ////Searching for a poisoned hero 
            //foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes)
            //{
            //    foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations)
            //    {
            //        if (se.statusEffect == StatusEffect.Poison)
            //        {
            //            //We have found a character that is poisoned, do something here...
            //        }
            //    }
            //}

            //#endregion

            #region MyCode

            //Get hero with initiative
            Hero activeHero = TeamHeroCoder.BattleState.heroWithInitiative;

            //Check for allies hp below 30%
            Hero lowHPAlly = null;
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if(hero.health < hero.maxHealth * 0.3f && hero.health > 0) {
                    lowHPAlly = hero;
                }
            }

            //Check if allies have ether 
            bool hasEther = false;
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.allyInventory) {
                if (ii.item == Item.Ether) {
                    if(ii.count > 0) { 
                        hasEther = true;
                        break;
                    }
                }
            }

            //Check if allies have potion
            bool hasPotion = false;
            foreach(InventoryItem ii in TeamHeroCoder.BattleState.allyInventory) {
                if (ii.item == Item.Potion) {
                    if (ii.count > 0) {
                        hasPotion = true;
                        break;
                    }
                }
            }

            //Check if allies have revive
            bool hasRevive = false;
            foreach(InventoryItem ii in TeamHeroCoder.BattleState.allyInventory) {
                if(ii.item == Item.Revive) {
                    if(ii.count > 0) {
                        hasRevive = true;
                        break;
                    }
                }
            }

            //Get essence amount
            int essenceAmount = 0;
            foreach(InventoryItem ii in TeamHeroCoder.BattleState.allyInventory) {
                if (ii.item == Item.Essence) {
                    essenceAmount = ii.count;
                }
            }

            //Check for allies with mana below 30%
            Hero lowMPAlly = null;
            if (hasEther) {
                foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                    if (hero.health <= 0) continue;

                    if(hero.mana < hero.maxMana * 0.3f) {
                        if(lowMPAlly == null || hero.mana < lowMPAlly.mana) {
                            lowMPAlly = hero;
                        }
                    }
                }
            }

            //Check for dead allies
            Hero deadAlly = null;
            foreach(Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if(hero.health <= 0) {
                    deadAlly = hero;
                }
            }

            //Check for low hp enemies
            Hero lowHPEnemy = null;
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if(hero.health < hero.maxHealth * 0.3f && hero.health > 0) {
                    lowHPEnemy = hero;
                }
            }

            //Check for lowest health enemy
            Hero target = null;
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if (hero.health > 0) {
                    if (target == null)
                        target = hero;
                    else if (hero.health < target.health)
                        target = hero;
                }
            }

            if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Fighter) {
                //The character with initiative is a figher, do something here...
                if (deadAlly != null && (activeHero.mana >= 25 || hasRevive)) {
                    if(activeHero.mana >= 25) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Resurrection, deadAlly);
                    }
                    else if (activeHero.mana < 25 && hasRevive) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                    }
                }
                else if (lowHPAlly != null && (activeHero.mana >= 20 || hasPotion)) {
                    if (activeHero.mana >= 20) {
                        TeamHeroCoder.PerformHeroAbility(Ability.CureSerious, lowHPAlly);
                    }
                    else if (activeHero.mana < 20 && hasPotion) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                    }
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }


            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric) {

                Hero autoLifeTarget = CheckAllyWithoutStatus(StatusEffect.AutoLife);
                Hero quickCleanseTarget = CheckAllyWithStatus(StatusEffect.Doom, StatusEffect.Petrifying, StatusEffect.Petrified);

                if (deadAlly != null && (activeHero.mana >= 25 || hasRevive)) {
                    if (activeHero.mana >= 25) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Resurrection, deadAlly);
                    }
                    else if (activeHero.mana < 25 && hasRevive) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                        Console.WriteLine("Stuck at revive");
                    }
                }
                else if(lowHPAlly != null && (activeHero.mana >= 20 || hasPotion)) {
                    if (activeHero.mana >= 20) {
                        TeamHeroCoder.PerformHeroAbility(Ability.CureSerious, lowHPAlly);
                    }
                    else if (activeHero.mana < 20 && hasPotion) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                    }
                }
                else if (lowMPAlly != null && hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if(quickCleanseTarget != null && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.QuickCleanse, quickCleanseTarget);
                }
                else if (autoLifeTarget != null && activeHero.mana >= 25) {
                    TeamHeroCoder.PerformHeroAbility(Ability.AutoLife, autoLifeTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }
            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard) {

                Hero doomTarget = CheckEnemyWithoutStatus(StatusEffect.Doom);
                bool enemyHasFullRemedy = CheckEnemyForItem(Item.FullRemedy);

                if(deadAlly != null && hasRevive) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if(lowHPAlly != null && hasPotion) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                }
                else if (lowMPAlly != null && hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (lowHPEnemy != null && activeHero.mana >= 30) {
                    TeamHeroCoder.PerformHeroAbility(Ability.FlameStrike, lowHPEnemy);
                }
                else if (doomTarget != null && activeHero.mana >= 15 && !enemyHasFullRemedy) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Doom, doomTarget);
                }
                else if (activeHero.mana >= 60) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Meteor, target);
                }
                else if (activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.MagicMissile, target);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }
               



            }else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Rogue) {
                Hero silenceTarget = CheckEnemyWithoutStatus(StatusEffect.Silence, HeroJobClass.Cleric, HeroJobClass.Wizard, HeroJobClass.Alchemist);

                if (lowMPAlly != null && hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if(silenceTarget != null && activeHero.mana >= 15) {
                    TeamHeroCoder.PerformHeroAbility(Ability.SilenceStrike, silenceTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }

            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Monk) {
                //Monk
                Hero debraveTarget = CheckEnemyWithoutStatus(StatusEffect.Debrave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);
                Hero defaithTarget = CheckEnemyWithoutStatus(StatusEffect.Defaith, HeroJobClass.Wizard, HeroJobClass.Cleric, HeroJobClass.Alchemist);
                

                if (deadAlly != null && hasRevive) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if(lowHPAlly != null && hasPotion) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                }
                else if (lowMPAlly != null && hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if(debraveTarget != null && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Debrave, debraveTarget);
                }
                else if(defaithTarget != null && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Defaith, defaithTarget);
                }
                else if(activeHero.mana >= 15) {
                    TeamHeroCoder.PerformHeroAbility(Ability.FlurryOfBlows, target);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }
                

            }else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Alchemist) {
                //Alchemist
                Hero cleanseTarget = CheckAllyWithStatus(StatusEffect.Silence, StatusEffect.Doom, StatusEffect.Petrified, StatusEffect.Slow, StatusEffect.Debrave);
                Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow);

                if (deadAlly != null && hasRevive) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if (lowMPAlly != null && hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (!hasRevive && essenceAmount >= 2 && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftRevive, activeHero);
                }
                else if(!hasEther && essenceAmount >=2 && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftEther, activeHero);
                }
                else if(!hasPotion && essenceAmount >= 2 && activeHero.mana >= 10) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftPotion, activeHero);
                }
                else if (cleanseTarget != null && activeHero.mana >= 15) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Cleanse, cleanseTarget);
                }
                else if (slowTarget != null && activeHero.mana >= 15) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Slow, slowTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }
                
                
            }
            #endregion
        }

        public static Hero CheckEnemyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            foreach(Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if (hero.health <= 0) continue;//Skip dead enemies

                //If a class is specified check for it
                if (jobs.Length > 0 && !jobs.Contains(hero.jobClass)) continue;

                bool hasStatus = false;

                //Search for enemy with desired status effect
                foreach(StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                    if(se.statusEffect == status) {
                        hasStatus = true;
                        break;
                    }
                }

                if (!hasStatus) {
                    return hero;
                }
            }
            return null;
        }

        public static bool CheckEnemyForItem(Item desiredItem) {
            //Check if allies have revive
            bool enemyHasItem = false;
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.foeInventory) {
                if (ii.item == desiredItem) {
                    return true;
                }
            }
            return false;
        }

        public static Hero CheckAllyWithoutStatus(StatusEffect status) {
            foreach(Hero hero in TeamHeroCoder.BattleState.allyHeroes) {

                bool hasStatus = false;

                //Search for ally with desired status effect
                foreach(StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                    if(se.statusEffect == status) {
                        hasStatus = true;
                        break;
                    }
                }

                if (!hasStatus) {
                    return hero;
                }
            }
            return null;
        }

        public static Hero CheckAllyWithStatus(params StatusEffect[] statuses) {
            foreach(Hero hero in TeamHeroCoder.BattleState.allyHeroes) {

                bool hasStatus = false;

                //Search for ally with desired status effect
                foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                    if (statuses.Contains(se.statusEffect)) {
                        hasStatus = true;
                        break;
                    }
                }

                if (hasStatus) {
                    return hero;
                }
            }
            return null;
        }
    }
}
