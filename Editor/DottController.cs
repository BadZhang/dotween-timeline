using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;

namespace Dott.Editor
{
    public class DottController : IDisposable
    {
        private double startTime;
        private float gotoTime;
        private DottAnimation[] currentPlayAnimations;

        public bool IsPlaying => DottEditorPreview.IsPlaying;
        public float ElapsedTime => Paused ? gotoTime : (float)(DottEditorPreview.CurrentTime - startTime);
        public bool Paused { get; private set; }

        public bool Loop
        {
            get => EditorPrefs.GetBool("Dott.Loop", false);
            set => EditorPrefs.SetBool("Dott.Loop", value);
        }

        public bool FreezeFrame
        {
            get => EditorPrefs.GetBool("Dott.FreezeFrame", false);
            set => EditorPrefs.SetBool("Dott.FreezeFrame", value);
        }

        public DottController()
        {
            DottEditorPreview.Completed += DottEditorPreviewOnCompleted;
        }

        public void Play(DottAnimation[] animations)
        {
            currentPlayAnimations = animations;

            Sort(animations).ForEach(PreviewTween);
            DottEditorPreview.Start();
            startTime = DottEditorPreview.CurrentTime;
            Paused = false;
        }

        public void GoTo(DottAnimation[] animations, in float time)
        {
            DottEditorPreview.Stop();

            gotoTime = time;
            var sortedAnimations = Sort(animations);
            foreach (var animation in sortedAnimations)
            {
                var tween = PreviewTween(animation);
                if (tween != null)
                {
                    var tweenTime = time - animation.Delay;
                    tween.Goto(tweenTime, andPlay: false);
                }
            }

            DottEditorPreview.QueuePlayerLoopUpdate();
        }

        public void Stop()
        {
            currentPlayAnimations = null;
            Paused = false;
            DottEditorPreview.Stop();
        }

        public void Pause()
        {
            Paused = true;
        }

        [CanBeNull]
        private static Tween PreviewTween(DottAnimation animation)
        {
            if (!animation.IsValid || !animation.IsActive) { return null; }

            var tween = animation.CreateEditorPreview();
            if (tween == null) { return null; }

            DottEditorPreview.Add(tween, animation.IsFrom);
            return tween;
        }

        private static IEnumerable<DottAnimation> Sort(DottAnimation[] animations)
        {
            return animations.OrderBy(animation => animation.Delay);
        }

        private void DottEditorPreviewOnCompleted()
        {
            if (!Loop)
            {
                Stop();
                return;
            }

            DottEditorPreview.Stop();
            Play(currentPlayAnimations);
        }

        public void Dispose()
        {
            Stop();
            DottEditorPreview.Completed -= DottEditorPreviewOnCompleted;
        }
    }
}