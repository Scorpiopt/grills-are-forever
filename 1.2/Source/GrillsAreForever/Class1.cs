using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GrillsAreForever
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("GrillsAreForever.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Thing), "AmbientTemperature", MethodType.Getter)]
    public static class AmbientTemperature_Patch
    {
        public static void Postfix(Thing __instance, ref float __result)
        {
            if (__instance.StoringThing() is BBQ_Storage storage && storage.compPowerTrader.PowerOn)
            {
                __result = 20;
            }
        }
    }

    [HarmonyPatch(typeof(CompRottable), "RotProgress", MethodType.Setter)]
    public static class ShouldTakeRotDamage_Patch
    {
        public static void Postfix(CompRottable __instance)
        {
            if (__instance.parent.StoringThing() is BBQ_Storage storage && storage.compPowerTrader.PowerOn)
            {
                RotStage stage = __instance.Stage;
                __instance.rotProgressInt = 0;
                if (stage != __instance.Stage)
                {
                    __instance.StageChanged();
                }
            }
        }
    }
    public class BBQ_Storage : Building_Storage
    {
        public CompPowerTrader compPowerTrader;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = this.TryGetComp<CompPowerTrader>();
        }
    }
}
