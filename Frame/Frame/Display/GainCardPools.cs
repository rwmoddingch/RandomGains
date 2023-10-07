using RandomGains.Frame.Core;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal static class GainCardTexturePool
    {
        static Queue<GameObject> renderQuadPool = new Queue<GameObject>();
        static Queue<GameObject> cameraObjectPool = new Queue<GameObject>();

        static Queue<GameObject> textMeshObjectPool = new Queue<GameObject>();

        public static GameObject GetTextMesh(GameObject parent, bool isSideA, Font font, float size = 1f, Color? color = null, string text = "")
        {
            GameObject result;
            if (textMeshObjectPool.Count > 0)
            {
                result = textMeshObjectPool.Dequeue();
                result.SetActive(true);
                result.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                result = new GameObject("GainCard_Text");
                result.layer = 9;
                result.AddComponent<TextMesh>();
                result.AddComponent<MeshRenderer>();
            }
            result.transform.parent = parent.transform;
            result.transform.localPosition = new Vector3(0, (isSideA ? -1 : 1) * 0.5f, -0.01f * (isSideA ? 1 : -1));
            result.GetComponent<TextMesh>().font = font;
            result.GetComponent<TextMesh>().text = text;
            result.GetComponent<TextMesh>().anchor = isSideA ? TextAnchor.LowerCenter : TextAnchor.UpperCenter;
            result.GetComponent<TextMesh>().alignment = (isSideA ? TextAlignment.Center : TextAlignment.Left);
            //Plugins.DescFont.dynamic = true;
            result.GetComponent<TextMesh>().fontSize = 100;
            if (!color.HasValue) color = Color.black;
            result.GetComponent<TextMesh>().color = color.Value;
            result.GetComponent<TextMesh>().characterSize = 0.01f * size;
            result.GetComponent<MeshRenderer>().material = font.material;
            result.GetComponent<MeshRenderer>().material.renderQueue = 3998 + (isSideA ? 0 : 1);
            result.transform.localScale = new Vector3((isSideA ? 1 : -1f) / 0.6f, 1f, 1f);
            return result;
        }

        public static void ReleaseTextMesh(GameObject textmesh)
        {
            textmesh.SetActive(false);
            textMeshObjectPool.Enqueue(textmesh);
        }
    
        public static GameObject GetCameraObject(RenderTexture target)
        {
            GameObject result;
            if(cameraObjectPool.Count > 0)
            {
                result = cameraObjectPool.Dequeue();
                result.SetActive(true);
            }
            else
            {
                result = new GameObject("GainCard_Camera");
                result.AddComponent<Camera>();
                result.layer = 9;
                result.GetComponent<Camera>().cullingMask = 1 << 9;
            }
            result.GetComponent<Camera>().targetTexture = target;

            return result;
        }
        
        public static void ReleaseCameraObject(GameObject cameraObject)
        {
            cameraObject.SetActive(false);
            cameraObjectPool.Enqueue(cameraObject);
        }
    
        public static GameObject GetRenderQuad(bool isSideA, GainStaticData staticData, Vector3 pos)
        {
            GameObject result;
            if(renderQuadPool.Count > 0)
            {
                result = renderQuadPool.Dequeue();
                result.SetActive(true);
                result.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                result = GameObject.CreatePrimitive(PrimitiveType.Quad);
                result.GetComponent<MeshRenderer>().material =
                   new Material(Custom.rainWorld.Shaders[Plugins.ModID + "CardBack"].shader);
                result.layer = 9;
            }
            
            if (!isSideA)
            {
                result.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",
                    Futile.atlasManager.GetAtlasWithName(Plugins.BackElementOfType(staticData.GainType)).texture);
            }
            else
            {
                if (staticData.faceElement?.atlas?.texture != null)
                    result.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",
                        staticData.faceElement.atlas.texture);
            }
            result.transform.position = pos + new Vector3(0, 0, 1.171f);
            result.transform.localScale = new Vector3(0.6f * (isSideA ? 1 : -1f), 1f, 1f);
            return result;
        }
    
        public static void ReleaseRenderQuad(GameObject quad)
        {
            quad.SetActive(false);
            renderQuadPool.Enqueue(quad);
        }
    }
}
