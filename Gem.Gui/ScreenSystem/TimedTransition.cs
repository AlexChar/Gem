﻿using Gem.Gui.Animations;
using Gem.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gem.Gui.ScreenSystem
{
    public delegate void TransitionRenderAction(ScreenState state, float progress, RenderTarget2D renderTarget, SpriteBatch batch);

    public class TimedTransition : ITransition
    {

        #region Fields

        private const float Full = 1.0f;
        private const float Empty = 0.0f;
        
        private readonly TransitionRenderAction transitionRenderAction;
        private TransitionDirection direction;
        private Func<AnimationContext, float> progressReporter;
        private TimeSpan transitionTime;

        #endregion

        #region Ctor

        public TimedTransition(TimeSpan transitionTime, TransitionRenderAction transitionRenderAction)
        {
            this.transitionTime = transitionTime;
            this.transitionRenderAction = transitionRenderAction;
        }

        #endregion

        #region Properties

        private float _progress;
        public float Progress
        {
            get
            {
                return _progress;
            }
            private set
            {
                _progress = MathHelper.Clamp(value, Empty, Full);
                OnReportProgress();
            }
        }

        public static TimedTransition Default
        {
            get
            {
                return new TimedTransition(TimeSpan.FromSeconds(0.0),
                                          (state, progress, target, batch) =>
                                          batch.Draw(target, Vector2.Zero, Color.White * progress));
            }
        }

        private bool IsFinished
        {
            get { return Progress == TargetProgress(); }
        }

        #endregion
        
        #region Events

        public event EventHandler<float> ProgressChanged;
        public event EventHandler<TransitionDirection> TransitionStarted;
        public event EventHandler<TransitionDirection> TransitionFinished;

        private void OnTransitionStarted()
        {
            var handler = TransitionStarted;
            if (handler != null)
            {
                handler(this, direction);
            }
        }

        private void OnReportProgress()
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                handler(this, Progress);
            }
        }

        private void OnTransitionFinished()
        {
            var handler = TransitionFinished;
            if (handler != null)
            {
                handler(this, direction);
            }
        }

        #endregion

        private float TargetProgress()
        {
            switch (direction)
            {
                case TransitionDirection.Enter:
                    return Full;
                case TransitionDirection.Leave:
                    return Empty;
                default:
                    return Full;
            }
        }

        public void Start(TransitionDirection direction)
        {
            this.direction = direction;
            OnTransitionStarted();

            switch (direction)
            {
                case TransitionDirection.Enter:
                    progressReporter = context => +(float)(context.TotalMilliseconds / transitionTime.TotalMilliseconds);
                    break;
                case TransitionDirection.Leave:
                    progressReporter = context => -(float)(context.TotalMilliseconds / transitionTime.TotalMilliseconds);
                    break;
                default:
                    break;
            }
        }

        public void Update(Animations.AnimationContext context)
        {
            Progress += progressReporter(context);

            if (IsFinished)
            {
                OnTransitionFinished();
            }
        }

        public void Draw(RenderTarget2D target, ScreenState state, SpriteBatch batch)
        {
            transitionRenderAction(state, this.Progress, target, batch);
        }

    }
}
