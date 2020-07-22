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

            // 수민 - 쿼리해서 예약 내역 가져오기
            // 이름 : stepContext.Values["name"]
            // 연락처 : stepContext.Values["phone"]

            var choices = new string[2];
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            //수민 - 예약 내역이 존재하지 않는 경우 조건문만 써주세요
            //if (예약 내역이 존재하지 않음)
            //{
            //    choices[0] = "예약하기";
            //    choices[1] = "종료";

            //    card.Body.Add(new AdaptiveTextBlock()
            //    {
            //        Text = $"{stepContext.Values["name"]}님의 예약 내역이 존재하지 않습니다.\r\n헌혈 예약 메뉴로 이동하시겠어요?",
            //    });
            //}

            // 수민 - 예약 정보 가져온 걸로 변수부분 다 바꿔주세요
            //else 
            //{
            //    choices[1] = "예약취소";
            //    choices[2] = "종료";

            //    card.Body.Add(new AdaptiveTextBlock()
            //    {
            //        Text = $"{(string)stepContext.Values["name"]}님의 예약정보",
            //        Size = AdaptiveTextSize.Medium,
            //        Color = AdaptiveTextColor.Accent,
            //        Weight = AdaptiveTextWeight.Bolder
            //    });

            //    card.Body.Add(new AdaptiveFactSet()
            //    {
            //        Spacing = AdaptiveSpacing.Medium,
            //        Facts = new List<AdaptiveFact>()
            //        {
            //            new AdaptiveFact()
            //            {
            //            Title = "이름",
            //            Value = $"{(string)stepContext.Values["name"]}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "성별",
            //            Value = $"{(string)stepContext.Values["sex"]}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "나이",
            //            Value = $"{Convert.ToInt32(stepContext.Values["age"])}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "연락처",
            //            Value = $"{(string)stepContext.Values["phone"]}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "헌혈의집",
            //            Value = $"{(string)stepContext.Values["center"]}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "날짜",
            //            Value = $"{(string)stepContext.Values["date"]}"
            //            },
            //            new AdaptiveFact()
            //            {
            //            Title = "시간",
            //            Value = $"{(string)stepContext.Values["time"]}"
            //            }
            //        }
            //    });
            //}

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "　",
                Size = AdaptiveTextSize.Medium
            });

            card.Actions = choices.Select(choice => new AdaptiveSubmitAction
            {
                Title = choice,
                Data = choice,
            }).ToList<AdaptiveAction>();

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
            if (((FoundChoice)stepContext.Result).Value == "예약하기")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("헌혈 예약 메뉴로 이동합니다."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            else if (((FoundChoice)stepContext.Result).Value == "예약취소")
            {
                // 수민 - 예약 내역 삭제하기

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("예약이 성공적으로 취소되었습니다."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            else // 처음으로
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
