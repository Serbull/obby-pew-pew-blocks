#if UNITY_EDITOR
using UnityEditor;
using YG.EditorScr;
#if PaymentsXsolla_yg
using Xsolla.Core;
#endif

namespace YG.Insides
{
    [InitializeOnLoad]
    public static class CG_Editor
    {
        public const string XSOLLA_DEFINE = "PaymentsXsolla_yg";

        static CG_Editor()
        {
            Undo.postprocessModifications += OnModifications;
            UpdateXsollaDefine();
        }

        private static UndoPropertyModification[] OnModifications(UndoPropertyModification[] modifications)
        {
            foreach (var mod in modifications)
            {
                if (mod.currentValue?.target is InfoYG)
                {
                    if (mod.currentValue.propertyPath.Contains("useXsolla"))
                    {
                        UpdateXsollaDefine();
                    }
                    else
                    {
                        UpdateXsollaSettings();
                    }
                }
            }
            return modifications;
        }

        public static void UpdateXsollaDefine()
        {
            var info = YG2.infoYG;

            if (info == null || PlatformSettings.currentPlatformBaseName != "CrazyGames" || !info.platformInfo.useXsolla)
            {
                DefineSymbols.RemoveDefine(XSOLLA_DEFINE);
            }
            else
            {
                DefineSymbols.AddDefine(XSOLLA_DEFINE);
            }
        }

        private static void UpdateXsollaSettings()
        {
            var info = YG2.infoYG;

            if (info != null)
            {
#if PaymentsXsolla_yg
                XsollaSettings.StoreProjectId = info.platformInfo.xsollaProjectId;
                XsollaSettings.IsSandbox = info.platformInfo.xsollaIsSandbox;
#endif
            }
        }
    }
}
#endif
