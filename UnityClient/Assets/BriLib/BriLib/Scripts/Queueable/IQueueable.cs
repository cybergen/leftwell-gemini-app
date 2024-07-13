using System;

namespace BriLib
{
    public interface IQueueable
    {
        Action<IQueueable> OnBegan { get; set; }
        Action<IQueueable> OnEnded { get; set; }
        Action<IQueueable> OnKilled { get; set; }

        void Begin();
        void Kill();
    }
}
