﻿using Newtonsoft.Json.Linq;
using RandomGains.Frame.Core;
using RandomGains.Frame.Display;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using RandomGains.Frame.Display.GainCardAnimations;
using UnityEngine;
using static RewiredConsts.Layout;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices.ComTypes;
using RandomGains.Frame.Utils;

namespace RandomGains.Frame
{
    /// <summary>
    /// 渲染部分
    /// </summary>
    public partial class GainCard
    {
        public class GainCardTexture
        {
            public GainID ID;
            public GainStaticData StaticData;
            public GainCard card;

            public FTexture mainTexture;

            public void Destroy()
            {
                destroyed = true;
                GainCardTexturePool.ReleaseRenderQuad(cardObjectA);
                GainCardTexturePool.ReleaseRenderQuad(cardObjectB);

                GainCardTexturePool.ReleaseCameraObject(cameraObject);

                GainCardTexturePool.ReleaseTextMesh(titleObject);
                GainCardTexturePool.ReleaseTextMesh(descObject);
                GainCardTexturePool.ReleaseTextMesh(stackObject);

                RenderTexture.ReleaseTemporary(Texture);
                Texture = null;
            }

            ~GainCardTexture()
            {
                if (!destroyed)
                {
                    Destroy();
                }
            }

            public GainCardTexture(GainCard card ,GainID gainID)
            {
                ID = gainID;
                StaticData = GainStaticDataLoader.GetStaticData(ID);
                count++;

                Texture.filterMode = FilterMode.Point;
                cameraObject = GainCardTexturePool.GetCameraObject(Texture);

                cardObjectA = GainCardTexturePool.GetRenderQuad(true, StaticData, CurrentSetPos);
                cardObjectB = GainCardTexturePool.GetRenderQuad(false, StaticData, CurrentSetPos);

                titleObject = GainCardTexturePool.GetTextMesh(cardObjectA, true, Plugins.TitleFont,1f, StaticData.color);
                descObject = GainCardTexturePool.GetTextMesh(cardObjectA, false, Plugins.DescFont,0.7f,Color.white);
                stackObject = GainCardTexturePool.GetTextMesh(cardObjectA, true, Plugins.TitleFont, 0.7f, StaticData.color);
                stackObject.GetComponent<TextMesh>().anchor = TextAnchor.UpperRight;

                stackObject.transform.localPosition = new Vector3(0, 0.5f, -0.01f);
                cameraObject.transform.position = CurrentSetPos;

                if (StaticData.stackable)
                {
                    if (!GainSave.Singleton.dataMapping.ContainsKey(ID))
                    {
                        stackObject.GetComponent<TextMesh>().text = "1";
                    }
                    else if (GainSave.Singleton.GetData(ID).StackLayer != 0)
                    {
                        if (card.IsMenu)
                            stackObject.GetComponent<TextMesh>().text = GainSave.Singleton.GetData(ID).StackLayer + " + 1";
                        else
                            stackObject.GetComponent<TextMesh>().text = GainSave.Singleton.GetData(ID).StackLayer.ToString();
                    }
                }

                Title = StaticData.gainName;
                Description = StaticData.gainDescription;

                cardObjectA.GetComponent<MeshRenderer>().enabled = card.sideA;
                titleObject.GetComponent<MeshRenderer>().enabled = card.sideA;
                cardObjectB.GetComponent<MeshRenderer>().enabled = !card.sideA;
                descObject.GetComponent<MeshRenderer>().enabled = !card.sideA;
                this.card = card;
            }

            //private GameObject CreateRenderQuad(bool isSideA)
            //{
            //    var re = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //    re.layer = 9;
            //    re.GetComponent<MeshRenderer>().sharedMaterial =
            //        new Material(Custom.rainWorld.Shaders[Plugins.ModID + "CardBack"].shader);
            //    if (!isSideA)
            //    {
            //        re.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",
            //            Futile.atlasManager.GetAtlasWithName(Plugins.BackElementOfType(StaticData.GainType)).texture);
            //    }
            //    else
            //    {
            //        if(StaticData.faceElement?.atlas?.texture != null)
            //            re.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",
            //                StaticData.faceElement.atlas.texture);
            //    }
            //    re.transform.position = CurrentSetPos + new Vector3(0,0, 1.171f);
            //    re.transform.localScale = new Vector3(0.6f* (isSideA ? 1 : -1f) , 1f,1f);
            //    return re;
            //}

