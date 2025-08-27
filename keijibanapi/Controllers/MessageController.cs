// ===============================
// Controllers/MessageController.cs (API側) - 返信機能追加版
// ===============================
using Microsoft.AspNetCore.Mvc;
using keijibanapi.Models;
using keijibanapi.Services;

namespace keijibanapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(
            IMessageService messageService,
            ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// 送信メッセージ一覧取得
        /// </summary>
        [HttpGet("sent")]
        public async Task<ActionResult<MessageListResponse>> GetSentMessages(
            [FromQuery] int fromDeptId,
            [FromQuery] int limit = 10)
        {
                _logger.LogInformation($"Getting sent messages for department {fromDeptId}");

                var result = await _messageService.GetSentMessagesAsync(fromDeptId, limit);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// 全送信メッセージ取得
        /// </summary>
        [HttpGet("sent/all")]
        public async Task<ActionResult<SentMessageListResponse>> GetAllSentMessages([FromQuery] int deptId)
        {
                _logger.LogInformation($"Getting all sent messages for department {deptId}");
                var result = await _messageService.GetAllSentMessagesAsync(deptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            
        }

        /// <summary>
        /// 受信メッセージ一覧取得
        /// </summary>
        [HttpGet("received")]
        public async Task<ActionResult<MessageListResponse>> GetReceivedMessages(
            [FromQuery] int toDeptId,
            [FromQuery] int limit = 10)
        {
                _logger.LogInformation($"Getting received messages for department {toDeptId}");

                var result = await _messageService.GetReceivedMessagesAsync(toDeptId, limit);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// 全受信メッセージ取得（詳細画面用）
        /// </summary>
        [HttpGet("received/all")]
        public async Task<ActionResult<MessageListResponse>> GetAllReceivedMessages(
            [FromQuery] int deptId)
        {
                _logger.LogInformation($"Getting all received messages for department {deptId}");

                var result = await _messageService.GetAllReceivedMessagesAsync(deptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// メッセージ詳細取得
        /// </summary>
        [HttpGet("{messageId}")]
        public async Task<ActionResult<MessageDetailResponse>> GetMessageDetail(
            int messageId,
            [FromQuery] int deptId)
        {
                _logger.LogInformation($"Getting message detail for message {messageId}, department {deptId}");

                var result = await _messageService.GetMessageDetailAsync(messageId, deptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// メッセージ送信
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<SendMessageResponse>> SendMessage(
            [FromBody] SendMessageRequest request,
            [FromQuery] int fromDeptId)
        {
                _logger.LogInformation($"Sending message from department {fromDeptId}");

                var result = await _messageService.SendMessageAsync(request, fromDeptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// 返信メッセージ送信
        /// ★ 新規追加: 返信送信時に元メッセージのアクション状態を「返信済み」に更新
        /// </summary>
        [HttpPost("send/reply")]
        public async Task<ActionResult<SendMessageResponse>> SendReplyMessage(
            [FromBody] SendMessageRequest request,
            [FromQuery] int fromDeptId,
            [FromQuery] int originalMessageId)
        {
                _logger.LogInformation($"Sending reply message from department {fromDeptId} for original message {originalMessageId}");

                var result = await _messageService.SendReplyMessageAsync(request, fromDeptId, originalMessageId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// アクション状況更新
        /// </summary>
        [HttpPost("action")]
        public async Task<ActionResult<ActionUpdateResponse>> UpdateActionStatus(
            [FromBody] ActionUpdateRequest request,
            [FromQuery] int deptId)
        {
                _logger.LogInformation($"Updating action status for message {request.MessageId}, department {deptId}");

                var result = await _messageService.UpdateActionStatusAsync(request, deptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }

        /// <summary>
        /// メッセージ完了状態更新
        /// </summary>
        [HttpPost("done")]
        public async Task<ActionResult<MessageDoneResponse>> UpdateMessageDone(
            [FromBody] MessageDoneRequest request,
            [FromQuery] int deptId)
        {
                _logger.LogInformation($"Updating message done status for message {request.MessageId}, department {deptId}");

                var result = await _messageService.UpdateMessageDoneAsync(request, deptId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
        }
    }
}
