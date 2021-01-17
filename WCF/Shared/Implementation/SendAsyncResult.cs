namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;

    internal sealed class SendAsyncResult : ChannelAsyncResult
    {
        public SendAsyncResult(IOutputChannel innerChannel, Message message, TimeSpan timeout, AsyncCallback onSendDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onSendDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;
            this.RequestId = message.Headers.MessageId;

            var originalResult = innerChannel.BeginSend(message, timeout, OnComplete, this);

            if (originalResult.CompletedSynchronously)
            {
                innerChannel.EndSend(originalResult);
                this.CompleteSynchronously();
            }
        }

        public IOutputChannel InnerChannel { get; private set; }

        public UniqueId RequestId { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var sar = (SendAsyncResult)result.AsyncState;

            try
            {
                sar.InnerChannel.EndSend(result);
                sar.Complete(false);
            }
            catch (Exception ex)
            {
                sar.Complete(false, ex);
            }
        }
    }
}
