﻿/*
Copyright (c) 2013, Maik Schreiber
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
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class Button : IButton {
		private static readonly Vector2 UNSIZED = new Vector2(float.NaN, float.NaN);

		private string text_;
		public string Text {
			set {
				if (!string.Equals(text_, value)) {
					text_ = value;
					if (text_.Trim() == "") {
						text_ = null;
					}

					// clear caches
					content_ = null;
					style_ = null;
					size_ = UNSIZED;
				}
			}
			get {
				return text_;
			}
		}

		private Color textColor_ = Color.white;
		public Color TextColor {
			set {
				if (!value.Equals(textColor_)) {
					textColor_ = value;

					// clear caches
					style_ = null;
				}
			}
			get {
				return textColor_;
			}
		}

		private string texturePath_;
		public string TexturePath {
			set {
				if ((value != null) && value.Contains('\\')) {
					throw new ArgumentException("texture path must use forward slash instead of backslash: " + value);
				}

				if (!string.Equals(texturePath_, value)) {
					texturePath_ = value;

					// clear caches
					texture_ = null;
					content_ = null;
					style_ = null;
					size_ = UNSIZED;
				}
			}
			get {
				return texturePath_;
			}
		}

		private GUIContent content_;
		internal GUIContent Content {
			get {
				if (content_ == null) {
					content_ = IsTextured ? new GUIContent(Texture) : new GUIContent(Text ?? "???");
				}
				return content_;
			}
		}

		private Texture2D texture_;
		private Texture2D Texture {
			get {
				if ((texture_ == null) && (texturePath_ != null)) {
					try {
						texture_ = GameDatabase.Instance.GetTexture(TexturePath, false);

						if ((texture_.width > 24) || (texture_.height > 24)) {
							Debug.LogError("button texture exceeds 24x24 pixels, ignoring texture: " + ns + "." + id);
							texture_ = null;
							texturePath_ = null;
						}
					} catch {
						Debug.LogError("error loading button texture: " + TexturePath);
						texturePath_ = null;
					}
				}
				return texture_;
			}
		}

		private string tooltip_;
		public string ToolTip {
			set {
				tooltip_ = value;
			}
			private get {
				return tooltip_;
			}
		}

		private bool visible_ = true;
		public bool Visible {
			set {
				visible_ = value;

				if (!visible_) {
					// we don't need these for now
					texture_ = null;
					content_ = null;
					style_ = null;
				}
			}
			get {
				return visible_;
			}
		}

		public IVisibility Visibility {
			get;
			set;
		}

		internal bool EffectivelyVisible {
			get {
				return Visible && ((Visibility == null) || Visibility.Visible);
			}
		}

		private GUIStyle style_;
		internal GUIStyle Style {
			get {
				if (style_ == null) {
					style_ = new GUIStyle(GUI.skin.button);
					style_.alignment = TextAnchor.MiddleCenter;
					style_.normal.textColor = TextColor;
					style_.onHover.textColor = TextColor;
					style_.hover.textColor = TextColor;
					style_.onActive.textColor = TextColor;
					style_.active.textColor = TextColor;
					style_.onFocused.textColor = TextColor;
					style_.focused.textColor = TextColor;
					if (IsTextured) {
						style_.padding = new RectOffset(0, 0, 0, 0);
					}
				}
				return style_;
			}

			private set {
				style_ = value;
			}
		}

		private Vector2 size_ = UNSIZED;
		internal Vector2 Size {
			get {
				if (size_.Equals(UNSIZED)) {
					if (IsTextured) {
						size_ = new Vector2(32, 32);
					} else {
						size_ = Style.CalcSize(Content);
						size_.x += Style.padding.left + Style.padding.right;
						size_.y += Style.padding.top + Style.padding.bottom;
					}
				}
				return size_;
			}
		}

		public bool Enabled {
			get;
			set;
		}

		internal bool IsTextured {
			get {
				return TexturePath != null;
			}
		}

		public event ClickHandler OnClick;
		public event Action OnDestroy;

		internal readonly string ns;
		internal readonly string id;

		internal Button(string ns, string id) {
			checkId(ns, "namespace");
			checkId(id, "ID");

			this.ns = ns;
			this.id = id;

			Enabled = true;
		}

		private void checkId(string id, string label) {
			if (id.Contains('.') || id.Contains(' ') || id.Contains('/') || id.Contains(':')) {
				throw new ArgumentException(label + " contains invalid characters: " + id);
			}
		}

		internal void draw(Rect rect) {
			bool oldEnabled = GUI.enabled;
			GUI.enabled = Enabled;

			bool clicked = GUI.Button(rect, Content, Style);

			GUI.enabled = oldEnabled;

			if (clicked && (OnClick != null)) {
				OnClick(new ClickEvent(this, Event.current.button));
			}
		}

		internal void drawToolTip() {
			if (ToolTip != null) {
				Vector2 mousePos = Utils.getMousePosition();
				Vector2 size = GUI.skin.box.CalcSize(new GUIContent(ToolTip));
				Rect rect = new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
				rect = rect.clampToScreen();

				int oldDepth = GUI.depth;
				GUI.depth = -1000;
				GUILayout.BeginArea(rect);
				GUILayout.Label(ToolTip, GUI.skin.box);
				GUILayout.EndArea();
				GUI.depth = oldDepth;
			}
		}

		public void Destroy() {
			if (OnDestroy != null) {
				OnDestroy();
			}
		}
	}
}