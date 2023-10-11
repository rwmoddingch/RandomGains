using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu;
using RWCustom;
using RandomGains.Frame.Core;


namespace RandomGains.Frame.Cardpedia
{
    public class Cardpedia
    {
        public class CardpediaMenuHook
        {
            private MainMenu menu;
            public CardpediaMenuHook()
            {

            }

            public void Hook()
            {
                On.Menu.MainMenu.ctor += MainMenu_ctor;
                On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
            }

            private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
            {
                orig(self, manager, showRegionSpecificBkg);
                float buttonWidth = MainMenu.GetButtonWidth(self.CurrLang);
                Vector2 pos = new Vector2(683f - buttonWidth / 2f, 0f);
                Vector2 size = new Vector2(buttonWidth, 30f);
                SimpleButton collectionButton = new SimpleButton(self, self.pages[0], self.Translate("CARDPEDIA"), "CARDPEDIA", pos, size);
                menu = self;
                self.AddMainMenuButton(collectionButton, new Action(CollectionButtonPressed), 0);
            }

            private void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
            {
                if (ID == CardpediaMenu.Cardpedia)
                {
                    self.currentMainLoop = new CardpediaMenu(self);
                }
                orig(self, ID);
            }


            public void CollectionButtonPressed()
            {
                if (menu != null)
                {
                    menu.manager.RequestMainProcessSwitch(CardpediaMenu.Cardpedia);
                    menu.PlaySound(SoundID.MENU_Switch_Page_In);
                }
            }
        }

        public class CardpediaMenu : Menu.Menu,CheckBox.IOwnCheckBox
        {
            public static ProcessManager.ProcessID Cardpedia = new ProcessManager.ProcessID("Cardpedia", true);

            public SimpleButton backButton;
            private SimpleButton positiveButton;
            private SimpleButton negativeButton;
            private SimpleButton dualityButton;

            private RoundedRect cardBoxBorder;
            private RoundedRect infoBoxBorder;

            private MenuLabel nameLabel;
            private MenuLabel triggerLabel,triggerContent;
            private MenuLabel descripLabel,descripContent;
            private MenuLabel stackLabel,stackContent;
            private string nameInfo;
            private string triggerInfo;
            private string descripInfo;
            private string stackInfo;
            private int textLength;

            private FSprite darkSprite;
            private FSprite cardBoxBack;
            private FSprite infoBoxBack;
            private FSprite displayCard;
            private string displaySprite;

            public static int lastGainType, currentGainType;
            public static Dictionary<GainType, int> cardUnfoldCondition = new Dictionary<GainType, int>()
            {
                {GainType.Positive,1},
                {GainType.Duality,0},
                {GainType.Negative,-1},
            };
            public static Dictionary<string, int> messageToCondition = new Dictionary<string, int>()
            {
                {"POSITIVEGAIN",1},
                {"DUALITYGAIN",0},
                {"NEGATIVEGAIN",-1},
            };
            public static Dictionary<int, GainType> conditionToType = new Dictionary<int, GainType>()
            {
                {1,GainType.Positive},
                {0,GainType.Duality},
                {-1,GainType.Negative},
            };

            private bool exiting;
            private bool changingCondition;
            private Vector2 startPos = new Vector2(400f, 200f);

