using System;
using System.Linq;
using TeamHeroCoderLibrary;

namespace PlayerCoder {
    internal class Program {
        private static void Main(string[] args) {
            Console.WriteLine("Connecting...");

            GameClientConnectionManager connectionManager = new GameClientConnectionManager();
            connectionManager.SetExchangePath(MyAI.FolderExchangePath);
            connectionManager.onHeroHasInitiative = MyAI.ProcessAI;
            connectionManager.StartListeningToGameClientForHeroPlayRequests();
        }
    }

    public static class MyAI {
        public static string FolderExchangePath = "C:/Users/Bohdan/AppData/LocalLow/Ludus Ventus/Team Hero Coder";

        //=========================================================================================
        //                                      HERO CONTEXT
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

            public int essenceAmount;
            public bool enemyInventoryEmpty;
        }

        //=========================================================================================
        //                                      MAIN AI ROUTER
        //=========================================================================================

        public static void ProcessAI() {
            HeroContext context = BuildHeroContext();
            LogTurnStart(context);

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
        //                                  JOB AI FUNCTIONS
        //=========================================================================================

        #region Fighter

        // Fighter priority:
        // 1. Revive / heal / restore allies
        // 2. Use remedies if needed
        // 3. Finish low HP enemies
        // 4. Brave physical allies
        // 5. Quick Hit / Attack main target
        public static void ProcessFighter(HeroContext context) {
            Hero braveTarget = CheckAllyWithoutStatus(StatusEffect.Brave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);

            if (TryBasicSupport(context)) return;
            if (TryRemedySupport(context)) return;

            if (TryAbility(Ability.QuickHit, context.lowHPEnemy)) return;
            if (TryAbility(Ability.Attack, context.lowHPEnemy)) return;

            if (TryAbility(Ability.Brave, braveTarget)) return;
            if (TryAbility(Ability.QuickHit, context.target)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
        }

        #endregion

        #region Cleric

        // Cleric priority:
        // 1. Revive / heal / restore allies
        // 2. Use remedies if needed
        // 3. Buff important allies
        // 4. Cleanse dangerous statuses
        // 5. Attack
        public static void ProcessCleric(HeroContext context) {
            Hero faithTarget = CheckAllyWithoutStatus(StatusEffect.Faith, HeroJobClass.Wizard);
            Hero quickCleanseTarget = CheckAllyWithStatus(StatusEffect.Doom, StatusEffect.Petrifying, StatusEffect.Petrified, StatusEffect.Silence);
            Hero autoLifeTarget = CheckAllyWithoutStatus(StatusEffect.AutoLife);
            Hero braveTarget = CheckAllyWithoutStatus(StatusEffect.Brave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);

            if (TryBasicSupport(context)) return;
            if (TryRemedySupport(context)) return;

            if (TryAbility(Ability.Faith, faithTarget)) return;
            if (TryAbility(Ability.QuickCleanse, quickCleanseTarget)) return;
            if (TryAbility(Ability.AutoLife, autoLifeTarget)) return;
            if (TryAbility(Ability.Brave, braveTarget)) return;

            if (TryAbility(Ability.Attack, context.target)) return;
        }

        #endregion

        #region Wizard

        // Wizard priority:
        // 1. Support allies if needed
        // 2. Finish low HP enemies
        // 3. Use poison synergy when useful
        // 4. Use Faith-powered Meteor
        // 5. Apply debuffs
        // 6. Use magic attacks
        // 7. Attack
        public static void ProcessWizard(HeroContext context) {
            Hero poisonTarget = CheckEnemyWithoutStatus(StatusEffect.Poison);
            Hero doomTarget = CheckEnemyWithoutStatus(StatusEffect.Doom);
            Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow, HeroJobClass.Alchemist, HeroJobClass.Rogue);

            bool enemyHasFullRemedy = CheckEnemyForItem(Item.FullRemedy);
            bool bitterBloomAllies = CheckAlliesForJob(HeroJobClass.Monk, HeroJobClass.Rogue);
            bool hasFaith = HeroHasStatus(context.activeHero, StatusEffect.Faith);
            int poisonedEnemies = CountEnemiesWithStatus(StatusEffect.Poison);

            if (TryBasicSupport(context)) return;
            if (TryRemedySupport(context)) return;

            if (TryAbility(Ability.Fireball, context.lowHPEnemy)) return;

            if (bitterBloomAllies && poisonedEnemies <= 1 && TryAbility(Ability.PoisonNova, poisonTarget)) return;
            if (hasFaith && TryAbility(Ability.Meteor, context.target)) return;
            if (!enemyHasFullRemedy && TryAbility(Ability.Doom, doomTarget)) return;
            if (TryAbility(Ability.Slow, slowTarget)) return;

            if (TryAbility(Ability.Meteor, context.target)) return;
            if (TryAbility(Ability.MagicMissile, context.target)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
        }

        #endregion

        #region Rogue

        // Rogue priority:
        // 1. Support allies if needed
        // 2. Steal when enemy inventory has items
        // 3. Silence dangerous enemy classes
        // 4. Attack poisoned enemies
        // 5. Attack main target
        public static void ProcessRogue(HeroContext context) {
            Hero stealTarget = CheckEnemyWithoutStatus(StatusEffect.Petrifying, HeroJobClass.Alchemist);
            Hero silenceTarget = CheckEnemyWithoutStatus(StatusEffect.Silence, HeroJobClass.Wizard, HeroJobClass.Cleric, HeroJobClass.Alchemist, HeroJobClass.Fighter);
            Hero poisonedEnemy = CheckEnemyWithStatus(StatusEffect.Poison);

            if (TryBasicSupport(context)) return;
            if (TryRemedySupport(context)) return;

            if (!context.enemyInventoryEmpty && stealTarget != null && TryAbility(Ability.Steal, stealTarget)) return;
            if (TryAbility(Ability.SilenceStrike, silenceTarget)) return;
            if (TryAbility(Ability.Attack, poisonedEnemy)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
        }

        #endregion

        #region Monk

        // Monk priority:
        // 1. Revive / heal allies
        // 2. Restore mana if useful
        // 3. Remedy allies
        // 4. Pressure poisoned or low HP enemies
        // 5. Debuff enemies while healthy
        // 6. Flurry / Attack
        public static void ProcessMonk(HeroContext context) {
            bool maxManaBelowHalf = context.activeHero.maxMana <= 40;
            bool healthBelowHalf = context.activeHero.health < context.activeHero.maxHealth * 0.5f;

            Hero poisonedEnemy = CheckEnemyWithStatus(StatusEffect.Poison);
            Hero debraveTarget = CheckEnemyWithoutStatus(StatusEffect.Debrave, HeroJobClass.Fighter, HeroJobClass.Monk, HeroJobClass.Rogue);
            Hero defaithTarget = CheckEnemyWithoutStatus(StatusEffect.Defaith, HeroJobClass.Wizard, HeroJobClass.Cleric, HeroJobClass.Alchemist);

            if (TryAbility(Ability.Revive, context.deadAlly)) return;
            if (TryAbility(Ability.Potion, context.lowHPAlly)) return;

            if (context.lowMPAlly != null && (AllyHasItem(Item.Ether) || !maxManaBelowHalf)) {
                if (!maxManaBelowHalf) {
                    if (TryAbility(Ability.Chakra, context.lowMPAlly)) return;
                }
                else if (TryAbility(Ability.Ether, context.lowMPAlly)) return;
            }

            if (TryRemedySupport(context)) return;

            if (poisonedEnemy != null || context.lowHPEnemy != null) {
                if (TryAbility(Ability.FlurryOfBlows, context.target)) return;
                if (TryAbility(Ability.Attack, context.target)) return;
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

        #region Alchemist

        // Alchemist priority:
        // 1. Haste useful allies
        // 2. Support allies with items
        // 3. Cleanse dangerous statuses
        // 4. Craft missing key items
        // 5. Slow enemies
        // 6. Attack
        public static void ProcessAlchemist(HeroContext context) {
            Hero hasteTarget = CheckAllyWithoutStatus(StatusEffect.Haste, HeroJobClass.Alchemist, HeroJobClass.Rogue);
            Hero cleanseTarget = CheckAllyWithStatus(StatusEffect.Silence, StatusEffect.Doom, StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Slow, StatusEffect.Debrave);
            Hero slowTarget = CheckEnemyWithoutStatus(StatusEffect.Slow);

            if (TryAbility(Ability.Haste, hasteTarget)) return;

            if (TryBasicSupport(context)) return;
            if (TryAbility(Ability.Cleanse, cleanseTarget)) return;

            if (!AllyHasItem(Item.Revive) && TryAbility(Ability.CraftRevive, context.activeHero)) return;
            if (!AllyHasItem(Item.Ether) && TryAbility(Ability.CraftEther, context.activeHero)) return;
            if (!AllyHasItem(Item.Potion) && TryAbility(Ability.CraftPotion, context.activeHero)) return;

            if (TryAbility(Ability.Slow, slowTarget)) return;
            if (TryAbility(Ability.Attack, context.target)) return;
        }

        #endregion

        //=========================================================================================
        //                                  CONTEXT BUILDING
        //=========================================================================================

        private static HeroContext BuildHeroContext() {
            HeroContext context = new HeroContext {
                activeHero = TeamHeroCoder.BattleState.heroWithInitiative,
                essenceAmount = TeamHeroCoder.BattleState.allyEssenceCount,
                enemyInventoryEmpty = IsEnemyInventoryEmpty()
            };

            SetRemedyTargets(context);
            SetAllyTargets(context);
            SetEnemyTargets(context);

            return context;
        }

        private static void SetRemedyTargets(HeroContext context) {
            context.silenceRemTarget = CheckAllyWithStatus(StatusEffect.Silence);
            context.poisonRemTarget = CheckAllyWithStatus(StatusEffect.Poison);
            context.petrifyRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying);
            context.fullRemTarget = CheckAllyWithStatus(StatusEffect.Petrified, StatusEffect.Petrifying, StatusEffect.Doom, StatusEffect.Silence);
        }

        private static void SetAllyTargets(HeroContext context) {
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if (hero.health <= 0) {
                    context.deadAlly = hero;
                    continue;
                }

                if (IsLowHealthAlly(hero) && IsLowerHealth(hero, context.lowHPAlly)) {
                    context.lowHPAlly = hero;
                }

                if (IsLowManaAlly(hero) && IsLowerMana(hero, context.lowMPAlly)) {
                    context.lowMPAlly = hero;
                }
            }
        }

        private static void SetEnemyTargets(HeroContext context) {
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if (hero.health <= 0) continue;

                if (hero.health < hero.maxHealth * 0.3f) {
                    context.lowHPEnemy = hero;
                }

                if (IsLowerHealth(hero, context.target)) {
                    context.target = hero;
                }
            }
        }

        private static bool IsLowHealthAlly(Hero hero) {
            return hero.health < hero.maxHealth * 0.3f && !HeroHasStatus(hero, StatusEffect.AutoLife);
        }

        private static bool IsLowManaAlly(Hero hero) {
            return AllyHasItem(Item.Ether) && hero.mana < hero.maxMana * 0.3f;
        }

        private static bool IsLowerHealth(Hero hero, Hero currentTarget) {
            return currentTarget == null || hero.health < currentTarget.health;
        }

        private static bool IsLowerMana(Hero hero, Hero currentTarget) {
            return currentTarget == null || hero.mana < currentTarget.mana;
        }

        private static bool IsEnemyInventoryEmpty() {
            foreach (InventoryItem item in TeamHeroCoder.BattleState.foeInventory) {
                if (item.count > 0) return false;
            }

            return true;
        }

        //=========================================================================================
        //                                  TARGET SEARCH HELPERS
        //=========================================================================================

        public static Hero CheckEnemyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            return FindHeroWithoutStatus(TeamHeroCoder.BattleState.foeHeroes, status, jobs);
        }

        public static Hero CheckAllyWithoutStatus(StatusEffect status, params HeroJobClass[] jobs) {
            return FindHeroWithoutStatus(TeamHeroCoder.BattleState.allyHeroes, status, jobs);
        }

        private static Hero FindHeroWithoutStatus(System.Collections.Generic.IEnumerable<Hero> heroes, StatusEffect status, params HeroJobClass[] jobs) {
            if (jobs.Length > 0) {
                foreach (HeroJobClass job in jobs) {
                    foreach (Hero hero in heroes) {
                        if (hero.health <= 0) continue;
                        if (hero.jobClass != job) continue;
                        if (!HeroHasStatus(hero, status)) return hero;
                    }
                }

                return null;
            }

            foreach (Hero hero in heroes) {
                if (hero.health <= 0) continue;
                if (!HeroHasStatus(hero, status)) return hero;
            }

            return null;
        }

        public static Hero CheckEnemyWithStatus(params StatusEffect[] statuses) {
            return FindHeroWithStatus(TeamHeroCoder.BattleState.foeHeroes, statuses);
        }

        public static Hero CheckAllyWithStatus(params StatusEffect[] statuses) {
            return FindHeroWithStatus(TeamHeroCoder.BattleState.allyHeroes, statuses);
        }

        private static Hero FindHeroWithStatus(System.Collections.Generic.IEnumerable<Hero> heroes, params StatusEffect[] statuses) {
            foreach (Hero hero in heroes) {
                if (hero.health <= 0) continue;
                if (HeroHasStatus(hero, statuses)) return hero;
            }

            return null;
        }

        public static int CountEnemiesWithStatus(params StatusEffect[] statuses) {
            int count = 0;

            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes) {
                if (hero.health <= 0) continue;
                if (HeroHasStatus(hero, statuses)) count++;
            }

            return count;
        }

