using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    public class DottView
    {
        private bool isTimeDragging;
        private bool isTweenDragging;

        public event Action TimeDragStart;
        public event Action TimeDragEnd;
        public event Action<float> TimeDrag;
        public event Action<IDOTweenAnimation> TweenSelected;
        public event Action<float> TweenDrag;
        public event Action AddClicked;
        public event Action CallbackClicked;
        public event Action RemoveClicked;
        public event Action DuplicateClicked;
        public event Action StopClicked;
        public event Action PlayClicked;
        public event Action<bool> LoopToggled;
        public event Action<bool> FreezeFrameClicked;

        public void DrawTimeline(IDOTweenAnimation[] animations, [CanBeNull] IDOTweenAnimation selected, bool isPlaying, float currentPlayingTime, bool isLooping, bool isFreezeFrame, bool isPaused)
        {
            var rect = DottGUI.GetTimelineControlRect(animations.Length);

            DottGUI.Background(rect);
            DottGUI.Header(rect);

            var timeScale = CalculateTimeScale(animations);
            var timeRect = DottGUI.Time(rect, timeScale, ref isTimeDragging, TimeDragStart, TimeDragEnd);
            var tweensRect = DottGUI.Tweens(rect, animations, timeScale, selected, ref isTweenDragging, TweenSelected);

            if (DottGUI.AddButton(rect))
            {
                AddClicked?.Invoke();
            }

            if (DottGUI.CallbackButton(rect))
            {
                CallbackClicked?.Invoke();
            }

            if (selected != null && DottGUI.RemoveButton(rect))
            {
                RemoveClicked?.Invoke();
            }

            if (selected != null && DottGUI.DuplicateButton(rect))
            {
                DuplicateClicked?.Invoke();
            }

            if (isPlaying || isPaused)
            {
                var time = currentPlayingTime * timeScale;
                DottGUI.TimeVerticalLine(tweensRect, time);
            }

            if (isTimeDragging)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);
                DottGUI.TimeVerticalLine(tweensRect, time);

                if (Event.current.type is EventType.MouseDrag or EventType.MouseDown)
                {
                    TimeDrag?.Invoke(time / timeScale);
                }
            }

            if (isTweenDragging && selected != null)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);

                if (Event.current.type == EventType.MouseDrag)
                {
                    TweenDrag?.Invoke(time / timeScale);
                }
            }

            switch (isPlaying)
            {
                case true when DottGUI.StopButton(rect):
                    StopClicked?.Invoke();
                    break;
                case false when DottGUI.PlayButton(rect):
                    PlayClicked?.Invoke();
                    break;
            }

            var loopResult = DottGUI.LoopToggle(rect, isLooping);
            if (loopResult != isLooping)
            {
                LoopToggled?.Invoke(loopResult);
            }

            var freezeFrameResult = DottGUI.FreezeFrameToggle(rect, isFreezeFrame);
            if (freezeFrameResult != isFreezeFrame)
            {
                FreezeFrameClicked?.Invoke(freezeFrameResult);
            }
        }

        public void DrawInspector(UnityEditor.Editor editor)
        {
            DottGUI.Inspector(editor);
        }

        public void DrawProperties(SerializedObject serializedObject)
        {
            DottGUI.Properties(serializedObject);
        }

        private static float CalculateTimeScale(IDOTweenAnimation[] animations)
        {
            var maxTime = animations.Length > 0
                ? animations.Max(animation => animation.Delay + animation.Duration * Mathf.Max(1, animation.Loops))
                : 1f;
            return 1f / maxTime;
        }
    }
}