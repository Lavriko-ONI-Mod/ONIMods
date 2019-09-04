﻿/*
 * Copyright 2019 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace PeterHan.PLib {
	/// <summary>
	/// Static utility functions used across mods.
	/// </summary>
	public static class PLibUtil {
		/// <summary>
		/// Centers and selects an entity.
		/// </summary>
		/// <param name="entity">The entity to center and focus.</param>
		public static void CenterAndSelect(KMonoBehaviour entity) {
			if (entity != null) {
				Transform transform = entity.transform;
				SelectTool.Instance.SelectAndFocus(transform.transform.GetPosition(),
					transform.GetComponent<KSelectable>(), Vector3.zero);
			}
		}

		/// <summary>
		/// Creates a popup message at the specified cell location on the Move layer.
		/// </summary>
		/// <param name="image">The image to display, likely from PopFXManager.Instance.</param>
		/// <param name="text">The text to display.</param>
		/// <param name="cell">The cell location to create the message.</param>
		public static void CreatePopup(Sprite image, string text, int cell) {
			CreatePopup(image, text, Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
		}

		/// <summary>
		/// Creates a popup message at the specified location.
		/// </summary>
		/// <param name="image">The image to display, likely from PopFXManager.Instance.</param>
		/// <param name="text">The text to display.</param>
		/// <param name="position">The position to create the message.</param>
		public static void CreatePopup(Sprite image, string text, Vector3 position) {
			PopFXManager.Instance.SpawnFX(image, text, null, position);
		}

		/// <summary>
		/// Highlights an entity. Use Color.black to unhighlight it.
		/// </summary>
		/// <param name="entity">The entity to highlight.</param>
		/// <param name="highlightColor">The color to highlight it.</param>
		public static void HighlightEntity(Component entity, Color highlightColor) {
			var component = entity?.GetComponent<KAnimControllerBase>();
			if (component != null)
				component.HighlightColour = highlightColor;
		}

		/// <summary>
		/// Loads a DDS sprite embedded in the current assembly.
		/// 
		/// It must be encoded using the DXT5 format.
		/// </summary>
		/// <param name="path">The fully qualified path to the DDS image to load.</param>
		/// <param name="width">The desired width.</param>
		/// <param name="height">The desired height.</param>
		/// <returns>The sprite thus loaded.</returns>
		/// <exception cref="ArgumentException">If the image could not be loaded.</exception>
		public static Sprite LoadSprite(string path, int width, int height) {
			// Open a stream to the image
			try {
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
						path)) {
					const int SKIP = 128;
					if (stream == null)
						throw new ArgumentException("Could not load image: " + path);
					// If len > int.MaxValue we will not go to space today, skip first 128
					// bytes of stream
					int len = (int)stream.Length - SKIP;
					if (len < 0)
						throw new ArgumentException("Image is too small: " + path);
					byte[] buffer = new byte[len];
					stream.Seek(SKIP, System.IO.SeekOrigin.Begin);
					stream.Read(buffer, 0, len);
					// Load the texture from the stream
					var texture = new Texture2D(width, height, TextureFormat.DXT5, false);
					texture.LoadRawTextureData(buffer);
					texture.Apply(true, true);
					// Create a sprite centered on the texture
					LogDebug("Loaded sprite: {0} ({1:D}x{2:D}, {3:D} bytes)".F(path, width,
						height, len));
					// pivot is in RELATIVE coordinates!
					return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(
						0.5f, 0.5f));
				}
			} catch (System.IO.IOException e) {
				throw new ArgumentException("Could not load image: " + path, e);
			}
		}

		/// <summary>
		/// Logs a message to the debug log.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void LogDebug(object message) {
			Debug.LogFormat("[PLib/{0}] {1}", Assembly.GetCallingAssembly()?.GetName()?.Name,
				message);
		}

		/// <summary>
		/// Logs an error message to the debug log.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void LogError(object message) {
			// Cannot make a utility property or method for Assembly.GetCalling... because
			// its caller would then be the assembly PLib is in, not the assembly which
			// invoked LogXXX
			Debug.LogErrorFormat("[PLib/{0}] {1}", Assembly.GetCallingAssembly()?.GetName()?.
				Name ?? "?", message);
		}

		/// <summary>
		/// Logs the mod name and version when a mod initializes.
		/// </summary>
		public static void LogModInit() {
			var assembly = Assembly.GetCallingAssembly();
			if (assembly != null) {
				// Mod version
				var fileVersions = assembly.GetCustomAttributes(typeof(
					AssemblyFileVersionAttribute), true);
				string modVersion = "Unknown";
				if (fileVersions != null && fileVersions.Length > 0) {
					// Retrieves the "File Version" attribute
					var assemblyFileVersion = (AssemblyFileVersionAttribute)fileVersions[0];
					if (assemblyFileVersion != null)
						modVersion = assemblyFileVersion.Version;
				}
				Debug.LogFormat("[PLib] Mod {0} initialized, version {1}",
					assembly.GetName()?.Name, modVersion);
			}
		}

		/// <summary>
		/// Logs an exception message to the debug log.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void LogException(Exception thrown) {
			Debug.LogErrorFormat("[PLib/{0}] {1} {2} at {3}", Assembly.GetCallingAssembly()?.
				GetName()?.Name ?? "?", thrown.GetType(), thrown.Message, thrown.StackTrace);
		}

		/// <summary>
		/// Logs a warning message to the debug log.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void LogWarning(object message) {
			Debug.LogWarningFormat("[PLib/{0}] {1}", Assembly.GetCallingAssembly()?.GetName()?.
				Name ?? "?", message);
		}
	}
}
