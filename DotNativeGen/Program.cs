using Newtonsoft.Json;
using System.Text;


// This is very bad

Console.WriteLine("Downloading natives...");
JsonNatives natives = await GetNatives();

StringBuilder sb = new();

// Header
sb.AppendLine("using GTA;");
sb.AppendLine("using GTA.Math;");
sb.AppendLine("using GTA.Native;\n");
sb.AppendLine("namespace GTA.Natives\n{");

// Content
Console.WriteLine("Generating code...");
sb.Append(JsonNativesToDot(natives));

// Footer
sb.AppendLine("}");

File.WriteAllText("Natives.cs", sb.ToString());
Console.WriteLine("Done...");

static string JsonNativesToDot(JsonNatives natives)
{
    return string.Join("\n",
        natives.Select(x => JsonNamespaceToDot(x.Key, x.Value)));
}

static string JsonNamespaceToDot(string name, IEnumerable<KeyValuePair<string, JsonNative>> natives)
{
    StringBuilder sb = new();

    sb.AppendLine($"\tpublic static class {name}\n\t{{");
    sb.Append(string.Join("\n", natives.Select(x => JsonNativeToDot(x.Key, x.Value))));
    sb.AppendLine("\t}");

    return sb.ToString();
}

static string JsonNativeToDot(string hash, JsonNative native)
{
    StringBuilder sb = new();

    // XML Comment
    sb.AppendLine("/// <summary>");
    if (string.IsNullOrEmpty(native.Comment))
        sb.Append("/// No description.\n");
    else
        sb.Append(string.Join("\n", native.Comment.Split('\n').Select(x => $"/// {x}")) + "\n");
    sb.AppendLine("/// </summary>");

    // Signature
    sb.Append($"public unsafe static {NormalizeType(native.Type, true)} {native.Name}(");
    sb.Append(string.Join(", ", native.Params.Select(x => $"{NormalizeType(x.Type)} {NormalizeParamName(x.Name)}")));
    sb.Append(")\n{\n");

    // Body
    if (native.Type == "void")
        sb.Append($"\tFunction.Call((Hash) {hash}");
    else
        sb.Append($"\treturn Function.Call<{NormalizeType(native.Type, true)}>((Hash) {hash}");
    sb.Append(string.Join(" ", native.Params.Select(x => ", " + NormalizeParamName(x.Name))));

    sb.Append(");\n}");

    // Return intended
    return string.Join("\n", sb.ToString().Split("\n").Select(x => $"\t\t{x}")) + "\n";
}

// Converts native type to C#
static string NormalizeType(string type, bool isReturn = false)
{
    if (type == "const char*") type = "string";
    if (type == "Any*") type = "int*"; // Pointer?
    if (type == "Any") type = "int";
    if (type == "BOOL") type = "bool";
    if (type == "ScrHandle") type = "int";
    if (type == "ScrHandle*") type = "int*";
    if (type == "Object") type = "int"; // object cant be used as input argument
    if (type == "object") type = "int"; // object cant be used as input argument
    if (type == "Cam") type = "int";
    if (type == "BOOL*") type = "bool*";
    if (type == "Interior") type = "int";
    if (type == "Object*") type = "int*";
    if (type == "object*") type = "int*";
    if (type == "Blip*") type = "int*";
    if (type == "Vehicle*") type = "int*";
    if (type == "Entity*") type = "int*";
    if (type == "Ped*") type = "int*";
    if (type == "FireId") type = "int";

    if (isReturn) type = type.Replace("*", "");

    return type;
}

// Resolves conflicting param names
static string NormalizeParamName(string name)
{
    if (name == "object") name = "_object";
    if (name == "override") name = "_override";
    if (name == "string") name = "_string";
    if (name == "event") name = "_event";
    if (name == "out") name = "_out";
    if (name == "base") name = "_base";

    return name;
}

static async Task<JsonNatives> GetNatives()
{
    using HttpClient http = new();
    var responce = await http.GetAsync(
        "https://raw.githubusercontent.com/alloc8or/gta5-nativedb-data/master/natives.json");
    var content = await responce.Content.ReadAsStringAsync();
    return JsonConvert.DeserializeObject<JsonNatives>(content);
}

namespace NATIVES
{
    public static class MATH
    {
        public static void SIN()
        {

        }
    }
}

class JsonNatives : Dictionary<string, Dictionary<string, JsonNative>>
{

}

class JsonNative
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; }

    [JsonProperty("return_type")]
    public string Type { get; set; }

    [JsonProperty("params")]
    public List<JsonNativeParam> Params { get; set; }
}

class JsonNativeParam
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}