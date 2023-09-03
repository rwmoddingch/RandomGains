using RandomGains.Frame.Display;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RewiredConsts.Layout;
using Random = UnityEngine.Random;
namespace RandomGains.Frame
{
   
    /// <summary>
    /// 渲染部分
    /// </summary>
    internal partial class GainCard
    {
        public class GainCardTexture
        {

            public void Destroy()
            {
                GameObject.Destroy(cardObjectA);
                GameObject.Destroy(cardObjectB);
                GameObject.Destroy(cameraObject);
                RenderTexture.ReleaseTemporary(Texture);
                Texture = null;
            }


            public GainCardTexture()
            {
                count++;

            
                cameraObject = new GameObject("GainCard_Camera");

                camera = cameraObject.AddComponent<Camera>();

                cameraObject.AddComponent<Transform>();
                cameraObject.layer = 9;
                camera.targetTexture = Texture;
                camera.cullingMask = 1 << 9;

                cardObjectA = CreateRenderQuad(true);
                cardObjectB = CreateRenderQuad(false);

                titleObject = CreateTextMesh(true, Plugins.TitleFont);
                descObject = CreateTextMesh(false, Plugins.TitleFont,0.7f,Color.white);
                cameraObject.transform.position = CurrentSetPos;

                Title = "测试卡牌";
                Description = "卡片内容因何而发生？ 一般来讲，我们都必须务必慎重的考虑考虑。 我认为， 现在，解决卡片内容的问题，是非常非常重要的";
            }

            private GameObject CreateRenderQuad(bool isSideA)
            {
                var re = GameObject.CreatePrimitive(PrimitiveType.Quad);
                re.layer = 9;
                re.GetComponent<MeshRenderer>().sharedMaterial =
                    new Material(Custom.rainWorld.Shaders[Plugins.ModID + "CardBack"].shader);
                if (!isSideA)
                {
                    re.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",
                        Futile.atlasManager.GetAtlasWithName(Plugins.MoonBack).texture);

                }
                re.transform.position = CurrentSetPos + new Vector3(0,0, 1.3f);
                re.transform.localScale = new Vector3(0.6f* (isSideA ? 1 : -1f) , 1f,1f);
                return re;
            }

            private GameObject CreateTextMesh(bool isSideA, Font font, float size = 1f, Color? color = null, string text = "")
            {
                var re = new GameObject("GainCard_Text");
                re.layer = 9;

                re.AddComponent<Transform>();
                re.transform.parent = cardObjectA.transform;
                re.transform.localPosition = new Vector3(0, (isSideA ? -1 : 1) * 0.5f, -0.01f * (isSideA ? 1 : -1));
                re.AddComponent<TextMesh>().font = font;
                re.GetComponent<TextMesh>().text = text;
                re.GetComponent<TextMesh>().anchor = isSideA ? TextAnchor.LowerCenter : TextAnchor.UpperCenter;
                re.GetComponent<TextMesh>().alignment = (isSideA ? TextAlignment.Center : TextAlignment.Left);
                Plugins.DescFont.dynamic = true;
                re.GetComponent<TextMesh>().fontSize = 100;
                if (!color.HasValue) color = Color.black;
                re.GetComponent<TextMesh>().color = color.Value;
                re.GetComponent<TextMesh>().characterSize = 0.01f * size;
                re.AddComponent<MeshRenderer>();
                re.GetComponent<MeshRenderer>().material.renderQueue = 3998 + (isSideA ? 0 : 1);
                re.transform.localScale = new Vector3((isSideA ? 1 : -1f) / 0.6f, 1f, 1f);

                return re;
            }

            private string LayoutText(string text,Font font, float characterSize, int fontSize, FontStyle style)
            {
                font.RequestCharactersInTexture(text, fontSize, style);
                StringBuilder builder = new StringBuilder();
                float width = 0;
                foreach (var c in text)
                {
                    if (font.GetCharacterInfo(c, out var info, fontSize))
                    {
                        var currentSize = info.advance * characterSize * 0.1f / 0.6f;
                        if (width + currentSize > 1)
                        {
                            width = 0;
                            builder.Append('\n');
                        }

                        builder.Append(c);
                        width += currentSize;
                    }
                }

                return builder.ToString();
            }

