namespace CoreMcp.Tools.System.Drives;

public sealed record DriveInformation(
    string Name,
    long Used,
    long Free);
