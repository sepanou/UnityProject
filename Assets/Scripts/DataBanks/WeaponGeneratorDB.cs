﻿using System;
using System.Collections.Generic;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon.MeleeWeapon;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DataBanks {
	[CreateAssetMenu(fileName = "WeaponGeneratorDB", menuName = "DataBanks/WeaponGenerator", order = 2)]
	public class WeaponGeneratorDB: ScriptableObject {
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
		
		// Used for name generation
		private static readonly IReadOnlyList<(bool, string)> BowNames =
			new List<(bool, string)> {
				(false, "L'arc"),				// false == masculine adjective, feminine otherwise.
				(false, "L'arc court"),
				(false, "L'arc long"),
				(false, "L'arc monobloc"),
				(false, "L'arc à poulies"),
				(false, "L'arc droit"),
				(false, "L'arc de chasse"),
				(false, "L'arc Yumi"),
				(true, "La Tempête"),
				(false, "Le Déluge") // Après moi le déluge
			}.AsReadOnly();

		private static readonly IReadOnlyList<(bool, string)> StaffNames =
			new List<(bool, string)> {
				(false, "Le sceptre"),
				(false, "Le bâton"),
				(true, "La baguette"),
				(true, "La crosse"),
				(false, "Le bourdon"),
				(false, "Le bâton"),
				(true, "La canne"),
				(false, "Le gourdin"),
				(false, "Le témoin")
			}.AsReadOnly();

		private static readonly IReadOnlyList<(bool, string)> MeleeNames =
			new List<(bool, string)> {
				(true, "L'épée"),
				(false, "Le glaive"),
				(true, "La broadsword"),
				(true, "La claymore"),
				(false, "L'espadon"),
				(true, "L'épée bâtarde"),
				(true, "La flamberge"),
				(true, "La rapière"),
				(true, "La canne-épée"),
				(false, "Le jian"),
				(true, "La machette"),
				(false, "Le katana"),
				(false, "Le sabre")
			}.AsReadOnly();

		private static readonly IReadOnlyList<(bool, string)> CharmNames =
			new List<(bool, string)> {
				(false, "Le talisman"),
				(false, "Le médaillon"),
				(true, "L'amulette"),
				(false, "Le sceau"),
				(true, "La médaille"),
				(true, "La relique"),
				(true, "La bague"),
				(false, "L'anneau"),
				(false, "Le collier"),
				(false, "Le pendentif"),
				(true, "La croix"),
				(false, "Le linceul"),
				(true, "La fiole"),
				(false, "Le charme"),
				(false, "Le totem")
			}.AsReadOnly();
		
		private static readonly List<(string, string)> Adjectives =
			new List<(string, string)> {
				("légendaire", "légendaire"), // (masculin, féminin)
				("mythique", "mythique"),
				("divin", "divine"),
				("incroyable", "incroyable"),
				("démoniaque", "démoniaque"),
				("diabolique", "diabolique"),
				("aiguisé", "aiguisée"),
				("réussi", "réussie"),
				("enchanté", "enchantée"),
				("maladroit", "maladroite"),
				("incassable", "incassable"),
				("indestructible", "indestructible"),
				("redouté", "redoutée"),
				("redoutable", "redoutable"),
				("perdu", "perdue"),
				("souillé", "souillée"),
				("brisé", "brisée"),
				("salvateur", "salvatrice"),
				("incroyable", "incroyable"),
				("horrible", "horrible"),
				("échoué", "échouée"),
				("fragmenté", "fragmentée"),
				("terrifiant", "terrifiante"),
				("enflamé", "enflamée"),
				("spectral", "spectrale"),
				("fantomatique", "fantomatique"),
				("piquant", "piquante"),
				("forcené", "forcenée"),
				("tranchant", "tranchante"),
				("brulant", "brulante"),
				("glacé", "glacée"),
				("froid", "froide"),
				("émoussé", "émoussée"),
				("détruit", "détruite"),
				("pervers", "perverse"),
				("irréel", "irréelle"),
				("lovecraftien", "lovecraftienne"),
				("machiavélique", "machiavélique"),
				("ennuyant", "ennuyante"),
				("fatiguant", "fatiguante"),
				("agile", "agile"),
				("standard", "standard"),
				("universel", "universelle"),
				("frénétique", "frénétique"),
				("maudit", "maudite"),
				("apocalyptique", "apocalyptique"),
				("bourrin", "bourrine"),
				("perrave", "perrave")
			};

		private static readonly List<string> NameComplements = 
			new List<string> {
				"du réel",
				"d'or",
				"de platine",
				"de Khrom",
				"de Gurdil",
				"du culte",
				"des tréfonds",
				"de la forêt",
				"du coin",
				"du marchand",
				"de l'ivrogne",
				"de l'aventurier",
				"de l'orc",
				"du nain",
				"du gobelin",
				"de l'elfe",
				"de l'humain",
				"de tartempion",
				"de Paimpont",
				"du roi Arthur",
				"des alentours",
				"du berger",
				"du forgeron",
				"de l'indigène",
				"du prêtre",
				"du mage",
				"du sorcier",
				"du voleur",
				"de Khorn",
				"du seigneur des Tombes",
				"des rats",
				"des recoins du monde",
				"d'ici",
				"du barbare",
				"du cultiste",
				"d'Oblivion",
				"des Indes",
				"de la colline",
				"des montagnes",
				"des plaines",
				"de Tzeentch",
				"de Slaneesh",
				"de Nurgle",
				"des cataclysmes",
				"du Pyromane"
			};

		public static string GenerateName(IReadOnlyList<(bool, string)> weaponType) {
			(bool adjType, string weaponName) = weaponType[Random.Range(0, weaponType.Count)];
			(string, string) adjs = Adjectives[Random.Range(0, Adjectives.Count)];
			string adj = !adjType ? adjs.Item1 : adjs.Item2;
			string comp = NameComplements[Random.Range(0, NameComplements.Count)];
			return weaponName + " " + adj + " " + comp;
		}

		private static T GetRandomInArray<T>(IReadOnlyList<T> array)
			=> array.Count == 0 ? default : array[Random.Range(0, array.Count)];

		private static float RoundRandomFloat(float min, float max, int tolerance = 3)
			=> (float) Math.Round(Random.Range(min, max), tolerance);

		private static CharmData GenerateCharmData() {
			CharmData result = new CharmData {
				defaultAttackDamageBonus = RoundRandomFloat(0f, 0.25f),
				specialAttackDamageBonus = RoundRandomFloat(0f, 0.5f),
				healthBonus = Random.Range(0, 30),
				powerBonus = Random.Range(0, 50),
				speedBonus = RoundRandomFloat(0f, 0.2f),
				cooldownReduction = RoundRandomFloat(0f, 0.2f)
			};
			return result;
		}

		private static RangedWeaponData GenerateRangeData(bool epic = false) {
			RangedWeaponData result = new RangedWeaponData {
				defaultDamageMultiplier = RoundRandomFloat(0.5f, 2f),
				specialDamageMultiplier = RoundRandomFloat(0.75f, 1.25f),
				projectileNumber = Random.Range(1, 7),
				projectileSpeedMultiplier = RoundRandomFloat(0.5f, 2f),
				projectileSizeMultiplier = RoundRandomFloat(0.5f, 2f)
			};
			if (epic)
				result *= 2;
			return result;
		}

		private static MeleeWeaponData GenerateMeleeData(bool epic = false) {
			MeleeWeaponData result = new MeleeWeaponData {
				defaultDamageMultiplier = RoundRandomFloat(0.5f, 3f),
				specialDamageMultiplier = RoundRandomFloat(0.75f, 1.5f),
				weaponSizeMultiplier = RoundRandomFloat(0.75f, 2f),
				knockbackMultiplier = RoundRandomFloat(0.75f, 3f)
			};
			if (epic)
				result *= 2;
			return result;
		}

		public Charm GenerateCharm(CharmData data = null, Sprite sprite = null) {
			GameObject obj = Instantiate(charmModel);
			Charm result = obj.GetComponent<Charm>();
			result.Bonuses = data ?? GenerateCharmData();
			result.name = GenerateName(CharmNames);
			result.GetSpriteRenderer().sprite = !sprite ? GetRandomInArray(charmSprites): sprite;
			return result;
		}

		public Bow GenerateBow() {
			GameObject obj = Instantiate(bowModel);
			Bow result = obj.GetComponent<Bow>();
			result.rangeData = GenerateRangeData();
			result.rangeData.name = GenerateName(BowNames);
			result.GetSpriteRenderer().sprite = GetRandomInArray(bowSprites);
			return result;
		}
		
		public Staff GenerateStaff() {
			GameObject obj = Instantiate(staffModel);
			Staff result = obj.GetComponent<Staff>();
			result.rangeData = GenerateRangeData();
			(bool feminine, string staffName) = StaffNames[Random.Range(0,StaffNames.Count-1)];
			string adj;
			if (feminine) 
				(_, adj) = Adjectives[Random.Range(0, Adjectives.Count - 1)];
			else
				(adj, _) = Adjectives[Random.Range(0, Adjectives.Count - 1)];
			string cName = NameComplements[Random.Range(0, NameComplements.Count - 1)];
			result.rangeData.name = staffName + " " + adj + " " + cName;
			result.GetSpriteRenderer().sprite = GetRandomInArray(staffSprites);
			return result;
		}
		
		public MeleeWeapon GenerateSword() {
			GameObject obj = Instantiate(swordModel);
			MeleeWeapon result = obj.GetComponent<MeleeWeapon>();
			result.meleeData = GenerateMeleeData();
			result.meleeData.name = GenerateName(MeleeNames);
			result.GetSpriteRenderer().sprite = GetRandomInArray(swordSprites);
			return result;
		}
	}
}
