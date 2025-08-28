// ====================================
// Repositories/MessageRepository.cs
// ====================================
using Dapper;
using keijibanapi.DataTransferObjects;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
        
        // MessageRepository.cs に追加 (1/6)
        public async Task<IEnumerable<Message>> GetSentMessagesAsync(int fromDeptId, int limit)
        {
            const string sql = @"
        SELECT message_id as MessageId, subject, from_dept_id as FromDeptId, priority, due_date as DueDate,
               requires_action as RequiresAction, created_at as CreatedAt, recipient_count as RecipientCount,
               read_count as ReadCount, action_required_count as ActionRequiredCount,
               action_completed_count as ActionCompletedCount, messagedone, original_message_id as OriginalMessageId
        FROM message_countplus
        WHERE from_dept_id = @FromDeptId AND is_active = TRUE AND messagedone = 0
          AND (expire_date IS NULL OR expire_date >= CURDATE())
        ORDER BY created_at DESC LIMIT @Limit";
            using var connection = CreateConnection();
            return await connection.QueryAsync<Message>(sql, new { FromDeptId = fromDeptId, Limit = limit });
        }

        // MessageRepository.cs に追加 (2/6)
        public async Task<IEnumerable<ReceivedMessageDto>> GetReceivedMessagesAsync(int toDeptId, int limit)
        {
            const string sql = @"
                SELECT m.message_id as MessageId, m.subject, m.from_dept_id as FromDeptId, m.priority,
                       m.due_date as DueDate, m.requires_action as RequiresAction, m.created_at as CreatedAt,
                       m.messagedone, m.original_message_id as OriginalMessageId, mr.is_read as IsRead,
                       ma.action_status as ActionStatus
                FROM messages m
                LEFT JOIN message_recipients mr ON m.message_id = mr.message_id
                LEFT JOIN message_actions ma ON m.message_id = ma.message_id AND mr.to_dept_id = ma.dept_id
                WHERE mr.to_dept_id = @ToDeptId AND m.is_active = TRUE AND m.messagedone = 0
                  AND (m.expire_date IS NULL OR m.expire_date >= CURDATE())
                ORDER BY mr.is_read ASC, m.priority DESC, m.created_at DESC LIMIT @Limit";
            using var connection = CreateConnection();
            return await connection.QueryAsync<ReceivedMessageDto>(sql, new { ToDeptId = toDeptId, Limit = limit });
        }

        // MessageRepository.cs に追加 (3/6)
        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            const string sql = @"
        SELECT message_id as MessageId, subject, content, from_dept_id as FromDeptId, priority, category,
               due_date as DueDate, requires_action as RequiresAction, created_at as CreatedAt,
               messagedone, original_message_id as OriginalMessageId
        FROM messages WHERE message_id = @MessageId";
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Message>(sql, new { MessageId = messageId });
        }

        public async Task<IEnumerable<MessageRecipient>> GetRecipientsByMessageIdAsync(int messageId)
        {
            const string sql = @"
        SELECT recipient_id as RecipientId, message_id as MessageId, to_dept_id as ToDeptId, is_read,
               read_at as ReadAt, requires_action as RequiresAction, created_at as CreatedAt
        FROM message_recipients WHERE message_id = @MessageId";
            using var connection = CreateConnection();
            return await connection.QueryAsync<MessageRecipient>(sql, new { MessageId = messageId });
        }

        public async Task<IEnumerable<MessageAction>> GetActionsByMessageIdAsync(int messageId)
        {
            const string sql = @"
        SELECT action_id as ActionId, message_id as MessageId, dept_id as DeptId, action_status as ActionStatus,
               action_comment as ActionComment, action_date as ActionDate, created_at as CreatedAt, updated_at as UpdatedAt
        FROM message_actions WHERE message_id = @MessageId ORDER BY updated_at DESC";
            using var connection = CreateConnection();
            return await connection.QueryAsync<MessageAction>(sql, new { MessageId = messageId });
        }

        public async Task MarkAsReadAsync(int messageId, int deptId)
        {
            const string sql = @"
        UPDATE message_recipients SET is_read = TRUE, read_at = NOW()
        WHERE message_id = @MessageId AND to_dept_id = @DeptId AND is_read = FALSE";
            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new { MessageId = messageId, DeptId = deptId });
        }

        // MessageRepository.cs に追加 (4/6)
        public async Task<long> CreateMessageAsync(SendMessageRequest request, int fromDeptId, int? originalMessageId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string insertMessageSql = @"
            INSERT INTO messages (from_dept_id, subject, content, priority, category, due_date, requires_action, recipient_count, original_message_id)
            VALUES (@FromDeptId, @Subject, @Content, @Priority, @Category, @DueDate, @RequiresAction, @RecipientCount, @OriginalMessageId);
            SELECT LAST_INSERT_ID();";

                var messageId = await connection.ExecuteScalarAsync<long>(insertMessageSql, new
                {
                    FromDeptId = fromDeptId,
                    request.Subject,
                    request.Content,
                    Priority = request.Priority.ToString(),
                    request.Category,
                    request.DueDate,
                    request.RequiresAction,
                    RecipientCount = request.ToDeptIds.Count,
                    OriginalMessageId = originalMessageId
                }, transaction);

                const string insertRecipientSql = @"
            INSERT INTO message_recipients (message_id, to_dept_id, requires_action)
            VALUES (@MessageId, @ToDeptId, @RequiresAction)";

                foreach (var toDeptId in request.ToDeptIds)
                {
                    await connection.ExecuteAsync(insertRecipientSql, new { MessageId = messageId, ToDeptId = toDeptId, request.RequiresAction }, transaction);
                }

                await transaction.CommitAsync();
                return messageId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // MessageRepository.cs に追加 (5/6)
        public async Task UpdateActionStatusAsync(ActionUpdateRequest request, int deptId)
        {
            const string checkSql = "SELECT action_id FROM message_actions WHERE message_id = @MessageId AND dept_id = @DeptId";
            using var connection = CreateConnection();
            var existingActionId = await connection.ExecuteScalarAsync<int?>(checkSql, new { request.MessageId, DeptId = deptId });

            if (existingActionId.HasValue)
            {
                const string updateSql = @"
            UPDATE message_actions SET action_status = @Status, action_comment = @Comment, action_date = NOW(), updated_at = NOW()
            WHERE action_id = @ActionId";
                await connection.ExecuteAsync(updateSql, new { Status = request.Status.ToString(), request.Comment, ActionId = existingActionId.Value });
            }
            else
            {
                const string insertSql = @"
            INSERT INTO message_actions (message_id, dept_id, action_status, action_comment, action_date)
            VALUES (@MessageId, @DeptId, @Status, @Comment, NOW())";
                await connection.ExecuteAsync(insertSql, new { request.MessageId, DeptId = deptId, Status = request.Status.ToString(), request.Comment });
            }
        }

        public async Task UpdateActionStatusToRepliedAsync(int originalMessageId, int replierDeptId)
        {
            var updateRequest = new ActionUpdateRequest
            {
                MessageId = originalMessageId,
                Status = ActionStatus.Replied,
                Comment = "返信済み"
            };
            await UpdateActionStatusAsync(updateRequest, replierDeptId);
        }

        // MessageRepository.cs に追加 (6/6)
        public async Task<IEnumerable<Message>> GetAllSentMessagesAsync(int fromDeptId)
        {
            const string sql = @"
        SELECT m.message_id as MessageId, m.subject, m.priority, m.due_date as DueDate,
               m.requires_action as RequiresAction, m.created_at as CreatedAt, m.messagedone,
               m.original_message_id as OriginalMessageId, GROUP_CONCAT(mr.to_dept_id SEPARATOR ',') as ToDeptIds
        FROM messages m
        LEFT JOIN message_recipients mr ON m.message_id = mr.message_id
        WHERE m.from_dept_id = @FromDeptId AND m.is_active = TRUE AND m.messagedone = 0
        GROUP BY m.message_id ORDER BY m.created_at DESC";
            using var connection = CreateConnection();
            return await connection.QueryAsync<Message>(sql, new { FromDeptId = fromDeptId });
        }

        public async Task<bool> UpdateMessageDoneAsync(int messageId, bool isDone)
        {
            const string sql = "UPDATE messages SET messagedone = @IsDone, updated_at = NOW() WHERE message_id = @MessageId";
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { MessageId = messageId, IsDone = isDone ? 1 : 0 });
            return rowsAffected > 0;
        }
    };
}