            public CardpediaMenu(ProcessManager manager) : base(manager, Cardpedia)
            {
                currentGainType = 1;
                lastGainType = currentGainType;
                textLength = this.CurrLang == InGameTranslator.LanguageID.Chinese ? 20 : 30;                
                
                //背景
                pages.Add(new Page(this, null, "main", 0));
                scene = new InteractiveMenuScene(this, null, manager.rainWorld.options.subBackground);
                pages[0].subObjects.Add(scene);

                darkSprite = new FSprite("pixel", true);
                darkSprite.color = new Color(0f, 0f, 0f);
                darkSprite.anchorX = 0f;
                darkSprite.anchorY = 0f;
                darkSprite.scaleX = 1368f;
                darkSprite.scaleY = 770f;
                darkSprite.x = -1f;
                darkSprite.y = -1f;
                darkSprite.alpha = 0.85f;
                pages[0].Container.AddChild(darkSprite);

                //切换按钮
                positiveButton = new SimpleButton(this, pages[0], Translate("POSITIVE GAIN"), "POSITIVEGAIN", new Vector2(120f, 700f), new Vector2(150f, 30f));
                negativeButton = new SimpleButton(this, pages[0], Translate("NEGATIVE GAIN"), "NEGATIVEGAIN", new Vector2(120f, 660f), new Vector2(150f, 30f));
                dualityButton = new SimpleButton(this, pages[0], Translate("DUALITY GAIN"), "DUALITYGAIN", new Vector2(120f, 620f), new Vector2(150f, 30f));

                backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(150f, 30f), new Vector2(110f, 30f));
                pages[0].subObjects.Add(backButton);
                pages[0].subObjects.Add(positiveButton);
                pages[0].subObjects.Add(negativeButton);
                pages[0].subObjects.Add(dualityButton);
                backObject = backButton;
                backButton.nextSelectable[0] = backButton;
                backButton.nextSelectable[2] = backButton;

                //边框
                cardBoxBorder = new RoundedRect(this, pages[0], new Vector2(315f, 50f), new Vector2(1000f, 320f), true);
                infoBoxBorder = new RoundedRect(this, pages[0], new Vector2(315f, 390f), new Vector2(1000f, 350f), true);

                cardBoxBack = new FSprite("pixel", true);
                cardBoxBack.color = new Color(0f, 0f, 0f);
                cardBoxBack.anchorX = 0f;
                cardBoxBack.anchorY = 0f;
                cardBoxBack.scaleX = cardBoxBorder.size.x - 12f;
                cardBoxBack.scaleY = cardBoxBorder.size.y - 12f;
                cardBoxBack.x = cardBoxBorder.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                cardBoxBack.y = cardBoxBorder.pos.y + 6f;
                cardBoxBack.alpha = 0.65f;
                pages[0].Container.AddChild(cardBoxBack);

                infoBoxBack = new FSprite("pixel", true);
                infoBoxBack.color = new Color(0f, 0f, 0f);
                infoBoxBack.anchorX = 0f;
                infoBoxBack.anchorY = 0f;
                infoBoxBack.scaleX = infoBoxBorder.size.x - 12f;
                infoBoxBack.scaleY = infoBoxBorder.size.y - 12f;
                infoBoxBack.x = infoBoxBorder.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                infoBoxBack.y = infoBoxBorder.pos.y + 6f;
                infoBoxBack.alpha = 0.65f;
                pages[0].Container.AddChild(infoBoxBack);

                //图鉴卡牌
                InitPediaCards();

                //卡牌信息文本
                nameInfo = this.Translate("? ? ?");
                triggerInfo = this.Translate("? ? ?");
                descripInfo = this.Translate("? ? ?");
                stackInfo = this.Translate("? ? ?");

                nameLabel = new MenuLabel(this, this.pages[0], this.Translate(nameInfo), new Vector2(600f, 690f), Vector2.zero, true);
                nameLabel.label.alignment = FLabelAlignment.Left;
                triggerLabel = new MenuLabel(this, this.pages[0], this.Translate("Activation: "), new Vector2(600f, 630f), Vector2.zero, false);
                triggerLabel.label.alignment = FLabelAlignment.Left;
                triggerContent = new MenuLabel(this, this.pages[0], this.Translate(triggerInfo), new Vector2(680f, 630f), Vector2.zero, false);
                triggerContent.label.alignment = FLabelAlignment.Left;
                stackLabel = new MenuLabel(this, this.pages[0], this.Translate("Stackability: "), new Vector2(600f, 590f), Vector2.zero, false);
                stackLabel.label.alignment = FLabelAlignment.Left;
                stackContent = new MenuLabel(this, this.pages[0], this.Translate(stackInfo), new Vector2(680f, 590f), Vector2.zero, false);
                stackContent.label.alignment = FLabelAlignment.Left;
                descripLabel = new MenuLabel(this, this.pages[0], this.Translate("Description: "), new Vector2(600f, 550f), Vector2.zero, false);
                descripLabel.label.alignment = FLabelAlignment.Left;
                descripContent = new MenuLabel(this, this.pages[0], this.Translate(descripInfo), new Vector2(680f, 550f), Vector2.zero, false);
                descripContent.label.alignment = FLabelAlignment.Left;

