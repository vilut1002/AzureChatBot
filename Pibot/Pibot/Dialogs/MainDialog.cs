// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Pibot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(BookingDialog bookingDialog, CheckAndCancelDialog checkAndCancelDialog, QnaDialog qnaDialog, QuizDialog quizDialog, ILogger<MainDialog> logger, UserState userState)
            : base(nameof(MainDialog))
        {
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(bookingDialog);
            AddDialog(checkAndCancelDialog);
            AddDialog(qnaDialog);
            AddDialog(quizDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
                ReturnStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Images = new List<CardImage> { new CardImage("http://drive.google.com/uc?export=view&id=1wU1TiDkOX54c_aeYEnOjNAzb0MB6JdoI") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.ImBack, title: "헌혈 예약하기", value: "헌혈 예약하기"),
                    new CardAction(ActionTypes.ImBack, title: "예약 확인·취소", value: "예약 확인·취소"),
                    new CardAction(ActionTypes.ImBack, title: "QnA", value: "QnA"),
                    new CardAction(ActionTypes.ImBack, title: "QUIZ", value: "QUIZ")
                },
            };

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.Attachments.Add(card.ToAttachment());
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            var messageText = "";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result == "헌혈 예약하기")
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            else if ((string)stepContext.Result == "예약 확인·취소")
                return await stepContext.BeginDialogAsync(nameof(CheckAndCancelDialog), null, cancellationToken);
            else if ((string)stepContext.Result == "QUIZ")
                return await stepContext.BeginDialogAsync(nameof(QuizDialog), null, cancellationToken);
            else
            {
                var msg = "헌혈에 대해 궁금하신 것을 알려드릴게요!\r\n" +
                          "다음과 같이 입력해보세요.\r\n" +
                          "- 헌혈의 집 운영시간 알려줘.\r\n" +
                          "- 여드름 치료제 복용 중인데 헌혈할 수 있을까?\r\n" +
                          "- 헌혈하러 갈 때 뭐 필요해?\r\n" +
                          "- 은평구 헌혈의집 / 성남시 헌혈의집 알려줘\r\n"+
                          "※ 그만하시려면 '종료'를 입력하세요.\r\n";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(QnaDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is BookingDetails result)
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{result.Name}님의 예약이 성공적으로 접수되었습니다. 감사합니다!"), cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Choices = ChoiceFactory.ToChoices(new List<string> { "처음으로" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}