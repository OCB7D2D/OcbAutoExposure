using HarmonyLib;
using UnityEngine;

public class OcbAutoExposureConfig
{

    // ####################################################################
    // ####################################################################

    public static Vector4 ParamsAE1 = new Vector4(0, 0.3f, 1.95f, 5f);
    public static Vector4 ParamsAE2 = new Vector4(0.25f, 7.5f, 1f, 1f);

    // ####################################################################
    // ####################################################################

    public static Vector2 ExposureDownUpSpeed = new Vector2(2.5f, 1.5f);
    public static Vector2 ExposureLuminanceRange = new Vector2(-9f, 5.5f);
    public static Vector2 ExposureFilterRange = new Vector2(15f, 90f);
    public static Vector2 ExposureFactorOutAndInsideDay = new Vector2(0.85f, 0.35f);
    public static Vector2 ExposureFactorOutAndInsideNight = new Vector2(0.25f, 0.5f);
    public static float ExposureOffset = 1.03f; // global offset for logarithm
    public static float ExposureLogBase = Mathf.Log(2f); // logarithmic scaler
    public static float ExposureFallOffFactor = 1.95f; // falloff factor
    public static float ExposureFactor = 1f; // factor after all calcs
    public static float ExposurePower = 1f; // power after all calcs
    public static float ExposureMin = 0.25f; // absolute max exposure
    public static float ExposureMax = 7.5f; // absolute min exposure

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(WorldEnvironment), "OnXMLChanged")]
    private static class AutoExposureConfigPatch
    {
        static void Postfix()
        {
            if (WorldEnvironment.Properties is DynamicProperties properties)
            {
                // Read parameters for existing auto exposure compute shader
                // Will be synced on `SetAutoExposure` to the existing structs
                properties.ParseVec("exposureDownUpSpeed", ref ExposureDownUpSpeed);
                properties.ParseVec("exposureLuminanceRange", ref ExposureLuminanceRange);
                properties.ParseVec("exposureFilterRange", ref ExposureFilterRange);
                properties.ParseVec("exposureFactorOutAndInsideDay", ref ExposureFactorOutAndInsideDay);
                properties.ParseVec("exposureFactorOutAndInsideNight", ref ExposureFactorOutAndInsideNight);
                // Read parameters for auto exposure compute shader
                properties.ParseFloat("exposureFactor", ref ExposureFactor);
                properties.ParseFloat("exposurePower", ref ExposurePower);
                properties.ParseFloat("exposureMin", ref ExposureMin);
                properties.ParseFloat("exposureMax", ref ExposureMax);
                properties.ParseFloat("exposureOffset", ref ExposureOffset);
                properties.ParseFloat("exposureLogBase", ref ExposureLogBase);
                properties.ParseFloat("exposureFallOffFactor", ref ExposureFallOffFactor);
                // Copy parameters over to our vector
                // A bit "convoluted", but it works
                ParamsAE1.w = ExposureOffset;
                ParamsAE1.y = ExposureLogBase;
                ParamsAE1.z = ExposureFallOffFactor;
                ParamsAE2.x = ExposureMin;
                ParamsAE2.y = ExposureMax;
                ParamsAE2.z = ExposureFactor;
                ParamsAE2.w = ExposurePower;
            }
        }
    }

    // ####################################################################
    // ####################################################################

}