                pages[0].subObjects.Add(nameLabel);
                pages[0].subObjects.Add(triggerLabel);
                pages[0].subObjects.Add(triggerContent);
                pages[0].subObjects.Add(descripLabel);
                pages[0].subObjects.Add(descripContent);
                pages[0].subObjects.Add(stackLabel);
                pages[0].subObjects.Add(stackContent);

                displaySprite = Futile.atlasManager.GetAtlasWithName(Plugins.BackElementOfType(conditionToType[currentGainType])).name;
                displayCard = new FSprite(displaySprite);
                displayCard.scale = 0.3f;
                displayCard.SetPosition(450f, 570f);
                pages[0].Container.AddChild(displayCard);

                pages[0].subObjects.Add(cardBoxBorder);
                pages[0].subObjects.Add(infoBoxBorder);

            }

            public bool GetChecked(CheckBox checkBox)
            {
                return false;
            }

            public void SetChecked(CheckBox box, bool c)
            {

            }

            public void InitPediaCards()
            {
                //首次加载游戏时填充图鉴卡池
                if (StaticCardPool.pediaCardPool[GainType.Positive].Count + StaticCardPool.pediaCardPool[GainType.Negative].Count + 
                    StaticCardPool.pediaCardPool[GainType.Duality].Count < StaticCardPool.maxCount)
                {
                    StaticCardPool.FillPediaPool(this, pages[0], startPos, new Vector2(0.2f, 0.2f));
                }
               
                //取出图鉴卡池中的卡牌
                int num1 = StaticCardPool.staticIDPool[GainType.Positive].Count;
                int num2 = StaticCardPool.staticIDPool[GainType.Negative].Count;
                int num3 = StaticCardPool.staticIDPool[GainType.Duality].Count;
                for (int i = 0; i < num1; i++)
                {
                    StaticCardPool.PediaCard pediaCard = StaticCardPool.PickOutPediaCard(1);
                    pages[0].subObjects.Add(pediaCard);
                    pediaCard.Inited = false;
                    pediaCard.UnFold();
                    Debug.Log("Pediacard(positive):" + pediaCard.ID + " added to menu");
                }
                for (int j = 0; j < num2; j++)
                {
                    StaticCardPool.PediaCard pediaCard = StaticCardPool.PickOutPediaCard(-1);
                    pages[0].subObjects.Add(pediaCard);
                    pediaCard.Inited = false;
                    pediaCard.UnFold();
                    Debug.Log("Pediacard(negative):" + pediaCard.ID + " added to menu");
                }
                for (int k = 0; k < num3; k++)
                {
                    StaticCardPool.PediaCard pediaCard = StaticCardPool.PickOutPediaCard(0);
                    pages[0].subObjects.Add(pediaCard);
                    pediaCard.Inited = false;
                    pediaCard.UnFold();
                    Debug.Log("Pediacard(duality):" + pediaCard.ID + " added to menu");
                }

                
                if (StaticCardPool.Scruffy.reloading)
                {
                    StaticCardPool.Scruffy.reloading = false;
                    for (int l = 0; l < this.pages[0].subObjects.Count; l++)
                    {
                        Menu.MenuObject menuObject = this.pages[0].subObjects[l];
                        if (menuObject is StaticCardPool.PediaCard)
                        {
                            StaticCardPool.PediaCard pediaCard = menuObject as StaticCardPool.PediaCard;
                            pediaCard.Reset();
                        }
                    }
                }
                
            }

