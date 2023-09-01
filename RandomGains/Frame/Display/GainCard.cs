using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace RandomGains.Frame
{
    public class GainCardTexture
    {
        class GainCardImpl : MonoBehaviour
        {
            public GainCardImpl()
            {
            }

            public void Update()
            {
                rotation += Time.deltaTime*10;
                card.cardObject.transform.rotation = Quaternion.Euler(0, rotation,0);
            }

            public float rotation;
            public GainCardTexture card;
        }

        public void Destroy()
        {
            GameObject.Destroy(cardObject);
            GameObject.Destroy(cameraObject);
            RenderTexture.ReleaseTemporary(Texture);
            Texture = null;
        }

    


        public GainCardTexture()
        {
            count++;

            cardObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cardObject.GetComponent<MeshRenderer>().sharedMaterial =
                new Material(Custom.rainWorld.Shaders["Basic"].shader);
           cameraObject = new GameObject("GainCard_Camera");

            camera = cameraObject.AddComponent<Camera>();
            
            cameraObject.AddComponent<Transform>();
            camera.targetTexture = Texture;

            gainCard = cardObject.AddComponent<GainCardImpl>();
            gainCard.card = this;
            cardObject.AddComponent<Transform>();

            cameraObject.transform.position = new Vector3( count * 700, -10000f, -100f);
            cardObject.transform.position = new Vector3(count * 700, -10000f, -100f + 2f);
            cardObject.transform.localScale = new Vector3(0.6f, 1f);
            
        }

        public RenderTexture Texture { get; private set; } = RenderTexture.GetTemporary(500, 300);

        private Camera camera;
        private GainCardImpl gainCard;

        private GameObject cardObject;
        private GameObject cameraObject;

        private static int count = 0;
    }
}