            //private GameObject CreateTextMesh(bool isSideA, Font font, float size = 1f, Color? color = null, string text = "")
            //{
            //    var re = new GameObject("GainCard_Text");
            //    re.layer = 9;

            //    re.AddComponent<Transform>();
            //    re.transform.parent = cardObjectA.transform;
            //    re.transform.localPosition = new Vector3(0, (isSideA ? -1 : 1) * 0.5f, -0.01f * (isSideA ? 1 : -1));
            //    re.AddComponent<TextMesh>().font = font;
            //    re.GetComponent<TextMesh>().text = text;
            //    re.GetComponent<TextMesh>().anchor = isSideA ? TextAnchor.LowerCenter : TextAnchor.UpperCenter;
            //    re.GetComponent<TextMesh>().alignment = (isSideA ? TextAlignment.Center : TextAlignment.Left);
            //    //Plugins.DescFont.dynamic = true;
            //    re.GetComponent<TextMesh>().fontSize = 100;
            //    if (!color.HasValue) color = Color.black;
            //    re.GetComponent<TextMesh>().color = color.Value;
            //    re.GetComponent<TextMesh>().characterSize = 0.01f * size;
            //    re.AddComponent<MeshRenderer>();
            //    re.GetComponent<MeshRenderer>().material = font.material;
            //    re.GetComponent<MeshRenderer>().material.renderQueue = 3998 + (isSideA ? 0 : 1);
            //    re.transform.localScale = new Vector3((isSideA ? 1 : -1f) / 0.6f, 1f, 1f);

            //    return re;
            //}

            public void InitiateSprites()
            {
                card.sprites.Add(mainTexture = new FTexture(Texture));
            }

            public void Hide()
            {
                mainTexture.isVisible = false;
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
            public RenderTexture Texture { get; private set; } = RenderTexture.GetTemporary(900,540);

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
                    stackObject.GetComponent<MeshRenderer>().enabled = value;
                    cardObjectB.GetComponent<MeshRenderer>().enabled = !value;
                    descObject.GetComponent<MeshRenderer>().enabled = !value;
                }
            }

            public string Title
            {
                get => titleObject.GetComponent<TextMesh>().text;
                set 
                {
                    var mesh = titleObject.GetComponent<TextMesh>();
                    var text = LayoutText(value, mesh.font, mesh.characterSize, mesh.fontSize, mesh.fontStyle);
                    mesh.text = text;
                }
            }

            public string Description
            {
                get => descObject.GetComponent<TextMesh>().text;
                set
                {
                    var mesh = descObject.GetComponent<TextMesh>();
                    var text = LayoutText(value, mesh.font, mesh.characterSize, mesh.fontSize, mesh.fontStyle);
                    mesh.text = text;
                } 
            }
        
            public float DescAlpha
            {
                get => cardObjectB.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Lerp");
                set => cardObjectB.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Lerp");//cardObjectB.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Lerp", value);
            }
            public void UpdateVisible()
            {
                IsSideA = cardObjectA.transform.forward.z > 0;
            }
            private Camera camera;

            private GameObject cardObjectA, cardObjectB;
            private GameObject titleObject, descObject, stackObject;
            private GameObject cameraObject;

