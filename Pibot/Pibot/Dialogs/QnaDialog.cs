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
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pibot.Dialogs
{
    public class QnaDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public QnaDialog(UserState userState)
            : base(nameof(QnaDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NotesStepAsync,
                NameStepAsync,
                SexStepAsync,
                AgeStepAsync,
                PhoneStepAsync,
                HouseChoiceStepAsync,
                DateStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }

        private async Task<DialogTurnResult> NotesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var notes = $"※ 헌혈 예약 시 유의사항 ※\r\n";
            notes += $"- 6개월 후까지 예약할 수 있으며, 당일 예약은 불가합니다.\r\n";
            notes += $"- 최근 헌혈혈액검사에 따라 헌혈이 제한될 수 있습니다.\r\n";
            notes += $"- 예약시간 경과 시 예약이 취소되니 주의해 주십시오.\r\n";
            notes += $"- 헌혈의 집 도착 시 예약헌혈자임을 직원에게 말씀해 주십시오.\r\n";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(notes), cancellationToken);

            var activity = new Attachment[]
            {
                new HeroCard(
                    title: "신체조건",
                    images: new CardImage[] { new CardImage() { Url = "https://www.bloodinfo.net/image/character_img.png" } },
                    buttons: new CardAction[]
                    {
                        new CardAction(title: "열기", type: ActionTypes.OpenUrl, value: "https://www.bloodinfo.net/image/character_img.png")
                    })
                .ToAttachment(),
                new HeroCard(
                    title: "약물",
                    images: new CardImage[] { new CardImage() { Url = "https://www.bloodinfo.net/image/character_img.png" } },
                    buttons: new CardAction[]
                    {
                        new CardAction(title: "열기", type: ActionTypes.OpenUrl, value: "https://www.bloodinfo.net/image/character_img.png")
                    })
                .ToAttachment(),
                new HeroCard(
                    title: "어쩌구",
                    images: new CardImage[] { new CardImage() { Url = "https://www.bloodinfo.net/image/character_img.png" } },
                    buttons: new CardAction[]
                    {
                        new CardAction(title: "열기", type: ActionTypes.OpenUrl, value: "https://www.bloodinfo.net/image/character_img.png")
                    })
                .ToAttachment()
            };

            var reply = MessageFactory.Attachment(activity);
            reply.Attachments = activity;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("유의사항 확인 및 자가 문진을 완료하셨나요?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("이름을 입력해주세요.") }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"유의사항 확인 및 자가 문진을 완료하지 않으시면 예약을 진행할 수 없습니다."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }
        }


        private async Task<DialogTurnResult> SexStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("성별을 선택해주세요."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "남성", "여성" }),
                }, cancellationToken);
        }

        /*
        private async Task<DialogTurnResult> SexStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // Define choices
            var choices = new[] { "남성", "여성" };
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(
                CHOICEPROMPT,
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                // Convert the AdaptiveCard to a JObject
                Content = JObject.FromObject(card),
                    }),
                    Choices = ChoiceFactory.ToChoices(choices),
            // Don't render the choices outside the card
            Style = ListStyle.None,
                },
                cancellationToken);
        }
        */

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["sex"] = ((FoundChoice)stepContext.Result).Value;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("나이를 입력해주세요."),
                RetryPrompt = MessageFactory.Text("다시 입력해주세요."),
            };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0);
        }

        private async Task<DialogTurnResult> PhoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((int)stepContext.Result >= 16 && (int)stepContext.Result <= 69)
            {
                stepContext.Values["age"] = (int)stepContext.Result;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("연락처를 입력해주세요.") }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"죄송합니다. 나이가 16세 미만, 69세 초과일 경우 헌혈을 하실 수 없습니다."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> HouseChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("방문하실 헌혈의 집 센터를 선택해주세요."),
                Choices = ChoiceFactory.ToChoices(new List<string> { "신촌점", "홍대점", "합정점" }),
            }, cancellationToken);
        }


        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["house"] = ((FoundChoice)stepContext.Result).Value;

            var bookingDetails = (BookingDetails)stepContext.Options;
            return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingDetails.Date, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["date"] = (string)stepContext.Result;

            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            var bookingDetails = (BookingDetails)stepContext.Options;

            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.Sex = (string)stepContext.Values["sex"];
            userProfile.Age = (int)stepContext.Values["age"];
            userProfile.Phone = (string)stepContext.Values["phone"];
            bookingDetails.House = (string)stepContext.Values["house"];
            bookingDetails.Date = (string)stepContext.Values["date"];

            var bookingResult = $"{userProfile.Name}님의 예약 정보입니다.{System.Environment.NewLine}";
            bookingResult += $"성별 : {userProfile.Sex}\r\n";
            bookingResult += $"나이 : {userProfile.Age}\r\n";
            bookingResult += $"연락처 : {userProfile.Phone}\r\n";
            bookingResult += $"지점 : {bookingDetails.House}\r\n";
            bookingResult += $"날짜 : {bookingDetails.Date}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(bookingResult), cancellationToken);

            var msg = $"예약 정보가 맞는지 확인해주세요.";
            var promptMessage = MessageFactory.Text(msg, msg, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var bookingDetails = (BookingDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"정보를 확인하시고 다시 예약해주세요."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }

}