            private Vector3 CurrentSetPos => new Vector3(count * 10 + 100f, -100f, -100f);

            /// <summary>
            /// 渲染贴图
            /// </summary>
            public RenderTexture Texture { get; private set; } = RenderTexture.GetTemporary(900, 540);

            public Vector3 Rotation
            {
                get => cardObjectA.transform.rotation.eulerAngles;
                set
                {
                    cardObjectB.transform.rotation = Quaternion.Euler(value.x, value.y, value.z);
                    cardObjectA.transform.rotation = Quaternion.Euler(value.x, value.y, value.z);
                }
            }

            public bool IsSideA
            {
                get => cardObjectA.GetComponent<MeshRenderer>().enabled;

                set
                {
                    cardObjectA.GetComponent<MeshRenderer>().enabled = value;
                    titleObject.GetComponent<MeshRenderer>().enabled = value;
                    cardObjectB.GetComponent<MeshRenderer>().enabled = !value;
                    descObject.GetComponent<MeshRenderer>().enabled = !value;
                }
            }

            public string Title
            {
                get => titleObject.GetComponent<TextMesh>().text;
                set => titleObject.GetComponent<TextMesh>().text = value;
            }

            public string Description
            {
                get => descObject.GetComponent<TextMesh>().text;
                set
                {
                    var mesh = descObject.GetComponent<TextMesh>();
                    mesh.text = LayoutText(value, mesh.font,mesh.characterSize,mesh.fontSize,mesh.fontStyle);
                } 
            }

            public float DescAlpha
            {
                get => cardObjectB.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Lerp");
                set => cardObjectB.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Lerp", value);
            }

            public void UpdateVisible()
            {
                IsSideA = cardObjectA.transform.forward.z > 0;
            }
            private Camera camera;

            private GameObject cardObjectA, cardObjectB;
            private GameObject titleObject, descObject;
            private GameObject cameraObject;

            private static int count = 0;
        }



        public GainCard()
        {
            container = new FContainer();
            cardTexture = new GainCardTexture();
        }


        public FContainer InitiateSprites(bool autoAddContainer = true)
        {
            sprites = new FSprite[1];
            sprites[0] = new FTexture(cardTexture.Texture);
            if (autoAddContainer)
                AddToContainer(null);
            return container;
        }

        public void AddToContainer(FContainer container)
        {
            if (container == null)
                container = this.container;

            foreach(var sprite in sprites) 
                container.AddChild(sprite);
        }

        public void Update()
        {
            RotateUpdate();
            InputUpdate();
        }

        public void DrawSprites(float timeStacker)
        {
            sprites[0].SetPosition(Vector2.Lerp(lastPos,pos,timeStacker));
            cardTexture.Rotation = LerpRotation(timeStacker);
            cardTexture.DescAlpha =Mathf.InverseLerp(100,180,Vector3.Lerp(rotationLast,rotationLerp,timeStacker).y);
        }

        public void Destroy()
        {
            cardTexture.Destroy();
            container.RemoveAllChildren();
        }