            private static int count = 0;
            bool destroyed;
        }

        public class LowPerformanceRenderer
        {
            GainCard card;
            FSprite[] sprites;

            public LowPerformanceRenderer(GainCard card)
            {
                this.card = card;
            }

            public void InitiateSprites()
            {
                sprites = new FSprite[2];
                sprites[0] = new FSprite(Futile.atlasManager.DoesContainElementWithName(card.staticData.faceElementName) ? card.staticData.faceElementName : "Futile_White");
                sprites[1] = new FSprite(Futile.atlasManager.GetAtlasWithName(Plugins.BackElementOfType(card.staticData.GainType)).name);
                AddToCard();
            }

            public void AddToCard()
            {
                foreach(var sprite in sprites)
                {
                    card.sprites.Add(sprite);
                }
            }

            public void DrawSprites(float timeStacker)
            {
                sprites[0].isVisible = card.sideA;
                sprites[1].isVisible = !card.sideA;
                var sprite = card.sideA ? sprites[0] : sprites[1];

                Vector2 center = card.Position(timeStacker);
                Vector3 rotaion = card.LerpRotation(timeStacker);

                sprite.SetPosition(center);
                sprite.rotation = rotaion.z;
                sprite.width = (card.origVertices[2].x - card.origVertices[3].x) * card.size * Mathf.Abs(Mathf.Cos(rotaion.y * Mathf.Deg2Rad));
                sprite.height = (card.origVertices[0].y - card.origVertices[3].y) * card.size * Mathf.Abs(Mathf.Cos(rotaion.x * Mathf.Deg2Rad));
            }

            public void Hide()
            {
                foreach (var sprite in sprites)
                    sprite.isVisible = false;
            }

            public void Destroy()
            {
                foreach(var sprite in sprites)
                {
                    sprite.RemoveFromContainer();
                }
            }
        }

        public GainCard(GainID gainID, bool lowPerformanceMode)
        {
            container = new FContainer();
            
            this.ID = gainID;
            staticData = GainStaticDataLoader.GetStaticData(gainID);
            this.lowPerformanceMode = lowPerformanceMode;
            OnMouseCardClick += MouseOnClick;
        }

        public void Hide()
        {
            lowPerformanceRenderer?.Hide();
            cardTexture?.Hide();
            Hidden = true;
        }

        public FContainer InitiateSprites()
        {
            if (lowPerformanceMode)
            {
                lowPerformanceRenderer = new LowPerformanceRenderer(this);
                lowPerformanceRenderer.InitiateSprites();
                EmgTxCustom.Log($"Init {ID}, lowperformance, {lowPerformanceRenderer}");
            }
            else
            {
                cardTexture = new GainCardTexture(this, ID);
                cardTexture.InitiateSprites();
            }
            AddToContainer();
            return container;
        }

        public void AddToContainer()
        {
            sprites.Insert(0, backGlow = new FSprite("Futile_White") { shader = Custom.rainWorld.Shaders[Plugins.ModID + "FlatLight"], color = staticData.color });
            sprites.Add(frontGlow = new FSprite("Futile_White") { shader = Custom.rainWorld.Shaders["FlatLight"], color = staticData.color });
            foreach (var sprite in sprites) 
                container.AddChild(sprite);
        }

        public void Update()
        {
            RotateUpdate();
            InputUpdate();
            AnimationUpdate();
        }

        public Vector2 Position(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker) + Vector2.up * 50f * Mathf.Lerp(lastFocusTimer,focusTimer,timeStacker) * size / 40f;
        }
        public void DrawSprites(float timeStacker)
        {
            if (cardTexture != null)
            {
                cardTexture.mainTexture.SetPosition(Position(timeStacker));
                cardTexture.mainTexture.scale = size / 40f;
                cardTexture.Rotation = LerpRotation(timeStacker);
                cardTexture.DescAlpha = Mathf.InverseLerp(100, 180, Vector3.Lerp(rotationLast, rotationLerp, timeStacker).y);
            }
       
            lowPerformanceRenderer?.DrawSprites(timeStacker);
      
            var fade = Mathf.Lerp(lastFadeTimer, fadeTimer, timeStacker);
            frontGlow.SetPosition(Position(timeStacker));
            frontGlow.width = (origVertices[2].x - origVertices[3].x) * size * 4 * Mathf.Abs(Mathf.Cos(LerpRotation(timeStacker).y * Mathf.Deg2Rad)) * Mathf.Lerp(0.2f, 1.5f, fade);
            frontGlow.height = (origVertices[0].y - origVertices[3].y) * size * 4 * Mathf.Abs(Mathf.Cos(LerpRotation(timeStacker).x * Mathf.Deg2Rad)) * Mathf.Lerp(0.2f,1.5f,fade);
            frontGlow.alpha = fade;

            if (!Hidden) fade = Mathf.Lerp(lastActiveTimer, activeTimer, timeStacker);
            backGlow.SetPosition(Position(timeStacker));
            backGlow.width = (origVertices[2].x - origVertices[3].x) * size * 2 * Mathf.Abs(Mathf.Cos(LerpRotation(timeStacker).y * Mathf.Deg2Rad));
            backGlow.height = (origVertices[0].y - origVertices[3].y) * size * 2 * Mathf.Abs(Mathf.Cos(LerpRotation(timeStacker).x * Mathf.Deg2Rad));
            backGlow.alpha = Mathf.Min(Mathf.Lerp(lastActiveTimer, activeTimer, timeStacker),fade);
        }

        public void ClearSprites()
        {
            if(cardTexture != null)
            {
                cardTexture.Destroy();
                cardTexture = null;
            }
            if(lowPerformanceRenderer != null)
            {
                lowPerformanceRenderer.Destroy();
                lowPerformanceRenderer = null;
            }

            foreach (var sprite in sprites)
                sprite.RemoveFromContainer();
            sprites.Clear();

            container.RemoveAllChildren();
        }

        public void SwitchToLowPerformanceMode()
        {
            if (lowPerformanceMode)
                return;
            lowPerformanceMode = true;
            ClearSprites();

            lowPerformanceRenderer = new LowPerformanceRenderer(this);
            lowPerformanceRenderer.InitiateSprites();

            AddToContainer();
        }

        public void SwitchToHighQualityMode()
        {
            if (!lowPerformanceMode)
                return;
            lowPerformanceMode = false;
            ClearSprites();

            cardTexture = new GainCardTexture(this, ID);
            cardTexture.InitiateSprites();

            AddToContainer();
        }

        public FContainer container;
        List<FSprite> sprites = new List<FSprite>();

        public LowPerformanceRenderer lowPerformanceRenderer;
        public GainCardTexture cardTexture;

        public GainID ID;
        public GainStaticData staticData;


        private FSprite backGlow;
        private FSprite frontGlow;

        public bool Active;
        private float activeTimer;
        private float lastActiveTimer;

        public float fadeTimer;
        private float lastFadeTimer;

        public bool Hidden { get; private set; }

        private bool lowPerformanceMode;

        public bool IsMenu { get; set; }
    }

    /// <summary>
    /// 旋转部分
    /// </summary>
    public partial class GainCard
    {
        public void RotateUpdate()
        {
            _mouseOnRotationLast = _mouseOnRotationSmooth;
            _mouseOnRotationSmooth = Vector3.Lerp(_mouseOnRotationLast, _mouseOnRotation, 0.2f);

            rotationLast = rotationLerp;
            rotationLerp = Vector3.Lerp(rotationLerp, rotation, 0.2f);

            lastPos = pos;

            norm = new Vector3(0f, 0f, 1f);
            norm = RotateRound(norm, Vector3.forward, Rotation.z % 360f, Vector3.zero);
            norm = RotateRound(norm, Vector3.right, Rotation.x % 360f, Vector3.zero);
            norm = RotateRound(norm, Vector3.up, Rotation.y % 360f, Vector3.zero);

            if (cardTexture != null)
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

        public Vector3 rotation;
        public Vector3 rotationLerp;
        public Vector3 rotationLast;

        Vector3 norm = new Vector3(0f, 0f, 1f);

        public Vector3 Rotation
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

        public Vector3 RotateRound(Vector3 position, Vector3 axis, float angle, Vector3 center)
        {
            return Quaternion.AngleAxis(angle, axis) * (position - center) + center;
        }

        //12
        //43
        public Vector3[] origVertices = new Vector3[4] { new Vector3(-3, 5, 0f), new Vector3(3, 5, 0f), new Vector3(3, -5, 0f), new Vector3(-3, -5, 0f) };
    }

    /// <summary>
    /// 输入部分
    /// </summary>
    public partial class GainCard
    {
        public float lastFocusTimer;
        public float focusTimer;
        public bool currentKeyboardFocused;
        public void InputUpdate()
        {
            lastMouseInside = MouseInside;
            lastLeftClick = leftClick;
            lastRightClick = rightClick;

            lastFocusTimer = focusTimer;
            focusTimer = Mathf.Lerp(focusTimer, currentKeyboardFocused ? 1 : 0, 0.1f);
            if (clickCounter > 0)
            {
                clickCounter--;
                if (clickCounter == 0)
                    OnMouseCardClick?.Invoke(this);
            }

            leftClick = Input.GetMouseButton(0) && MouseInside && internalInteractive;
            rightClick = Input.GetMouseButton(1) && MouseInside && internalInteractive;
            MouseInside = CheckMouseInside();

            if (MouseInside && !lastMouseInside && OnMouseCardEnter != null)
                OnMouseCardEnter.Invoke();
            if (!MouseInside && lastMouseInside && OnMouseCardExit != null)
                OnMouseCardExit.Invoke();
            if (MouseInside && rightClick && !lastRightClick)
                OnMouseCardRightClick?.Invoke(this);

            if (!lastLeftClick && leftClick)
            {
                if (clickCounter != 0)
                {
                    OnMouseCardDoubleClick?.Invoke(this);
                    clickCounter = 0;
                }
                else
                {
                    clickCounter = 10;
                }
            }

            MouseOnAnim();
        }

        public void KeyBoardClick()
        {
            OnMouseCardClick?.Invoke(this);
        }

        public void KeyBoardRightClick()
        {
            OnMouseCardRightClick?.Invoke(this);
        }

        public void KeyBoardDoubleClick()
        {
            OnMouseCardDoubleClick?.Invoke(this);
        }
        private void MouseOnClick(GainCard card)
        {
            if (!(internalInteractive || currentKeyboardFocused))
                return;
            Rotation = sideA ? new Vector3(rotation.x, rotation.y + 180f, rotation.z) : new Vector3(rotation.x, rotation.y - 180f, rotation.z);
        }
        private void MouseOnAnim()
        {
            if(!internalInteractive)
            {
                _mouseOnRotation = Vector3.zero;
                return;
            }

            if (remainMoveCounter > 0)
                remainMoveCounter--;
            if (MouseInside)
                remainMoveCounter = 20;

            Vector2 midMousePos = MouseInside ? MouseLocalPos - new Vector2(0.5f, 0.5f) : Vector2.zero;
            _mouseOnRotation = new Vector3(20 * midMousePos.x * (sideA ? -1f : 1f), -20 * midMousePos.y, 0f);
        }

        bool CheckMouseInside()
        {
            if(animation != null && animation.ignoreMouseInput)
                return false;

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

        public void ClearInputEvents()
        {
            OnMouseCardClick = null;
            OnMouseCardDoubleClick = null;
            OnMouseCardRightClick = null;
            OnMouseCardEnter = null;
            OnMouseCardExit = null;
            OnMouseCardUpdate = null;
            OnMouseCardClick += MouseOnClick;
        }

        public event Action<GainCard> OnMouseCardClick;
        public event Action<GainCard> OnMouseCardDoubleClick;
        public event Action<GainCard> OnMouseCardRightClick;
        public event Action OnMouseCardEnter;
        public event Action OnMouseCardExit;
        public event Action OnMouseCardUpdate;

        public Vector2 MouseLocalPos { get; private set; }

        internal bool sideA => norm.z > 0;

        private bool lastMouseInside;
        public bool MouseInside { get; private set; }

        private int remainMoveCounter;

        private bool lastLeftClick;
        private bool leftClick;

        private bool lastRightClick;
        private bool rightClick;

        public bool internalInteractive = true; 

        private int clickCounter = 0;
    }

    /// <summary>
    /// 动画部分
    /// </summary>
    public partial class GainCard
    {
        public CardAnimationBase animation;

        void AnimationUpdate()
        {
            if(animation != null)
            {
                if(animation.stillActive)
                    animation.Update();
                else
                {
                    animation.Destroy(true);
                    animation = null;
                }
            }

            lastActiveTimer = activeTimer;
            activeTimer = Mathf.Lerp(activeTimer, Active ? 1 : 0, 0.1f);
            lastFadeTimer = fadeTimer;
        }

        public void TryAddAnimation(CardAnimationID id, CardAnimationArg arg)
        {
            EmgTxCustom.Log($"New animation : {id}");
            if(animation != null)
            {
                if (animation.id == id)
                    return;

                if (animation.stillActive)
                {
                    animation.stillActive = false;
                    animation.Destroy(false);
                }
                animation = null;
            }

            arg.startRotaion = rotation;
            arg.startPos = pos;
            arg.startSize = size;
            if(id == CardAnimationID.DrawCards_FlipIn)
            {
                animation = new DrawCards_FlipInAnimation(this, (DrawCards_FlipAnimationArg)arg);
            }
            else if(id == CardAnimationID.DrawCards_FlipOut_NotChoose)
            {
                animation = new DrawCars_FlipOut_NotChooseAnimation(this, (DrawCards_FlipAnimationArg)arg);
            }
            else if(id == CardAnimationID.HUD_CardPickAnimation)
            {
                animation = new HUD_CardPickAnimation(this, (HUD_CardFlipAnimationArg)arg);
            }
            else if (id == CardAnimationID.HUD_CardRightAnimation)
            {
                animation = new HUD_CardRightAnimation(this, (HUD_CardRightAnimationArg)arg);
            }
        }


        public abstract class CardAnimation<T> : CardAnimationBase where T : CardAnimationArg
        {
            protected T animationArg;
            public CardAnimation(GainCard card, T animationArg, int maxLife, CardAnimationID id) : base(card, maxLife, id)
            {
                this.animationArg = animationArg;
            }

            public override void Destroy(bool hardSetTransform)
            {
                base.Destroy(hardSetTransform);
                animationArg.OnDestroyAction?.Invoke();
            }
        }

        public abstract class CardAnimationBase
        {
            protected GainCard card;
            public CardAnimationID id;

            protected int life;
            protected int maxLife;
            protected float tInLife => Mathf.Min(life / (float)(maxLife - 30), 1f);

            public bool stillActive = true;

            public bool ignoreMouseInput = true;

            public CardAnimationBase(GainCard card, int maxLife, CardAnimationID id)
            {
                this.card = card;
                this.maxLife = maxLife + 30;//略微延长时间让lerp函数工作正常
            }

            public virtual void Update()
            {
                if (maxLife > 0)
                {
                    if (life < maxLife)
                        life++;
                    else
                        stillActive = false;
                }
            }

            public virtual void Destroy(bool hardSetTransform)
            {
            }
        }

        public abstract class CardAnimationArg
        {
            public Vector3 startRotaion;
            public float startSize;
            public Vector2 startPos;

            public Action OnDestroyAction;
        }

        public class CardAnimationID : ExtEnum<CardAnimationID>
        {
            public CardAnimationID(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly CardAnimationID DrawCards_FlipIn = new CardAnimationID("DrawCards_FlipIn", true);
            public static readonly CardAnimationID DrawCards_FlipOut_NotChoose = new CardAnimationID("DrawCards_FlipOut_NotChoose", true);
            public static readonly CardAnimationID HUD_CardPickAnimation = new CardAnimationID("HUD_CardPickAnimation", true);
            public static readonly CardAnimationID HUD_CardRightAnimation = new CardAnimationID("HUD_CardRightAnimation", true);
        }
    }
}
