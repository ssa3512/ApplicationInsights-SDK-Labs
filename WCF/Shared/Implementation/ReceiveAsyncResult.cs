namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;

    internal sealed class ReceiveAsyncResult : ChannelAsyncResult
    {
        public ReceiveAsyncResult(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback onReceiveDone, AsyncCallback callback, object state)
            : base(onReceiveDone, callback, state, null)
        {
            this.InnerChannel = innerChannel;

            var originalResult = innerChannel.BeginReceive(timeout, OnComplete, this);
            if (originalResult.CompletedSynchronously)
            {
                this.Message = innerChannel.EndReceive(originalResult);
                this.CompleteSynchronously();
            }
        }

        public IInputChannel InnerChannel { get; private set; }

        public Message Message { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var rar = (ReceiveAsyncResult)result.AsyncState;
            try
            {
                rar.Message = rar.InnerChannel.EndReceive(result);
                rar.Complete(false);
            }
            catch (Exception ex)
            {
                rar.Complete(false, ex);
            }
        }
    }
}
