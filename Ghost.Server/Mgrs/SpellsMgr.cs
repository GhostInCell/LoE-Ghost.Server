using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs
{
    public static class SpellsMgr
    {
        public static bool CanCast(MapPlayer player, TargetEntry target)
        {
            DB_Spell spell; DB_SpellEffect main; WorldObject targetWO;
            if (DataMgr.Select(target.SkillID, out spell) &&
                spell.Effects.TryGetValue(0, out main) &&
                player.Stats.Energy >= main.Data01)
            {
                if (target.HasGuid)
                {
                    if (player.Server.Objects.TryGetNVObject((ushort)target.Guid, out targetWO) && targetWO.IsSpawned)
                    {
                        if (Vector3.Distance(targetWO.Position, player.Object.Position) <= main.Data02)
                        {
                            var isSelf = targetWO == player.Object;
                            if ((main.Target & SpellTarget.Creature) != 0)
                            {
                                if (targetWO.IsPlayer)
                                {
                                    if ((main.Target & SpellTarget.Self) != 0 && isSelf)
                                        return true;
                                    else if ((main.Target & SpellTarget.Player) != 0)
                                        return targetWO.IsPlayer && !isSelf;
                                }
                                else
                                    return targetWO.IsCreature && (targetWO as CreatureObject).HasStats;
                            }
                            else if ((main.Target & SpellTarget.Player) != 0)
                            {
                                if (isSelf)
                                    return (main.Target & SpellTarget.Self) != 0;
                                return targetWO.IsPlayer;
                            }
                            else if ((main.Target & SpellTarget.Self) != 0)
                                return isSelf;
                        }
                    }
                }
                else if ((main.Target & SpellTarget.Position) != 0)
                    return Vector3.Distance(target.Position, player.Object.Position) <= main.Data02;
            }
            return false;
        }
        public static void PerformSkill(MapPlayer player, TargetEntry target)
        {
            DB_Spell spell = DataMgr.SelectSpell(target.SkillID); bool area = false, magick;
            DB_SpellEffect main = spell.Effects[0]; WorldObject targetWO;
            player.View.PerformSkill(target);
            player.Skills.AddCooldown(spell.ID, main.Data03);
            player.Stats.ModCurren(Stats.Energy, -main.Data01);
            if (target.HasGuid)
            {
                player.Server.Objects.TryGetNVObject((ushort)target.Guid, out targetWO);
                foreach (var item in spell.Effects.Values)
                {
                    float effect = item.BaseConst + (item.LevelModifer * player.Object.Stats.Level) + (item.AttackModifer * player.Stats.Attack);
                    switch (item.Type)
                    {
                        case SpellEffectType.Damage:
                        case SpellEffectType.MagickDamage:
                            if (targetWO != player.Object)
                                (targetWO as CreatureObject).Stats.DoDamage(player.Object, effect, item.Type == SpellEffectType.MagickDamage);
                            break;
                        case SpellEffectType.FrontDamage:
                        case SpellEffectType.MagicFrontDamage:
                            magick = item.Type == SpellEffectType.MagicFrontDamage;
                            Vector3 direction = Vector3.Transform(Vector3.UnitZ, Quaternion.CreateFromAxisAngle(Vector3.UnitY, player.Object.Rotation.Y));
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in player.Server.Objects.GetMobsInRadius(player.Object.Position + direction * item.Data01, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(player.Object, effect, magick);
                            break;
                        case SpellEffectType.SplashDamage:
                        case SpellEffectType.MagicSplashDamage:
                            magick = item.Type == SpellEffectType.MagicSplashDamage;
                            if ((item.Target & SpellTarget.NotMain) == 0 && targetWO != player.Object)
                                (targetWO as CreatureObject).Stats.DoDamage(player.Object, effect, magick);
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in player.Server.Objects.GetMobsInRadiusExcept(player.Object, targetWO, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(player.Object, effect, magick);
                            break;
                        case SpellEffectType.Heal:
                            (targetWO as CreatureObject).Stats.DoHeal(player.Object, effect);
                            break;
                        case SpellEffectType.AreaInit:
                            if (!area)
                            {
                                new WO_VoidZone(player.Object, spell, item);
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
                    float effect = item.BaseConst + (item.LevelModifer * player.Object.Stats.Level) + (item.AttackModifer * player.Stats.Attack);
                    switch (item.Type)
                    {
                        case SpellEffectType.AreaInit:
                            if (!area)
                            {
                                new WO_VoidZone(player.Object, spell, item);
                                area = true;
                            }
                            break;
                        case SpellEffectType.FrontDamage:
                        case SpellEffectType.MagicFrontDamage:
                            magick = item.Type == SpellEffectType.MagicFrontDamage;
                            Vector3 direction = Vector3.Transform(Vector3.UnitZ, Quaternion.CreateFromAxisAngle(Vector3.UnitY, player.Object.Rotation.Y));
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in player.Server.Objects.GetMobsInRadius(player.Object.Position + direction * item.Data01, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(player.Object, effect, magick);
                            break;
                        case SpellEffectType.SplashDamage:
                        case SpellEffectType.MagicSplashDamage:
                            magick = item.Type == SpellEffectType.MagicFrontDamage;
                            if ((item.Target & SpellTarget.Creature) != 0)
                                foreach (var entry in player.Server.Objects.GetMobsInRadius(player.Object, item.Data02).ToArray())
                                    entry.Stats?.DoDamage(player.Object, effect, magick);
                            break;
                    }
                }
            }
        }
    }
}