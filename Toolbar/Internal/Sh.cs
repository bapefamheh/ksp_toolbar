﻿/*
Copyright (c) 2013-2014, Maik Schreiber
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Toolbar {
	[KSPAddonFixed(KSPAddon.Startup.MainMenu, false, typeof(Sh))]
	internal class Sh : MonoBehaviour {
		private static bool done;

		private GameObject kerbal;
		private Vector3 kerbalOrigPos;
		private Quaternion kerbalOrigRot;
#if DEBUG
		private Rect windowRect = new Rect(50, 50, 0, 0);
		private GameObject gameObjectToPosition;
		private Vector3 gameObjectToPositionOrigPos;
		private Quaternion gameObjectToPositionOrigRot;
		private Vector3 posOffset = Vector3.zero;
		private Vector3 rotation = Vector3.zero;
		private float scale = 1;
#endif

		internal void Start() {
			if (done) {
				return;
			}

			kerbal = GameObject.Find("kbEVA@idle");
			if (kerbal == null) {
				return;
			}

#if !DEBUG
			if (ToolbarManager.InternalInstance.UpdateChecker.Done) {
				UnityEngine.Random.seed = (int) DateTime.UtcNow.getSeconds();
				float random = UnityEngine.Random.Range(0f, 100f);
				Log.debug("sh: {0:F1} vs. {1:F1}", random, ToolbarManager.InternalInstance.UpdateChecker.Sh);
				if (random < ToolbarManager.InternalInstance.UpdateChecker.Sh) {
#endif
					sh();
					done = true;
#if !DEBUG
				}
			}
#endif
		}

		private void sh() {
			GameObject sh = GameDatabase.Instance.GetModel("000_Toolbar/sh");
			if (sh == null) {
				return;
			}

			kerbalOrigPos = kerbal.transform.position;
			kerbalOrigRot = kerbal.transform.rotation;

			sh.transform.position = kerbalOrigPos + new Vector3(1.63f, -0.15f, 2.06f);
			sh.transform.rotation = kerbalOrigRot * Quaternion.Euler(-29, 147, 0);
			sh.transform.localScale = Vector3.one * 100;
			sh.SetActive(true);

			GameObject sh2 = GameDatabase.Instance.GetModel("000_Toolbar/sh");
			sh2.transform.position = kerbalOrigPos + new Vector3(17.94f, -0.31f, 2.45f);
			sh2.transform.rotation = kerbalOrigRot * Quaternion.Euler(-85.5f, -14, 0);
			sh2.transform.localScale = Vector3.one * 100;
			sh2.SetActive(true);

			kerbal.transform.rotation *= Quaternion.Euler(0, -100, 0);
			undress(kerbal);

			doKerbal(new Vector3(14.48f, -0.18f, 3.98f), -140);
			doKerbal(new Vector3(17.33f, -0.27f, 5.7f), -90);
			doKerbal(new Vector3(18.54f, -0.34f, 3.96f), 160);
			doKerbal(new Vector3(12.96f, 0.1f, -0.3f), 0);

			GameObject rcsTank = GameDatabase.Instance.GetModel("Squad/Parts/FuelTank/RCSFuelTank/model");
			if (rcsTank != null) {
				rcsTank.transform.position = new Vector3(16.67f, -0.89f, 4.32f);
				rcsTank.transform.rotation = Quaternion.Euler(2, 134, 0);
				rcsTank.transform.localScale = Vector3.one / 2f;
				rcsTank.SetActive(true);
			}

			GameObject rcsBlock = GameDatabase.Instance.GetModel("Squad/Parts/Utility/RCS block/model");
			if (rcsBlock != null) {
				rcsBlock.transform.position = new Vector3(16.56f, -0.69f, 4.38f);
				rcsBlock.transform.rotation = Quaternion.Euler(3, 35, -89);
				rcsBlock.transform.localScale = Vector3.one / 2f;
				rcsBlock.SetActive(true);
			}

			GameObject rcsBlock2 = GameDatabase.Instance.GetModel("Squad/Parts/Utility/RCS block/model");
			if (rcsBlock2 != null) {
				rcsBlock2.transform.position = new Vector3(16.3f, -0.96f, 4.29f);
				rcsBlock2.transform.rotation = Quaternion.Euler(-49, 25, -44);
				rcsBlock2.transform.localScale = Vector3.one / 2f;
				rcsBlock2.SetActive(true);
			}

#if DEBUG
			//gameObjectToPosition = kerbal2;
			//gameObjectToPositionOrigPos = gameObjectToPosition.transform.position;
			//gameObjectToPositionOrigRot = gameObjectToPosition.transform.rotation;
#endif
		}

		private
#if DEBUG
			GameObject
#else
			void
#endif
			doKerbal(Vector3 offset, float rotation) {

			Vector3 pos = kerbalOrigPos + offset;
			Quaternion rot = kerbalOrigRot * Quaternion.Euler(0, rotation, 0);
			GameObject k = (GameObject) Instantiate(kerbal, pos, rot);
			undress(k);
#if DEBUG
			return k;
#endif
		}

		private void undress(GameObject kerbal) {
			foreach (Renderer renderer in kerbal.GetComponentsInChildren<Renderer>()
				.Where(r => (r.name == "helmet") || (r.name == "visor") || r.name.StartsWith("jetpack_base") ||
					r.name.Contains("handle") || r.name.Contains("thruster") || r.name.Contains("tank") ||
					r.name.Contains("pivot") || r.name.EndsWith("_a01") || r.name.EndsWith("_b01"))) {

				renderer.enabled = false;
			}
		}

#if DEBUG
		private void OnGUI() {
			if (gameObjectToPosition != null) {
				windowRect = GUILayout.Window(1, windowRect, id => drawWindow(), "Positioning");
			}
		}

		private void drawWindow() {
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			posOffset.x = GUILayout.HorizontalSlider(posOffset.x, -1000, 1000, GUILayout.Width(400));
			GUILayout.Label(string.Format("x: {0:F2}", gameObjectToPosition.transform.position.x));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			posOffset.y = GUILayout.HorizontalSlider(posOffset.y, -10, 10, GUILayout.Width(400));
			GUILayout.Label(string.Format("y: {0:F2}", gameObjectToPosition.transform.position.y));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			posOffset.z = GUILayout.HorizontalSlider(posOffset.z, -500, 500, GUILayout.Width(400));
			GUILayout.Label(string.Format("z: {0:F2}", gameObjectToPosition.transform.position.z));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			rotation.z = GUILayout.HorizontalSlider(rotation.z, -179, 180, GUILayout.Width(400));
			GUILayout.Label(string.Format("rotZ: {0:F0}", rotation.z));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			rotation.x = GUILayout.HorizontalSlider(rotation.x, -179, 180, GUILayout.Width(400));
			GUILayout.Label(string.Format("rotX: {0:F0}", rotation.x));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			rotation.y = GUILayout.HorizontalSlider(rotation.y, -179, 180, GUILayout.Width(400));
			GUILayout.Label(string.Format("rotY: {0:F0}", rotation.y));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			scale = GUILayout.HorizontalSlider(scale, 1, 200, GUILayout.Width(400));
			GUILayout.Label(string.Format("scale: {0:F0}", scale));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private void Update() {
			if (gameObjectToPosition != null) {
				gameObjectToPosition.transform.position = gameObjectToPositionOrigPos + posOffset;
				gameObjectToPosition.transform.rotation = Quaternion.Euler(rotation);
				gameObjectToPosition.transform.localScale = Vector3.one * scale;
			}
		}
#endif
	}
}
