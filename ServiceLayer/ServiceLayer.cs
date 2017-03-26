using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common;
using NLog;

namespace ServiceLayer
{
    public class ServiceLayer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Task _frontEndTask;
        private Task _opUiTask;

        private readonly List<MessageData> _dataMessages;
        private readonly SemaphoreSlim _dataMsgLock;

        public ServiceLayer()
        {
            // Initialize the containers for messages.
            _dataMessages = new List<MessageData>();
            // Initialize Semaphores.
            _dataMsgLock = new SemaphoreSlim(1, 1);

            FrontEndConnection.OnFrontEndData += OnDataMessage; // Set the frontEnd data event listener.
        }

        public void Run(CancellationToken ct)
        {
            // Start the servers.
            _frontEndTask = FrontEndConnection.Run(ct); 
            _opUiTask = OperatorConnection.Run(ct); 
        }

        /// <summary>
        /// Function returning MessageData-objects.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<MessageData> ReadDataAsync(CancellationToken ct)
        {
            MessageData dataMsg = null; // Set return value as null.

            while (!ct.IsCancellationRequested)
            {
                if (!_dataMessages.Any()) // No data messages atm.
                {
                    await Task.Delay(20, ct); // Wait for 20ms and check again.
                }
                else // We have messages.
                {
                    await _dataMsgLock.WaitAsync(ct); // Wait for the lock.
                    dataMsg = _dataMessages.First(); // Get Data message and
                    _dataMessages.Remove(_dataMessages.First()); // remove it.
                    _dataMsgLock.TryRelease(); // Release the lock.
                    break; // Break out of the loop.
                }
            }
            ct.ThrowIfCancellationRequested();
            return dataMsg; // Return the data message.
        }

        public async Task<MessageConfig> ReadConfigAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, MessageData data, bool result)
        {
            const string funcName = nameof(WriteAsync);

            // Format the message.
            var message = new Message
            {
                Header = {
                MessageType = "RESPONSE",
                MessageContent = "DATA",
                Result = result,
                ResultSpecified = true
                },
                Item = data,
            };
            if (!data.RespondToOperatorUI) // Respond to front end.
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(Message));
                    var writer = new StringWriter();
                    serializer.Serialize(writer, message);
                    var responseString = writer.ToString();
                    return await FrontEndConnection.WriteResponse(responseString, ct); //     
                }
                catch (Exception ex)
                {
                    Log.ErrorEx(funcName, $"Exception occurred while writing data response to front-end: {ex}");
                    // TODO: Throw?
                }
            }
            else // Respond to operator ui.
            {
                var opMessage = new OperatorMessage
                {
                    Header = new OperatorMessageHeader
                    {
                        // TODO: Set Header infos.
                    },
                    Item = message // Append front end message as the item field.
                };
                try
                {
                    var serializer = new XmlSerializer(typeof(OperatorMessage));
                    var writer = new StringWriter();
                    serializer.Serialize(writer, opMessage);
                    var responseString = writer.ToString();
                    return await OperatorConnection.WriteResponse(responseString, ct);
                }
                catch (Exception ex)
                {
                    Log.ErrorEx(funcName, $"Exception occurred while writing data response to operator: {ex}");
                    // TODO: Throw?
                }
            }
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, MessageConfig config, bool result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event handler for data messages. Adds them to the private List-container incase they are properly formatted.
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task OnDataMessage(MessageData msgData,CancellationToken ct)
        {
            const string funcName = nameof(OnDataMessage);

            // TODO: See if all message data is properly formatted.
            if (msgData.Type.Equals("ORDER", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!msgData.OrderAmountSpecified || !msgData.OrderIdSpecified || msgData.Recipe.Length < 1) // TODO: See if this works.
                {
                    Log.ErrorEx(funcName, "Order contained illegal parameters.");

                    msgData.ErrorMsg = "Order contained illegal parameters.";
                    await WriteAsync(ct, msgData, false); // Write failure response.
                }
                else // Properly formmated, add new datamessage to list.
                {
                    await _dataMsgLock.WaitAsync(ct);
                    _dataMessages.Add(msgData);
                    _dataMsgLock.TryRelease();
                }
            }
        }
    }
}
