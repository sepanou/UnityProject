using Entity.DynamicEntity.LivingEntity.Player;
using System;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity;
using UnityEngine;
using Random = System.Random;

namespace Entity.DynamicEntity.Weapon
{
    public abstract class Weapon : DynamicEntity
    {
        public Player holder;
        public bool equipped;
        public int defaultDamage;
        // For cooldown purposes
        protected float LastAttackTime;
        protected int specialAttackCost;
        private string _name;
        private Sprite _displayedSprite;
        [SerializeField] protected Vector3 defaultCoordsWhenLikedToPlayer;

        public string Name { get; private set; }
        public Sprite DisplayedSprite { get; private set; }
        
        public void OnSelect<T>(T source)
        {
            if (source is Player player)
                holder = player;
            else
                return;
            transform.parent = holder.transform;
            transform.localPosition = defaultCoordsWhenLikedToPlayer;
            equipped = true;
            gameObject.SetActive(true);
        }

        public void OnUse<T>(T source)
        {
            if (source is Player player && player == holder && !CanAttack())
                return;
            // Can't run the default & special attack simultaneously !
            if (Input.GetButtonDown("Fire1"))
                DefaultAttack();
            else if (holder.HasEnoughEnergy(specialAttackCost) && Input.GetButtonDown("Fire2"))
                SpecialAttack();
        }

        public void OnDeselect<T>(T source)
        {
            equipped = false;
            gameObject.SetActive(false);
        }

        public void OnDrop<T>(T source)
        {
            holder = null;
            equipped = false;
            gameObject.transform.parent = null;
            // source == mob or player or NPC
            if (source is LivingEntity.LivingEntity livingEntity)
                gameObject.transform.position = livingEntity.transform.position;
            gameObject.SetActive(true);
        }

        protected abstract void DefaultAttack();
        protected abstract void SpecialAttack();

        private bool CanAttack()
        {
            return holder &&
                   equipped &&
                   (float.IsNaN(LastAttackTime) || !(Time.fixedTime - LastAttackTime < GetSpeed()));
        }

        protected void InitialiseWeapon()
        {
            Name = _name;
            DisplayedSprite = _displayedSprite;
            LastAttackTime = float.NaN;
            InstantiateDynamicEntity();
        }
        
        // Part for the random name generator

        public string GenerateName(Dictionary<int, (int, string)> weaponType)
        {
            Random random = new Random();
            (int adjType, string weaponName) = weaponType[random.Next(weaponType.Count)];
            (string, string) adjs = Adjectives[random.Next(Adjectives.Count)];
            string adj;
            if (adjType == 0)
            {
                (adj, _) = adjs;
            }
            else
            {
                (_, adj) = adjs;
            }

            string comp = NameComplements[random.Next(NameComplements.Count)];
            return weaponName + " " + adj + " " + comp;
        }

        protected Dictionary<int, (string, string)> Adjectives =
            new Dictionary<int, (string, string)>()
            {
                {0, ("légendaire", "légendaire")}, // (masculin, féminin)
                {1, ("mythique", "mythique")},
                {2, ("divin", "divine")},
                {3, ("incroyable", "incroyable")},
                {4, ("démoniaque", "démoniaque")},
                {5, ("diabolique", "diabolique")},
                {6, ("aiguisé", "aiguisée")},
                {7, ("réussi", "réussie")},
                {8, ("enchanté", "enchantée")},
                {9, ("maladroit", "maladroite")},
                {10, ("incassable", "incassable")},
                {11, ("indestructible", "indestructible")},
                {12, ("redouté", "redoutée")},
                {13, ("redoutable", "redoutable")},
                {14, ("perdu", "perdue")},
                {15, ("souillé", "souillée")},
                {16, ("brisé", "brisée")},
                {17, ("salvateur", "salvatrice")},
                {18, ("incroyable", "incroyable")},
                {19, ("horrible", "horrible")},
                {20, ("échoué", "échouée")},
                {21, ("fragmenté", "fragmentée")},
                {22, ("terrifiante", "terrifiante")},
                {23, ("enflamé", "enflamée")},
                {24, ("spectral", "spectrale")},
                {25, ("fantomatique", "fantomatique")},
                {26, ("piquante", "piquante")},
                {27, ("forcené", "forcenée")},
                {28, ("tranchant", "tranchante")},
                {29, ("brulant", "brulante")},
                {30, ("glacé", "glacée")},
                {31, ("froid", "froide")},
                {32, ("émoussé", "émoussée")},
                {33, ("détruit", "détruite")},
                {34, ("pervers", "perverse")},
                {35, ("irréel", "irréelle")},
                {36, ("lovecraftien", "lovecraftienne")},
                {37, ("machiavélique", "machiavélique")},
                {38, ("ennuyant", "ennuyante")},
                {39, ("fatiguant", "fatiguante")},
                {40, ("agile", "agile")},
                {41, ("standard", "standard")},
                {42, ("universel", "universelle")}
            };

        protected Dictionary<int, string> NameComplements = new Dictionary<int, string>()
        {
            {0, "du réel"},
            {1, "d'or"},
            {2, "de platine"},
            {3, "de Khrom"},
            {4, "de Gurdil"},
            {5, "du culte"},
            {6, "des tréfonds"},
            {7, "de la forêt"},
            {8, "du coin"},
            {9, "du marchand"},
            {10, "de l'ivrogne"},
            {11, "de l'aventurier"},
            {12, "de l'orc"},
            {13, "du nain"},
            {14, "du gobelin"},
            {15, "de l'elfe"},
            {16, "de l'humain"},
            {17, "de tartempion"},
            {18, "de Paimpont"},
            {19, "du roi Arthur"},
            {20, "des alentours"},
            {21, "du berger"},
            {22, "du forgeron"},
            {23, "de l'indigène"},
            {24, "du prêtre"},
            {25, "du mage"},
            {26, "du sorcier"},
            {27, "du voleur"},
            {28, "de Khorn"},
            {29, "du seigneur des tombes"},
            {30, "des rats"},
            {31, "des recoins du monde"},
            {32, "d'ici"},
            {33, "du barbare"},
            {34, "du cultiste"},
            {35, "d'oblivion"},
            {36, "des indes"},
            {37, "de la colline"},
            {38, "des montagnes"},
            {39, "des plaines"},
            {40, "de Tzeentch"},
            {41, "de Slaneesh"},
            {42, "de Nurgle"}
        };
    }
}