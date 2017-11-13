using System;
using System.Windows;

namespace Walterlv.Contracts
{
    public interface IAddInUI : IDisposable
    {
        FrameworkElement View { get; }
    }
}
