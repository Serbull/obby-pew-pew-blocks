namespace YG.Insides
{
    public partial class PlatformInfo
    {
        [Platform("CrazyGames")]
        public bool useXsolla;

#if PaymentsXsolla_yg
        [Platform("CrazyGames")]
        public string xsollaProjectId;

        [Platform("CrazyGames")]
        public bool xsollaIsSandbox = true;
#endif
    }
}
