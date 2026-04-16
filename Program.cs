using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TeamHeroCoderLibrary;

namespace PlayerCoder {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Connecting...");
            GameClientConnectionManager connectionManager;
            connectionManager = new GameClientConnectionManager();
            connectionManager.SetExchangePath(MyAI.FolderExchangePath);
            connectionManager.onHeroHasInitiative = MyAI.ProcessAI;
            connectionManager.StartListeningToGameClientForHeroPlayRequests();
        }
    }

    public static class MyAI {
        public static string FolderExchangePath = "C:/Users/Bohdan/AppData/LocalLow/Ludus Ventus/Team Hero Coder";

        //=========================================================================================
        //                                     CONTEXT CLASS
        //=========================================================================================
        public class HeroContext {
            public Hero activeHero;

            public Hero lowHPAlly;
            public Hero lowMPAlly;
            public Hero deadAlly;

            public Hero lowHPEnemy;
            public Hero target;

            public Hero silenceRemTarget;
            public Hero poisonRemTarget;
            public Hero petrifyRemTarget;
            public Hero fullRemTarget;

            public bool hasEther;
            public bool hasPotion;
            public bool hasRevive;
            public bool hasSilenceRem;
            public bool hasPoisonRem;
            public bool hasPetrifyRem;
            public bool hasFullRem;

            public int essenceAmount;
            public bool enemyInventoryEmpty;
        }

        static public void ProcessAI() {
            Console.WriteLine("Processing AI!");

            HeroContext context = BuildHeroContext();

            switch (context.activeHero.jobClass) {
                case HeroJobClass.Fighter:
                    ProcessFighter(context);
                    break;
                case HeroJobClass.Cleric:
                    ProcessCleric(context);
                    break;
                case HeroJobClass.Wizard:
                    ProcessWizard(context);
                    break;
                case HeroJobClass.Rogue:
                    ProcessRogue(context);
                    break;
                case HeroJobClass.Monk:
                    ProcessMonk(context);
                    break;
                case HeroJobClass.Alchemist:
                    ProcessAlchemist(context);
                    break;
            }
        }

        //=========================================================================================
        //                                PROCESS AI FUNCTIONS
        //=========================================================================================

        #region Process Fighter
        //Fighter:
        //1.Revive or heal allies if needed
        //2.Brave ally Fighters, Monks and Rogues
        //3.Quick Hit
        //4.Attack
        public static void ProcessFighter(HeroContext context) {
            Hero braveTarget = CheckAllyWithoutStatus(StatusEffect.Brave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);

            if (context.deadAlly != null && (Utility.AreAbilityAndTargetLegal(Ability.Resurrection, context.deadAlly, true) || context.hasRevive)) {
                if (TryAbility(Ability.Resurrection, context.deadAlly)) return;
                if (TryAbility(Ability.Revive, context.deadAlly)) return;
            }
            if (context.lowHPAlly != null && (Utility.AreAbilityAndTargetLegal(Ability.CureSerious, context.lowHPAlly, true) || context.hasPotion)) {
                if (TryAbility(Ability.CureSerious, context.lowHPAlly)) return;
                if (TryAbility(Ability.Potion, context.lowHPAlly)) return;
            }

            if (TryAbility(Ability.FullRemedy, context.fullRemTarget)) return;
            if (TryAbility(Ability.Ether, context.lowMPAlly)) return;

            if (context.lowHPEnemy != null) {
                if (TryAbility(Ability.QuickHit, context.lowHPEnemy)) return;
                if (TryAbility(Ability.Attack, context.lowHPEnemy)) return;
            }

            if (TryAbility(Ability.Brave, braveTarget)) return;
            if (TryAbility(Ability.QuickHit, context.target)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
        }
        #endregion

        #region Process Cleric
        //Cleric:
        //1.Asist allies with revive, health and mana
        //2.Quick Cleanse ally if needed
        //3.Autolife allies that do not have auto life
        //4.Attack
        public static void ProcessCleric(HeroContext context) {
            Hero autoLifeTarget = CheckAllyWithoutStatus(StatusEffect.AutoLife);
            Hero quickCleanseTarget = CheckAllyWithStatus(StatusEffect.Doom, StatusEffect.Petrifying, StatusEffect.Petrified);

            if (context.deadAlly != null) {
                if (TryAbility(Ability.Resurrection, context.deadAlly)) return;
                if (TryAbility(Ability.Revive, context.deadAlly)) return;
            }
            else if (context.lowHPAlly != null) {
                if (TryAbility(Ability.CureSerious, context.lowHPAlly)) return;      
                if (TryAbility(Ability.Potion, context.lowHPAlly)) return;
            }

            if (TryAbility(Ability.Ether, context.lowMPAlly)) return;
            if (TryAbility(Ability.QuickCleanse, quickCleanseTarget)) return;
            if (TryAbility(Ability.AutoLife, autoLifeTarget)) return;
            if(TryAbility(Ability.Attack, context.target)) return;

        }
        #endregion

        #region Process Wizard
        //Wizard:
        //1.Check if allies need revive, potion or ether
        //2.Flame Strike enemies below 30% HP
        //3.Poison nova if enemies are not poisoned
        //4.Doom enemies without doom if enemies do not have a full remedy
        //5.Meteor
        //6.MagicMissile
        //7.Attack
        public static void ProcessWizard(HeroContext context) {
            Hero doomTarget = CheckEnemyWithoutStatus(StatusEffect.Doom);
            Hero poisonTarget = CheckEnemyWithoutStatus(StatusEffect.Poison);
            Hero quickDispelTarget = CheckEnemyWithStatus(StatusEffect.Brave, StatusEffect.AutoLife, StatusEffect.Haste);
            bool enemyHasFullRemedy = CheckEnemyForItem(Item.FullRemedy);
            bool bitterBloomAllies = CheckAlliesForJob(HeroJobClass.Monk, HeroJobClass.Rogue);
            Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow, HeroJobClass.Alchemist, HeroJobClass.Rogue);
            bool hasSilence = SelfHasStatus(context.activeHero, StatusEffect.Silence);
            bool hasDoom = SelfHasStatus(context.activeHero, StatusEffect.Doom, StatusEffect.Petrifying);

            //Heal 
            if (TryAbility(Ability.Revive, context.deadAlly)) return;
            if (TryAbility(Ability.Potion, context.lowHPAlly)) return;
            if (TryAbility(Ability.Ether, context.lowMPAlly)) return;

            //Remedy
            if (hasSilence && TryAbility(Ability.SilenceRemedy, context.activeHero)) return;
            if (hasDoom && TryAbility(Ability.FullRemedy, context.activeHero)) return;
            if (TryAbility(Ability.PetrifyRemedy, context.petrifyRemTarget)) return;

            if (TryAbility(Ability.Fireball, context.lowHPEnemy)) return;

            //Debuff
            if (bitterBloomAllies && TryAbility(Ability.PoisonNova, poisonTarget)) return;
            if (TryAbility(Ability.Slow, slowTarget)) return;
            if (!enemyHasFullRemedy && TryAbility(Ability.Doom, doomTarget)) return;

            //Magic attack
            if (TryAbility(Ability.Meteor, context.target)) return;
            if (TryAbility(Ability.MagicMissile, context.target)) return;

            if(TryAbility(Ability.Attack, context.target)) return;

        }
        #endregion

        #region Process Rogue
        //Rogue:
        //1.Check if allies need revive, potion, ether or remedies
        //2.Steal from enemies if there is an enemy rogue
        //3.Target poisoned enemies
        //4.Silence Clerics, Wizards, Alchemists and Fighters
        //5.Attack
        public static void ProcessRogue(HeroContext context) {
            Hero silenceTarget = CheckEnemyWithoutStatus(StatusEffect.Silence, HeroJobClass.Cleric, HeroJobClass.Wizard, HeroJobClass.Alchemist, HeroJobClass.Fighter);

            Hero poisonedEnemy = CheckEnemyWithStatus(StatusEffect.Poison);
            Hero stealTarget = CheckEnemyWithoutStatus(StatusEffect.Petrifying, HeroJobClass.Alchemist);

            if (TryAbility(Ability.Revive, context.deadAlly)) return;

            if (TryAbility(Ability.Potion, context.lowHPAlly)) return;

            if (TryAbility(Ability.Ether, context.lowMPAlly)) return;

            if (TryAbility(Ability.SilenceRemedy, context.silenceRemTarget)) return;

            if (TryAbility(Ability.PoisonRemedy, context.poisonRemTarget)) return;

            if (TryAbility(Ability.PetrifyRemedy, context.petrifyRemTarget)) return;

            if (TryAbility(Ability.FullRemedy, context.fullRemTarget)) return;

            if (TryAbility(Ability.Steal, stealTarget) && !context.enemyInventoryEmpty && stealTarget != null) return;

            if (TryAbility(Ability.Attack, poisonedEnemy)) return;

            if (TryAbility(Ability.SilenceStrike, silenceTarget)) return;

            if (TryAbility(Ability.Attack, context.target)) return;

        }
        #endregion

        #region Process Monk
        //Monk:
        //1.Assit allies with health, mana, and revive
        //2.If enemy is poisoned use flurry of blows or attack
        //3.If health is above half debrave and defaith enemies
        //4.If health is below half use flurry of blows
        //5.Attack
        public static void ProcessMonk(HeroContext context) {
            //Stats
            bool maxManaBelowHalf = false;
            if (context.activeHero.maxMana <= 40) {
                maxManaBelowHalf = true;
            }

            bool healthBelowHalf = false;
            if (context.activeHero.health < context.activeHero.maxHealth * 0.5f) {
                healthBelowHalf = true;
            }

            //Targets
            Hero debraveTarget = CheckEnemyWithoutStatus(StatusEffect.Debrave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);
            Hero defaithTarget = CheckEnemyWithoutStatus(StatusEffect.Defaith, HeroJobClass.Wizard, HeroJobClass.Cleric, HeroJobClass.Alchemist);
            Hero poisonedEnemy = CheckEnemyWithStatus(StatusEffect.Poison);

            if (TryAbility(Ability.Revive, context.deadAlly)) return;

            if (TryAbility(Ability.Potion, context.lowHPAlly)) return;

            if (context.lowMPAlly != null && (context.hasEther || !maxManaBelowHalf)) {
                if (!maxManaBelowHalf) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Chakra, context.lowMPAlly);
                }
                else if (context.hasEther) {
                    TeamHeroCoder.PerformHeroAbility(Ability.Ether, context.lowMPAlly);
                }
            }
            if (TryAbility(Ability.FullRemedy, context.fullRemTarget)) return;

            if (poisonedEnemy != null || context.lowHPEnemy != null) {
                if (TryAbility(Ability.FlurryOfBlows, context.target)) return;
                if(TryAbility(Ability.Attack, context.target)) return;
            }
            if (!healthBelowHalf) {
                if (TryAbility(Ability.Debrave, debraveTarget)) return;
                if (TryAbility(Ability.Defaith, defaithTarget)) return;
                if (TryAbility(Ability.Attack, context.target)) return;
            }

            if (TryAbility(Ability.FlurryOfBlows, context.target)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
            

        }
        #endregion

        #region Process Alchemist
        //Alchemist:
        //1.Give haste to self
        //2.Check if allies need revive or ether
        //3.Check if items need crafted
        //4.Cleanse allies if needed
        //5.Slow enemies that are not slow
        //6.Attack
        public static void ProcessAlchemist(HeroContext context) {
            //Ally Targets 
            Hero cleanseTarget = CheckAllyWithStatus(StatusEffect.Silence, StatusEffect.Doom, StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Slow, StatusEffect.Debrave);
            Hero hasteTarget = CheckAllyWithoutStatus(StatusEffect.Haste, HeroJobClass.Alchemist);

            //Enemy targets
            Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow);

            //Self buff
            if (TryAbility(Ability.Haste, hasteTarget)) return;

            //Items
            if (TryAbility(Ability.Revive, context.deadAlly)) return;
            if (TryAbility(Ability.Ether, context.lowMPAlly)) return;

            //Craft
            if (!context.hasRevive && TryAbility(Ability.CraftRevive, context.activeHero)) return;
            if (!context.hasEther && TryAbility(Ability.CraftEther, context.activeHero)) return;
            if (!context.hasPotion && TryAbility(Ability.CraftPotion, context.activeHero)) return;

            if (TryAbility(Ability.Cleanse, cleanseTarget)) return;

            if (TryAbility(Ability.Slow, slowTarget)) return;

            if(TryAbility(Ability.Attack, context.target)) return;
        }
        #endregion

        //=========================================================================================
        //                                   HELPER FUNCTIONS
        //=========================================================================================

        private static HeroContext BuildHeroContext() {
            HeroContext context = new HeroContext();

            // Active hero
            context.activeHero = TeamHeroCoder.BattleState.heroWithInitiative;

            // Item checks
            context.hasEther = CheckAllyForItem(Item.Ether);
            context.hasPotion = CheckAllyForItem(Item.Potion);
            context.hasRevive = CheckAllyForItem(Item.Revive);
            context.hasSilenceRem = CheckAllyForItem(Item.SilenceRemedy);
            context.hasPoisonRem = CheckAllyForItem(Item.PoisonRemedy);
            context.hasPetrifyRem = CheckAllyForItem(Item.PetrifyRemedy);
            context.hasFullRem = CheckAllyForItem(Item.FullRemedy);

            // Remedy targets
            context.silenceRemTarget = CheckAllyWithStatus(StatusEffect.Silence);
            context.poisonRemTarget = CheckAllyWithStatus(StatusEffect.Poison);
            context.petrifyRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying);
            context.fullRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Doom, StatusEffect.Silence);

            // Essence
            context.essenceAmount = TeamHeroCoder.BattleState.allyEssenceCount;

            // Allies
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if (hero.health > 0 && hero.health < hero.maxHealth * 0.3f) {
                    context.lowHPAlly = hero;
                }

                if (hero.health <= 0) {
                    context.deadAlly = hero;
                }

                if (context.hasEther && hero.health > 0 && hero.mana < hero.maxMana * 0.3f) {
                    if (context.lowMPAlly == null || hero.mana < context.lowMPAlly.mana) {
                        context.lowMPAlly = hero;
                    }
                }
            }

            // Enemies
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if (hero.health > 0 && hero.health < hero.maxHealth * 0.3f) {
                    context.lowHPEnemy = hero;
                }

                if (hero.health > 0) {
                    if (context.target == null || hero.health < context.target.health) {
                        context.target = hero;
                    }
                }
            }

            // Inventory
            context.enemyInventoryEmpty = true;
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.foeInventory) {
                if (ii.count > 0) {
                    context.enemyInventoryEmpty = false;
                    break;
                }
            }

            return context;
        }

        public static Hero CheckEnemyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            // If no jobs specified fallback to old behavior
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
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {

                //If a class is specified check for it
                if (jobs.Length > 0 && !jobs.Contains(hero.jobClass)) continue;

                bool hasStatus = false;

                //Search for ally with desired status effect
                foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations) {
                    if (se.statusEffect == status) {
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
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {

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

        public static bool CheckAlliesForJob(params HeroJobClass[] jobs) {
            // If no jobs provided, nothing to check
            if (jobs.Length == 0)
                return false;

            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if (hero.health <= 0) continue;

                if (jobs.Contains(hero.jobClass))
                    return true;
            }

            return false;
        }

        public static bool TryAbility(Ability ability, Hero target) {
            if (target != null && Utility.AreAbilityAndTargetLegal(ability, target, true)) {
                TeamHeroCoder.PerformHeroAbility(ability, target);
                return true;
            }
            return false;
        }
    }
}