        public static bool CheckAlliesForJob(params HeroJobClass[] jobs) {
            if (jobs.Length == 0) return false;

            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes) {
                if (hero.health <= 0) continue;
                if (jobs.Contains(hero.jobClass)) return true;
            }

            return false;
        }

        public static bool HeroHasStatus(Hero hero, params StatusEffect[] statuses) {
            foreach (StatusEffectAndDuration statusData in hero.statusEffectsAndDurations) {
                if (statuses.Contains(statusData.statusEffect)) return true;
            }

            return false;
        }

        //=========================================================================================
        //                                  INVENTORY HELPERS
        //=========================================================================================

        public static bool CheckEnemyForItem(Item desiredItem) {
            return InventoryHasItem(TeamHeroCoder.BattleState.foeInventory, desiredItem);
        }

        public static bool CheckAllyForItem(Item desiredItem) {
            return InventoryHasItem(TeamHeroCoder.BattleState.allyInventory, desiredItem);
        }

        public static bool AllyHasItem(Item item) {
            return CheckAllyForItem(item);
        }

        private static bool InventoryHasItem(System.Collections.Generic.IEnumerable<InventoryItem> inventory, Item desiredItem) {
            return inventory.Any(item => item.item == desiredItem && item.count > 0);
        }

        //=========================================================================================
        //                                  ACTION HELPERS
        //=========================================================================================

        public static bool TryAbility(Ability ability, Hero target) {
            if (target == null) return false;
            if (!Utility.AreAbilityAndTargetLegal(ability, target, true)) return false;

            LogDecision(ability, target);
            TeamHeroCoder.PerformHeroAbility(ability, target);
            return true;
        }

        public static bool TryAbilities(Hero target, params Ability[] abilities) {
            foreach (Ability ability in abilities) {
                if (TryAbility(ability, target)) return true;
            }

            return false;
        }

        public static bool TryBasicSupport(HeroContext context) {
            if (TryAbilities(context.deadAlly, Ability.Resurrection, Ability.Revive)) return true;

            if (context.activeHero.jobClass == HeroJobClass.Cleric) {
                bool hasFaith = HeroHasStatus(context.activeHero, StatusEffect.Faith);

                if (hasFaith && TryAbility(Ability.QuickHeal, context.lowHPAlly)) return true;
            }

            if (TryAbilities(context.lowHPAlly, Ability.CureSerious, Ability.Potion)) return true;
            if (TryAbility(Ability.Ether, context.lowMPAlly)) return true;

            return false;
        }

        public static bool TryRemedySupport(HeroContext context) {
            if (TryAbility(Ability.PetrifyRemedy, context.petrifyRemTarget)) return true;
            if (TryAbility(Ability.PoisonRemedy, context.poisonRemTarget)) return true;
            if (TryAbility(Ability.SilenceRemedy, context.silenceRemTarget)) return true;
            if (TryAbility(Ability.FullRemedy, context.fullRemTarget)) return true;

            return false;
        }

        //=========================================================================================
        //                                  DEBUG / DECISION LOGGING
        //=========================================================================================

        private static void LogTurnStart(HeroContext context) {
            Console.WriteLine($"Processing AI: {context.activeHero.jobClass} | HP: {context.activeHero.health}/{context.activeHero.maxHealth} | MP: {context.activeHero.mana}/{context.activeHero.maxMana}");
        }

        private static void LogDecision(Ability ability, Hero target) {
            Console.WriteLine($"AI Decision: using {ability} on {target.jobClass} | Target HP: {target.health}/{target.maxHealth}");
        }
    }
}
