using UnityEditor;

public sealed class BuildRequest
{
    public BuildTarget BuildTarget;
    public string OutputDirectory;
    public string ResourceOutputDirectory;
    public string FileName;
    public string[] Scenes;
    public string Defines;
    public string VersionName;
    public int VersionCode;
    public bool Development;
    public ResourceBuildMode ResourceMode;
    public int PlayMode;
    public string Language;
    public string PackageVersion;
    public string Decryption;
    public string Encryption;
}
