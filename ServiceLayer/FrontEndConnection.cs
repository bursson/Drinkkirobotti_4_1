using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common;
using NLog;

namespace ServiceLayer
{
    public static class FrontEndConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static event Func<MessageData,CancellationToken,Task> OnFrontEndData; // Events for data and config messages.
        public static event Func<MessageConfig,CancellationToken,Task> OnFrontEndConfig; //TODO:

        private const int FrontEndPort = 10001;
        private const string EndOfMessage = "\r\n"; // TODO End of message-string

        private static readonly Server ServerConnection = new Server(FrontEndPort, EndOfMessage, true, nameof(FrontEndConnection));
        

        public static async Task Run(CancellationToken ct)
        {
            var connTask = ServerConnection.Run(ct);
            while (!ct.IsCancellationRequested)
            {
                await ServerConnection.GetConnectedAsync(ct);
                Log.DebugEx(nameof(Run), "Connected");
                await HandleConnection(ct);
            }
            ct.ThrowIfCancellationRequested();
        }

        public static async Task<bool> WriteResponse(string msg, CancellationToken ct)
        {
            return await ServerConnection.WriteAsync(msg, ct, true); // Write the response.
        }

        private static async Task HandleConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await ServerConnection.ReadAsync(ct)) != null)
            {
                await HandleMessage(msg, ct);
            }
        }

        private static async Task HandleMessage(string msg, CancellationToken ct)
        {
            const string funcName = nameof(HandleMessage);

            Log.DebugEx(funcName, $"Received FrontEndMessage: {msg}");

            var serializer = new XmlSerializer(typeof(Message)); // Deserialize the message.
            var reader = new StreamReader(msg);
            try
            {
                var messageObject = (Message) serializer.Deserialize(reader);
                if (messageObject.Header.MessageType.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                    // Read Data message.
                    try
                    {
                        // TODO: Handle header stuff?
                        var msgData = (MessageData) messageObject.Item;
                        OnFrontEndData?.Invoke(msgData,ct);
                    }
                    catch (Exception ex)
                    {
                        Log.WarnEx(funcName, $"Data message handling failed: {ex}");
                        // TODO: What should we do?
                    }
                else if (messageObject.Header.MessageType.Equals("CONFIG", StringComparison.OrdinalIgnoreCase))
                { // Config Message.
                    try
                    {
                        // TODO: Handle header stuff?
                        var msgCfg = (MessageConfig)messageObject.Item;
                        OnFrontEndConfig?.Invoke(msgCfg, ct);
                    }
                    catch (Exception ex)
                    {
                        Log.WarnEx(funcName, $"Config message handling failed: {ex}");
                        // TODO: What should we do?
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorEx(funcName, $"Deserializing Front-end message failed: {ex}");
                var response = new Message
                {
                    Header = new MessageHeader()
                    {
                        MessageType = "RESPONSE",
                        MessageContent = "ERR",
                        Result = false,
                        ResultSpecified = true,
                    },
                    Item = new MessageData {
                    ErrorMsg = "Deserializing the message failed. Invalid message format."
                    }
                };
                var xmlSerializer = new XmlSerializer(typeof(Message));
                var responseStringWriter = new StringWriter();
                xmlSerializer.Serialize(responseStringWriter, response);
                await WriteResponse(responseStringWriter.ToString(), ct);
            }
        }

        
    }
}
