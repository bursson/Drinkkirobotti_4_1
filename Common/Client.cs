using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Common
{
    public static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Quick extension for SemaphoreSlim to try exiting the semaphore (once by default).
        /// </summary>
        /// <param name="semaphore"></param>
        /// <param name="releaseCount"></param>
        public static void TryRelease(this SemaphoreSlim semaphore, int releaseCount = 1)
        {
            try
            {
                semaphore.Release(releaseCount);
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }
    
    public class Client : IConnection
    {
        private enum ConnectionStatusEnum
        {
            Disconnected,
            Connected
        }
        
        public Client(IPAddress hostIp, int hostPort, string endOfMessageString, bool implicitEndOfMessageString, string logName, int receiveTimeoutMs = Timeout.Infinite)
        {
            _hostIp = hostIp;
            _hostPort = hostPort;
            _endOfMessageString = endOfMessageString;
            _implicitEndOfMessageString = implicitEndOfMessageString;
            _logName = logName;
            _receiveTimeoutMs = receiveTimeoutMs;
        }
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds received messages.
        /// </summary>
        private ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
        private SemaphoreSlim _messageInQueue = new SemaphoreSlim(0);

        private readonly string _logName;
        private readonly string _endOfMessageString;
        private readonly bool _implicitEndOfMessageString;
        private readonly int _receiveTimeoutMs;
        private readonly IPAddress _hostIp;
        private readonly int _hostPort;

        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        
        private ConnectionStatusEnum _connectionStatus = ConnectionStatusEnum.Disconnected;
        private event Action<ConnectionStatusEnum> ConnectionStatusChanged;

        private ConnectionStatusEnum ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                if (_connectionStatus == value) return;
                _connectionStatus = value;
                ConnectionStatusChanged?.Invoke(_connectionStatus);
            }
        }
        
        /// <summary>
        /// Get ConnectionStatus changed asynchronously.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private Task<ConnectionStatusEnum> GetConnectionStatusChangedAsync(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ConnectionStatusEnum>();
            Action<ConnectionStatusEnum> connectionStatusChangedAction = status => tcs.TrySetResult(status);
            
            // Register callback status change
            ConnectionStatusChanged += connectionStatusChangedAction;

            // Register callback task canceled
            var ctRegistration = ct.Register(() => tcs.TrySetCanceled());

            return tcs.Task.ContinueWith(async t =>
            {
                // Unregister callbacks
                ctRegistration.Dispose();
                ConnectionStatusChanged -= connectionStatusChangedAction;
                return await t;
            }, CancellationToken.None).Unwrap();
        }

        private SemaphoreSlim _receiveMessageSignal;
        private SemaphoreSlim _sendMessageFailedSignal;

        /// <summary>
        /// Locks _tcpClient's network stream when writing.
        /// </summary>
        private readonly SemaphoreSlim _sendMessageLock = new SemaphoreSlim(1, 1);
        
        /// <summary>
        /// Send a message to the client. Cancelling <paramref name="ct"/> during WriteAsync
        /// will result in a lost connection. Thread-safe.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ct"></param>
        /// <returns>True if message sent. False if connection lost.</returns>
        public async Task<bool> WriteAsync(string msg, CancellationToken ct)
        {
            const string funcName = nameof(WriteAsync);

            if (ConnectionStatus == ConnectionStatusEnum.Disconnected)
            {
                return false;
            }

            Log.DebugEx(funcName, $"[{_logName}] Sending message {msg}", false);

            // Add end chars here
            var msgBytes = Encoding.UTF8.GetBytes(msg + (_implicitEndOfMessageString ? _endOfMessageString : string.Empty));

            await _sendMessageLock.WaitAsync(ct); // Lock

            try
            {
                using (ct.Register(() => _tcpStream.Dispose()))
                {
                    // NetworkStream does not override WriteAsync.
                    // This means that it will get the default behavior
                    // of Stream.WriteAsync which just throws the token away after checking it initially.
                    // Disposing _tcpStream cancels the write operation.
                    // 
                    // Throw IOException when write fails.
                    // Throw ObjectDisposedException when stream disposed.
                    await _tcpStream.WriteAsync(msgBytes, 0, msgBytes.Length, ct);

                    return true;
                }
            }
            catch (Exception e) when (e is ObjectDisposedException || e is IOException)
            {
                _sendMessageFailedSignal.TryRelease();
                ct.ThrowIfCancellationRequested();
                return false;
            }
            finally
            {
                _sendMessageLock.Release(); // Release
            }
        }

        /// <summary>
        /// Get a message from the client. 
        /// Returns null immediately when disconnected.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string> ReadAsync(CancellationToken ct)
        {
            var localCts = new CancellationTokenSource();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, localCts.Token))
            {
                // Register for connectionstatus changes
                var connChanged = GetConnectionStatusChangedAsync(linkedCts.Token);

                // Check current status
                if (ConnectionStatus == ConnectionStatusEnum.Disconnected)
                {
                    localCts.Cancel();
                    return null;
                }

                // Wait for a message
                var msgInQueue = _messageInQueue.WaitAsync(linkedCts.Token);
                
                // Wait for message or disconnect
                var done = await Task.WhenAny(msgInQueue, connChanged);
                ct.ThrowIfCancellationRequested();
                localCts.Cancel();

                if (connChanged == done) return null; // Disconnected
                
                // There is a message after _messageInQueue is released.
                string msg;
                return _messageQueue.TryDequeue(out msg) ? msg : null;
            }
        }

        /// <summary>
        /// Returns when the underlying connection is connected.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task GetConnectedAsync(CancellationToken ct)
        {
            if (ConnectionStatus == ConnectionStatusEnum.Connected) return;

            var localCts = new CancellationTokenSource();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, localCts.Token))
            {
                Task<ConnectionStatusEnum> connChanged;
                // Loop here while connectionstatus is disconnected.
                do
                {
                    connChanged = GetConnectionStatusChangedAsync(linkedCts.Token);

                    if (ConnectionStatus == ConnectionStatusEnum.Connected)
                    {
                        localCts.Cancel();
                        return;
                    }
                } while (await connChanged == ConnectionStatusEnum.Disconnected);
            }
        }

        public async Task Run(CancellationToken ct)
        {
            const string funcName = nameof(Run);
            
            Log.InfoEx(funcName, $"[{_logName}] Start client connection to host {_hostIp}:{_hostPort}", false);

            // Register CancellationToken to call _tcpClient.Close() on cancel.
            // ConnectAsync() then throws ObjectDisposedException and cancels properly.
            using (ct.Register(() => _tcpClient?.Close()))
            {
                await MainTask(ct).ContinueWith(t =>
                    Log.DebugEx(funcName, $"[{_logName}] Stop client connection to host {_hostIp}:{_hostPort}"), CancellationToken.None);
            }
        }

        private async Task MainTask(CancellationToken ct)
        {
            const string funcName = nameof(MainTask);

            while (!ct.IsCancellationRequested)
            {
                // Accept the new tcp connection
                try
                {
                    // ConnectAsync() throws ObjectDisposedException (derived from InvalidOperationException) when canceled.
                    // This is done by calling _tcpClient.Close().
                    // CancellationToken is registered on upper level function to call _tcpClient.Close() on cancel.
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(_hostIp, _hostPort);

                    // Reset these values for the new connection
                    _sendMessageFailedSignal = new SemaphoreSlim(0, 1);
                    _receiveMessageSignal = new SemaphoreSlim(0, 1);

                    Log.InfoEx(funcName, $"[{_logName}] Connected to {_tcpClient.Client.RemoteEndPoint}", false);

                    // Throw InvalidOperationException when disconnected and trying to get stream.
                    _tcpStream = _tcpClient.GetStream();

                    ConnectionStatus = ConnectionStatusEnum.Connected;
                }
                catch (Exception e) when (e is SocketException || e is InvalidOperationException)
                {
                    ConnectionStatus = ConnectionStatusEnum.Disconnected;
                    _tcpClient?.Close();

                    // Cancellation is done by throwing SocketException.
                    // Check CancellationToken.
                    ct.ThrowIfCancellationRequested();

                    Log.DebugEx(funcName, e.ToString(), false);
                    await Task.Delay(10000, ct);

                    continue;
                }

                try
                {
                    var localCts = new CancellationTokenSource();

                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, localCts.Token))
                    {
                        var receiveTask = ReceiveMessages(linkedCts.Token);
                        var sendFailTask = _sendMessageFailedSignal.WaitAsync(linkedCts.Token);
                        var messageTooOldTask = MessageTooOld(linkedCts.Token);
                        await Task.WhenAny(receiveTask, sendFailTask, messageTooOldTask);
                        ct.ThrowIfCancellationRequested();
                        localCts.Cancel();
                    }
                }
                finally
                {
                    ConnectionStatus = ConnectionStatusEnum.Disconnected;
                    _tcpStream.Dispose();
                    _tcpClient.Close();
                    Log.DebugEx(funcName, $"[{_logName}] Connection lost", false);

                    if (!_messageQueue.IsEmpty || _messageInQueue.CurrentCount > 0)
                    {
                        _messageQueue = new ConcurrentQueue<string>();
                        _messageInQueue = new SemaphoreSlim(0);
                    }
                }
            }
        }

        private async Task MessageTooOld(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var msg = await _receiveMessageSignal.WaitAsync(_receiveTimeoutMs, ct);
                if (!msg)
                {
                    return;
                }
            }
        }

        private async Task ReceiveMessages(CancellationToken ct)
        {
            const string funcName = nameof(ReceiveMessages);

            var rawResult = new byte[1024];
            var result = "";

            while (!ct.IsCancellationRequested)
            {
                int readBytes;

                try
                {
                    // NetworkStream does not override ReadAsync.
                    // This means that it will get the default behavior
                    // of Stream.ReadAsync which just throws the token away after checking it initially.
                    // Disposing _tcpStream cancels the read operation.
                    // 
                    // Throw IOException when read fails.
                    // Throw ObjectDisposedException when stream disposed.
                    readBytes = await _tcpStream.ReadAsync(rawResult, 0, rawResult.Length, ct);
                }
                catch (Exception e) when (e is ObjectDisposedException || e is IOException)
                {
                    ct.ThrowIfCancellationRequested();
                    return;
                }

                // If the remote host shuts down the connection, and all available data has been received, 
                // the Read method completes immediately and return zero bytes.
                if (readBytes < 1)
                {
                    return;
                }
                
                _receiveMessageSignal.TryRelease();

                result += Encoding.UTF8.GetString(rawResult, 0, readBytes);

                // Parse and handle each message
                while (!ct.IsCancellationRequested)
                {
                    var msgEndIndex = result.IndexOf(_endOfMessageString, StringComparison.Ordinal);

                    if (msgEndIndex < 0) break; // msgEndIndex == -1 => message end chars not found

                    var msg = result.Substring(0, _implicitEndOfMessageString ? msgEndIndex : msgEndIndex + _endOfMessageString.Length);
                    result = result.Substring(msgEndIndex + _endOfMessageString.Length);

                    Log.DebugEx(funcName, $"[{_logName}] Received message {msg}", false);

                    // Check token before adding message.
                    ct.ThrowIfCancellationRequested();

                    _messageQueue.Enqueue(msg);
                    _messageInQueue.Release();
                }
            }
        }
    }
}
