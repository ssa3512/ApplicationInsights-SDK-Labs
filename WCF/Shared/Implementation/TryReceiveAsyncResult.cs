namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;

    internal sealed class TryReceiveAsyncResult : ChannelAsyncResult
    {
        public TryReceiveAsyncResult(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback onReceiveDone, AsyncCallback callback, object state)
            : base(onReceiveDone, callback, state, null)
        {
            this.InnerChannel = innerChannel;

            var originalResult = innerChannel.BeginTryReceive(timeout, OnComplete, this);
            if (originalResult.CompletedSynchronously)
            {
                Message message = null;
                this.Result = innerChannel.EndTryReceive(originalResult, out message);
                this.Message = message;
                this.CompleteSynchronously();
            }
        }

        public IInputChannel InnerChannel { get; private set; }

        public Message Message { get; private set; }

        public bool Result { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var trar = (TryReceiveAsyncResult)result.AsyncState;
            try
            {
                Message message = null;
                trar.Result = trar.InnerChannel.EndTryReceive(result, out message);
                trar.Message = message;
                trar.Complete(false);
            }
            catch (Exception ex)
            {
                trar.Complete(false, ex);
            }
        }
    }
}
