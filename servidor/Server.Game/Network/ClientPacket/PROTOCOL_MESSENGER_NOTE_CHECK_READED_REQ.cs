using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game.Network.ClientPacket
{
    /// <summary>
    /// Handles client requests to mark messenger notes/messages as read
    /// Updates message state in database and extends expiration date
    /// </summary>
    public class PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ : GameClientPacket
    {
        #region Private Fields

        /// <summary>
        /// List of message IDs to mark as read
        /// </summary>
        private readonly List<int> messageIds = new List<int>();

        /// <summary>
        /// Maximum number of messages that can be processed in a single request
        /// </summary>
        private const int MAX_MESSAGES_PER_REQUEST = 100;

        /// <summary>
        /// Number of days to extend message expiration when marked as read
        /// </summary>
        private const int EXPIRATION_EXTENSION_DAYS = 7;

        #endregion Private Fields

        #region Packet Reading

        /// <summary>
        /// Reads the packet data containing message IDs to mark as read
        /// </summary>
        public override void Read()
        {
            try
            {
                int messageCount = ReadC(); // Read number of messages (1 byte)

                // Validate message count to prevent abuse
                if (messageCount < 0 || messageCount > MAX_MESSAGES_PER_REQUEST)
                {
                    CLogger.Print($"Invalid message count in PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ: {messageCount}", LoggerType.Warning);
                    return;
                }

                // Read each message ID
                for (int i = 0; i < messageCount; i++)
                {
                    int messageId = ReadD(); // Read message ID (4 bytes)

                    // Validate message ID
                    if (messageId > 0)
                    {
                        messageIds.Add(messageId);
                    }
                    else
                    {
                        CLogger.Print($"Invalid message ID received: {messageId}", LoggerType.Warning);
                    }
                }

                CLogger.Print($"Read request to mark {messageIds.Count} messages as read", LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error reading PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Packet Reading

        #region Packet Processing

        /// <summary>
        /// Processes the request to mark messages as read
        /// Updates database with new expiration date and read state
        /// </summary>
        public override void Run()
        {
            try
            {
                // Validate client and player
                if (Client?.Player == null)
                {
                    CLogger.Print("PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ received from invalid client", LoggerType.Warning);
                    return;
                }

                // Validate we have messages to process
                if (messageIds.Count == 0)
                {
                    CLogger.Print($"No valid messages to mark as read for player {Client.PlayerId}", LoggerType.Debug);
                    return;
                }

                // Process the read status update
                bool updateSuccess = UpdateMessageReadStatus();

                // Send response if update was successful
                if (updateSuccess)
                {
                    SendReadConfirmation();
                    CLogger.Print($"Successfully marked {messageIds.Count} messages as read for player {Client.PlayerId}", LoggerType.Debug);
                }
                else
                {
                    CLogger.Print($"Failed to update message read status for player {Client.PlayerId}", LoggerType.Warning);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error in PROTOCOL_MESSENGER_NOTE_CHECK_READED_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Packet Processing

        #region Private Methods

        /// <summary>
        /// Updates the read status and expiration date of messages in the database
        /// </summary>
        /// <returns>True if update was successful</returns>
        private bool UpdateMessageReadStatus()
        {
            try
            {
                // Calculate new expiration date (7 days from now)
                DateTime newExpirationDate = DateTimeUtil.Now().AddDays(EXPIRATION_EXTENSION_DAYS);
                long expirationTimestamp = long.Parse(newExpirationDate.ToString("yyMMddHHmm"));

                // Prepare database update parameters
                string[] columnsToUpdate = { "expire_date", "state" };
                object[] newValues = { expirationTimestamp, 0 }; // 0 = read state

                // Convert message IDs to array for database operation
                int[] messageIdArray = messageIds.ToArray();

                // Log the database operation
                CLogger.Print($"Updating messages {string.Join(",", messageIdArray)} for player {Client.PlayerId}", LoggerType.Debug);
                CLogger.Print($"New expiration: {newExpirationDate:yyyy-MM-dd HH:mm}, State: Read", LoggerType.Debug);

                // Perform database update
                bool updateResult = ComDiv.UpdateDB(
                    "player_messages",           // tabla
                    "object_id",                 // columna WHERE
                    messageIdArray,              // valores WHERE
                    "owner_id",                  // columna WHERE adicional
                    Client.PlayerId,             // valor WHERE adicional
                    columnsToUpdate,             // columnas a actualizar
                    newValues                    // nuevos valores
                );

                if (!updateResult)
                {
                    CLogger.Print($"Database update failed for player {Client.PlayerId} messages: {string.Join(",", messageIdArray)}", LoggerType.Error);
                }

                return updateResult;
            }
            catch (FormatException ex)
            {
                CLogger.Print($"Error formatting expiration date: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error updating message read status: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Sends confirmation packet to client with the list of messages marked as read
        /// </summary>
        private void SendReadConfirmation()
        {
            try
            {
                var confirmationPacket = new PROTOCOL_MESSENGER_NOTE_CHECK_READED_ACK(messageIds);
                Client.SendPacket(confirmationPacket);

                CLogger.Print($"Sent read confirmation for {messageIds.Count} messages to player {Client.PlayerId}", LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error sending read confirmation packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Gets a summary of the messages being processed (for logging/debugging)
        /// </summary>
        /// <returns>String summary of message IDs</returns>
        private string GetMessageSummary()
        {
            if (messageIds.Count == 0)
                return "No messages";

            if (messageIds.Count <= 5)
                return $"Messages: [{string.Join(", ", messageIds)}]";

            return $"Messages: [{string.Join(", ", messageIds.Take(3))}...and {messageIds.Count - 3} more]";
        }

        #endregion Private Methods

        #region Public Properties (for debugging)

        /// <summary>
        /// Gets the number of messages in this request (for testing/debugging)
        /// </summary>
        public int MessageCount => messageIds.Count;

        /// <summary>
        /// Gets a copy of the message IDs (for testing/debugging)
        /// </summary>
        public IReadOnlyList<int> MessageIds => messageIds.AsReadOnly();

        #endregion Public Properties (for debugging)
    }
}