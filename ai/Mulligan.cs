﻿using System;
using System.Collections.Generic;

namespace HREngine.Bots
{

    public class Mulligan
    {
        public class CardIDEntity
        {
            public string id = "";
            public int entitiy = 0;
            public CardIDEntity(string id, int entt)
            {
                this.id = id;
                this.entitiy = entt;
            }
        }

        class mulliitem
        {
            public string cardid = "";
            public string enemyclass = "";
            public string ownclass = "";
            public int howmuch = 2;
            public string[] requiresCard = null;
            public int manarule = -1;
            public string rulestring = "";
            public int coinrule = 0; //0 = for both, 1 = if you are first, 2 if you are second

            /*public mulliitem(string id, string own, string enemy, int number, string[] req = null, int coinr = 0, int mrule = -1)
            {
                this.cardid = id;
                this.ownclass = own;
                this.enemyclass = enemy;
                this.howmuch = number;
                this.requiresCard = req;
                this.manarule = mrule;
                this.coinrule = coinr;
            }*/

            public mulliitem(string all, string id, string own, string enemy, int number, string[] req = null, int coinr = 0, int mrule = -1)
            {
                this.cardid = id;
                this.ownclass = own;
                this.enemyclass = enemy;
                this.howmuch = number;
                this.requiresCard = req;
                this.manarule = mrule;
                this.rulestring = all;
                this.coinrule = coinr;
            }

        }

        class concedeItem
        {
            public HeroEnum urhero = HeroEnum.None;
            public List<HeroEnum> enemhero = new List<HeroEnum>();
        }

        List<mulliitem> holdlist = new List<mulliitem>();
        List<mulliitem> deletelist = new List<mulliitem>();
        List<concedeItem> concedelist = new List<concedeItem>();
        public bool loserLoserLoser = false;

        private string ownClass = Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.heroname);
        private string deckName = Hrtprozis.Instance.deckName;


        private static Mulligan instance;

