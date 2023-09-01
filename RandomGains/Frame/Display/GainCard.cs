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
    public class GainCardTexture
    {
        class GainCardImpl : MonoBehaviour
        {
            public GainCardImpl()
            {
            }

            public void Update()
            {
                rotation += Time.deltaTime * 10;
                card.cardObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
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
    
    internal partial class GainCard
    {
        public FContainer container;
        FSprite[] sprites;
        FCustomLabel label;

        Color color;

        public GainCard()
        {
            container = new FContainer();
        }

        public FContainer InitiateSprites(bool autoAddContainer = true)
        {
            sprites = new FSprite[2];

            sprites[0] = new CustomFSprite("pixel");
            sprites[1] = new CustomFSprite(Plugins.MoonBack);
            label = new FCustomLabel(Custom.GetDisplayFont(), "测试卡牌");
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
            container.AddChild(label);
            label.MoveToFront();
        }

        public void Update()
        {
            RotateUpdate();
            InputUpdate();
        }

        public void DrawSprites(float timestacker)
        {
            (sprites[0] as CustomFSprite).isVisible = SideA;
            for (int i = 0; i < 4; i++)
            {
                (sprites[0] as CustomFSprite).MoveVertice(i, GetReferenceVertice(i, timestacker));
                (sprites[0] as CustomFSprite).verticeColors[i] = color;
            }

            (sprites[1] as CustomFSprite).isVisible = SideB;
            for (int i = 0; i < 4; i++)
            {
                (sprites[1] as CustomFSprite).MoveVertice(i, GetReferenceVertice(i, timestacker));
                (sprites[1] as CustomFSprite).verticeColors[i] = Color.white * Light;
            }

            label.isVisible = SideA;
            label.SetVertice(0, GetReferenceVertice(3, timestacker));
            label.SetVertice(1, GetReferenceVertice(0, timestacker) * 0.2f + GetReferenceVertice(3, timestacker) * 0.8f);
            label.SetVertice(2, GetReferenceVertice(1, timestacker) * 0.2f + GetReferenceVertice(2, timestacker) * 0.8f);
            label.SetVertice(3, GetReferenceVertice(2, timestacker));
            label.color = Color.black;
            container.RemoveFromContainer();
            foreach(var sprite in sprites)
            {
                sprite.RemoveFromContainer();
            }
        }
    }

    internal partial class GainCard
    {
        public float size;

        public bool SideA { get; private set; }//卡牌A面是否渲染
        public bool SideB { get; private set; }//卡牌B面是否渲染

        public Vector2 lastPos;
        public Vector2 pos;

        public float Light { get; private set; }
        Vector3 _norm;

        Vector3 _mouseOnRotationLast;
        Vector3 _mouseOnRotation;
        Vector3 _mouseOnRotationSmooth;

        Vector3 _rotation;
        Vector3 Rotation
        {
            get => _rotation + _mouseOnRotationSmooth;
            set
            {
                if (_rotation == value)
                    return;
                _rotation = value;
                _isRotationDirty = true;
            }
        }

        //12
        //43
        Vector3[] origVertices = new Vector3[4] { new Vector3(-3, 5, 0f), new Vector3(3, 5, 0f), new Vector3(3, -5, 0f), new Vector3(-3, -5, 0f) };
        Vector3[] vertices = new Vector3[4];
        Vector3[] lastVertices = new Vector3[4];
        
        bool _isRotationDirty = true;

        public void RotateUpdate()
        {
            for(int i = 0; i < 4; i++)
                lastVertices[i] = vertices[i];

            _mouseOnRotationLast = _mouseOnRotationSmooth;
            _mouseOnRotationSmooth = Vector3.Lerp(_mouseOnRotationLast, _mouseOnRotation, 0.2f);

            lastPos = pos;

            if (_isRotationDirty)
                Update3DVertices();
        }

        public void Update3DVertices()
        {
            for(int i = 0;i < 4; i++)
            {
                Vector3 v = origVertices[i];

                v = RotateRound(v, Vector3.forward, Rotation.z, Vector3.zero);
                v = RotateRound(v, Vector3.right, Rotation.x, Vector3.zero);
                v = RotateRound(v, Vector3.up, Rotation.y, Vector3.zero);

                //apply sim perspective
                Vector3 delta = v * 0.005f * v.magnitude * v.z;
                v += delta;

                vertices[i] = v;
            }

            _norm = Vector3.Cross((vertices[1] - vertices[0]), (vertices[3] - vertices[0])).normalized;
            SideA = _norm.z < 0;
            SideB = _norm.z >= 0;

            Light = Vector3.Dot(_norm, new Vector3(0f, 0f, 1f));

            _isRotationDirty = false;
        }

        public Vector3 RotateRound(Vector3 position, Vector3 axis, float angle, Vector3 center)
        {
            return Quaternion.AngleAxis(angle, axis) * (position - center) + center;
        }

        public Vector2 GetReferenceVertice(int index, float timestacker)
        {
            Vector3 smoothed = Vector3.Lerp(lastVertices[index], vertices[index], timestacker);
            return new Vector2(smoothed.x, smoothed.y) * size + Vector2.Lerp(lastPos, pos, timestacker);
        }

        public Vector2 GetFrameVertice(int index)
        {
            return new Vector2(origVertices[index].x, origVertices[index].y) * size + pos;
        }
    }

    internal partial class GainCard
    {
        public event Action OnMouseCardClick;
        public event Action OnMoueCardEnter;
        public event Action OnMoueCardExit;
        public event Action OnMoueCardUpdate;

        public Vector2 MouseLocalPos { get; private set; }

        bool _lastMouseInside;
        public bool MouseInside { get; private set; }

        int remainMoveCounter;

        public void InputUpdate()
        {
            _lastMouseInside = MouseInside;
            MouseInside = CheckMouseInside();

            if (MouseInside && !_lastMouseInside && OnMoueCardEnter != null)
                OnMoueCardEnter.Invoke();
            if (!MouseInside && _lastMouseInside && OnMoueCardExit != null)
                OnMoueCardExit.Invoke();

            color = MouseInside ? Color.red : Color.white;

            MouseOnAnim();
        }

        void MouseOnAnim()
        {
            if (remainMoveCounter > 0)
                remainMoveCounter--;
            if (MouseInside)
                remainMoveCounter = 20;

            Vector2 midMousePos = MouseInside ? MouseLocalPos - new Vector2(0.5f, 0.5f) : Vector2.zero;
            _mouseOnRotation = new Vector3(-360 * midMousePos.x, 360 * midMousePos.y, 0f);
            _isRotationDirty = _isRotationDirty || (remainMoveCounter > 0);
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
    }
}
