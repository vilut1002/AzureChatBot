# AzureChatBot

## 프로젝트 소개


## 시연 링크
https://vilut1002.github.io/AzureChatBot/index.html


## develop_environ

- [VISUAL STUDIO](https://www.visualstudio.com)
- [Bot Framework v4 SDK Templates for Visual Studio](https://aka.ms/bf-bc-vstemplate)
- [Bot Framework emulator](https://github.com/Microsoft/BotFramework-Emulator)
- [Azure Trial](https://azure.microsoft.com/ko-kr/free/)
- .NET framework


## 사용한 애져 리소스들


## Bot framework emulator를 사용한 프로젝트 실행 방법
1. [개발환경](#develop_environ) 셋팅
2. Visual Studio에서 로컬로 봇을 실행 (디버그)
3. Bot framework emulator에서 Open Bot
4. Bot Url ex) http://localhost:3978/api/messages
5. Microsoft App ID, password : appsettings.json 파일에서 확인
6. 봇 실행 후 사용자가 먼저 말을 걸면 대화 시작


## 유튜브 시연영상, 설명영상 링크
[[KCC2020 MS Azure ChatBot 경진대회] PiBot - preSNACKS팀 시스템 아키텍트와 구현 설명영상](https://youtu.be/W8mF2LLnX9Y)
[[KCC2020 MS Azure ChatBot 경진대회] PiBot - preSNACKS팀 시연영상](https://youtu.be/6Q9ZaLvIgfs) 

    
         
  
# 중심내용과 코드 설명  

## 기본 프로그램 아키텍처 설명
액티비티다이어그램, 전체소스 구조 그림으로 넣기

## 예약, 조회, 취소
### 파일 깃허브 링크
[예약 하기 BookingDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/BookingDialog.cs)
[예약 조회 및 취소 CheckAndCancelDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/CheckAndCancelDialog.cs)
### 코드 구조
### 사용한 샘플 봇
[Microsoft / csharp dotnet CoreBot](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/13.core-bot)
### 참고 문서
- 채팅 GUI: [Adaptive Cards Schema](https://adaptivecards.io/explorer/) 
- 데이터 베이스 : [MS Azure SQL](https://docs.microsoft.com/ko-kr/azure/azure-sql/)
    <details><summary>.NET 코드 예시</summary><code>try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "yourServerName.database.windows.net";
                builder.UserID = "yourID";
                builder.Password = "yourPW";
                builder.InitialCatalog = "yourDBname";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string sql_query = $"SELECT * from reservInfo WHERE Phone = '{stepContext.Values["phone"]}'AND reserv_date>current_timestamp;";
                    using (var command = new SqlCommand(sql_query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // 애트리뷰트별로 index 구분해서 bookingDetail 인스턴스에 저장
                                BookingDetails bookingQuery = new BookingDetails();
                                bookingQuery.ID = reader.GetInt32(0);
                                bookingQuery.Name = reader.GetString(1);
                                bookingQuery.Sex = reader.GetString(2);
                                bookingQuery.Age = reader.GetInt32(3);
                                bookingQuery.Phone = reader.GetString(4);
                                DateTime myDate = reader.GetDateTime(5);
                                string convertedDate = myDate.ToString("yyyy-MM-ddhh:mm");
                                string dateStr = convertedDate.Substring(0, 10);
                                string timeStr = convertedDate.Substring(10);
                                bookingQuery.Date = dateStr;
                                bookingQuery.Time = timeStr;
                                bookingQuery.Center = reader.GetString(6);

                                return bookingQuery;
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
    </code></details>  
- [Google map API (maps static API)](https://developers.google.com/maps/documentation/maps-static/overview?&hl=ko)

## QnA
### 파일 깃허브 링크
 [QnaDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/QnaDialog.cs)
### 코드 구조(이미지)
### LUIS (Azure 자연어처리 서비스)
LUIS applications(https://www.luis.ai/applications)에서 Authoring resource 생성 후, intent와 entity를 의도에 맞게 추가함. 
### 참고한 샘플 봇 
Microsoft / BotFramework-Samples/nlp-with-dispatch
https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/14.nlp-with-dispatch 
### 참고 문서
Microsoft docs, Azure/cognitive service/LUIS https://docs.microsoft.com/ko-kr/azure/cognitive-services/luis/what-is-luis



## 퀴즈
- 파일 깃허브 링크
https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/QuizDialog.cs
- 코드 구조
- 사용한 샘플 봇 파일과 그 url(저희 봇샘플에서 가져온거)
- 참고한 문서


### About preSNACKS

