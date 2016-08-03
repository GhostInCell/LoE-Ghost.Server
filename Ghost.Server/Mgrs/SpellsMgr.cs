using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs
{
    public static class SpellsMgr
    {
        public static bool CanCast(CreatureObject creature, TargetEntry target)
        {
            DB_Spell spell; DB_SpellEffect main; CreatureObject targetCO;
            if (DataMgr.Select(target.SpellID, out spell) &&
                spell.Effects.TryGetValue(0, out main) &&
                creature.Stats.Energy >= main.Data01)
            {
                if (target.HasGuid)
                {
                    if (creature.Server.Objects.TryGetCreature((ushort)target.Guid, out targetCO) && !targetCO.IsDead)
                    {
                        if (Vector3.DistanceSquared(targetCO.Position, creature.Position) <= main.Data02 * main.Data02)
                        {
                            var isSelf = targetCO == creature;
                            if ((main.Target & SpellTarget.Creature) != 0)
                            {
                                if (targetCO.IsPlayer)
                                {
                                    if ((main.Target & SpellTarget.Self) != 0 && isSelf)
                                        return true;
                                    else if ((main.Target & SpellTarget.Player) != 0)
                                        return targetCO.IsPlayer && !isSelf;
                                }
                                else
                                    return targetCO.HasStats;
                            }
                            else if ((main.Target & SpellTarget.Player) != 0)
                            {
                                if (isSelf)
                                    return (main.Target & SpellTarget.Self) != 0;
                                return targetCO.IsPlayer;
                            }
                            else if ((main.Target & SpellTarget.Self) != 0)
                                return isSelf;
                        }
                    }
                }
                else if ((main.Target & SpellTarget.Position) != 0)
                    return Vector3.DistanceSquared(target.Position, creature.Position) <= main.Data02 * main.Data02;
            }
            return false;
        }
        public static void PerformSkill(CreatureObject creature, TargetEntry target)
        {
            DB_Spell spell = DataMgr.SelectSpell(target.SpellID); bool area = false, magick;
            DB_SpellEffect main = spell.Effects[0]; CreatureObject targetCO;
            creature.View.PerformSkill(target);
            if (creature.IsPlayer)
            {
                ((WO_Player)creature).Player.Skills.AddCooldown(spell.ID, main.Data03);
            }
            creature.Stats.DecreaseCurrent(Stats.Energy, main.Data01);
            if (target.HasGuid)
            {
                creature.Server.Objects.TryGetCreature((ushort)target.Guid, out targetCO);
                foreach (var item in spell.Effects.Values)
                {
                    float effect = item.BaseConst + (item.LevelModifer * creature.Stats.Level) + (item.AttackModifer * creature.Stats.Attack);
                    switch (item.Type)
                    {
                        case SpellEffectType.Damage:
                        case SpellEffectType.MagickDamage:
                            if (targetCO != creature)
                                targetCO.Stats.DoDamage(creature, effect, item.Type == SpellEffectType.MagickDamage);
                            break;
                        case SpellEffectType.FrontAreaDamage:
                        case SpellEffectType.MagicFrontAreaDamage:
                            magick = item.Type == SpellEffectType.MagicFrontAreaDamage;
                            Vector3 direction = MathHelper.GetDirection(creature);
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in creature.Server.Objects.GetMobsInRadius(creature.Position + direction * item.Data01, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(creature, effect, magick);
                            break;
                        case SpellEffectType.SplashDamage:
                        case SpellEffectType.MagicSplashDamage:
                            magick = item.Type == SpellEffectType.MagicSplashDamage;
                            if ((item.Target & SpellTarget.NotMain) == 0 && targetCO != creature)
                                targetCO.Stats.DoDamage(creature, effect, magick);
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in creature.Server.Objects.GetMobsInRadiusExcept(creature, targetCO, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(creature, effect, magick);
                            break;
                        case SpellEffectType.Heal:
                            targetCO.Stats.DoHeal(creature, effect);
                            break;
                        case SpellEffectType.Modifier:
                            targetCO.Stats.AddModifier((Stats)item.Data01, effect, item.Data03, item.Data02 == 1);
                            break;
                        case SpellEffectType.Teleport:
                            creature.Movement.Teleport(targetCO.Position);
                            break;
                        case SpellEffectType.AreaInit:
                            if (!area)
                            {
                                new WO_VoidZone(creature, spell, item);
                                area = true;
                            }
                            break;
                    }
                }
            }
            else
            {
                foreach (var item in spell.Effects.Values)
                {
                    float effect = item.BaseConst + (item.LevelModifer * creature.Stats.Level) + (item.AttackModifer * creature.Stats.Attack);
                    switch (item.Type)
                    {
                        case SpellEffectType.Teleport:
                            creature.Movement.Teleport(target.Position);
                            break;
                        case SpellEffectType.AreaInit:
                            if (!area)
                            {
                                new WO_VoidZone(creature, spell, item);
                                area = true;
                            }
                            break;
                        case SpellEffectType.FrontAreaDamage:
                        case SpellEffectType.MagicFrontAreaDamage:
                            magick = item.Type == SpellEffectType.MagicFrontAreaDamage;
                            Vector3 direction = MathHelper.GetDirection(creature);
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in creature.Server.Objects.GetMobsInRadius(creature.Position + direction * item.Data01, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(creature, effect, magick);
                            break;
                        case SpellEffectType.SplashDamage:
                        case SpellEffectType.MagicSplashDamage:
                            magick = item.Type == SpellEffectType.MagicSplashDamage;
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in creature.Server.Objects.GetMobsInRadius(creature, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(creature, effect, magick);
                            break;
                    }
                }
            }
        }
    }
}