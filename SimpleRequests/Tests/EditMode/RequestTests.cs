using NUnit.Framework;
using Systems.SimpleRequests.Abstract;
using Systems.SimpleRequests.Utility;

namespace Systems.SimpleRequests.Tests
{
    public sealed class RequestTests
    {
        private static int _handlerOrder;
        private static int _noticeHandlerCallCount;
        private static int _firstHandlerOrder;
        private static int _secondHandlerOrder;
        private static int _thirdHandlerOrder;

        [SetUp]
        public void SetUp()
        {
            _handlerOrder = 0;
            _noticeHandlerCallCount = 0;
            _firstHandlerOrder = 0;
            _secondHandlerOrder = 0;
            _thirdHandlerOrder = 0;
            RequestAPI.ClearHandlers<NoticeRequest>();
            RequestAPI.ClearHandlers<ScoreRequest, ScoreResponse>();
        }

        [TearDown]
        public void TearDown()
        {
            RequestAPI.ClearHandlers<NoticeRequest>();
            RequestAPI.ClearHandlers<ScoreRequest, ScoreResponse>();
        }

        [Test]
        public void Send_InvokesHandlersInReverseRegistrationOrder()
        {
            RequestAPI.RegisterHandler<NoticeRequest>(HandleFirstNotice);
            RequestAPI.RegisterHandler<NoticeRequest>(HandleSecondNotice);

            NoticeRequest request = new NoticeRequest(10);
            RequestAPI.Send(in request);

            Assert.AreEqual(2, _firstHandlerOrder);
            Assert.AreEqual(1, _secondHandlerOrder);
        }

        [Test]
        public void RegisterHandler_WhenAlreadyRegistered_InvokesHandlerOnlyOnce()
        {
            RequestAPI.RegisterHandler<NoticeRequest>(CountNotice);
            RequestAPI.RegisterHandler<NoticeRequest>(CountNotice);

            NoticeRequest request = new NoticeRequest(10);
            RequestAPI.Send(in request);

            Assert.AreEqual(1, _noticeHandlerCallCount);
        }

        [Test]
        public void Send_InvokesHigherPriorityHandlersBeforeLowerPriorityHandlers()
        {
            RequestAPI.RegisterHandler<NoticeRequest>(HandleFirstNotice, priority: -10);
            RequestAPI.RegisterHandler<NoticeRequest>(HandleSecondNotice, priority: 100);
            RequestAPI.RegisterHandler<NoticeRequest>(HandleThirdNotice, priority: 10);

            NoticeRequest request = new NoticeRequest(10);
            RequestAPI.Send(in request);

            Assert.AreEqual(3, _firstHandlerOrder);
            Assert.AreEqual(1, _secondHandlerOrder);
            Assert.AreEqual(2, _thirdHandlerOrder);
        }

        [Test]
        public void UnregisterHandler_RemovesOnlyTheRequestedHandler()
        {
            RequestAPI.RegisterHandler<NoticeRequest>(CountNotice);
            RequestAPI.RegisterHandler<NoticeRequest>(HandleFirstNotice);
            RequestAPI.UnregisterHandler<NoticeRequest>(CountNotice);

            NoticeRequest request = new NoticeRequest(10);
            RequestAPI.Send(in request);

            Assert.AreEqual(0, _noticeHandlerCallCount);
            Assert.AreEqual(1, _firstHandlerOrder);
        }

        [Test]
        public void ClearHandlers_RemovesAllHandlersForRequestType()
        {
            RequestAPI.RegisterHandler<NoticeRequest>(CountNotice);
            RequestAPI.ClearHandlers<NoticeRequest>();

            NoticeRequest request = new NoticeRequest(10);
            RequestAPI.Send(in request);

            Assert.AreEqual(0, _noticeHandlerCallCount);
        }

        [Test]
        public void SendWithResponse_UsesSharedResponseAndSupportsClearingHandlers()
        {
            RequestAPI.RegisterHandler<ScoreRequest, ScoreResponse>(AppendOne, priority: -10);
            RequestAPI.RegisterHandler<ScoreRequest, ScoreResponse>(AppendTwo, priority: 10);

            ScoreRequest request = new ScoreRequest(10);
            ScoreResponse response = RequestAPI.Send<ScoreRequest, ScoreResponse>(in request);

            Assert.AreEqual(21, response.Value);

            RequestAPI.ClearHandlers<ScoreRequest, ScoreResponse>();
            ScoreResponse clearedResponse = RequestAPI.Send<ScoreRequest, ScoreResponse>(in request);

            Assert.AreEqual(0, clearedResponse.Value);
        }

        [Test]
        public void SendWithResponse_AllowsTheLowLevelStoreToReceiveAnInitialResponse()
        {
            RequestAPI.RegisterHandler<ScoreRequest, ScoreResponse>(AppendOne, priority: -10);
            RequestAPI.RegisterHandler<ScoreRequest, ScoreResponse>(AppendTwo, priority: 10);

            ScoreRequest request = new ScoreRequest(10);
            ScoreResponse response = new ScoreResponse { Value = 7 };
            Request<ScoreRequest, ScoreResponse>.Send(in request, ref response);

            Assert.AreEqual(721, response.Value);
        }

        private static void CountNotice(in NoticeRequest request)
        {
            _noticeHandlerCallCount++;
        }

        private static void HandleFirstNotice(in NoticeRequest request)
        {
            _firstHandlerOrder = ++_handlerOrder;
        }

        private static void HandleSecondNotice(in NoticeRequest request)
        {
            _secondHandlerOrder = ++_handlerOrder;
        }

        private static void HandleThirdNotice(in NoticeRequest request)
        {
            _thirdHandlerOrder = ++_handlerOrder;
        }

        private static void AppendOne(in ScoreRequest request, ref ScoreResponse response)
        {
            response.Value = response.Value * 10 + 1;
        }

        private static void AppendTwo(in ScoreRequest request, ref ScoreResponse response)
        {
            response.Value = response.Value * 10 + 2;
        }

        private readonly struct NoticeRequest : IRequestContext
        {
            public NoticeRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct ScoreRequest : IRequestContext
        {
            public ScoreRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private struct ScoreResponse
        {
            public int Value;
        }
    }
}
