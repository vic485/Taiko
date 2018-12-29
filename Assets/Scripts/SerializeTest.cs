using System.IO;
using DataModels;
using Newtonsoft.Json;
using UnityEngine;

public class SerializeTest : MonoBehaviour
{
    private void Start()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "000-君にふれて", "mapinfo.json");

        var jsonData = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<BeatmapInfo>(jsonData);
    }

}
