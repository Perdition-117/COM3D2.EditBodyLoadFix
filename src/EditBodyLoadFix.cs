using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using SceneEditWindow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.EditBodyLoadFix;

[BepInPlugin("net.perdition.com3d2.editbodyloadfix", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
class EditBodyLoadFix : BaseUnityPlugin {
	private static readonly MPN[] FixMpns = {
		Mpn.MuneL,
		Mpn.MuneS,
		Mpn.MuneTare,
		Mpn.MuneUpDown,
		Mpn.MuneYori,
		Mpn.MuneYawaraka,
		Mpn.EyeBallPosY,
		Mpn.EyeBallSclX,
		Mpn.EyeBallSclY,
	};

	private void Awake() {
		var version = new Version(GameUty.GetBuildVersionText());
		Harmony.CreateAndPatchAll(typeof(EditBodyLoadFix));
		Harmony.CreateAndPatchAll(version.Major >= 3 ? typeof(EditBodyLoadFix30) : typeof(EditBodyLoadFix20));
	}

	[HarmonyPatch(typeof(Maid), nameof(Maid.SetProp), typeof(MaidProp), typeof(string), typeof(int), typeof(bool), typeof(bool))]
	[HarmonyPostfix]
	private static void PostSetProp(Maid __instance, MaidProp mp) {
		if ((MPN)mp.idx == Mpn.body && SceneManager.GetActiveScene().name == "SceneEdit") {
			__instance.StartCoroutine(ResetBody(__instance));
		}
	}

	private static IEnumerator ResetBody(Maid maid) {
		yield return new WaitForSeconds(0.5f);
		ResetPose();
		ResetParts(maid);
		ResetCustomPartsEdit(maid);
		ResetTouchJump(maid);
	}

	// fixes motorbike pose
	private static void ResetPose() {
		var itemData = PoseIconData.GetItemData(SceneEdit.Instance.pauseIconWindow.selectedIconId);
		itemData.ExecScript();
	}

	// fixes pupil parameters and rigid breasts
	private static void ResetParts(Maid maid) {
		foreach (var mpn in FixMpns) {
			var maidProp = maid.GetProp(mpn);
			if (maidProp.type == 1) {
				maid.body0.VertexMorph_FromProcItem(maidProp.name, maidProp.value / 100f);
			} else if (maidProp.type == 2) {
				maid.body0.BoneMorph_FromProcItem(maidProp.name, maidProp.value / 100f);
			}
		}
	}

	// fixes maid animation not freezing when placing accessories
	private static void ResetCustomPartsEdit(Maid maid) {
		SceneEdit.Instance.customPartsWindow.animation = maid.GetAnimation();
	}

	// fixes touch jump and VR grabbing
	private static void ResetTouchJump(Maid maid) {
		var maidColliderCollect = MaidColliderCollect.AddColliderCollect(maid);

		var sceneEdit = SceneEdit.Instance;

		// Also resets VR grabbing
		var touchJumpColliderList = maidColliderCollect.AddCollider(MaidColliderCollect.ColliderType.Grab);

		if (GameMain.Instance.VRMode) {
			touchJumpColliderList.Clear();
		} else {
			var boneDictionary = IKManager.CreateBoneDic(maid);

			var transformToTouchType = new Dictionary<Transform, SceneEdit.TouchType> {
				[boneDictionary[IKManager.BoneType.Head].Value.transform] = SceneEdit.TouchType.Head,
				[boneDictionary[IKManager.BoneType.Spine1].Value.transform] = SceneEdit.TouchType.UpperBody,
				[boneDictionary[IKManager.BoneType.Bust_R].Value.transform] = SceneEdit.TouchType.Bust,
				[boneDictionary[IKManager.BoneType.Bust_L].Value.transform] = SceneEdit.TouchType.Bust,
				[boneDictionary[IKManager.BoneType.Pelvis].Value.transform] = SceneEdit.TouchType.LowerBody,
				[boneDictionary[IKManager.BoneType.Thigh_L].Value.transform] = SceneEdit.TouchType.Leg,
				[boneDictionary[IKManager.BoneType.Thigh_R].Value.transform] = SceneEdit.TouchType.Leg,
			};

			var touchTypeToCallback = new Dictionary<SceneEdit.TouchType, Action> {
				[SceneEdit.TouchType.Head] = () => sceneEdit.OnTouchJump(SceneEdit.TouchType.Head),
				[SceneEdit.TouchType.Bust] = () => sceneEdit.OnTouchJump(SceneEdit.TouchType.Bust),
				[SceneEdit.TouchType.UpperBody] = () => sceneEdit.OnTouchJump(SceneEdit.TouchType.UpperBody),
				[SceneEdit.TouchType.LowerBody] = () => sceneEdit.OnTouchJump(SceneEdit.TouchType.LowerBody),
				[SceneEdit.TouchType.Leg] = () => sceneEdit.OnTouchJump(SceneEdit.TouchType.Leg),
			};

			foreach (var collider in touchJumpColliderList) {
				var colliderEvent = collider.gameObject.AddComponent<ColliderEvent>();
				var transform = colliderEvent.transform;

				while (transform != null && !transformToTouchType.ContainsKey(transform)) {
					transform = transform.parent;
				}

				if (transform == null) {
					continue;
				}

				var touchType = transformToTouchType[transform];
				colliderEvent.onMouseDown = touchTypeToCallback[touchType];
			}
		}

		sceneEdit.touchJumpColliderList = touchJumpColliderList;
		sceneEdit.m_bUseTouchJump = sceneEdit.m_bUseTouchJump;
	}

	// loads custom bodies from presets
	private static IEnumerable<CodeInstruction> PresetSetTranspiler(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
			.End()
			.MatchStartBackwards(
				new CodeMatch(OpCodes.Ldarg_1),
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Maid), nameof(Maid.AllProcPropSeqStart))))
			.Advance(1)
			.Insert(
				new CodeInstruction(OpCodes.Ldarg_1),
				new CodeInstruction(OpCodes.Ldarg_2),
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EditBodyLoadFix), nameof(PostPresetSet))))
			.InstructionEnumeration();
	}

	class EditBodyLoadFix30 {
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset), typeof(bool))]
		private static IEnumerable<CodeInstruction> PresetSetTranspiler(IEnumerable<CodeInstruction> instructions) => EditBodyLoadFix.PresetSetTranspiler(instructions);
	}

	class EditBodyLoadFix20 {
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset))]
		private static IEnumerable<CodeInstruction> PresetSetTranspiler(IEnumerable<CodeInstruction> instructions) => EditBodyLoadFix.PresetSetTranspiler(instructions);
	}

	private static void PostPresetSet(Maid f_maid, CharacterMgr.Preset f_prest) {
		if (f_prest.ePreType == CharacterMgr.PresetType.Body || f_prest.ePreType == CharacterMgr.PresetType.All) {
			var maidProp = f_prest.listMprop.Find(e => e.idx == (int)Mpn.body);
			if (maidProp != null && GameMain.Instance.CharacterMgr.IsEnableMenu(maidProp.strFileName)) {
				f_maid.SetProp((MPN)maidProp.idx, maidProp.strFileName, maidProp.nFileNameRID, false, false);
			}
		}
	}
}
