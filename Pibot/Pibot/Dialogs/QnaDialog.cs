// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Pibot.CognitiveModels;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Pibot.Dialogs
{
    public class QnaDialog : ComponentDialog
    {
        private readonly ILogger<QnaDialog> _logger;
        private readonly IBotServices _botServices;

        public QnaDialog(IBotServices botServices, ILogger<QnaDialog> logger, BookingDialog bookingDialog, UserState userState)
            : base(nameof(QnaDialog))
        {
            _logger = logger;
            _botServices = botServices;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("무엇이 궁금하세요?\r\n(처음으로 돌아가려면 '종료', 예약하려면 '예약'을 입력하세요.)") }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult.GetTopScoringIntent();

            if (topIntent.intent == "예약")
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);

            else if (topIntent.intent == "종료")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            else
            {
                switch (topIntent.intent)
                {
                    case "조건":
                        await stepContext.Context.SendActivityAsync($"헌혈 가능한 나이는 만 16세부터 69세까지이며, 헌혈 가능한 체중은 남자 50kg 이상, 여자 45kg 이상입니다.");
                        break;
                    case "운영_시간":
                        await stepContext.Context.SendActivityAsync($"헌혈의 집 운영시간은 8:00 ~ 20:00 이며 지점마다 다를 수 있으니 확인 부탁드립니다.");
                        break;
                    case "준비물":
                        await stepContext.Context.SendActivityAsync($"신분증을 준비해주세요. (주민등록증, 여권 등 관공서 및 공공기관이 발행한 것으로 사진과 주민등록번호가 확인가능한 것)");
                        break;
                    case "약물":
                        await stepContext.Context.SendActivityAsync($"약물 복용과 관련한 헌혈 가능조건은 다음과 같습니다.\n\n" +
                            $" - 건선 치료제 복용 후 3년 경과(일부는 영구 헌혈금지)\r\n" +
                            $" - 전립선비대증 치료제 복용 후 1개월 또는 6개월 경과\r\n" +
                            $" - 탈모증 치료제 복용 후 1개월 경과\r\n" +
                            $" - 여드름 치료제 복용 후 1개월 경과\r\n" +
                            $" - 기타 헌혈금지약물 복용 후 일정기간 경과");
                        break;

                    default:
                        var didntUnderstandMessageText = $"죄송합니다. 이해하지 못했어요.";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }

                return await stepContext.BeginDialogAsync(nameof(QnaDialog), new BookingDetails(), cancellationToken);
            }
        }
    }

}


