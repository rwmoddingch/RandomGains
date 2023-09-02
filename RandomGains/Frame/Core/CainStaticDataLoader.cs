using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal class CainStaticDataLoader
    {
        static List<GainStaticData> staticData = new List<GainStaticData>();
        static Dictionary<GainID, GainStaticData> idDataMapping = new Dictionary<GainID, GainStaticData>();
        public static void Load(RainWorld rainWorld){
            string rootPath = AssetManager.ResolveFilePath("gainassets/cardinfos");
        }
        static void LoadInDirectory(DirectoryInfo directoryInfo, RainWorld rainWorld){
            foreach(var directory in directoryInfo.GetDirectories()){
                LoadInDirectory(directory, rainWorld);
            }

            foreach(var file in directoryInfo.GetFiles()){
                if(file.Extension.EndsWith("json")){
                    try
                    {
                        var data = new GainStaticData(directoryInfo, file, rainWorld);
                        staticData.Add(data);
                        idDataMapping.Add(data.GainID, data);
                    }
                    catch(Exception ex) 
                    {
                        EmgTxCustom.Log($"CainStaticDataLoader : exception when trying to load at {directoryInfo.FullName}");
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public static GainStaticData GetStaticData(GainID gainID){
            if(idDataMapping.TryGetValue(gainID, out GainStaticData data)){
                return data;
            }
            return null;
        }
    }

    public class GainStaticData
    {
        public GainID GainID { get; private set; }

        public string faceElementName;
        public FAtlasElement faceElement;
        public string gainName;
        public string gainDescription;

        public GainStaticData(DirectoryInfo directoryInfo, FileInfo jsonFile, RainWorld rainWorld){
            string text = File.ReadAllText(jsonFile.FullName);
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

            string dir = directoryInfo.FullName.Split(new []{"cardinfos"}, StringSplitOptions.None)[1];
            string imagePath = $"gainassets/cardinfos{dir}/{data["faceName"]}";
            //faceElement = Futile.atlasManager.LoadImage(imagePath).elements[0];
            faceElementName = faceElement.name;

            GainID = new GainID(data["gainID"]);
            gainName = data["gainName"];
            gainDescription = data["gainDescription"];

            EmgTxCustom.Log($"CainStaticDataLoader : load static data:\nname : {gainName}\ndescription : {gainDescription}\nfaceName : {faceElementName}");
        }
    }

}
