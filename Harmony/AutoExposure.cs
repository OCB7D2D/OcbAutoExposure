using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OcbAutoExposure : IModApi
{

    public static ComputeShader compute;

    // ####################################################################
    // ####################################################################

    public void InitMod(Mod mod)
    {
        if (GameManager.IsDedicatedServer) return;
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        var ShaderBundle = System.IO.Path.Combine(
            mod.Path, "Resources/AutoExposure.unity3d");
        AssetBundleManager.Instance.LoadAssetBundle(ShaderBundle);
        compute = AssetBundleManager.Instance.Get<ComputeShader>(ShaderBundle, "AutoExposure");
    }

    // ####################################################################
    // ####################################################################

    private static void SetAutoExposure()
    {
        if (!(Camera.main?.transform?.FindInChilds("WeaponCamera") is Transform main)) return;
        if (!(main.GetComponent<Camera>() is Camera camera)) return;
        if (!(camera.GetComponent<PostProcessVolume>() is PostProcessVolume processor)) return;
        if (!(processor?.profile?.GetSetting<AutoExposure>() is AutoExposure ae)) return;
        // ae.keyValue.Override(99999f); // not used anymore, has been replaced
        ae.minLuminance.Override(OcbAutoExposureConfig.ExposureLuminanceRange.x);
        ae.maxLuminance.Override(OcbAutoExposureConfig.ExposureLuminanceRange.y);
        ae.filtering.Override(OcbAutoExposureConfig.ExposureFilterRange);
        ae.speedDown.Override(OcbAutoExposureConfig.ExposureDownUpSpeed.x);
        ae.speedUp.Override(OcbAutoExposureConfig.ExposureDownUpSpeed.y);
    }

    [HarmonyPatch(typeof(GameRenderManager), "ApplyCameraOptions", new System.Type[] { })]
    private static class GameRenderManagerApplyCameraOptionsPatch
    {
        static void Postfix() => SetAutoExposure();
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(PostProcessLayer), "RenderBuiltins")]
    private static class PostProcessLayerRenderBuiltinsPatch
    {
        static void Prefix(PostProcessRenderContext context) =>
            context.resources.computeShaders.autoExposure = compute;
    }

    // ####################################################################
    // ####################################################################

}
