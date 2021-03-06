﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreatures
    {
        public string Nome { get; set; }     
        public int Experiencia { get {return _Experiencia; } 
            set {
                _Experiencia = value;
                OnPropertyChanged("Experiencia");
                OnPropertyChanged("Level");
            } 
        }
        public int _Experiencia;

        public int Dinheiro { get { return _Dinheiro; }
            set
            {
                _Dinheiro = value;
                OnPropertyChanged("Dinheiro");  
            }
        }

        public int _Dinheiro;
        public int Level { get { return ((Experiencia / 100) + 1); }   
        }
        public Location CurrentLocation { get; set; }
        public BindingList<ItemInventario> Inventario { get; set; }
        public BindingList<PlayerQuest> Quest { get; set; }

        public List<Weapon> Weapons {
            get { return Inventario.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); } }

        public List<PocaoDeVida> Pocao
        {
            get { return Inventario.Where(x => x.Details is PocaoDeVida).Select(x => x.Details as PocaoDeVida).ToList(); }
        }

        public Player(string nome, int vida, int vidamaxima, int dinheiro, int experiencia) : base(vida, vidamaxima)
        {
            Nome = nome;
            Experiencia = experiencia;
            Dinheiro = dinheiro;
            Inventario = new BindingList<ItemInventario>();
            Quest = new BindingList<PlayerQuest>();
        }

        private void RaiseInventoryChangedEvent(Item item)
        {
            if(item is Weapon)
            {
                OnPropertyChanged("Weapons");               
            }
            if(item is PocaoDeVida)
            {
                OnPropertyChanged("Potions");
            }
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player("Kevin", 10, 10, 0, 0);
            player.Inventario.Add(new ItemInventario(Mundo.ItemById(Mundo.ITEM_ID_RUSTY_SWORD),1));
            player.CurrentLocation = Mundo.LocationById(Mundo.LOCATION_ID_HOME);           
          
            return player;
        }

        public static Player CreatePlayerFromXMLString(string xmlPlayerData)
        {
            try
            {
                XmlDocument PlayerData = new XmlDocument();

                PlayerData.LoadXml(xmlPlayerData);
                string nome = Convert.ToString(PlayerData.SelectSingleNode("Player/Nome").InnerText);
                int vida = Convert.ToInt32(PlayerData.SelectSingleNode("/Player/Stats/Vida").InnerText);
                int vidaMaxima = Convert.ToInt32(PlayerData.SelectSingleNode("/Player/Stats/VidaMaxima").InnerText);
                int dinheiro = Convert.ToInt32(PlayerData.SelectSingleNode("/Player/Stats/Dinheiro").InnerText);
                int Experiencia = Convert.ToInt32(PlayerData.SelectSingleNode("/Player/Stats/Experiencia").InnerText);

                Player player = new Player(nome,vida,vidaMaxima,dinheiro,Experiencia);

                int CurrentLocationID = Convert.ToInt32(PlayerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = Mundo.LocationById(CurrentLocationID);

                foreach(XmlNode node in PlayerData.SelectNodes("/Player/Items/Inventario"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantidade = Convert.ToInt32(node.Attributes["Quantidade"].Value);

                    for(int i = 0; i<quantidade; i++)
                    {
                        player.AddItemToInvetory(Mundo.ItemById(id));
                    }
                }

                foreach(XmlNode node in PlayerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(Mundo.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quest.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                return Player.CreateDefaultPlayer();
            }
        }

        public string ToXmlString()
        {
            XmlDocument PlayerData = new XmlDocument();

            XmlNode player = PlayerData.CreateElement("Player");
            PlayerData.AppendChild(player);

            XmlNode stats = PlayerData.CreateElement("Stats");
            player.AppendChild(stats);

            XmlNode nome = PlayerData.CreateElement("Nome");
            nome.AppendChild(PlayerData.CreateTextNode(this.Nome));
            player.AppendChild(nome);

            XmlNode vida = PlayerData.CreateElement("Vida");
            vida.AppendChild(PlayerData.CreateTextNode(this.Vida.ToString()));
            stats.AppendChild(vida);

            XmlNode VidaMaxima = PlayerData.CreateElement("VidaMaxima");
            VidaMaxima.AppendChild(PlayerData.CreateTextNode(this.VidaMaxima.ToString()));
            stats.AppendChild(VidaMaxima);

            XmlNode dinheiro = PlayerData.CreateElement("Dinheiro");
            dinheiro.AppendChild(PlayerData.CreateTextNode(this.Dinheiro.ToString()));
            stats.AppendChild(dinheiro);

            XmlNode experiencia = PlayerData.CreateElement("Experiencia");
            experiencia.AppendChild(PlayerData.CreateTextNode(this.Experiencia.ToString()));
            stats.AppendChild(experiencia);

            XmlNode currentLocation = PlayerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(PlayerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            XmlNode Items = PlayerData.CreateElement("Items");
            player.AppendChild(Items);

            foreach (ItemInventario item in this.Inventario)
            {
                XmlNode inventarioItem = PlayerData.CreateElement("Inventario");

                XmlAttribute idAttibute = PlayerData.CreateAttribute("ID");
                idAttibute.Value = item.Details.ID.ToString();
                inventarioItem.Attributes.Append(idAttibute);

                XmlAttribute quantidadeatribute = PlayerData.CreateAttribute("Quantidade");
                quantidadeatribute.Value = item.Quantidade.ToString();
                inventarioItem.Attributes.Append(quantidadeatribute);

                Items.AppendChild(inventarioItem);
            }

            XmlNode PlayerQuests = PlayerData.CreateElement("PlayerQuests");
            player.AppendChild(PlayerQuests);

            foreach (PlayerQuest quest in this.Quest)
            {
                XmlNode PlayerQuest = PlayerData.CreateElement("PlayerQuest");

                XmlAttribute idAttibute = PlayerData.CreateAttribute("ID");
                idAttibute.Value = quest.Details.ID.ToString();
                PlayerQuest.Attributes.Append(idAttibute);

                XmlAttribute isCompletedAttribute = PlayerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                PlayerQuest.Attributes.Append(isCompletedAttribute);

                PlayerQuests.AppendChild(PlayerQuest);
            }

            return PlayerData.InnerXml;
        }
        public void RemoveItem(Item ItemToRemove, int QuantToRemove)
        {
            ItemInventario item = Inventario.SingleOrDefault(i => i.Details.ID == ItemToRemove.ID);
            item.Quantidade -= QuantToRemove;
            
            if(item.Quantidade <= 0)
            {
                Inventario.Remove(item);
            }

            RaiseInventoryChangedEvent(ItemToRemove);
        }

        public void AddItemToInvetory(Item itemtoadd)
        {
            ItemInventario item = Inventario.SingleOrDefault(i => i.Details.ID == itemtoadd.ID);

            if(item == null)
            {
                Inventario.Add(new ItemInventario(itemtoadd, 1));
            }
            else
            {
                item.Quantidade++;
            }

            RaiseInventoryChangedEvent(itemtoadd);
        }

    }
}
