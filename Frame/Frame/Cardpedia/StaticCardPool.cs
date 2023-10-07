using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Menu;
using RandomGains.Frame.Core;
using UnityEngine;
using static RandomGains.Frame.GainCard;
using RWCustom;

namespace RandomGains.Frame.Cardpedia
{
    public static class StaticCardPool
    {

        /*
        public static Dictionary<GainType, Dictionary<GainID, GainCard>> staticCardPool = new Dictionary<GainType, Dictionary<GainID, GainCard>>()
        {
            { GainType.Positive, new Dictionary < GainID, GainCard >() },
            { GainType.Duality,  new Dictionary < GainID, GainCard >() },
            { GainType.Negative, new Dictionary < GainID, GainCard >() },

        };
        */

        //填满后不会变动的总卡池，储存已加载的所有卡牌ID和类型
        public static Dictionary<GainType, List<GainID>> staticIDPool = new Dictionary<GainType, List<GainID>>()
        {
            { GainType.Positive, new List<GainID>() },
            { GainType.Duality, new List<GainID>() },
            { GainType.Negative, new List<GainID>() },
        };

        //用于匹配卡牌ID和增益类型
        public static Dictionary<GainID, GainType> getTypeOfGain = new Dictionary<GainID, GainType>();

        //图鉴卡池
        public static Dictionary<GainType, Queue<PediaCard>> pediaCardPool = new Dictionary<GainType, Queue<PediaCard>>()
        {
            { GainType.Positive, new Queue<PediaCard>() },
            { GainType.Duality, new Queue<PediaCard>() },
            { GainType.Negative, new Queue<PediaCard>() },
        };

        public static int maxCount = 0;

        //填充图鉴卡池
        public static void FillPediaPool(Menu.Menu menu, MenuObject menuObject, Vector2 pos, Vector2 size)
        {
            if (pediaCardPool.Count < staticIDPool[GainType.Positive].Count)
            {
                for (int i = 0; i < staticIDPool[GainType.Positive].Count; i++)
                {
                    PediaCard pediaCard = new PediaCard(staticIDPool[GainType.Positive][i], menu, menuObject, pos + new Vector2(i * Scruffy.fatness, 0f), size);
                    pediaCardPool[GainType.Positive].Enqueue(pediaCard);
                    //Debug.Log("[[[Added One Pediacard]]]:" + pediaCard.ID + ", type:" + pediaCard.gainType);
                }
            }

            if (pediaCardPool.Count < staticIDPool[GainType.Negative].Count)
            {
                for (int j = 0; j < staticIDPool[GainType.Negative].Count; j++)
                {
                    PediaCard pediaCard = new PediaCard(staticIDPool[GainType.Negative][j], menu, menuObject, pos + new Vector2(j * Scruffy.fatness, 0f), size);
                    pediaCardPool[GainType.Negative].Enqueue(pediaCard);
                    //Debug.Log("[[[Added One Pediacard]]]:" + pediaCard.ID + ", type:" + pediaCard.gainType);
                }
            }

            if (pediaCardPool.Count < staticIDPool[GainType.Duality].Count)
            {
                for (int k = 0; k < staticIDPool[GainType.Duality].Count; k++)
                {
                    PediaCard pediaCard = new PediaCard(staticIDPool[GainType.Duality][k], menu, menuObject, pos + new Vector2(k * Scruffy.fatness, 0f), size);
                    pediaCardPool[GainType.Duality].Enqueue(pediaCard);
                    //Debug.Log("[[[Added One Pediacard]]]:" + pediaCard.ID + ", type:" + pediaCard.gainType);
                }
            }

        }

        //从图鉴卡池中取出卡牌
        public static PediaCard PickOutPediaCard(int gainType)
        {
            if (gainType == 1)
            {
                return pediaCardPool[GainType.Positive].Dequeue();

            }
            else if (gainType == 0)
            {
                return pediaCardPool[GainType.Duality].Dequeue();
            }
            else
            {
                return pediaCardPool[GainType.Negative].Dequeue();
            }
        }

        //回收图鉴卡牌
        public static void RecyclePediaCard(PediaCard pediaCard)
        {
            GainType type = getTypeOfGain[pediaCard.ID];
            if (pediaCardPool.Count < staticIDPool[type].Count)
            {
                pediaCardPool[type].Enqueue(pediaCard);
            }
            else pediaCard.Destroy();
        }

        /*
        public static void AddToIDPool(GainType gainType, GainID gainID)
        {

            if (staticIDPool.Count < maxCount && !staticIDPool[gainType].Contains(gainID))
            {
                staticIDPool[gainType].Add(gainID);
                if (!getTypeOfGain.ContainsKey(gainID))
                {
                    getTypeOfGain.Add(gainID, gainType);
                }
            }
        }

        public static void RemoveDataFromPool(GainType gainType, GainID gainID)
        {
            staticIDPool[gainType].Remove(gainID);
        }

        public static void RecycleToPool(GainCard gainCard)
        {
            GainID id = gainCard.ID;
            GainType type = getTypeOfGain[id];
            if (!staticIDPool[type].Contains(id))
            {
                staticIDPool[type].Add(id);

            }
        }
        */

