using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using SceneEditWindow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.EditBodyLoadFix {
	[BepInPlugin("net.perdition.com3d2.editbodyloadfix", "EditBodyLoadFix", "1.0.0")]
	class EditBodyLoadFix : BaseUnityPlugin {
		static readonly List<MPN> FixMpns = new List<MPN> {
			MPN.MuneL,
			MPN.MuneS,
			MPN.MuneTare,
			MPN.MuneUpDown,
			MPN.MuneYori,
			MPN.MuneYawaraka,
		};

		void Awake() {
			Harmony.CreateAndPatchAll(typeof(EditBodyLoadFix));
		}

		[HarmonyPatch(typeof(Maid), "SetProp", typeof(MaidProp), typeof(string), typeof(int), typeof(bool), typeof(bool))]
		[HarmonyPostfix]
		static void PostSetProp(Maid __instance, MaidProp mp) {
			if ((MPN)mp.idx == MPN.body && SceneManager.GetActiveScene().name == "SceneEdit") {
				__instance.StartCoroutine(ResetBody(__instance));
			}
		}

		static IEnumerator ResetBody(Maid maid) {
			yield return new WaitForSeconds(0.5f);
			ResetPose();
			ResetParts(maid);
			ResetCustomPartsEdit(maid);
		}

		static void ResetPose() {
			var itemData = PoseIconData.GetItemData(SceneEdit.Instance.pauseIconWindow.selectedIconId);
			itemData.ExecScript();
		}

		static void ResetParts(Maid maid) {
			foreach (var mpn in FixMpns) {
				var maidProp = maid.GetProp(mpn);
				if (maidProp.type == 1) {
					maid.body0.VertexMorph_FromProcItem(maidProp.name, maidProp.value / 100f);
				} else if (maidProp.type == 2) {
					maid.body0.BoneMorph_FromProcItem(maidProp.name, maidProp.value / 100f);
				}
			}
		}

		static void ResetCustomPartsEdit(Maid maid) {
			SceneEdit.Instance.customPartsWindow.animation = maid.GetAnimation();
		}

		[HarmonyPatch(typeof(CharacterMgr), "PresetSet", typeof(Maid), typeof(CharacterMgr.Preset))]
		[HarmonyPostfix]
		static void PostPresetSet(Maid f_maid, CharacterMgr.Preset f_prest) {
			if (f_prest.ePreType == CharacterMgr.PresetType.Body || f_prest.ePreType == CharacterMgr.PresetType.All) {
				var maidProp = f_prest.listMprop.Find(e => e.idx == (int)MPN.body);
				if (IsEnableMenu(maidProp.strFileName)) {
					f_maid.SetProp((MPN)maidProp.idx, maidProp.strFileName, maidProp.nFileNameRID, false, false);
				}
			}
		}

		static bool IsEnableMenu(string f_strFileName) {
			if (CharacterMgr.EditModeLookHaveItem) {
				return GameMain.Instance.CharacterMgr.status.IsHavePartsItem(f_strFileName) && GameUty.IsExistFile(f_strFileName, null);
			}
			return GameUty.IsExistFile(f_strFileName, null);
		}
	}
}