        public FContainer container;
        FSprite[] sprites;
        public GainCardTexture cardTexture;
    }

    internal partial class GainCard
    {
   
        public void RotateUpdate()
        {
            _mouseOnRotationLast = _mouseOnRotationSmooth;
            _mouseOnRotationSmooth = Vector3.Lerp(_mouseOnRotationLast, _mouseOnRotation, 0.2f);

            rotationLast = rotationLerp;
            rotationLerp = Vector3.Lerp(rotationLerp, rotation, 0.2f);

            lastPos = pos;

            cardTexture.UpdateVisible();
        }



        public Vector2 GetFrameVertice(int index)
        {
            return new Vector2(origVertices[index].x, origVertices[index].y) * size + pos;
        }

        public float size;

        public Vector2 lastPos;
        public Vector2 pos;

        public float Light { get; private set; }

        private Vector3 _mouseOnRotationLast;
        private Vector3 _mouseOnRotation;
        private Vector3 _mouseOnRotationSmooth;

        private Vector3 rotation;
        private Vector3 rotationLerp;
        private Vector3 rotationLast;

        private Vector3 Rotation
        {
            get => rotationLerp + _mouseOnRotationSmooth;
            set
            {
                if (rotation == value)
                    return;
                rotation = value;
            }
        }

        private Vector3 LerpRotation(float timeStacker)
        {
            return Vector3.Lerp(rotationLast + _mouseOnRotationSmooth, Rotation, timeStacker);
        }

        //12
        //43
        private Vector3[] origVertices = new Vector3[4] { new Vector3(-3, 5, 0f), new Vector3(3, 5, 0f), new Vector3(3, -5, 0f), new Vector3(-3, -5, 0f) };

    }

    /// <summary>
    /// 输入部分
    /// </summary>
    internal partial class GainCard
    {
        
        public void InputUpdate()
        {
            lastMouseInside = MouseInside;
            lastClick = click;

            click = Input.GetMouseButton(0);
            MouseInside = CheckMouseInside();

            if (MouseInside && !lastMouseInside && OnMoueCardEnter != null)
                OnMoueCardEnter.Invoke();
            if (!MouseInside && lastMouseInside && OnMoueCardExit != null)
                OnMoueCardExit.Invoke();
            if (!lastClick && click)
            {
                MouseOnClick();
                OnMouseCardClick?.Invoke();
            }

            MouseOnAnim();
        }

        private void MouseOnClick()
        {
            Rotation = cardTexture.IsSideA ? new Vector3(0, 180, 0) : new Vector3(0, 0, 0);
        }
        private void MouseOnAnim()
        {
            if (remainMoveCounter > 0)
                remainMoveCounter--;
            if (MouseInside)
                remainMoveCounter = 20;

            Vector2 midMousePos = MouseInside ? MouseLocalPos - new Vector2(0.5f, 0.5f) : Vector2.zero;
            _mouseOnRotation = new Vector3(-20 * midMousePos.x, 20 * midMousePos.y, 0f);
        }

        bool CheckMouseInside()
        {
            Vector3 mousePos = new Vector3(Futile.mousePosition.x, Futile.mousePosition.y, 0f);
            Vector3[] checkPoints = new Vector3[4]; 

            for(int i = 0; i < 4; i++)
            {
                checkPoints[i] = new Vector3(GetFrameVertice(i).x, GetFrameVertice(i).y, 0f);
            }
            Vector3 norm = Vector3.Cross((checkPoints[3] - checkPoints[2]), (checkPoints[1] - checkPoints[2]));

            bool result = true;
            result = result && CheckLeft(checkPoints[3], checkPoints[2], mousePos, norm);
            result = result && CheckLeft(checkPoints[2], checkPoints[1], mousePos, norm);
            result = result && CheckLeft(checkPoints[1], checkPoints[0], mousePos, norm);
            result = result && CheckLeft(checkPoints[0], checkPoints[3], mousePos, norm);

            Vector3 delta = mousePos - checkPoints[0];
            Vector3 dirA = (checkPoints[3] - checkPoints[0]).normalized;
            float lA = (checkPoints[3] - checkPoints[0]).magnitude;

            Vector3 dirB = (checkPoints[1] - checkPoints[0]).normalized;
            float lB = (checkPoints[1] - checkPoints[0]).magnitude;

            float dLa = Vector3.Dot(delta, dirA);
            float dLb = Vector3.Dot(delta, dirB);

            MouseLocalPos = new Vector2(dLa / lA, dLb / lB);

            return result;
        }

        bool CheckLeft(Vector3 a, Vector3 b, Vector3 mouse, Vector3 norm)
        {
            return Vector3.Dot(Vector3.Cross((a - b), (mouse - b)), norm) > 0f;
        }

        public event Action OnMouseCardClick;
        public event Action OnMoueCardEnter;
        public event Action OnMoueCardExit;
        public event Action OnMoueCardUpdate;

        public Vector2 MouseLocalPos { get; private set; }

        private bool lastMouseInside;
        public bool MouseInside { get; private set; }

        private int remainMoveCounter;

        private bool lastClick;
        private bool click;
    }
}