            public void ResetDisplayInfo()
            {
                string str = "? ? ?";
                nameInfo = str;
                triggerInfo = str;
                stackInfo = str;
                descripInfo = str;
                displayCard.element = Futile.atlasManager.GetElementWithName(displaySprite);
            }

            public override void Singal(MenuObject sender, string message)
            {
                if (message == "BACK")
                {
                    OnExit();
                }
                else if (message == "POSITIVEGAIN" || message == "NEGATIVEGAIN" || message == "DUALITYGAIN")
                {
                    currentGainType = messageToCondition[message];
                }
            }


            public void OnExit()
            {
                if (exiting)
                {
                    return;
                }

                //将图鉴卡牌回收进图鉴卡池
                int num = this.pages[0].subObjects.Count;

                for (int i = 0; i < num; i++)
                {
                    Menu.MenuObject menuObject = this.pages[0].subObjects[0];
                    if (menuObject is StaticCardPool.PediaCard)
                    {                       
                        StaticCardPool.PediaCard pediaCard = menuObject as StaticCardPool.PediaCard;
                        this.pages[0].subObjects.Remove(pediaCard);
                        StaticCardPool.RecyclePediaCard(pediaCard);
                    }
                }               

                StaticCardPool.Scruffy.reloading = true;
                exiting = true;
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                PlaySound(SoundID.MENU_Switch_Page_Out);
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                displayCard.scale = 0.3f * (StaticCardPool.Scruffy.standardW / displayCard.element.sourcePixelSize.x);
                if(lastGainType != currentGainType)
                {
                    ResetDisplayInfo();
                    lastGainType = currentGainType;
                }
            }

            public override void Update()
            {
                base.Update();
                displaySprite = Futile.atlasManager.GetAtlasWithName(Plugins.BackElementOfType(conditionToType[currentGainType])).name;
                nameLabel.text = nameInfo;
                triggerContent.text = triggerInfo;
                descripContent.text = StaticCardPool.Scruffy.GenerateLongText(descripInfo,textLength);
                stackContent.text = stackInfo;
                int lines = descripContent.text.Length / textLength + (descripContent.text.Length % textLength == 0? 0:1);
                float lineW = descripContent.label.FontLineHeight;
                descripContent.size.y = -lineW * (lines - 1);

                for (int j = this.pages[0].subObjects.Count - 1; j > 0; j--)
                {
                    if (!(this.pages[0].subObjects[j] is StaticCardPool.PediaCard)) continue;
                    else
                    {
                        StaticCardPool.PediaCard pediaCard = this.pages[0].subObjects[j] as StaticCardPool.PediaCard;

                        if (PediaSessionHook.unlockedCards != null && PediaSessionHook.unlockedCards.Contains(pediaCard.ID.value))
                        {
                            pediaCard.unlocked = true;
                        }
                        else pediaCard.unlocked = false;

                        if (pediaCard.popUp)
                        {
                            if (pediaCard.unlocked)
                            {
                                nameInfo = pediaCard.staticData.gainName;
                                triggerInfo = pediaCard.staticData.triggerable ? this.Translate("Manually") : this.Translate("Automatically");
                                stackInfo = pediaCard.staticData.stackable ? this.Translate("Stackable") : this.Translate("Unstackable");
                                descripInfo = pediaCard.staticData.gainDescription;
                                displayCard.element = pediaCard.cardSprite.element;
                            }
                            else
                            {
                                this.ResetDisplayInfo();
                            }
                        }

                    }

                }

                for (int i = this.pages[0].subObjects.Count - 1; i > 0; i--)
                {
                    if (!(this.pages[0].subObjects[i] is StaticCardPool.PediaCard)) continue;
                    else
                    {
                        StaticCardPool.PediaCard pediaCard = this.pages[0].subObjects[i] as StaticCardPool.PediaCard;
                        if (pediaCard.Inited)
                        {
                            pediaCard.aboveOtherCards = true;
                            return;
                        }
                        else continue;
                    }

                }

                
            }
        }

    }
}
