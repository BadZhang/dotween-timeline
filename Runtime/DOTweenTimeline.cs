using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        [CanBeNull] public Sequence Sequence { get; private set; }

        // Do not override the onKill callback because it is used internally to reset the Sequence
        public Sequence Play()
        {
            TryGenerateSequence();
            return Sequence.Play();
        }

        // Wrapper for UnityEvent (requires void return type)
        public void DOPlay() => Play();

        public Sequence Restart()
        {
            TryGenerateSequence();
            Sequence.Restart();
            return Sequence;
        }

        private void TryGenerateSequence()
        {
            if (Sequence != null) { return; }

            Sequence = DOTween.Sequence();
            Sequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            Sequence.OnKill(() => Sequence = null);
            var components = GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation animation:
                        animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, animation.tween);
                        break;

                    case IDOTweenAnimation animation:
                        var tween = animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, tween);
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            // Already handled by SetLink, but needed to avoid warnings from children DOTweenAnimation.OnDestroy
            Sequence?.Kill();
        }

        private void OnValidate()
        {
            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoGenerate = false;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create ASMDEF")]
        private void CreateAsmdef()
        {
            var packagePath = GetCurrentFilePath();
            packagePath = System.IO.Directory.GetParent(packagePath).Parent.FullName;

            var runtimePath = System.IO.Path.Combine(packagePath, "Runtime", "Dott.asmdef");
            using (var stream = System.IO.File.CreateText(runtimePath))
            {
                stream.WriteLine("{");
                stream.WriteLine("\t\"name\": \"Dott\",");
                stream.WriteLine("\t\"references\": [");
                stream.WriteLine("\t\t\"DOTweenPro.Scripts\"");
                stream.WriteLine("\t]");
                stream.WriteLine("}");
            }

            var editorPath = System.IO.Path.Combine(packagePath, "Editor", "Dott.Editor.asmdef");
            using (var stream = System.IO.File.CreateText(editorPath))
            {
                stream.WriteLine("{");
                stream.WriteLine("\t\"name\": \"Dott.Editor\",");
                stream.WriteLine("\t\"references\": [");
                stream.WriteLine("\t\t\"Dott\",");
                stream.WriteLine("\t\t\"DOTweenPro.Scripts\"");
                stream.WriteLine("\t],");
                stream.WriteLine("\t\"includePlatforms\": [");
                stream.WriteLine("\t\t\"Editor\"");
                stream.WriteLine("\t]");
                stream.WriteLine("}");
            }

            UnityEditor.AssetDatabase.Refresh();
        }

        private static string GetCurrentFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = null) => filePath;
#endif
    }
}