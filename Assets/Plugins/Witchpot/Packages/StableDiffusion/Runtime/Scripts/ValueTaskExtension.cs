using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Witchpot.Runtime.StableDiffusion
{
    public static class ValueTaskExtension
    {
        public static void Forget(this ValueTask task, bool logWarning = false)
        {
            task.AsTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (t.Exception != null)
                    {
                        foreach (var e in t.Exception.Flatten().InnerExceptions)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else
                    {
                        if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was faulted."); }
                    }

                }
                else if (t.IsCanceled)
                {
                    if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was canceled."); }
                }
            });
        }

        public static void Forget<T>(this ValueTask<T> task, bool logWarning = false, bool log = false)
        {
            task.AsTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (t.Exception != null)
                    {
                        foreach (var e in t.Exception.Flatten().InnerExceptions)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else
                    {
                        if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was faulted."); }
                    }

                }
                else if (t.IsCanceled)
                {
                    if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was canceled."); }
                }
                else if (t.IsCompleted)
                {
                    if (log) { Debug.Log($"Task[{t.Id}]: completed with {t.Result}"); }
                }
            });
        }
    }
}