        //图鉴卡牌
        public class PediaCard : RectangularMenuObject, SelectableMenuObject, ButtonMenuObject
        {
            //行为相关
            public bool unlocked;
            public bool popUp;
            public bool pickedOut;
            public bool disposed;
            public bool Inited;
            public bool aboveOtherCards;
            //图像相关
            public string cardImageName;
            public string cardBack;
            public FSprite cardSprite;
            public FSprite backSprite;
            public FSprite boarderSprite;
            public Vector2 centralPos;
            public float cardAlpha;
            public bool lightingUp;
            public float lightness;
            public float popHeight;
            //数据储存
            public GainStaticData staticData;
            public GainID ID;
            public GainType gainType;
            public Vector2 origCentralPos;
            public float origAlpha;
            public Vector2 origSize;
            public string origImage;

            public PediaCard(GainID ID, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
            {
                this.ID = ID;
                staticData = GainStaticDataLoader.GetStaticData(this.ID);
                cardImageName = Futile.atlasManager.DoesContainElementWithName(staticData.faceElementName) ? staticData.faceElementName : "Futile_White";
                cardSprite = new FSprite(cardImageName);
                gainType = staticData.GainType;

                unlocked = true;
                //popUp = false;
                pickedOut = false;
                disposed = false;
                aboveOtherCards = false;

                this.size = size;
                this.pos = pos;
                cardAlpha = 1;
                centralPos = Vector2.zero;
                lightness = 0;
                lightingUp = true;

                cardSprite.color = Color.white;
                cardSprite.x = this.pos.x;
                cardSprite.y = this.pos.y;
                cardSprite.scaleX = this.size.x * (Scruffy.standardW / cardSprite.element.sourcePixelSize.x);
                cardSprite.scaleY = this.size.y * (Scruffy.standardH / cardSprite.element.sourcePixelSize.y);
                cardSprite.alpha = cardAlpha;

                boarderSprite = new FSprite("pixel");
                boarderSprite.color = Color.white;
                boarderSprite.x = this.pos.x;
                boarderSprite.y = this.pos.y;
                boarderSprite.scaleX = 125f;
                boarderSprite.scaleY = 205f;

                Container.AddChild(boarderSprite);
                Container.AddChild(cardSprite);

                RecordOrigData(pos,size);
                Inited = true;
            }

            public override void Update()
            {
                base.Update();
                if (Cardpedia.CardpediaMenu.cardUnfoldCondition[gainType] != Cardpedia.CardpediaMenu.currentGainType)
                {
                    FoldUp();
                }
                else
                {
                    UnFold();
                }
                if (!Inited) return;
                if (IsMouseOverMe)
                {
                    if (lightness >= 1) lightingUp = false;
                    if (lightness <= 0) lightingUp = true;

                    if (lightingUp)
                    {
                        lightness += 0.04f;
                    }
                    else lightness -= 0.04f;

                    popUp = true;
                    if (popHeight <= 1f)
                    {
                        popHeight += 0.1f;
                    }     
                    
                }
                else
                {
                    popUp = false;
                    if(popHeight >= 0.1f)
                    {
                        popHeight -= 0.1f;
                    }
                }
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                if (!Inited) return;

                float num2 = Scruffy.GetPowLerpParam(popHeight + 0.1f * (popUp ? timeStacker : -timeStacker));
                this.cardSprite.y = origCentralPos.y + 60f * Mathf.Clamp(num2,0,1);
                this.boarderSprite.y = this.cardSprite.y;

                if (IsMouseOverMe)
                {
                    float num = Scruffy.GetPowLerpParam(lightness + 0.04f * (lightingUp ? timeStacker : -timeStacker));
                    this.boarderSprite.alpha = Mathf.Clamp(num, 0, 1);
                }
                else
                {
                    this.boarderSprite.alpha = 0;
                }
            }

            public void FoldUp()
            {
                size = Vector2.zero;
                pos = Vector2.zero;
                cardAlpha = 0;
                boarderSprite.alpha = 0;
                cardSprite.alpha = 0;
                cardSprite.scaleX = 0;
                cardSprite.scaleY = 0;
                Inited = false;
                //Debug.Log("Folding for condition:" + this.ID + Cardpedia.CardpediaMenu.currentGainType);
            }

            public void UnFold()
            {
                if (Inited) return;
                size = origSize;
                pos = origCentralPos;
                cardAlpha = 1;
                boarderSprite.alpha = 1;
                cardSprite.scaleX = size.x * (Scruffy.standardW / cardSprite.element.sourcePixelSize.x);
                cardSprite.scaleY = size.y * (Scruffy.standardH / cardSprite.element.sourcePixelSize.y);
                cardSprite.alpha = cardAlpha;
                cardSprite.SetPosition(pos);
                boarderSprite.SetPosition(pos);
                Inited = true;                
            }

            public void Reset()
            {
                this.Inited = false;
                this.popUp = false;
                this.pickedOut = false;
                this.UnFold();
                Container.AddChild(boarderSprite);
                Container.AddChild(cardSprite);
            }

            public void RecordOrigData(Vector2 pos,Vector2 size)
            {
                origSize = size;
                origCentralPos = pos;
                origAlpha = 1;
                origImage = cardImageName;
            }

            public void Destroy()
            {
                Debug.Log("Destroy pediacard:" + ID);
                disposed = true;
                size = Vector2.zero;
                pos = Vector2.zero;
                popUp = false;
                pickedOut = false;
                cardAlpha = 0;
                cardImageName = "";
                cardSprite = null;
                staticData = null;
                ID = null;
            }

            public Vector2[] MouseOverPos()
            {
                return new Vector2[3]
                {
                    new Vector2 (pos.x - 40f, origCentralPos.y),
                    new Vector2 (20f,100f),
                    new Vector2 (60f,100f),
                };
            }

            //堆放接口
            #region
            public void Clicked()
            {

            }


            public bool IsMouseOverMe
            {
                get
                {
                    bool flag = (this.menu.mousePosition.x >= MouseOverPos()[0].x - MouseOverPos()[1].x) && (this.menu.mousePosition.x < MouseOverPos()[0].x + MouseOverPos()[1].x);
                    bool flag2 = (this.menu.mousePosition.y >= MouseOverPos()[0].y - MouseOverPos()[1].y) && (this.menu.mousePosition.y < MouseOverPos()[0].y + MouseOverPos()[1].y);
                    bool flag3 = (this.menu.mousePosition.x >= pos.x - MouseOverPos()[2].x) && (this.menu.mousePosition.x < pos.x + MouseOverPos()[2].x);
                    bool flag4 = (this.menu.mousePosition.y >= origCentralPos.y - MouseOverPos()[2].y) && (this.menu.mousePosition.y < origCentralPos.y + MouseOverPos()[2].y);
                    if(!aboveOtherCards)
                        return flag && flag2;
                    else
                        return flag3 && flag4;
                }
            }

            public bool CurrentlySelectableMouse { get; }

            public bool CurrentlySelectableNonMouse { get; }

            public ButtonBehavior GetButtonBehavior
            {
                get;
            }
            #endregion
        }

