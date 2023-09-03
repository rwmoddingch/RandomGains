using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RWCustom;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal class GainStaticDataLoader
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

        public GainType GainType{ get; private set; }
        public GainProperty GainProperty{ get; private set; }
        public readonly bool triggerable;

        public readonly string faceElementName;
        public readonly FAtlasElement faceElement;
        public readonly string gainName;
        public readonly string gainDescription;
        public readonly Color color;

        public GainStaticData(DirectoryInfo directoryInfo, FileInfo jsonFile, RainWorld rainWorld){
            string text = File.ReadAllText(jsonFile.FullName);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);

            string dir = directoryInfo.FullName.Split(new []{"cardinfos"}, StringSplitOptions.None)[1];
            string imagePath = $"gainassets/cardinfos{dir}/{data["faceName"]}";
            faceElement = Futile.atlasManager.LoadImage(imagePath).elements[0];
            faceElementName = faceElement.name;

            GainID = new GainID(data["gainID"].ToString());
            GainType = Custom.ParseEnum<GainType>(data["gainType"].ToString());
            GainProperty = Custom.ParseEnum<GainProperty>(data["gainProperty"].ToString());
            triggerable = bool.Parse(data["triggerable"].ToString());
            gainName = data["gainName"].ToString();
            gainDescription = data["gainDescription"].ToString();

            color = Color.white;
            if(data.TryGetValue("color",out var colorVal)){
                ColorUtility.TryParseHtmlString(colorVal.ToString(), out color);
            }

            EmgTxCustom.Log($"CainStaticDataLoader : load static data:\nname : {gainName}\ntype : {GainType}\nproperty : {GainProperty}\ndescription : {gainDescription}\nfaceName : {faceElementName}");
        }
    }

}
