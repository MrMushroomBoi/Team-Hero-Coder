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

            #region MyCode

            //Get hero with initiative
            Hero activeHero = TeamHeroCoder.BattleState.heroWithInitiative;

            //Check if allies have ether 
            bool hasEther = CheckAllyForItem(Item.Ether);

            //Check if allies have potion
            bool hasPotion = CheckAllyForItem(Item.Potion);

            //Check if allies have elixir
            bool hasElixir = CheckAllyForItem(Item.Elixir);

            //Check if allies have mega elixir
            bool hasMegaElixir = CheckAllyForItem(Item.MegaElixir);

            //Check if allies have revive
            bool hasRevive = CheckAllyForItem(Item.Revive);

            //Check if allies have silence remedy
            bool hasSilenceRem = CheckAllyForItem(Item.SilenceRemedy);

            //Check if allies have poison remedy
            bool hasPoisonRem = CheckAllyForItem(Item.PoisonRemedy);

            //Check if allies have petrify remedy
            bool hasPetrifyRem = CheckAllyForItem(Item.PetrifyRemedy);

            //Check if allies have full remedy
            bool hasFullRem = CheckAllyForItem(Item.FullRemedy);

            //Remedy Targets
            Hero silenceRemTarget = CheckAllyWithStatus(StatusEffect.Silence);
            Hero poisonRemTarget = CheckAllyWithStatus(StatusEffect.Poison);
            Hero petrifyRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying);
            Hero fullRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Doom, StatusEffect.Silence, StatusEffect.Slow);

            //Get essence amount
            int essenceAmount = TeamHeroCoder.BattleState.allyEssenceCount;

            //Check for allies hp below 30%
            Hero lowHPAlly = null;
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if (hero.health < hero.maxHealth * 0.3f && hero.health > 0) {
                    lowHPAlly = hero;
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

            //Check enemy inventory for items
            bool enemyInventoryEmpty = false;
            foreach(InventoryItem ii in TeamHeroCoder.BattleState.foeInventory) {
                if(ii.count > 0) {
                    enemyInventoryEmpty = false;
                }
                else {
                    enemyInventoryEmpty = true;
                }
            }


            if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Fighter) {
                //The character with initiative is a figher, do something here...
                Hero braveTarget = CheckAllyWithoutStatus(StatusEffect.Brave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);

                if (deadAlly != null && (Utility.AreAbilityAndTargetLegal(Ability.Resurrection, deadAlly, true) || hasRevive)) {
                    if(Utility.AreAbilityAndTargetLegal(Ability.Resurrection, deadAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Resurrection, deadAlly);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                    }
                }
                else if (lowHPAlly != null && (Utility.AreAbilityAndTargetLegal(Ability.CureSerious, lowHPAlly, true) || hasPotion)) {
                    if (Utility.AreAbilityAndTargetLegal(Ability.CureSerious, lowHPAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.CureSerious, lowHPAlly);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Potion, lowHPAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                    }
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Brave, braveTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Brave, braveTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.QuickHit, target, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.QuickHit, target);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }


            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric) {
                Hero autoLifeTarget = CheckAllyWithoutStatus(StatusEffect.AutoLife);
                Hero quickCleanseTarget = CheckAllyWithStatus(StatusEffect.Doom, StatusEffect.Petrifying, StatusEffect.Petrified);
                bool isSilenced = SelfHasStatus(activeHero, StatusEffect.Silence);

                if (deadAlly != null && ((Utility.AreAbilityAndTargetLegal(Ability.Resurrection, deadAlly, true)) || hasRevive)) {
                    if (Utility.AreAbilityAndTargetLegal(Ability.Resurrection, deadAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Resurrection, deadAlly);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                    }
                }
                else if (lowHPAlly != null && ((Utility.AreAbilityAndTargetLegal(Ability.CureSerious, lowHPAlly, true)) || hasPotion)) {
                    if (Utility.AreAbilityAndTargetLegal(Ability.CureSerious, lowHPAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.CureSerious, lowHPAlly);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Potion, lowHPAlly, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                    }
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Ether, lowMPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.QuickCleanse, quickCleanseTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.QuickCleanse, quickCleanseTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.AutoLife, autoLifeTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.AutoLife, autoLifeTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }

        }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard) {

                Hero doomTarget = CheckEnemyWithoutStatus(StatusEffect.Doom);
                Hero poisonTarget = CheckEnemyWithoutStatus(StatusEffect.Poison);
                bool enemyHasFullRemedy = CheckEnemyForItem(Item.FullRemedy);
                bool isSilenced = SelfHasStatus(activeHero, StatusEffect.Silence);

                if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Potion, lowHPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Ether, lowMPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (!isSilenced) {
                    if ( Utility.AreAbilityAndTargetLegal(Ability.FlameStrike, lowHPEnemy, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.FlameStrike, lowHPEnemy);
                    }
                    else if(Utility.AreAbilityAndTargetLegal(Ability.PoisonNova, poisonTarget, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.PoisonNova, poisonTarget);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Doom, doomTarget, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Doom, doomTarget);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Meteor, target, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Meteor, target);
                    }
                    else if (activeHero.mana >= 10) {
                        TeamHeroCoder.PerformHeroAbility(Ability.MagicMissile, target);
                    }
                    else {
                        TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                    }
                    
                }
                else if (isSilenced) {
                    if (hasSilenceRem || hasFullRem) {
                        if (hasSilenceRem) {
                            TeamHeroCoder.PerformHeroAbility(Ability.SilenceRemedy, activeHero);
                        }
                        else if (hasFullRem) {
                            TeamHeroCoder.PerformHeroAbility(Ability.FullRemedy, activeHero);
                        }
                    }
                    else {
                        TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                    }
                }
               



            }else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Rogue) {
                Hero silenceTarget = CheckEnemyWithoutStatus(StatusEffect.Silence, HeroJobClass.Cleric, HeroJobClass.Wizard, HeroJobClass.Alchemist, HeroJobClass.Fighter);

                Hero poisonedEnemy = CheckEnemyWithStatus(StatusEffect.Poison);
                Hero stealTarget = CheckEnemyWithoutStatus(StatusEffect.Petrifying, HeroJobClass.Alchemist);

                if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if(Utility.AreAbilityAndTargetLegal(Ability.Potion, lowHPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Ether, lowMPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.SilenceRemedy, silenceRemTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.SilenceRemedy, silenceRemTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.PoisonRemedy, poisonRemTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.PoisonRemedy, poisonRemTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.PetrifyRemedy, petrifyRemTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.PetrifyRemedy, petrifyRemTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.FullRemedy, fullRemTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.FullRemedy, fullRemTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Steal, stealTarget, true) && !enemyInventoryEmpty && stealTarget != null) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Steal, stealTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Attack, poisonedEnemy, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, poisonedEnemy);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.SilenceStrike, silenceTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.SilenceStrike, silenceTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }

            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Monk) {
                //Monk

                //Stats
                bool maxManaBelowHalf = false;
                if(activeHero.maxMana <= 40) {
                    maxManaBelowHalf = true;
                }

                bool healthBelowHalf = false;
                if (activeHero.health < activeHero.maxHealth * 0.5f) {
                    healthBelowHalf = true;
                }

                //Targets
                Hero debraveTarget = CheckEnemyWithoutStatus(StatusEffect.Debrave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);
                Hero defaithTarget = CheckEnemyWithoutStatus(StatusEffect.Defaith, HeroJobClass.Wizard, HeroJobClass.Cleric, HeroJobClass.Alchemist);
                
           
                if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if(Utility.AreAbilityAndTargetLegal(Ability.Potion, lowHPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Potion, lowHPAlly);
                }
                else if (lowMPAlly != null && (hasEther || !maxManaBelowHalf)) {
                    if (!maxManaBelowHalf) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Chakra, lowMPAlly);
                    }
                    else if (hasEther) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                    }
                }
                else if (!healthBelowHalf) {
                    if (Utility.AreAbilityAndTargetLegal(Ability.Debrave, debraveTarget, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Debrave, debraveTarget);
                    }
                    else if (Utility.AreAbilityAndTargetLegal(Ability.Defaith, defaithTarget, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.Defaith, defaithTarget);
                    }
                    else {
                        TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                    }
                }
                else {
                    if (Utility.AreAbilityAndTargetLegal(Ability.FlurryOfBlows, target, true)) {
                        TeamHeroCoder.PerformHeroAbility(Ability.FlurryOfBlows, target);
                    }
                    else {
                        TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                    }
                }
                
                

            }else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Alchemist) {
                //Alchemist

                //Ally Targets 
                Hero cleanseTarget = CheckAllyWithStatus(StatusEffect.Silence, StatusEffect.Doom, StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Slow, StatusEffect.Debrave);
                Hero hasteTarget = CheckAllyWithoutStatus(StatusEffect.Haste, HeroJobClass.Alchemist);

                //Enemy targets
                Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow);

                if (Utility.AreAbilityAndTargetLegal(Ability.Haste, hasteTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Haste, hasteTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Revive, deadAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Revive, deadAlly);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Ether, lowMPAlly, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, lowMPAlly);
                }
                else if (!hasRevive && Utility.AreAbilityAndTargetLegal(Ability.CraftRevive, activeHero, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftRevive, activeHero);
                }
                else if(!hasEther && Utility.AreAbilityAndTargetLegal(Ability.CraftEther, activeHero, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftEther, activeHero);
                }
                else if(!hasPotion && Utility.AreAbilityAndTargetLegal(Ability.CraftPotion, activeHero, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.CraftPotion, activeHero);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Cleanse, cleanseTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Cleanse, cleanseTarget);
                }
                else if (Utility.AreAbilityAndTargetLegal(Ability.Slow, slowTarget, true)) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Slow, slowTarget);
                }
                else {
                    TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);
                }
                
                
            }
            #endregion
        }

        //=========================================================================================
        //                                   HELPER FUNCTIONS
        //=========================================================================================

        public static Hero CheckEnemyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            // If no jobs specified → fallback to old behavior
            if (jobs.Length == 0) {
                foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                    if (hero.health <= 0) continue;

                    bool hasStatus = false;

                    foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                        if (se.statusEffect == status) {
                            hasStatus = true;
                            break;
                        }
                    }

                    if (!hasStatus)
                        return hero;
                }
            }
            else {
                // PRIORITY LOOP
                foreach (HeroJobClass job in jobs) {
                    foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                        if (hero.health <= 0) continue;
                        if (hero.jobClass != job) continue;

                        bool hasStatus = false;

                        foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                            if (se.statusEffect == status) {
                                hasStatus = true;
                                break;
                            }
                        }

                        if (!hasStatus)
                            return hero;
                    }
                }
            }

            return null;
        }

        public static Hero CheckEnemyWithStatus(params StatusEffect[] statuses) {
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {

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

        public static bool CheckEnemyForItem(Item desiredItem) {
            //Check if allies have revive
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.foeInventory) {
                if (ii.item == desiredItem && ii.count > 0) {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckAllyForItem(Item desiredItem) {
            //Check if allies have revive
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.allyInventory) {
                if (ii.item == desiredItem && ii.count > 0) {
                    return true;
                }
            }
            return false;
        }

        public static bool SelfHasStatus(Hero hero, params StatusEffect[] statuses) {
            foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                if (statuses.Contains(se.statusEffect)) {
                    return true;
                }
            }
            return false;
        }

        public static Hero CheckAllyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            foreach(Hero hero in TeamHeroCoder.BattleState.allyHeroes) {

                //If a class is specified check for it
                if (jobs.Length > 0 && !jobs.Contains(hero.jobClass)) continue;

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
