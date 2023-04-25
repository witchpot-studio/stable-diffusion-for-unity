using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Witchpot.Runtime.StableDiffusion
{
    // https://qiita.com/k7a/items/80984aaf4abae180816c

    public static class UnityWebRequestAsyncOperationExtension
    {
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }
    }

    public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        UnityWebRequestAsyncOperation _asyncOperation;

        public bool IsCompleted
        {
            get { return _asyncOperation.isDone; }
        }

        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
        {
            _asyncOperation = asyncOperation;
        }

        public void GetResult()
        {
            // NOTE: 結果はUnityWebRequestからアクセスできるので、ここで返す必要性は無い
        }

        public void OnCompleted(Action continuation)
        {
            _asyncOperation.completed += _ => { continuation(); };
        }
    }
}
