using Godot;
using Godot.Collections;

namespace Game.Utils;

public static partial class GameUtils
{
    public static T[] ParseVectorArrayString<T>(string input, System.Func<float, float, T> factory)
    {
        input = input.Trim('[', ']');

        var matches = MyRegex().Matches(input);

        var list = new System.Collections.Generic.List<T>();
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            float x = float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            float y = float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
            list.Add(factory(x, y));
        }

        return [.. list];
    }

    public static Vector2[] ParseVector2ArrayString(string input)
    {
        return ParseVectorArrayString(input, (x, y) => new Vector2(x, y));
    }

    public static Vector2I[] ParseVector2IArrayString(string input)
    {
        return ParseVectorArrayString(input, (x, y) => new Vector2I(Mathf.RoundToInt(x), Mathf.RoundToInt(y)));
    }

    public static Vector2 ParseVector2(string input)
    {
        return GD.StrToVar("Vector2" + input).As<Vector2>();
    }

    public static Vector3 ParseVector3(string input)
    {
        return GD.StrToVar("Vector3" + input).As<Vector3>();
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\((-?\d+\.?\d*),\s*(-?\d+\.?\d*)\)")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();

    public static void SaveSpriteFramesToDictionary(Dictionary<string, SpriteFrames> spriteFramesDict, Dictionary rawDict)
    {
        foreach (var (k, v) in spriteFramesDict)
        {
            rawDict[k] = v.ResourcePath;
        }
    }

    public static Dictionary<string, SpriteFrames> LoadSpriteFramesFromDictionary(Dictionary rawDict)
    {
        var typedSpriteFrames = new Dictionary<string, SpriteFrames>();

        foreach (var (k, v) in rawDict)
        {
            if (k.Obj is string key && v.Obj is string path)
            {
                if (path?.Length == 0)
                {
                    GD.PrintErr("Empty path for SpriteFrames");
                    continue;
                }

                var frames = ResourceLoader.Load<SpriteFrames>(path);
                if (frames != null)
                    typedSpriteFrames[key] = frames;
                else
                    GD.PrintErr($"Failed to load SpriteFrames at path: {path}");
            }
        }

        return typedSpriteFrames;
    }

    public static Dictionary<string, Variant> LoadDictionaryFromJSON(string FilePath)
    {
        if (FileAccess.FileExists(FilePath))
        {
            var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
            string jsonContent = file.GetAsText();
            var jsonParser = new Json();
            Error error = jsonParser.Parse(jsonContent);

            if (error == Error.Ok)
            {
                var data = jsonParser.GetData();
                if (data is Variant variant)
                {
                    file.Close();
                    return variant.AsGodotDictionary<string, Variant>();
                }
                else
                {
                    GD.Print("Parsed JSON is not a Dictionary<string, Variant>.");
                    file.Close();
                    return [];
                }
            }
            else
            {
                GD.Print("Error parsing character JSON: ", error);
                file.Close();
            }
        }
        else
        {
            GD.Print("File not found: ", FilePath);
        }

        return [];
    }

    public static Variant[] LoadArrayFromJSON(string FilePath)
    {
        if (FileAccess.FileExists(FilePath))
        {
            var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
            string jsonContent = file.GetAsText();
            var jsonParser = new Json();
            Error error = jsonParser.Parse(jsonContent);

            if (error == Error.Ok)
            {
                var data = jsonParser.GetData();
                if (data is Variant variant)
                {
                    file.Close();
                    var arr = variant.AsGodotArray<Variant>();
                    var result = new Variant[arr.Count];
                    for (int i = 0; i < arr.Count; i++)
                        result[i] = arr[i];
                    return result;
                }
                else
                {
                    GD.Print("Parsed JSON is not an Array.");
                    file.Close();
                    return [];
                }
            }
            else
            {
                GD.Print("Error parsing JSON: ", error);
                file.Close();
            }
        }
        else
        {
            GD.Print("File not found: ", FilePath);
        }

        return [];
    }

    public static void SaveResource(Resource resource, string filePath)
    {
        var baseDir = filePath.GetBaseDir();

        if (!DirAccess.DirExistsAbsolute(baseDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(baseDir);
        }

        ResourceSaver.Save(resource, filePath);
    }
}