        public static Mulligan Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Mulligan();
                }
                return instance;
            }
        }

        public void updateInstance()
        {
            instance = new Mulligan();
            ownClass = Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.heroname);
            deckName = Hrtprozis.Instance.deckName;
        }

        private Mulligan()
        {
            readCombos();
        }

        private void readCombos()
        {
            string[] lines = new string[0] { };
            this.holdlist.Clear();
            this.deletelist.Clear();
            
            string path = Settings.Instance.path;
            string datapath = path + "Data" + System.IO.Path.DirectorySeparatorChar;
            string classpath = datapath + ownClass + System.IO.Path.DirectorySeparatorChar;
            string deckpath = classpath + deckName + System.IO.Path.DirectorySeparatorChar;


            // if we have a deckName then we have a real ownClass too, not the default "druid"
            if (deckName != "" && System.IO.File.Exists(deckpath + "_mulligan.txt"))
            {
                path = deckpath;
                Helpfunctions.Instance.ErrorLog("read deck " + deckName + System.IO.Path.DirectorySeparatorChar + "_mulligan.txt...");
            }
            else if (deckName != "" && System.IO.File.Exists(classpath + "_mulligan.txt"))
            {
                path = classpath;
                Helpfunctions.Instance.ErrorLog("read class " + ownClass + System.IO.Path.DirectorySeparatorChar + "_mulligan.txt...");
            }
            else if (deckName != "" && System.IO.File.Exists(datapath + "_mulligan.txt"))
            {
                path = datapath;
                Helpfunctions.Instance.ErrorLog("read Data" + System.IO.Path.DirectorySeparatorChar + "_mulligan.txt...");
            }
            else if (System.IO.File.Exists(path + "_mulligan.txt"))
            {
                Helpfunctions.Instance.ErrorLog("read base _mulligan.txt...");
            }
            else
            {
                Helpfunctions.Instance.ErrorLog("can't find _mulligan.txt (if you didn't create your own mulligan file, ignore this message)");
                return;
            }

            try
            {
                lines = System.IO.File.ReadAllLines(path + "_mulligan.txt");
            }
            catch
            {
                Helpfunctions.Instance.ErrorLog("_mulligan.txt read error. Continuing without user-defined rules.");
                return;
            }

            foreach (string line in lines)
            {
                string shortline = line.Replace(" ", "");
                if (shortline.StartsWith("//")) continue;
                if (shortline.Length == 0) continue;

                if (line.StartsWith("loser"))
                {
                    this.loserLoserLoser = true;
                    continue;
                }

                if (line.StartsWith("concede:"))
                {
                    try
                    {
                        string ownh = line.Split(':')[1];
                        concedeItem ci = new concedeItem();
                        ci.urhero = Hrtprozis.Instance.heroNametoEnum(ownh);
                        string enemlist = line.Split(':')[2];
                        foreach (string s in enemlist.Split(','))
                        {
                            ci.enemhero.Add(Hrtprozis.Instance.heroNametoEnum(s));
                        }
                        concedelist.Add(ci);
                    }
                    catch
                    {
                        Helpfunctions.Instance.ErrorLog("mullimaker cant read: " + line);
                    }
                    continue;
                }

                if (line.StartsWith("hold;"))
                {
                    try
                    {
                        string ownclass = line.Split(';')[1];
                        string enemyclass = line.Split(';')[2];
                        string cardlist = line.Split(';')[3];

                        int coinrule = 0;
                        if (line.Split(';').Length >= 5)
                        {
                            string coin = line.Split(';')[4];
                            if (coin == "nocoin") coinrule = 1;
                            if (coin == "coin") coinrule = 2;
                        }

                        foreach (string crd in cardlist.Split(','))
                        {
                            if (crd.Contains(":"))
                            {
                                if ((crd.Split(':')).Length == 3)
                                {
                                    this.holdlist.Add(new mulliitem(line, crd.Split(':')[0], ownclass, enemyclass, Convert.ToInt32(crd.Split(':')[1]), crd.Split(':')[2].Split('/'),coinrule,-1));
                                }
                                else
                                {
                                    this.holdlist.Add(new mulliitem(line, crd.Split(':')[0], ownclass, enemyclass, Convert.ToInt32(crd.Split(':')[1]),null, coinrule,-1));
                                }

                            }
                            else
                            {
                                this.holdlist.Add(new mulliitem(line, crd, ownclass, enemyclass, 2,null,coinrule,-1));
                            }
                        }

                        

                        if (line.Split(';').Length >= 6)
                        {
                            string mr = (line.Split(';')[5]).Replace(" ", "");
                            if (mr == "") continue;
                            int manarule = Convert.ToInt32(mr);
                            if (manarule <= 0) continue;
                            Console.WriteLine("manarule "+mr); 
                            this.holdlist.Add(new mulliitem(line, "#MANARULE", ownclass, enemyclass, 2, null, coinrule, manarule));
                        }

                    }
                    catch
                    {
                        Helpfunctions.Instance.ErrorLog("mullimaker cant read: " + line);
                    }
                }
                else
                {
                    if (line.StartsWith("discard;"))
                    {
                        try
                        {
                            string ownclass = line.Split(';')[1];
                            string enemyclass = line.Split(';')[2];
                            string cardlist = line.Split(';')[3];
                            int coinrule = 0;
                            if (line.Split(';').Length >= 5)
                            {
                                string coin = line.Split(';')[4];
                                if (coin == "nocoin") coinrule = 1;
                                if (coin == "coin") coinrule = 2;
                            }

                            foreach (string crd in cardlist.Split(','))
                            {
                                if (crd == null || crd == "") continue;
                                this.deletelist.Add(new mulliitem(line, crd, ownclass, enemyclass, 2,null, coinrule, -1));
                            }

                            if (line.Split(';').Length >= 6)
                            {
                                string mr = (line.Split(';')[5]).Replace(" ","");
                                if (mr == "") continue;
                                int manarule = Convert.ToInt32(mr);
                                if (manarule <= 0) continue;
                                this.deletelist.Add(new mulliitem(line, "#MANARULE", ownclass, enemyclass, 2, null, coinrule, manarule));
                            }

                        }
                        catch
                        {
                            Helpfunctions.Instance.ErrorLog("mullimaker cant read: " + line);
                        }
                    }
                    else
                    {

                    }
                }

            }

            if (holdlist.Count > 0) Helpfunctions.Instance.ErrorLog(holdlist.Count + " hold rules found");
            if (deletelist.Count > 0) Helpfunctions.Instance.ErrorLog(deletelist.Count + " discard rules found");
            if (concedelist.Count > 0) Helpfunctions.Instance.ErrorLog(concedelist.Count + " concede rules found");
        }

        public bool hasmulliganrules(string ownclass, string enemclass)
        {
            if (this.holdlist.Count == 0 && this.deletelist.Count == 0) return false;
            bool hasARule = false;
            foreach (mulliitem mi in this.holdlist)
            {
                if ((mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass)) hasARule = true;
            }
            foreach (mulliitem mi in this.deletelist)
            {
                if ((mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass)) hasARule = true;
            }
            return hasARule;
        }

        public bool hasHoldListRule(string ownclass, string enemclass)
        {
            bool hasARule = false;
            foreach (mulliitem mi in this.holdlist)
            {
                if ((mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass)) hasARule = true;
            }
            return hasARule;
        }

        public List<int> whatShouldIMulligan(List<CardIDEntity> cards, string ownclass, string enemclass, bool hascoin)
        {
            Helpfunctions.Instance.ErrorLog("do mulligan...");
            if (hascoin)
            {
                Helpfunctions.Instance.ErrorLog("we hold the coin");
            }
            else
            {
                Helpfunctions.Instance.ErrorLog("we dont hold the coin");
            }
            List<int> discarditems = new List<int>();
            bool usedManarule = false;
            foreach (mulliitem mi in this.deletelist)
            {
                if (hascoin && mi.coinrule == 1) continue;
                if (!hascoin && mi.coinrule == 2) continue;

                foreach (CardIDEntity c in cards)
                {
                    if (mi.cardid == "#MANARULE" && (mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass))
                    {
                        usedManarule = true;
                        if (CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(c.id)).cost >= mi.manarule)
                        {
                            if (discarditems.Contains(c.entitiy)) continue;
                            Helpfunctions.Instance.ErrorLog("discard " + c.id + " because of this rule " + mi.rulestring);
                            discarditems.Add(c.entitiy);
                        }
                        continue;
                    }

                    if (c.id == mi.cardid && (mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass))
                    {
                        if (discarditems.Contains(c.entitiy)) continue;
                        Helpfunctions.Instance.ErrorLog("discard " + c.id + " because of this rule " + mi.rulestring);
                        discarditems.Add(c.entitiy);
                    }
                }
            }

            if (holdlist.Count == 0 || !hasHoldListRule(ownclass, enemclass)) return discarditems;

            Dictionary<string, int> holddic = new Dictionary<string, int>();
            List<string> combosToHold = new List<string>();
            foreach (CardIDEntity c in cards)
            {
                bool delete = true;
                foreach (mulliitem mi in this.holdlist)
                {

                    if (hascoin && mi.coinrule == 1) continue;
                    if (!hascoin && mi.coinrule == 2) continue;

                    if (mi.cardid == "#MANARULE" && (mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass))
                    {
                        if (CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(c.id)).cost <= mi.manarule)
                        {
                            delete = false;
                        }
                        continue;
                    }

                    if (c.id == mi.cardid && (mi.enemyclass == "all" || mi.enemyclass == enemclass) && (mi.ownclass == "all" || mi.ownclass == ownclass))
                    {

                        if (mi.requiresCard == null)
                        {

                            if (holddic.ContainsKey(c.id)) // we are holding one of the cards
                            {
                                if (mi.howmuch == 2)
                                {
                                    delete = false;
                                }
                            }
                            else
                            {
                                delete = false;
                            }
                        }
                        else
                        {
                            bool hasRequirements = false;

                            foreach (string s in mi.requiresCard)
                            {
                                if (s.Contains("+"))
                                {
                                    int shouldadd = 1;
                                    foreach (string ss in s.Split('+'))
                                    {
                                        if(ss=="") continue;

                                        bool hascard = false;
                                        foreach (CardIDEntity reqs2 in cards)
                                        {
                                            if (reqs2.id == ss) hascard = true;
                                        }
                                        if (hascard)
                                        {
                                            shouldadd++;
                                        }
                                        else
                                        {
                                            shouldadd -= 1000;
                                        }
                                    }

                                    if (shouldadd >= 1)
                                    {
                                        hasRequirements = true;

                                        foreach (string ss in s.Split('+'))
                                        {
                                            if (ss == "") continue;

                                            combosToHold.Add(ss);
                                        }
                                    }

                                }
                                else
                                {
                                    //Helpfunctions.Instance.ErrorLog("search cards for: " + s);
                                    foreach (CardIDEntity reqs in cards)
                                    {
                                        //Helpfunctions.Instance.ErrorLog("in hand " + reqs.id);
                                        if (s == reqs.id)
                                        {
                                            hasRequirements = true;
                                            combosToHold.Add(s);
                                            //break;
                                        }
                                    }
                                    //Helpfunctions.Instance.ErrorLog("hasreqiresments " + hasRequirements);
                                    
                                }
                            }

                            
                            if (hasRequirements)
                            {
                                if (holddic.ContainsKey(c.id)) // we are holding one of the cards
                                {
                                    if (mi.howmuch == 2)
                                    {
                                        delete = false;
                                    }
                                }
                                else
                                {
                                    delete = false;
                                }
                            }

                        }
                    }
                }


                if (!usedManarule)
                {
                    if (delete)
                    {
                        if (discarditems.Contains(c.entitiy)) continue;
                        discarditems.Add(c.entitiy);
                    }
                    else
                    {
                        discarditems.RemoveAll(x => x == c.entitiy);

                        if (holddic.ContainsKey(c.id))
                        {
                            holddic[c.id]++;
                        }
                        else
                        {
                            holddic.Add(c.id, 1);
                        }
                    }
                }
                else
                {//used manarules in discard line
                    if (!delete)
                    {
                        discarditems.RemoveAll(x => x == c.entitiy);

                        if (holddic.ContainsKey(c.id))
                        {
                            holddic[c.id]++;
                        }
                        else
                        {
                            holddic.Add(c.id, 1);
                        }
                    }
                }

            }

            //remove combo items from discard list
            foreach (CardIDEntity cie in cards)
            {
                int amountInDiscard = 0;
                int amountInHand =0 ;
                foreach (CardIDEntity cie2 in cards)
                {
                    if(cie.id == cie2.id)
                    {
                        amountInHand++;

                        if(discarditems.Contains(cie2.entitiy))  amountInDiscard++;
                    }
                }


                if (amountInDiscard == amountInHand && combosToHold.Contains(cie.id))
                {
                    //remove item

                    discarditems.Remove(cie.entitiy);
                }

            }

            return discarditems;

        }

        public void setAutoConcede(bool mode)
        {
            this.loserLoserLoser = mode;
        }

        public bool shouldConcede(HeroEnum ownhero, HeroEnum enemHero)
        {

            foreach (concedeItem ci in concedelist)
            {
                if (ci.urhero == ownhero && ci.enemhero.Contains(enemHero)) return true;
            }

            return false;
        }

    }

}