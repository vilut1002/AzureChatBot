// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

using Pibot.CognitiveModels;

namespace Pibot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(BookingDialog bookingDialog, QnaDialog qnaDialog, QuizDialog quizDialog, ILogger<MainDialog> logger, UserState userState)
            : base(nameof(MainDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(qnaDialog);
            AddDialog(quizDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // 히어로카드 
            var card = new HeroCard
            {
                Images = new List<CardImage> { new CardImage("http://drive.google.com/uc?export=view&id=1naJclWdMneN6JrZHoFRdU36DFjx8AlDj") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.ImBack, title: "헌혈 예약하기", value: "헌혈 예약하기"),
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
            if((string)stepContext.Result=="헌혈 예약하기")
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            else if ((string)stepContext.Result == "QUIZ")
                return await stepContext.BeginDialogAsync(nameof(QuizDialog), new BookingDetails(), cancellationToken);
            else
                return await stepContext.BeginDialogAsync(nameof(QnaDialog), new BookingDetails(), cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is BookingDetails result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                var timeProperty = new TimexProperty(result.Date);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"{userProfile.Name}님의 예약이 성공적으로 접수되었습니다. 감사합니다!";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
