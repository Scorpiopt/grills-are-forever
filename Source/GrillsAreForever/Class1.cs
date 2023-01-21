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
    internal class CompProperties_SecondLayer : CompProperties
    {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600000A RID: 10 RVA: 0x0000228E File Offset: 0x0000048E
        public float Altitude
        {
            get
            {
                return this.altitudeLayer.AltitudeFor();
            }
        }

        // Token: 0x0600000B RID: 11 RVA: 0x0000229B File Offset: 0x0000049B
        public CompProperties_SecondLayer()
        {
            this.compClass = typeof(CompSecondLayer);
        }

        // Token: 0x04000009 RID: 9
        public GraphicData graphicData;

        // Token: 0x0400000A RID: 10
        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;
    }

    internal class CompSecondLayer : ThingComp
    {
        // Token: 0x17000009 RID: 9
        // (get) Token: 0x0600001E RID: 30 RVA: 0x00002973 File Offset: 0x00000B73
        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)this.props;
            }
        }

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x0600001F RID: 31 RVA: 0x00002980 File Offset: 0x00000B80
        public virtual Graphic Graphic
        {
            get
            {
                if (this.graphicInt == null)
                {
                    if (this.Props.graphicData == null)
                    {
                        ThingDef def = this.parent.def;
                        Log.ErrorOnce(((def != null) ? def.ToString() : null) + " has no SecondLayer graphicData but we are trying to access it.", 764532);
                        return BaseContent.BadGraphic;
                    }
                    this.graphicInt = this.Props.graphicData.GraphicColoredFor(this.parent);
                }
                return this.graphicInt;
            }
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000029F8 File Offset: 0x00000BF8
        public override void PostDraw()
        {
            base.PostDraw();
            this.Graphic.Draw(GenThing.TrueCenter(this.parent.Position, this.parent.Rotation, this.parent.def.size, this.Props.Altitude), this.parent.Rotation, this.parent, 0f);
        }

        // Token: 0x0400000F RID: 15
        private Graphic graphicInt;
    }

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

    [HarmonyPatch(typeof(CompRottable), "CompInspectStringExtra")]
    public static class CompInspectStringExtra_Patch
    {
        public static bool Prefix(CompRottable __instance, ref string __result)
        {
            Log.Message("__instance.parent.StoringThing(): " + __instance.parent.StoringThing());
            if (__instance.parent.StoringThing() is BBQ_Storage storage && storage.compPowerTrader.PowerOn)
            {
                __result = CompInspectStringExtra(__instance);
                return false;
            }
            return true;
        }

        public static string CompInspectStringExtra(CompRottable __instance)
        {
            if (!__instance.Active)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            switch (__instance.Stage)
            {
                case RotStage.Fresh:
                    stringBuilder.Append("RotStateFresh".Translate() + ".");
                    break;
                case RotStage.Rotting:
                    stringBuilder.Append("RotStateRotting".Translate() + ".");
                    break;
                case RotStage.Dessicated:
                    stringBuilder.Append("RotStateDessicated".Translate() + ".");
                    break;
            }
            return stringBuilder.ToString();
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
