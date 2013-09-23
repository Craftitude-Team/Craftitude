namespace Craftitude
{
    public enum PackageAction : byte
    {
        Uninstall = 1,
        Purge = 2,
        Install = 3,
        Update = 4,
        Configure = 5
    }
}