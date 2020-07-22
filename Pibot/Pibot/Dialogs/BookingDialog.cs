// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AdaptiveCards;

using Bot.AdaptiveCard.Prompt;
using System;

namespace Pibot.Dialogs
{
    public class BookingDialog : ComponentDialog
    {
        static string AdaptivePromptId = "adaptive";

        public BookingDialog(UserState userState)
            : base(nameof(BookingDialog))
        {
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NotesStepAsync,
                CheckStepAsync,
                PersonalInfoStepAsync,
                HouseChoiceStepAsync,
                //Center1StepAsync,
                //Center2StepAsync,
                DateStepAsync,
                TimeStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
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

        private async Task<DialogTurnResult> NotesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new[] { "완료" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,
                }).ToList<AdaptiveAction>(),
            };

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "헌혈 예약을 시작하기 전에, 유의사항을 확인해주세요.\r\n" +
                        "- 6개월 후까지 예약할 수 있으며, 당일 예약은 불가합니다.\r\n" +
                        "- 최근 헌혈혈액검사에 따라 헌혈이 제한될 수 있습니다.\r\n" +
                        "- 예약시간 경과 시 예약이 취소되니 주의해 주십시오.\r\n" +
                        "- 헌혈의 집 도착 시 예약헌혈자임을 직원에게 말씀해 주십시오.",
                    Size = AdaptiveTextSize.Default
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
        
            private async Task<DialogTurnResult> CheckStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (((FoundChoice)stepContext.Result).Value == "완료")
            {
                var checkcards = new Attachment[]
                {
                    new HeroCard(
                        title: "자격조건 확인",
                        text: "　\r\n" +
                              "헌혈 예약을 위해 찾아주신 것을 진심으로 환영합니다!\r\n" +
                              "하지만 헌혈을 하려면 자격조건을 모두 만족해야 한답니다.\r\n" +
                              "자격조건을 꼼꼼히 읽어보세요.\r\n" +
                              "그리고 결격 사유는 없는지 스스로 자격조건을 확인해주세요.\r\n" +
                              "카드를 끝까지 넘겨볼까요?"
                        ).ToAttachment(),
                    new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1NILDL2SVEePd4maHuVNfN7rQhdmtfXTO" } }
                        ).ToAttachment(),
                    new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1Cb-epikRSeWtcpsJaiszoj731gWKRV-W" } }
                        ).ToAttachment(),
                    new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1o8tHm3TaOyCjwi0W-h1r0Mg82MXC6aKj" } }
                        ).ToAttachment(),
                   new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1rTUP3ikaB75EOQtjw4AMGPJ_boCBdGYZ" } }
                        ).ToAttachment(),
                    new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1Iw5GI4pGxfTnAitqYOrGi02tJbYZ0pvG" } }
                        ).ToAttachment(),
                    new HeroCard(
                        images: new CardImage[]
                        { new CardImage() { Url = "http://drive.google.com/uc?export=view&id=1c7Iqbm4WlSm9RaiIfLjd_omk3rXyICH9" } }
                        ).ToAttachment(),
                    new HeroCard(
                        title: "확인하셨나요?",
                        text: "　\r\n" +
                              "자격 조건을 하나라도 만족하지 못한다면 아쉽지만 헌혈에 참여하실 수 없어요...\r\n" +
                              "혹시 결격 사유가 있나요?\r\n" +
                              "　\r\n" ,
                        buttons: new List<CardAction>
                        {
                            new CardAction(ActionTypes.ImBack, title: "있음", value: "있음"),
                            new CardAction(ActionTypes.ImBack, title: "없음", value: "없음"),
                        }
                        ).ToAttachment(),
                };

                var reply = MessageFactory.Attachment(checkcards);
                reply.Attachments = checkcards;
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                var messageText = "";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"죄송합니다. 유의사항을 확인하지 않으시면 예약을 진행할 수 없어요."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> PersonalInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result == "없음")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"감사합니다! 예약에 필요한 개인정보를 입력해주세요."), cancellationToken);

                var cardJson = File.ReadAllText("./Cards/personalInfoCard.json");
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

                return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"죄송합니다. 결격 사유가 있으시면 헌혈에 참여하실 수 없어요."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> HouseChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);

            stepContext.Values["agree"] = jobj["agree"].ToString();

            if ((string)stepContext.Values["agree"] == "false")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"죄송합니다. 개인정보 수집 및 이용에 동의하지 않으시면 예약을 진행할 수 없어요."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            stepContext.Values["name"] = jobj["name"].ToString();
            stepContext.Values["sex"] = jobj["sex"].ToString();
            stepContext.Values["age"] = jobj["age"].ToString();
            stepContext.Values["phone"] = jobj["phone"].ToString();

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{(string)stepContext.Values["name"]}님의 소중한 개인정보 저장이 완료되었습니다."), cancellationToken);

            // Create the Adaptive Card
            var cardJson = File.ReadAllText("./Cards/houseCard.json");
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            // Create the text prompt
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

        /*
        private async Task<DialogTurnResult> Center1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);

            stepContext.Values["agree"] = jobj["agree"].ToString();

        }
        */

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //await stepContext.Context.SendActivityAsync($"{stepContext.Result}"); //데이터 확인 출력용

            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);

            stepContext.Values["center"] = jobj["center"].ToString();

            // Create the Adaptive Card
            var cardJson = File.ReadAllText("./Cards/dateCard.json");
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            // Create the text prompt
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

        private async Task<DialogTurnResult> TimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);

            stepContext.Values["date"] = jobj["date"].ToString();

            // Create the Adaptive Card
            var cardJson = File.ReadAllText("./Cards/timeCard.json");
            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            // Create the text prompt
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

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {   
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            
            stepContext.Values["time"] = jobj["time"].ToString();

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"예약 정보를 확인하신 후 예약을 확정해주세요."), cancellationToken);

            var choices = new[] { "예약확정"};
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,
                }).ToList<AdaptiveAction>(),
            };

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
            if (((FoundChoice)stepContext.Result).Value == "예약확정")
            {
                var bookingDetails = (BookingDetails)stepContext.Options;

                bookingDetails.Name = (string)stepContext.Values["name"];
                bookingDetails.Sex = (string)stepContext.Values["sex"];
                bookingDetails.Age = Convert.ToInt32(stepContext.Values["age"]);
                bookingDetails.Phone = (string)stepContext.Values["phone"];
                bookingDetails.Center = (string)stepContext.Values["center"];
                bookingDetails.Date = (string)stepContext.Values["date"];
                bookingDetails.Time = (string)stepContext.Values["time"];

                return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"죄송합니다. 다시 예약해주세요."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

    }

}
