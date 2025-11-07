// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Async;
using Utilities.WebRequestRest.Interfaces;
using Utilities.WebSockets;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechSession : IDisposable
    {
        /// <summary>
        /// Enable or disable logging.
        /// </summary>
        public bool EnableDebug { get; set; }

        /// <summary>
        /// The timeout in seconds to wait for a response from the server.
        /// </summary>
        public int EventTimeout { get; set; } = 30;

        #region Internal

        internal event Action<IServerSentEvent> OnEventReceived;

        internal event Action<Exception> OnError;

        private readonly object eventLock = new();
        private readonly WebSocket websocketClient;
        private readonly ConcurrentQueue<IServerSentEvent> events = new();

        private bool isCollectingEvents;

        internal TextToSpeechSession(WebSocket webSocket, bool enableDebug)
        {
            websocketClient = webSocket;
            websocketClient.OnMessage += OnMessage;
            EnableDebug = enableDebug;
        }

        private void OnMessage(DataFrame dataFrame)
        {
            if (dataFrame.Type == OpCode.Text)
            {
                if (EnableDebug)
                {
                    Console.WriteLine(dataFrame.Text);
                }

                try
                {
                    var @event = JsonConvert.DeserializeObject<IServerSentEvent>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions);

                    lock (eventLock)
                    {
                        events.Enqueue(@event);
                    }

                    OnEventReceived?.Invoke(@event);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    OnError?.Invoke(e);
                }
            }
        }

        ~TextToSpeechSession() => Dispose(false);

        internal async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var connectTcs = new TaskCompletionSource<State>();
            websocketClient.OnOpen += OnWebsocketClientOnOpen;
            websocketClient.OnError += OnWebsocketClientOnError;

            try
            {
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                // don't call async because it is blocking until connection is closed.
                websocketClient.Connect();
                await connectTcs.Task.WithCancellation(cancellationToken).ConfigureAwait(false);

                if (websocketClient.State != State.Open)
                {
                    throw new Exception($"Failed to start new session! {websocketClient.State}");
                }
            }
            finally
            {
                websocketClient.OnOpen -= OnWebsocketClientOnOpen;
                websocketClient.OnError -= OnWebsocketClientOnError;
            }

            return;

            void OnWebsocketClientOnError(Exception e)
                => connectTcs.TrySetException(e);

            void OnWebsocketClientOnOpen()
                => connectTcs.TrySetResult(websocketClient.State);
        }

        #region IDisposable

        private bool isDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                websocketClient.OnMessage -= OnMessage;
                websocketClient.Dispose();
                isDisposed = true;
            }
        }

        #endregion IDisposable

        #endregion Internal

        /// <summary>
        /// Receive callback updates from the server
        /// </summary>
        /// <typeparam name="T"><see cref="IServerSentEvent"/> to subscribe for updates to.</typeparam>
        /// <param name="sessionEvent">The event to receive updates for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        /// <exception cref="Exception">If <see cref="ReceiveUpdatesAsync{T}(CancellationToken)"/> is already running.</exception>
        public async Task ReceiveUpdatesAsync<T>(Action<T> sessionEvent, CancellationToken cancellationToken) where T : IServerSentEvent
        {
            try
            {
                lock (eventLock)
                {
                    if (isCollectingEvents)
                    {
                        throw new Exception($"{nameof(ReceiveUpdatesAsync)} is already running!");
                    }

                    isCollectingEvents = true;
                }

                do
                {
                    try
                    {
                        T @event = default;

                        lock (eventLock)
                        {
                            if (events.TryDequeue(out var dequeuedEvent) &&
                                dequeuedEvent is T typedEvent)
                            {
                                @event = typedEvent;
                            }
                        }

                        if (@event != null)
                        {
                            sessionEvent(@event);
                        }

                        await Task.Yield();
                    }
                    catch (Exception e)
                    {
                        switch (e)
                        {
                            case TaskCanceledException:
                            case OperationCanceledException:
                                break;
                            default:
                                Console.WriteLine(e);
                                break;
                        }
                    }
                } while (!cancellationToken.IsCancellationRequested && websocketClient.State == State.Open);
            }
            finally
            {
                lock (eventLock)
                {
                    isCollectingEvents = false;
                }
            }
        }

        /// <summary>
        /// Receive callback updates from the server
        /// </summary>
        /// <typeparam name="T"><see cref="IServerSentEvent"/> to subscribe for updates to.</typeparam>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/>.</returns>
        /// <exception cref="Exception">If <see cref="ReceiveUpdatesAsync{T}(CancellationToken)"/> is already running.</exception>
        public async IAsyncEnumerable<T> ReceiveUpdatesAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken) where T : IServerSentEvent
        {
            try
            {
                lock (eventLock)
                {
                    if (isCollectingEvents)
                    {
                        throw new Exception($"{nameof(ReceiveUpdatesAsync)} is already running!");
                    }

                    isCollectingEvents = true;
                }

                do
                {
                    T @event = default;

                    try
                    {

                        lock (eventLock)
                        {
                            if (events.TryDequeue(out var dequeuedEvent) &&
                                dequeuedEvent is T typedEvent)
                            {
                                @event = typedEvent;
                            }
                        }

                        await Task.Yield();
                    }
                    catch (Exception e)
                    {
                        switch (e)
                        {
                            case TaskCanceledException:
                            case OperationCanceledException:
                                break;
                            default:
                                Console.WriteLine(e);
                                break;
                        }
                    }

                    if (@event != null)
                    {
                        yield return @event;
                    }
                } while (!cancellationToken.IsCancellationRequested && websocketClient.State == State.Open);
            }
            finally
            {
                lock (eventLock)
                {
                    isCollectingEvents = false;
                }
            }
        }

        /// <summary>
        /// Send a client event to the server.
        /// </summary>
        /// <typeparam name="T"><see cref="IServerSentEvent"/> to send to the server.</typeparam>
        /// <param name="event">The event to send.</param>
        public async void Send<T>(T @event) where T : IServerSentEvent
            => await SendAsync(@event).ConfigureAwait(false);

        /// <summary>
        /// Send a client event to the server.
        /// </summary>
        /// <typeparam name="T"><see cref="IServerSentEvent"/> to send to the server.</typeparam>
        /// <param name="event">The event to send.</param>
        /// <param name="@event">Optional, <see cref="Action{IServerSentEvent}"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task{IServerSentEvent}"/>.</returns>
        public async Task<IServerSentEvent> SendAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IServerSentEvent
            => await SendAsync(@event, null, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Send a client event to the server.
        /// </summary>
        /// <typeparam name="T"><see cref="IServerSentEvent"/> to send to the server.</typeparam>
        /// <param name="event">The event to send.</param>
        /// <param name="sessionEvents">Optional, <see cref="Action{IServerSentEvent}"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task{IServerSentEvent}"/>.</returns>
        public async Task<IServerSentEvent> SendAsync<T>(T @event, Action<IServerSentEvent> sessionEvents, CancellationToken cancellationToken = default) where T : IServerSentEvent
        {
            if (websocketClient.State != State.Open)
            {
                throw new Exception($"Websocket connection is not open! {websocketClient.State}");
            }

            IServerSentEvent clientEvent = @event;
            var payload = clientEvent.ToJsonString();

            if (EnableDebug)
            {
                Console.WriteLine(payload);
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(EventTimeout));
            using var eventCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
            var tcs = new TaskCompletionSource<IServerSentEvent>();
            eventCts.Token.Register(() => tcs.TrySetCanceled());
            OnEventReceived += EventCallback;

            lock (eventLock)
            {
                events.Enqueue(clientEvent);
            }

            var eventId = Guid.NewGuid().ToString("N");

            if (EnableDebug)
            {
                Console.WriteLine($"[{eventId}] sending {clientEvent}");
            }

            await websocketClient.SendAsync(payload, eventCts.Token).ConfigureAwait(false);

            if (EnableDebug)
            {
                Console.WriteLine($"[{eventId}] sent {clientEvent}");
            }


            var response = await tcs.Task.WithCancellation(eventCts.Token).ConfigureAwait(false);

            if (EnableDebug)
            {
                Console.WriteLine($"[{eventId}] received {response}");
            }

            return response;

            void EventCallback(IServerSentEvent serverEvent)
            {
                sessionEvents?.Invoke(serverEvent);

                try
                {
                    switch (clientEvent)
                    {
                        default:
                            Complete();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return;

                void Complete()
                {
                    if (EnableDebug)
                    {
                        Console.WriteLine($"{clientEvent} -> {serverEvent}");
                    }

                    tcs.TrySetResult(serverEvent);
                    OnEventReceived -= EventCallback;
                }
            }
        }
    }
}
