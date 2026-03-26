using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace vanhaodev.soundmanager
{
	/// <summary>
	/// Utility class for generating enums for SoundManager.
	/// Non-static so you can configure per instance if needed.
	/// </summary>
	public class SoundManagerUtils
	{
		/// <summary>
		/// Generate an enum from a list of names, overwrite existing enum if found.
		/// </summary>
		public void GenerateEnum(List<string> items, string enumName, SoundManagerSO so, out string path,
			string enumNamespace = "vanhaodev.soundmanager.generated")
		{
			path = null;

			if (items == null || items.Count == 0)
			{
				EditorUtility.DisplayDialog(
					"Generate Enum",
					"No items to generate enum.",
					"OK"
				);
				return;
			}

			// Search the whole project for existing enum file
			string[] guids = AssetDatabase.FindAssets(enumName + " t:Script");
			string enumPath = null;

			foreach (var guid in guids)
			{
				string filePath = AssetDatabase.GUIDToAssetPath(guid);
				string text = File.ReadAllText(filePath);
				if (text.Contains($"namespace {enumNamespace}") && text.Contains($"enum {enumName}"))
				{
					enumPath = filePath; // Found existing enum, will overwrite
					break;
				}
			}

			// If not found, generate in the SO folder
			if (enumPath == null)
			{
				string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(so));
				enumPath = Path.Combine(folder, enumName + ".cs");
			}

			path = enumPath;

			// Sanitize names
			List<string> sanitizedNames = SanitizeNames(items);

			// Build enum text with auto-generation warning
			string enumText = "// Auto-generated enum by SoundManager\n";
			enumText += "// Do not edit manually. Changes will be overwritten.\n\n";
			enumText += $"namespace {enumNamespace}\n{{\n";
			enumText += $"\tpublic enum {enumName}\n\t{{\n";
			for (int i = 0; i < sanitizedNames.Count; i++)
				enumText += $"\t\t{sanitizedNames[i]} = {i},\n";
			enumText += "\t}\n}";

			// Write file and refresh
			File.WriteAllText(enumPath, enumText);
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Sanitize a list of names, handle duplicates and invalid characters.
		/// Returns a list of unique, safe names.
		/// </summary>
		public List<string> SanitizeNames(List<string> items)
		{
			List<string> sanitizedNames = new List<string>();
			HashSet<string> usedNames = new HashSet<string>();

			for (int i = 0; i < items.Count; i++)
			{
				string name = items[i] ?? $"Item{i}";

				// 1. Replace spaces with underscore
				string sanitized = name.Replace(" ", "_");

				// 2. Remove invalid characters (keep only letters, digits, underscore)
				sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");

				// 3. Fallback if empty
				if (string.IsNullOrEmpty(sanitized))
					sanitized = $"Item{i}";

				// 4. Prepend underscore if starts with a digit
				if (char.IsDigit(sanitized[0]))
					sanitized = "_" + sanitized;

				// 5. Optional: enforce PascalCase / Uppercase (uncomment if needed)
				// sanitized = char.ToUpper(sanitized[0]) + sanitized.Substring(1);

				// 6. Handle duplicates
				string finalName = sanitized;
				int suffix = 1;
				while (usedNames.Contains(finalName))
				{
					finalName = sanitized + "_" + suffix;
					suffix++;
				}

				usedNames.Add(finalName);
				sanitizedNames.Add(finalName);
			}

			return sanitizedNames;
		}
	}
}