namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;

    internal sealed class OpenAsyncResult : ChannelAsyncResult
    {
        public OpenAsyncResult(IChannel innerChannel, TimeSpan timeout, AsyncCallback onOpenDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onOpenDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;

            var originalResult = innerChannel.BeginOpen(timeout, OnComplete, this);
            if (originalResult.CompletedSynchronously)
            {
                innerChannel.EndOpen(originalResult);
                this.CompleteSynchronously();
            }
        }

        public IChannel InnerChannel { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var oar = (OpenAsyncResult)result.AsyncState;
            try
            {
                oar.InnerChannel.EndOpen(result);
                oar.Complete(false);
            }
            catch (Exception ex)
            {
                oar.Complete(false, ex);
            }
        }
    }
}
