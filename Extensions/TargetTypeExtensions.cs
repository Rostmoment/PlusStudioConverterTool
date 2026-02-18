using PlusStudioConverterTool.Models;

namespace PlusStudioConverterTool.Extensions;

internal static class TargetTypeExtensions
{
    public static string ToExtension(this TargetType type) => type switch
    {
        TargetType.BLDtoEBPL => ".bld",
        TargetType.CBLDtoBLD => ".cbld",
        TargetType.CBLDtoRBPL => ".cbld",
        TargetType.RBPLtoEBPL => ".rbpl",
        TargetType.PBPLtoEBPL => ".pbpl",
        TargetType.BPLtoEBPL => ".bpl",
        TargetType.PBPLtoLUA => ".pbpl",
        _ => string.Empty
    };
    public static TargetType ToTarget(string from, string to)
    {
        string fromExt = Path.GetExtension(from);
        string toExt = Path.GetExtension(to);

        return (fromExt.ToLowerInvariant(), toExt.ToLowerInvariant()) switch
        {
            (".bld", ".ebpl") => TargetType.BLDtoEBPL,

            (".cbld", ".bld") => TargetType.CBLDtoBLD,
            (".cbld", ".rbpl") => TargetType.CBLDtoRBPL,

            (".rbpl", ".ebpl") => TargetType.RBPLtoEBPL,

            (".pbpl", ".ebpl") => TargetType.PBPLtoEBPL,
            (".pbpl", ".lua") => TargetType.PBPLtoLUA,

            (".bpl", ".ebpl") => TargetType.BPLtoEBPL,

            _ => throw new ArgumentException($"Cannot convert {from} to {to}")
        };
    }

}