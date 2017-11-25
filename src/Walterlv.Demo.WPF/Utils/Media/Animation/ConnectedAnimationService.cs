using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Walterlv.Annotations;

namespace Walterlv.Demo.Media.Animation
{
    public class ConnectedAnimationService
    {
        private ConnectedAnimationService()
        {
        }

        private readonly Dictionary<string, ConnectedAnimation> _connectingAnimations =
            new Dictionary<string, ConnectedAnimation>();

        public void PrepareToAnimate([NotNull] string key, [NotNull] UIElement source)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (_connectingAnimations.TryGetValue(key, out var info))
            {
                throw new ArgumentException("指定的 key 已经做好动画准备，不应该重复进行准备。", nameof(key));
            }

            info = new ConnectedAnimation(key, source, OnAnimationCompleted);
            _connectingAnimations.Add(key, info);
        }

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            var key = ((ConnectedAnimation) sender).Key;
            if (_connectingAnimations.ContainsKey(key))
            {
                _connectingAnimations.Remove(key);
            }
        }

        [CanBeNull]
        public ConnectedAnimation GetAnimation([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_connectingAnimations.TryGetValue(key, out var info))
            {
                return info;
            }
            return null;
        }

        private static readonly DependencyProperty AnimationServiceProperty =
            DependencyProperty.RegisterAttached("AnimationService",
                typeof(ConnectedAnimationService), typeof(ConnectedAnimationService),
                new PropertyMetadata(default(ConnectedAnimationService)));

        public static ConnectedAnimationService GetForCurrentView(Visual visual)
        {
            var window = Window.GetWindow(visual);
            if (window == null)
            {
                throw new ArgumentException("此 Visual 未连接到可见的视觉树中。", nameof(visual));
            }

            var service = (ConnectedAnimationService) window.GetValue(AnimationServiceProperty);
            if (service == null)
            {
                service = new ConnectedAnimationService();
                window.SetValue(AnimationServiceProperty, service);
            }
            return service;
        }
    }
}
