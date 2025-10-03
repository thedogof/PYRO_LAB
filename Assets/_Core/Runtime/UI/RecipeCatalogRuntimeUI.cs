using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PyroLab.Fireworks
{
    public class RecipeCatalogRuntimeUI : MonoBehaviour
    {
        [SerializeField] private RecipeCatalog catalog;
        [SerializeField] private FireworkSpawner spawner;
        [SerializeField] private FireworkLauncher launcher;
        [SerializeField] private bool autoApplyOnSelection = true;

        private string[] recipeNames = System.Array.Empty<string>();
        private int selectedIndex;
        private Rect windowRect = new Rect(16f, 16f, 260f, 180f);

        private void Awake()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<RecipeCatalog>("RecipeCatalog");
                if (catalog == null)
                {
                    var catalogs = Resources.LoadAll<RecipeCatalog>(string.Empty);
                    if (catalogs.Length > 0)
                    {
                        catalog = catalogs[0];
                    }
#if UNITY_EDITOR
                    if (catalog == null)
                    {
                        catalog = AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/_Core/Data/RecipeCatalog.asset");
                    }
#endif
                }
            }

            if (spawner == null)
            {
                spawner = FindObjectOfType<FireworkSpawner>();
            }

            if (launcher == null)
            {
                launcher = FindObjectOfType<FireworkLauncher>();
            }

            RebuildOptions();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (spawner == null)
            {
                spawner = FindObjectOfType<FireworkSpawner>();
            }

            if (launcher == null)
            {
                launcher = FindObjectOfType<FireworkLauncher>();
            }
        }

        private void RebuildOptions()
        {
            if (catalog == null)
            {
                recipeNames = System.Array.Empty<string>();
                return;
            }

            var entries = new List<string>();
            foreach (var recipe in catalog.Recipes)
            {
                entries.Add(recipe != null ? recipe.name : "(Missing Recipe)");
            }

            recipeNames = entries.ToArray();
            selectedIndex = Mathf.Clamp(selectedIndex, 0, recipeNames.Length > 0 ? recipeNames.Length - 1 : 0);

            if (autoApplyOnSelection)
            {
                ApplySelection();
            }
        }

        private void OnGUI()
        {
            if (catalog == null || recipeNames.Length == 0)
            {
                return;
            }

            windowRect = GUILayout.Window(GetInstanceID(), windowRect, DrawWindowContents, "Recipe Catalog");
        }

        private void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Select a recipe from the library.");
            int newIndex = Mathf.Clamp(selectedIndex, 0, recipeNames.Length - 1);
            newIndex = GUILayout.Popup(newIndex, recipeNames);
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                if (autoApplyOnSelection)
                {
                    ApplySelection();
                }
            }

            autoApplyOnSelection = GUILayout.Toggle(autoApplyOnSelection, "Auto apply to spawner");

            if (GUILayout.Button("Apply Recipe"))
            {
                ApplySelection();
            }

            if (GUILayout.Button("Launch Once"))
            {
                LaunchSelection();
            }

            if (GUILayout.Button("Reload Catalog"))
            {
                RebuildOptions();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void ApplySelection()
        {
            if (catalog == null || recipeNames.Length == 0)
            {
                return;
            }

            if (catalog.TryGetRecipe(selectedIndex, out var recipe))
            {
                if (spawner != null)
                {
                    spawner.SetActiveRecipe(recipe);
                }

                if (launcher != null)
                {
                    launcher.Recipe = recipe;
                }
            }
        }

        private void LaunchSelection()
        {
            if (spawner != null)
            {
                spawner.LaunchNext();
            }
            else if (launcher != null)
            {
                launcher.ResetAndLaunch();
            }
        }

        public void SetCatalog(RecipeCatalog newCatalog)
        {
            catalog = newCatalog;
            RebuildOptions();
        }

        public void SetSpawner(FireworkSpawner newSpawner)
        {
            spawner = newSpawner;
        }

        public void SetLauncher(FireworkLauncher newLauncher)
        {
            launcher = newLauncher;
        }
    }

    public static class RecipeCatalogRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeUI()
        {
            if (Object.FindObjectOfType<RecipeCatalogRuntimeUI>() != null)
            {
                return;
            }

            var spawner = Object.FindObjectOfType<FireworkSpawner>();
            if (spawner == null)
            {
                return;
            }

            var go = new GameObject("RecipeCatalogRuntimeUI");
            var ui = go.AddComponent<RecipeCatalogRuntimeUI>();
            ui.SetSpawner(spawner);
            ui.SetLauncher(Object.FindObjectOfType<FireworkLauncher>());
        }
    }
}
