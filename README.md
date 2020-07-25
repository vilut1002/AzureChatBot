# PiBot
<p><img src="http://drive.google.com/uc?export=view&id=1wU1TiDkOX54c_aeYEnOjNAzb0MB6JdoI" width="500"></p>
피봇은 MS Azure를 사용해 만든 챗봇으로, 대한적십자사의 헌혈 예약, 조회, 취소 등의 서비스와 함께 헌혈에 대한 다양한 정보를 제공합니다.

## 시연 링크
- [챗봇 실행주소](https://vilut1002.github.io/AzureChatBot/index.html)
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


## 기본 프로그램 아키텍처 설명
### 전체 소스코드 구조
<p> <img src="http://drive.google.com/uc?export=view&id=1BbyL3hi-wq3SnMhOuwLSdPFCXP1meyhH" width="800"> </p>
### 액티비티 다이어그램
<p> <img src="http://drive.google.com/uc?export=view&id=1k2Uz0AtBLjfEIr4qvw83lPE4rp_pnUmd" width="800"> </p>

## 예약, 조회, 취소   
### 다이얼로그 파일
* [예약 하기 BookingDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/BookingDialog.cs)   
* [예약 조회 및 취소 CheckAndCancelDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/CheckAndCancelDialog.cs)
### 코드 구조
### 사용한 봇 
[Microsoft / csharp dotnet CoreBot](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/13.core-bot)
### 참고 문서
- 채팅 GUI: [Adaptive Cards Schema](https://adaptivecards.io/explorer/) 
- 데이터 베이스 : [MS Azure SQL](https://docs.microsoft.com/ko-kr/azure/azure-sql/)
    <details><summary>.NET 코드 예시</summary>   
            
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

## QnA

### 다이얼로그 파일
 [QnaDialog.cs](https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/QnaDialog.cs)

## 코드 구조(이미지)

### LUIS (Azure 자연어처리 서비스)
[LUIS applications](https://www.luis.ai/applications)에서 Authoring resource 생성 후, intent와 entity를 의도에 맞게 추가함. 

### 사용한 봇 프레임워크 
[Microsoft / BotFramework-Samples/nlp-with-dispatch](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/14.nlp-with-dispatch)

### 참고 문서
[Microsoft docs, Azure/cognitive service/LUIS](https://docs.microsoft.com/ko-kr/azure/cognitive-services/luis/what-is-luis)




## About preSNACKS
이화여자대학교 컴퓨터공학 전공   
이수민 vilut1002@gmail.com   
김은아 eaz@ewhain.net   
이다해 dah3132@gmail.com   
