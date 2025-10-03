using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public class FireworkSpawner : MonoBehaviour
    {
        [SerializeField] private List<FireworkLauncher> launchers = new();
        [SerializeField] private List<FireworkRecipe> recipes = new();
        [SerializeField] private float interval = 5f;
        [SerializeField] private bool autoLoop = true;
        [SerializeField] private KeyCode triggerKey = KeyCode.Space;

        private float timer;
        private int nextRecipeIndex;

        private void Update()
        {
            if (autoLoop)
            {
                timer += Time.deltaTime;
                if (timer >= interval)
                {
                    timer = 0f;
                    LaunchNext();
                }
            }

            if (Input.GetKeyDown(triggerKey))
            {
                LaunchNext();
            }
        }

        public void LaunchNext()
        {
            if (recipes.Count == 0 || launchers.Count == 0)
            {
                return;
            }

            var recipe = recipes[nextRecipeIndex];
            foreach (var launcher in launchers)
            {
                launcher.Recipe = recipe;
                launcher.ResetAndLaunch();
            }

            nextRecipeIndex = (nextRecipeIndex + 1) % recipes.Count;
        }
    }
}
