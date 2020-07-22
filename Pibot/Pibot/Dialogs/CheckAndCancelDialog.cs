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
using System.Data.SqlClient;
using System.Text;


namespace Pibot.Dialogs 
{
    public class CheckAndCancelDialog : ComponentDialog
    {
        static string AdaptivePromptId = "adaptive";

        private BookingDetails bookingDetails= new BookingDetails();

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
            stepContext.Values["phone"] = (string)jobj["phone"].ToString();

            // 수민 - 쿼리해서 예약 내역 가져오기

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "team19-server.database.windows.net";
                builder.UserID = "chatbot19";
                builder.Password = "presnacks2020!";
                builder.InitialCatalog = "pibotDB";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    bookingDetails = Submit_Query(connection, $"SELECT * from reservInfo WHERE Phone = '{stepContext.Values["phone"]}';");
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("쿼리보냄"), cancellationToken);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            var choices = new string[2];
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            //수민 - 예약 내역이 존재하지 않는 경우 조건문만 써주세요
            if (bookingDetails == null)
            {
                choices[0] = "예약하기";
                choices[1] = "종료";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(bookingDetails.Name.ToString()), cancellationToken);
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"{stepContext.Values["name"]}님의 예약 내역이 존재하지 않습니다.\r\n헌혈 예약 메뉴로 이동하시겠어요?",
                });
            }
            // 수민 - 예약 정보 가져온 걸로 변수부분 다 바꿔주세요
            else 
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(bookingDetails.Center.ToString()), cancellationToken);
                stepContext.Values["name"] = bookingDetails.Name;
                stepContext.Values["sex"] = bookingDetails.Sex;
                stepContext.Values["age"] = bookingDetails.Age;
                stepContext.Values["phone"] = bookingDetails.Phone;
                stepContext.Values["date"] = bookingDetails.Date;
                stepContext.Values["time"] = bookingDetails.Time;
                stepContext.Values["center"] = bookingDetails.Center;

                choices[0] = "예약취소";
                choices[1] = "종료";
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
            }

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
                try
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                    builder.DataSource = "team19-server.database.windows.net";
                    builder.UserID = "chatbot19";
                    builder.Password = "presnacks2020!";
                    builder.InitialCatalog = "pibotDB";

                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        StringBuilder sb = new StringBuilder();
                        Submit_Cancel(connection,bookingDetails.ID);
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.ToString());
                }
                //-------------------------------------------------------예약 삭제 끝

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

        public BookingDetails Submit_Query(SqlConnection connection, string sql_query)
        {

            using (var command = new SqlCommand(sql_query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    
                    //while (reader.Read())
                    //{
                    if (reader.Read())
                    {

                        BookingDetails bookingQuery = new BookingDetails();
                        bookingQuery.ID = reader.GetInt32(0);   //ID
                        bookingQuery.Name = reader.GetString(1); //name
                        bookingQuery.Sex = reader.GetString(2); //sex
                        bookingQuery.Age = reader.GetInt32(3); //Age
                        bookingQuery.Phone = reader.GetString(4);
                        bookingQuery.Date = (reader.GetDateTime(5)).ToString().Substring(0, 11);    //Date
                        bookingQuery.Time = (reader.GetDateTime(5)).ToString().Substring(11);     //Time
                        bookingQuery.Center = reader.GetString(6);      //center

                        return bookingQuery;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        static void Submit_Cancel(SqlConnection connection, int ID)
        {
            string sql = $"DELETE FROM reservInfo WHERE reserv_id = '{ID}';";

            using (var command = new SqlCommand(sql, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine(rowsAffected + " = rows affected.");
            }
        }

    }
}
