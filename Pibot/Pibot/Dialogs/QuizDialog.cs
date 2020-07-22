// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using AdaptiveCards;
using System.IO;
using Newtonsoft.Json;
using Bot.AdaptiveCard.Prompt;

namespace Pibot.Dialogs
{
    public class QuizDialog : ComponentDialog
    {
        static string AdaptivePromptId = "adaptive";

        private static int score = 0;
        public QuizDialog(UserState userState) : base(nameof(QuizDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                StartStepAsync,
                Q1StepAsync,
                Q1checkStepAsync,
                Q2StepAsync,
                Q2checkStepAsync,
                Q3StepAsync,
                Q3checkStepAsync,
                Q4StepAsync,
                Q4checkStepAsync,
                Q5StepAsync,
                Q5checkStepAsync,
                Q6StepAsync,
                Q6checkStepAsync,
                Q7StepAsync,
                Q7checkStepAsync,
                Q8StepAsync,
                Q8checkStepAsync,
                Q9StepAsync,
                Q9checkStepAsync,
                Q10StepAsync,
                Q10checkStepAsync,
                FinalStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));

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

        private async Task<DialogTurnResult> StartStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new[] { "시작하기" };
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
                Text = "헌혈에 대한 여러 오해와 진실을 알아보는 시간!",
                Color = AdaptiveTextColor.Accent,
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "헌혈에 관한 퀴즈는 총 10문제 입니다.",
                Size = AdaptiveTextSize.Default
            });

            return await stepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(new Attachment
                    {ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card),}),
                    Choices = ChoiceFactory.ToChoices(choices),
                    Style = ListStyle.None,
                },
                cancellationToken); 
        }

        private static async Task<DialogTurnResult> Q1StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz1.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q1checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈증서는 혈액관리법 제3조(혈액매매행위등의 금지)에 의해 매매가 금지되어 있습니다. 최근 인터넷상에 수혈관련 사연을 등록하여 헌혈증서를 모아서 이를 다시 수혈이 필요한 환자 및 보호자 등에 판매한다는 기사가 소개된 적이 있습니다.\n\n " +
                       "혈액관리법에서는 '누구든지 금전, 재산상의 이익 기타 대가적 급부를 주거나 주기로 하고 타인의 혈액(제14조의 규정에 의한 헌혈증서를 포함한다)을 제공하거나 이를 약속하여서는 아니된다'고 규정하고 있습니다. 그러므로 헌혈증서를 사고 파는 것은 위법 행위이며 관련법규에 의하여 처벌을 받을 수 있습니다.\n\n" +
                       "헌혈증서가 필요하신 분께서는 전국 적십자사 혈액원(기관리스트 참조)으로 문의하시면 기증증서를 소정의 절차를 거쳐 받으실 수 있습니다.자세한 사항은 해당지역 혈액원으로 연락주시기 바랍니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q2StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz2.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q2checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "우리 몸에 있는 혈액량은 남자의 경우 체중의 8%, 여자는 7% 정도입니다. 예를 들어 체중이 60Kg인 남자의 몸 속에는 약 4,800mL의 혈액이 있고, 50Kg인 여자는 3,500mL 정도의 혈액을 가지고 있습니다. \n\n" +
                      "전체 혈액량의 15 % 는 비상시를 대비해 여유로 가지고 있는 것으로, 헌혈 후 충분한 휴식을 취하면 건강에 아무런 지장을 주지 않습니다. \n\n" +
                      "신체 내·외부의 변화에 대한 조절능력이 뛰어난 우리 몸은 헌혈 후 1~2일 정도면 일상생활에 지장이 없도록 혈관 내외의 혈액순환이 회복됩니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q3StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz3.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q3checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈과정은 매우 안전합니다.\n\n" + 
                       "헌혈에 사용되는 모든 기구(바늘, 혈액백 등)은 무균처리되어 있으며, 한번 사용 후에는 모두 폐기처분 하기 때문에 헌혈로 인해 에이즈등 다른질병에 감염될 위험은 전혀 없습니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q4StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz4.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q4checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈을 하면 헌혈량 만큼이 체외로 빠져나오는 것은 사실이지만 조직에 있던 혈액이 혈관 내로 바로 이동하여 보상하며, 이후 며칠 또는 몇 주간 음식 및 수분 섭취 등으로 원래 상태로 보충됩니다. \n\n" +
                       "따라서 헌혈은 다이어트와는 무관합니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q5StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz5.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q5checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "o")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 O 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈은 자기 몸에 여유로 가지고 있는 혈액을 나눠주는 것으로, 헌혈 전에 충분한 혈액이 있는지를 판단하기 위해 적혈구 내의 혈색소(헤모글로빈) 치를 측정합니다. 따라서 헌혈로 빈혈에 걸리지는 않습니다.\n\n" + 
                      "또한 헌혈자를 보호하기 위하여 연간 헌혈가능 횟수도 전혈헌혈은 5회로 제한하고 있습니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q6StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz6.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q6checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "혈관은 외부로부터 바늘이 들어오면 순간적으로 수축합니다.\n\n" +
                       "그러나 곧 본래의 상태로 회복되므로, 헌혈의 횟수와 혈관수축과는 아무런 상관이 없습니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q7StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz7.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q7checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "o")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 O 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "일시적으로 헌혈에 참여하지 못한 경우, 다시 헌혈에 참여하실 수 있습니다. \n\n" +
                      "헌혈 부적격 사유는 매우 다양하지만 우리나라의 헌혈 부적격 사유 중 가장 많은 비율을 차지하고 있는 것은 저비중(최근 5년간 부적격 사유 중 평균 43.7% 차지) 입니다. \n\n" +
                      "혈액속의 혈색소(헤모글로빈)는 항상 일정하지 않기 때문에 헌혈 전 검사를 통해 헌혈가능 여부를 확인하고 있으며, 기타 질병 또는 약복용과 관련된 부적격은 사유별로 기간이 다르므로 헌혈의 집 간호사나 각 혈액원에 문의 하는 것이 좋습니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q8StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz8.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q8checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "o")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 O 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈한 혈액은 혈액형검사, B형간염 항원검사, C형간염 항체검사, ALT검사, 매독항체검사, HIV검사를 실시하나 에이즈 검사를 목적으로 헌혈에 참여하는 것을 막기 위해서 에이즈(HIV) 검사결과는 통보해 주지 않습니다.\n\n" +
                      "현재 각 구청 보건소에서는 개인의 비밀을 보장하면서 에이즈 검사를 실시하고 있으니 참고바랍니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q9StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz9.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q9checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "헌혈자의 모든 헌혈기록이나 검사결과는 비밀이 보장되며, 본인이 아닌 다른 분들에게는 공개되지 않도록 법적으로 보호됩니다. \n\n " +
                      "또한 개인정보보호를 위해 독립된 문진공간에서 문진이 진행되며 문진항목에 대한 답변 또한 비밀이 유지됩니다. \n\n" +
                      "또한 헌혈혈액 검사결과는 헌혈 후 1개월 정도 이내에 개인이 원하는 장소로 검사결과통보서를 발송해 줍니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> Q10StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardJson = File.ReadAllText("./Cards/Quiz/quiz10.json");
            var cardAttachment = new Attachment() { ContentType = "application/vnd.microsoft.card.adaptive", Content = JsonConvert.DeserializeObject(cardJson), };
            var opts = new PromptOptions { Prompt = new Activity { Attachments = new List<Attachment>() { cardAttachment }, Type = ActivityTypes.Message, } };
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private static async Task<DialogTurnResult> Q10checkStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string json = @$"{stepContext.Result}";
            JObject jobj = JObject.Parse(json);
            stepContext.Values["choice"] = jobj["choice"].ToString();

            if (jobj["choice"].ToString() == "x")
                score++;

            var choices = new[] { "다음" };
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            { Actions = choices.Select(choice => new AdaptiveSubmitAction { Title = choice, Data = choice, }).ToList<AdaptiveAction>(), };

            card.Body.Add(new AdaptiveTextBlock()
            { Text = "정답은 X 입니다.", Size = AdaptiveTextSize.Default, Weight = AdaptiveTextWeight.Bolder });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Size = AdaptiveTextSize.Default,
                Text = "많은 사람들이 아직도 혈액사업에 대해 많은 오해를 가지고 있습니다. 가장 많은 오해가 바로 혈액을 병원에 공급하고 받는 혈액수가와 연관된 부분일 것입니다. \n\n" +
                      "대한적십자사는 혈액관리에 사용되는 재원을 혈액수가에만 의존하고 있으며, 국민들이 지로 형태로 납부하는 적십자회비와는 전혀 무관합니다. \n\n" +
                      "혈액수가는 혈액원의 인건비, 의료품비, 기념품비, 헌혈의 집 임대비등 운영비와 홍보비 등에 사용되며, 우리나라의 혈액수가는 일본, 미국 등 주요 OECD국가의 1 / 4 수준입니다."
            });

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None,
            }, cancellationToken); ;
        }

        private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new[] { "결과 보기" };
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
                Text = "퀴즈 완료",
                Color = AdaptiveTextColor.Accent,
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "점수를 확인해보세요!",
                Size = AdaptiveTextSize.Default
            });

            return await stepContext.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(new Attachment
                    { ContentType = AdaptiveCard.ContentType, Content = JObject.FromObject(card), }),
                    Choices = ChoiceFactory.ToChoices(choices),
                    Style = ListStyle.None,
                },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"당신의 퀴즈 점수는 100점 만점에 {score*10}점 입니다. \n\n 퀴즈를 통해 유익한 헌혈 지식을 얻어가셨기를 바랍니다!"), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
