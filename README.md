# AzureChatBot

## 프로젝트 소개


## 시연 링크
https://vilut1002.github.io/AzureChatBot/index.html


## 개발 환경

- VISUAL STUDIO https://www.visualstudio.com

- Bot Framework v4 SDK Templates for Visual Studio https://aka.ms/bf-bc-vstemplate

- Bot Framework emulator https://github.com/Microsoft/BotFramework-Emulator

- Azure Trial https://azure.microsoft.com/ko-kr/free/


## 사용한 애져 리소스들


## Bot framework emulator를 사용한 프로젝트 실행 방법
1. Project, Bot framework emulator 다운로드
2. Visual Studio에서 로컬로 봇을 실행 (디버그)
3. Bot framework emulator에서 Open Bot
4. Bot Url : http://localhost:3978/api/messages
5. Microsoft App ID, password : appsettings.json 파일에서 확인
6. 봇 실행 후 사용자가 먼저 말을 걸면 대화 시작


## 유튜브 시연영상, 설명영상 링크
[KCC2020 MS Azure ChatBot 경진대회] PiBot - preSNACKS팀 시스템 아키텍트와 구현 설명영상 https://youtu.be/W8mF2LLnX9Y
[KCC2020 MS Azure ChatBot 경진대회] PiBot - preSNACKS팀 시연영상 https://youtu.be/6Q9ZaLvIgfs 


  
# 중심내용과 코드 설명  

## 기본 프로그램 아키텍처 설명
액티비티다이어그램, 전체소스 구조 그림으로 넣기

## 예약, 조회, 취소
### 파일 깃허브 링크
https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/BookingDialog.cs
https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/CheckAndCancelDialog.cs
### 코드 구조
### 사용한 샘플 봇
Microsoft / BotFramework-Samples https://github.com/microsoft/BotFramework-Samples
### 참고 문서
- Adaptive Cards Schema https://adaptivecards.io/explorer/ 
- 데이터 베이스  https://docs.microsoft.com/ko-kr/azure/azure-sql/
코드
- Google map API (maps static API) https://developers.google.com/maps/documentation/maps-static/overview?&hl=ko

## QnA
### 파일 깃허브 링크
https://github.com/vilut1002/AzureChatBot/blob/master/Pibot/Pibot/Dialogs/QnaDialog.cs
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


# About preSNACKS