        /*
        public class RectCardBoarder : RectangularMenuObject
        {
            public FSprite[] sprites;
            public float thickness;

            public RectCardBoarder(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size):base(menu,owner,pos,size)
            {
                this.thickness = 3;
                this.sprites = new FSprite[8];
                for (int i = 0; i < 8; i++)
                {
                    this.sprites[i] = new FSprite("pixel");
                    this.sprites[i].SetAnchor(new Vector2(0,0));
                    if (i<=3)
                    {
                        this.sprites[i].scale = thickness;
                    }
                    else if (i == 4 || i == 5)
                    {
                        this.sprites[i].scaleY = thickness;
                        this.sprites[i].scaleX = size.x;
                    }
                    else
                    {
                        this.sprites[i].scaleX = thickness;
                        this.sprites[i].scaleY = size.y;
                    }
                    //this.Container.AddChild(this.sprites[i]);
                }

                float w = 0.5f * (size.x + thickness);
                float h = 0.5f * (size.y + thickness); 
                this.sprites[0].SetPosition(pos + new Vector2(-w,h));
                this.sprites[1].SetPosition(pos + new Vector2(w,h));
                this.sprites[2].SetPosition(pos + new Vector2(w,-h));
                this.sprites[3].SetPosition(pos + new Vector2(-w, -h));
                this.sprites[4].SetPosition(pos + new Vector2(-0.5f * sprites[4].scaleX,h));
                this.sprites[5].SetPosition(pos + new Vector2(-0.5f * sprites[4].scaleX,-h));
                this.sprites[6].SetPosition(pos + new Vector2(-w, -0.5f * sprites[6].scaleY));
                this.sprites[7].SetPosition(pos + new Vector2(w, -0.5f * sprites[6].scaleY));

            }

        }
        */

        //用于堆放零散的有用数值
        public static class Scruffy
        {
            public static bool reloading;

            public static float fatness = 40f;
            public static float standardH = 1000f;
            public static float standardW = 600f;

            public static float GetPowLerpParam(float t, float pow = 3)
            {
                float a = 1f / Mathf.Pow(t + 1, pow);
                float b = 1f / Mathf.Pow(2, pow);

                return (1f - a) / (1f - b);
            }

            public static string GenerateLongText(string text, int rollLength)
            {               
                if(text.Length <= 0) return text;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < text.Length; i++)
                {
                    if (i != 0 && i % rollLength == 0)
                    {
                        builder.Append('\n');
                    }
                    builder.Append(text[i]);
                }
                return builder.ToString();
            }
        }

    }
}
