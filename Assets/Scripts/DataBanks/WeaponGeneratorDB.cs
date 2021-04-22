using System.Collections.Generic;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon.MeleeWeapon;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DataBanks
{
    [CreateAssetMenu(fileName = "WeaponGeneratorDB", menuName = "DataBanks/WeaponGenerator", order = 2)]
    public class WeaponGeneratorDB : ScriptableObject
    {
        public static WeaponGeneratorDB Instance;

        // Charms
        [SerializeField] private GameObject charmModel;
        [SerializeField] private Sprite[] charmSprites;
        
        // Bows
        [SerializeField] private GameObject bowModel;
        [SerializeField] private Sprite[] bowSprites;
        
        // Staffs
        [SerializeField] private GameObject staffModel;
        [SerializeField] private Sprite[] staffSprites;
        
        // Swords
        [SerializeField] private GameObject swordModel;
        [SerializeField] private Sprite[] swordSprites;
        
        private readonly Dictionary<int, (string, string)> _adjectives =
            new Dictionary<int, (string, string)>
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

        private readonly Dictionary<int, string> _nameComplements = 
            new Dictionary<int, string>
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

        private void OnEnable()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(this);
        }

        public string GenerateName(Dictionary<int, (int, string)> weaponType)
        {
            (int adjType, string weaponName) = weaponType[Random.Range(0, weaponType.Count)];
            (string, string) adjs = _adjectives[Random.Range(0, _adjectives.Count)];
            string adj;
            if (adjType == 0)
                (adj, _) = adjs;
            else
                (_, adj) = adjs;

            string comp = _nameComplements[Random.Range(0, _nameComplements.Count)];
            return weaponName + " " + adj + " " + comp;
        }

        private T GetRandomInArray<T>(T[] array)
            => array.Length == 0 ? default : array[Random.Range(0, array.Length)];

        private CharmData GenerateCharmData()
        {
            CharmData result = new CharmData();
            if (Random.Range(0, 2) == 1)
                result.DefaultAttackDamageBonus = Random.Range(0f, 0.05f);
            if (Random.Range(0, 2) == 1)
                result.SpecialAttackDamageBonus = Random.Range(0f, 0.025f);
            if (Random.Range(0, 2) == 1)
                result.HealthBonus = Random.Range(0, 10);
            if (Random.Range(0, 2) == 1)
                result.PowerBonus = Random.Range(0, 5);
            if (Random.Range(0, 2) == 1)
                result.SpeedBonus = Random.Range(0f, 0.025f);
            if (Random.Range(0, 2) == 1)
                result.CooldownReduction = Random.Range(0f, 0.025f);
            return result;
        }

        private RangedWeaponData GenerateRangeData(bool epic = false)
        {
            RangedWeaponData result = new RangedWeaponData();
            result.DefaultDamageMultiplier = Random.Range(0.5f, 2f);
            result.SpecialDamageMultiplier = Random.Range(0.75f, 1.25f);
            result.ProjectileNumber = Random.Range(1, 7);
            result.ProjectileSpeedMultiplier = Random.Range(0.5f, 2f);
            result.ProjectileSizeMultiplier = Random.Range(0.5f, 2f);
            if (epic)
                result *= 2;
            return result;
        }

        private MeleeWeaponData GenerateMeleeData(bool epic = false)
        {
            MeleeWeaponData result = new MeleeWeaponData();
            result.DefaultDamageMultiplier = Random.Range(0.5f, 3f);
            result.SpecialDamageMultiplier = Random.Range(0.75f, 1.5f);
            result.WeaponSizeMultiplier = Random.Range(0.75f, 2f);
            result.KnockbackMultiplier = Random.Range(0.75f, 3f);
            if (epic)
                result *= 2;
            return result;
        }

        public Charm GenerateCharm(CharmData data = null, Sprite sprite = null)
        {
            GameObject obj = Instantiate(charmModel);
            Charm result = obj.GetComponent<Charm>();
            result.Bonuses = data ?? GenerateCharmData();
            result.GetSpriteRenderer().sprite = !sprite ? GetRandomInArray(charmSprites): sprite;
            return result;
        }

        public Bow GenerateBow()
        {
            GameObject obj = Instantiate(bowModel);
            Bow result = obj.GetComponent<Bow>();
            result.RangeData = GenerateRangeData();
            result.RangeData.Name = "Mighty Bow";
            result.GetSpriteRenderer().sprite = GetRandomInArray(bowSprites);
            return result;
        }
        
        public Staff GenerateStaff()
        {
            GameObject obj = Instantiate(staffModel);
            Staff result = obj.GetComponent<Staff>();
            result.RangeData = GenerateRangeData();
            result.RangeData.Name = "Mighty Staff";
            result.GetSpriteRenderer().sprite = GetRandomInArray(staffSprites);
            return result;
        }
        
        public MeleeWeapon GenerateSword()
        {
            GameObject obj = Instantiate(swordModel);
            MeleeWeapon result = obj.GetComponent<MeleeWeapon>();
            result.MeleeData = GenerateMeleeData();
            result.MeleeData.Name = "Mighty Sword";
            result.GetSpriteRenderer().sprite = GetRandomInArray(swordSprites);
            return result;
        }
    }
}
