using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bot.AdaptiveCard.Prompt;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using AdaptiveCards;
using System;


namespace Pibot.Dialogs 
{
    public class CheckAndCancelDialog : ComponentDialog
    {
        static string AdaptivePromptId = "adaptive";

        public CheckAndCancelDialog(UserState userState) : base(nameof(CheckAndCancelDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                ActStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("예약 확인을 위해 이름과 연락처를 입력해주세요."), cancellationToken);

            var cardJson = File.ReadAllText("./Cards/bookingCheckCard.json");
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() { cardAttachment },
                    Type = ActivityTypes.Message,
                }
            };

            // Display a Text Prompt and wait for input
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);

            stepContext.Values["name"] = jobj["name"].ToString(); 
            stepContext.Values["phone"] = jobj["phone"].ToString(); 

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"테스트용 출력 사용자 이름 : {stepContext.Values["name"]}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"테스트용 출력 연락처 : {stepContext.Values["phone"]}"), cancellationToken);

            // 수민 쿼리해서 예약 내역 가져오기

            var choices = new[] { "예약취소", "처음으로" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,
                }).ToList<AdaptiveAction>(),
            };

            /* 수민 예약내역 가져온 걸로 변수부분 다 채우고 주석 풀기
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"{(string)stepContext.Values["name"]}님의 예약정보",
                Size = AdaptiveTextSize.Medium,
                Color = AdaptiveTextColor.Accent,
                Weight = AdaptiveTextWeight.Bolder
            });

            card.Body.Add(new AdaptiveFactSet()
            {
                Spacing = AdaptiveSpacing.Medium,
                Facts = new List<AdaptiveFact>()
                {
                    new AdaptiveFact()
                    {
                    Title = "이름",
                    Value = $"{(string)stepContext.Values["name"]}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "성별",
                    Value = $"{(string)stepContext.Values["sex"]}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "나이",
                    Value = $"{Convert.ToInt32(stepContext.Values["age"])}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "연락처",
                    Value = $"{(string)stepContext.Values["phone"]}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "헌혈의집",
                    Value = $"{(string)stepContext.Values["center"]}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "날짜",
                    Value = $"{(string)stepContext.Values["date"]}"
                    },
                    new AdaptiveFact()
                    {
                    Title = "시간",
                    Value = $"{(string)stepContext.Values["time"]}"
                    }
                }
            });
            */

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "　",
                Size = AdaptiveTextSize.Medium
            });

            return await stepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JObject.FromObject(card),
                    }),
                    Choices = ChoiceFactory.ToChoices(choices),
                    Style = ListStyle.None,
                },
                cancellationToken); ;
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (((FoundChoice)stepContext.Result).Value == "예약취소")
            {
                // 수민 예약 취소하기

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("예약이 성공적으로 취소되었습니다."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            else //처음으로
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}
