# KCC2020 Chatbot 경진대회
한국정보과학회 x Microsoft x 디모아 공동개최의 Chatbot 경진대회   
최고기술상 및 특별상 수상
<p><img src="https://user-images.githubusercontent.com/53745427/90875133-a8597780-e3db-11ea-9e93-51e1fb784497.png" width="500"></p>

# PiBot
<p><img src="https://user-images.githubusercontent.com/53745427/90875386-0be3a500-e3dc-11ea-859c-cb6c40afb530.png" width="500"></p>

PiBot 실행하기[https://vilut1002.github.io/AzureChatBot/index.html](https://vilut1002.github.io/AzureChatBot/index.html)

피봇은 MS Azure를 사용해 만든 챗봇으로, 대한적십자사의 헌혈 예약, 조회, 취소 등의 서비스와 함께 헌혈에 대한 다양한 정보를 제공합니다.

## 시연 링크
- [챗봇 시연영상](https://youtu.be/6Q9ZaLvIgfs)    
- [시스템 아키텍트와 구현 설명영상](https://youtu.be/W8mF2LLnX9Y)   



## 개발 환경

- [VISUAL STUDIO](https://www.visualstudio.com)
- [Bot Framework v4 SDK Templates for Visual Studio](https://aka.ms/bf-bc-vstemplate)
- [Bot Framework emulator](https://github.com/Microsoft/BotFramework-Emulator)
- [Azure Trial](https://azure.microsoft.com/ko-kr/free/)
- .NET framework

## Bot framework emulator를 사용한 프로젝트 실행 방법
1. [개발환경](#개발-환경) 셋팅
2. git clone 
    ```git clone https://github.com/vilut1002/AzureChatBot```
3. Visual Studio에서 로컬로 봇을 실행 (디버그)
4. Bot framework emulator에서 Open Bot 후 실행
    Bot Url ex) localhost:3978/api/messages
 
* * *
## 기본 프로그램 아키텍처 설명
### 전체 소스코드 구조
<p> <img src="https://user-images.githubusercontent.com/53745427/90875378-0ab27800-e3dc-11ea-87cd-c5e80caba878.png" width="800"> </p>

### 액티비티 다이어그램
<p> <img src="https://user-images.githubusercontent.com/53745427/90875377-09814b00-e3dc-11ea-980e-92484bea647e.jpg" width="600"> </p>

## 프로그램 기능

### 예약, 조회, 취소   

#### 다이얼로그 파일
* [예약 하기 BookingDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/BookingDialog.cs)   
* [예약 조회 및 취소 CheckAndCancelDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/CheckAndCancelDialog.cs)

#### 코드 구조
- 예약
<p> <img src="https://user-images.githubusercontent.com/53745427/90875385-0be3a500-e3dc-11ea-9433-43f1da91a125.png" width="500"> </p>

- 조회 및 취소
<p> <img src="https://user-images.githubusercontent.com/53745427/90875381-0b4b0e80-e3dc-11ea-8a8f-9f4ae33c1380.png" width="500"> </p>

#### 사용한 봇 
[Microsoft / csharp dotnet CoreBot](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/13.core-bot)
#### 참고 문서
- 채팅 GUI: [Adaptive Cards Schema](https://adaptivecards.io/explorer/) 
- 데이터 베이스 : [MS Azure SQL](https://docs.microsoft.com/ko-kr/azure/azure-sql/)
    * [데이터베이스 구축방법](https://kimbuzzi.tistory.com/2)
    * <details><summary>.NET 코드 예시</summary>   
            
            try
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
                                // 애트리뷰트 순서로 index 구분해서 bookingDetail 인스턴스에 저장
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
    </details>  
- [Google map API (maps static API)](https://developers.google.com/maps/documentation/maps-static/overview?&hl=ko)
    <details> <summary> maps static API 코드 예시 </summary>
    
        http://maps.google.com/maps/api/staticmap?center={위도},{경도}&zoom=16&size=512x512&maptype=roadmap&markers=color:red%7C{위도},{경도}&sensor=false&key=<발급받은 API key>
    </details>
    
* * *
### QnA

#### 다이얼로그 파일
 [QnaDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/QnaDialog.cs)

#### 코드 구조
<p> <img src="https://user-images.githubusercontent.com/53745427/90875379-0ab27800-e3dc-11ea-860d-3a7fd0e0792e.png" width="500"> </p>

#### LUIS (Azure 자연어처리 서비스)
[LUIS applications](https://www.luis.ai/applications)에서 Authoring resource 생성 후, intent와 entity를 의도에 맞게 추가함. 
    <details><summary>LUIS 사용방법</summary>  
        [BotServices.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/BotServices.cs)와 [IBotServices.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/IBotServices.cs)를 추가한다.
    
           private readonly IBotServices _botServices;
           var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
           var topIntent = recognizerResult.GetTopScoringIntent();
           
           //intent에 따라 다른 기능을 실행
           if (topIntent.intent == "예약")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("헌혈 예약 메뉴로 이동합니다."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            else if (topIntent.intent == "종료")
                return await stepContext.EndDialogAsync(null, cancellationToken);
        
   </details>  

#### 사용한 봇 프레임워크 
[Microsoft / BotFramework-Samples/nlp-with-dispatch](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/14.nlp-with-dispatch)

#### 참고 문서
[Microsoft docs, Azure/cognitive service/LUIS](https://docs.microsoft.com/ko-kr/azure/cognitive-services/luis/what-is-luis)



* * *
## About preSNACKS
이화여자대학교 컴퓨터공학 전공   
이수민 vilut1002@gmail.com   
김은아 eaz03@ewhain.net   
이다해 dah3132@gmail.com   
