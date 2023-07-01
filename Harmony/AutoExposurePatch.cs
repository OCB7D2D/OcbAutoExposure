using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

public class AutoExposurePatch
{

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(WorldEnvironment), "AmbientSpectrumFrameUpdate")]
    private static class WorldEnvironmentAmbientSpectrumFrameUpdatePatch
    {
        static bool Prefix(World ___world, EntityPlayerLocal ___localPlayer,
            ref float ___insideCurrent, ref float ___dataAmbientInsideSpeed, ref float ___dataAmbientInsideThreshold,
            ref Vector2 ___dataAmbientEquatorScale, ref Vector2 ___dataAmbientGroundScale,
            ref Vector2 ___dataAmbientInsideEquatorScale, ref Vector2 ___dataAmbientInsideGroundScale,
            ref Vector2 ___dataAmbientMoon, ref Vector2 ___dataAmbientSkyScale,
            ref Vector2 ___dataAmbientSkyDesat, ref Vector2 ___dataAmbientInsideSkyScale)
        {
            if (___world == null || ___world.BiomeAtmosphereEffects == null) return false;
            float target = ___localPlayer == null ? 0.0f : ___localPlayer.Stats.LightInsidePer;

            // Inside measure is quite shaky, the timer here helps to make the transition more smooth
            ___insideCurrent = Mathf.MoveTowards(___insideCurrent, target, ___dataAmbientInsideSpeed * Time.deltaTime);

            float dayPercent = SkyManager.dayPercent;

            Color skyColor = SkyManager.GetSkyColor();
            Color fogColor = SkyManager.GetFogColor();
            Color sunColor = SkyManager.GetSunLightColor();

            Color skyLightColor = Color.LerpUnclamped(skyColor, new Color(0.7f, 0.7f, 0.7f, 1f),
                Mathf.LerpUnclamped(___dataAmbientSkyDesat.y, ___dataAmbientSkyDesat.x, dayPercent));

            float ambientSkyFactor = Mathf.LerpUnclamped(
                Mathf.LerpUnclamped(___dataAmbientSkyScale.y, ___dataAmbientSkyScale.x, dayPercent),
                Mathf.LerpUnclamped(___dataAmbientInsideSkyScale.y, ___dataAmbientInsideSkyScale.x, dayPercent),
                ___insideCurrent);

            float ambientEquatorFactor = Mathf.LerpUnclamped(
                Mathf.LerpUnclamped(___dataAmbientEquatorScale.y, ___dataAmbientEquatorScale.x, dayPercent),
                Mathf.LerpUnclamped(___dataAmbientInsideEquatorScale.y, ___dataAmbientInsideEquatorScale.x, dayPercent),
                ___insideCurrent);

            float ambientGroundOutside = Mathf.LerpUnclamped(___dataAmbientGroundScale.y, ___dataAmbientGroundScale.x, dayPercent);
            float ambientGroundInside = Mathf.LerpUnclamped(___dataAmbientInsideGroundScale.y, ___dataAmbientInsideGroundScale.x, dayPercent);
            float ambientGroundFactor = Mathf.LerpUnclamped(ambientGroundOutside, ambientGroundInside, ___insideCurrent); // * num3;


            RenderSettings.ambientSkyColor = skyLightColor * ambientSkyFactor;
            RenderSettings.ambientEquatorColor = fogColor * ambientEquatorFactor;
            RenderSettings.ambientGroundColor = sunColor * ambientGroundFactor;

            WorldEnvironment.AmbientTotal = SkyManager.GetLuma(skyColor) * ambientSkyFactor
                + SkyManager.GetLuma(sunColor) * ambientGroundOutside;

            float OutsideFactor = Mathf.LerpUnclamped(
                OcbAutoExposureConfig.ExposureFactorOutAndInsideNight.x,
                OcbAutoExposureConfig.ExposureFactorOutAndInsideDay.x,
                dayPercent);

            float InsideFactor = Mathf.LerpUnclamped(
                OcbAutoExposureConfig.ExposureFactorOutAndInsideNight.y,
                OcbAutoExposureConfig.ExposureFactorOutAndInsideDay.y,
                dayPercent);

            OcbAutoExposureConfig.ParamsAE1.x = Mathf.LerpUnclamped(
                OutsideFactor, InsideFactor, ___insideCurrent);

            return false;
        }
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch]
    private static class AutoExposureRenderPatch
    {
        // Target class is sealed, so use reflection
        static MethodInfo TargetMethod() => AccessTools
            .TypeByName("AutoExposureRenderer").GetMethod("Render");

        // Main function handling the IL patching
        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldstr) continue;
                if (!"_ScaleOffsetRes".Equals(codes[i].operand)) continue;
                for (int n = i; n < codes.Count && n < i + 12; n++)
                {
                    if (codes[n].opcode != OpCodes.Brfalse) continue;
                    var param1 = AccessTools.Field(typeof(OcbAutoExposureConfig), "ParamsAE1");
                    var param2 = AccessTools.Field(typeof(OcbAutoExposureConfig), "ParamsAE2");
                    codes.InsertRange(n, new List<CodeInstruction>()
                    {
                        // Insert setter IL for `ParamsAE`
                        new CodeInstruction(codes[i - 2].opcode, codes[i - 2].operand), // push var `command`
                        new CodeInstruction(codes[i - 1].opcode, codes[i - 1].operand), // push var `autoExposure`
                        new CodeInstruction(codes[i - 0].opcode, "_ParamsAE1"), // push new shader parameter name
                        new CodeInstruction(OpCodes.Ldsfld, param1), // push new `Vector4` compute shader parameter
                        new CodeInstruction(codes[n - 1].opcode, codes[n - 1].operand), // call `SetComputeVectorParam`
                        // Insert setter IL for `ParamsAE2`
                        new CodeInstruction(codes[i - 2].opcode, codes[i - 2].operand), // push var `command`
                        new CodeInstruction(codes[i - 1].opcode, codes[i - 1].operand), // push var `autoExposure`
                        new CodeInstruction(codes[i - 0].opcode, "_ParamsAE2"), // push new shader parameter name
                        new CodeInstruction(OpCodes.Ldsfld, param2), // push new `Vector4` compute shader parameter
                        new CodeInstruction(codes[n - 1].opcode, codes[n - 1].operand), // call `SetComputeVectorParam`
                    }); ;
                    return codes;
                }
            }
            return codes;
        }
    }

    // ####################################################################
    // ####################################################################

}
