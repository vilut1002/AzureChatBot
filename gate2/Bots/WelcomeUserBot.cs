// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    // Represents a bot that processes incoming activities.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.
    // For example, the "MemoryStorage" object and associated
    // IStatePropertyAccessor{T} object are created with a singleton lifetime.
    public class WelcomeUserBot : ActivityHandler
    {
        // Messages sent to the user.
        private const string WelcomeMessage = "안녕하세요~ pre-SNACKS입니다!";


        private BotState _userState;

        // Initializes a new instance of the "WelcomeUserBot" class.
        public WelcomeUserBot(UserState userState)
        {
            _userState = userState;
        }


         // Greet when users are added to the conversation.
         // Note that all channels do not send the conversation update activity.
         // If you find that this bot works in the emulator, but does not in
         // another channel the reason is most likely that the channel does not
         // send this activity.
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync($"{WelcomeMessage}", cancellationToken: cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState());

            // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.
            var text = turnContext.Activity.Text.ToLowerInvariant();
            switch (text)
            {
                case "목적 알려주세요":
                    await turnContext.SendActivityAsync($"우리는 단계별 레시피와 응원을 통해 초보 유저도 쉽고 즐겁게 요리할 수 있도록 돕는 챗봇을 만들고 있습니다.");
                    break;
                default:
                    await turnContext.SendActivityAsync($"'목적 알려주세요'를 입력해보세요.", cancellationToken: cancellationToken);
                    break;
            }
            

            // Save any state changes.
            await _userState.SaveChangesAsync(turnContext);
        }

        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard();
            card.Title = "Welcome to Bot Framework!";
            card.Text = @"Welcome to Welcome Users bot sample! This Introduction card
                         is a great way to introduce your Bot to the user and suggest
                         some things to get them started. We use this opportunity to
                         recommend a few next steps for learning more creating and deploying bots.";
            card.Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") };
            card.Buttons = new List<CardAction>()
            {
                new CardAction(ActionTypes.OpenUrl, "Get an overview", null, "Get an overview", "Get an overview", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                new CardAction(ActionTypes.OpenUrl, "Ask a question", null, "Ask a question", "Ask a question", "https://stackoverflow.com/questions/tagged/botframework"),
                new CardAction(ActionTypes.OpenUrl, "Learn how to deploy", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
            };
            
            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }
    }
}
